using Microsoft.EntityFrameworkCore;
using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// Per-context DbContext for Audit & Telemetry, over the shared physical database
/// (Principle I context isolation). Enforces append-only immutability for
/// AuditEntry in-process (FR-019, SC-006); full DB-level UPDATE/DELETE revocation
/// is a Phase 7 (T081) hardening task.
/// </summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<ExposureEvent> ExposureEvents => Set<ExposureEvent>();
    public DbSet<ConversionEvent> ConversionEvents => Set<ConversionEvent>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<PushDevice> PushDevices => Set<PushDevice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExposureEvent>(builder =>
        {
            builder.ToTable("ExposureEvents");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.HasIndex(e => e.FlagKey);
            builder.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<ConversionEvent>(builder =>
        {
            builder.ToTable("ConversionEvents");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.FlagKey).IsRequired().HasMaxLength(120);
            builder.Property(e => e.Goal).IsRequired().HasMaxLength(120);
            builder.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(200);
            builder.Property(e => e.VariantKey).HasMaxLength(120);
            builder.Property(e => e.ContextKey).HasMaxLength(200);
            builder.Property(e => e.Value).HasPrecision(18, 4);

            // UNIQUE, and this is the whole point: a retried report must collide here rather
            // than insert a second row. Enforced by the database, not by a read-then-write in
            // application code, which would race with a concurrent duplicate.
            builder.HasIndex(e => e.IdempotencyKey).IsUnique();
            builder.HasIndex(e => e.FlagKey);
        });

        modelBuilder.Entity<Alert>(builder =>
        {
            builder.ToTable("Alerts");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Kind).HasConversion<string>().HasMaxLength(40);
            builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Detail).IsRequired().HasMaxLength(2000);
            builder.Property(e => e.DedupeKey).IsRequired().HasMaxLength(300);
            builder.Property(e => e.FlagKey).HasMaxLength(120);
            builder.Property(e => e.AcknowledgedBy).HasMaxLength(200);

            // UNIQUE for the same reason ConversionEvents.IdempotencyKey is: detectors evaluate
            // a STANDING condition on a timer, so a spike lasting ten minutes would otherwise
            // raise an alert every pass. Enforced by the database rather than a read-then-write,
            // which would race two detector runs against each other.
            builder.HasIndex(e => e.DedupeKey).IsUnique();
            builder.HasIndex(e => e.RaisedAt);
        });

        modelBuilder.Entity<PushDevice>(builder =>
        {
            builder.ToTable("PushDevices");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Platform).HasConversion<string>().HasMaxLength(20);
            builder.Property(e => e.Token).IsRequired().HasMaxLength(400);

            // One row per token. A device that re-registers the same token must not accumulate
            // rows, or one alert becomes several notifications to one phone.
            builder.HasIndex(e => e.Token).IsUnique();
            builder.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<AuditEntry>(builder =>
        {
            builder.ToTable("AuditEntries");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.ChangeType).HasConversion<string>().HasMaxLength(30);

            builder.ComplexProperty(e => e.Actor, actor =>
            {
                actor.Property(a => a.ActorId).HasColumnName("Actor_Id");
                actor.Property(a => a.Role).HasColumnName("Actor_Role").HasConversion<string>().HasMaxLength(20);
                actor.Property(a => a.DisplayName).HasColumnName("Actor_DisplayName").HasMaxLength(200);
            });

            builder.Property(e => e.BeforeStateJson).HasColumnName("BeforeState");
            builder.Property(e => e.AfterStateJson).HasColumnName("AfterState");

            builder.HasIndex(e => e.TargetId);
            builder.HasIndex(e => e.Timestamp);
        });
    }

    /// <summary>
    /// Append-only enforcement (FR-019, T081).
    ///
    /// BOTH save overloads are guarded. Only the async one was, which left a silent bypass:
    /// `SaveChanges()` is a different virtual method, so an ordinary synchronous save would
    /// modify or delete an audit row without ever reaching this check — and would pass every
    /// test and every gate on the way.
    ///
    /// This is the application-layer half of the guarantee. It protects code going through this
    /// DbContext and nothing else; anything holding the connection string (psql, a migration, a
    /// raw SQL call, a restore) is unaffected. The database-level half is a trigger installed by
    /// the AuditImmutability migration, which is what makes the claim true rather than merely
    /// intended.
    /// </summary>
    public override int SaveChanges()
    {
        GuardAppendOnly();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        GuardAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GuardAppendOnly();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        GuardAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void GuardAppendOnly()
    {
        foreach (var entry in ChangeTracker.Entries<AuditEntry>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    "AuditEntry is append-only — modification and deletion are not permitted (FR-019).");
        }
    }
}
