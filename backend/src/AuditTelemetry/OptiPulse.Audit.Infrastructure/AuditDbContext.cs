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

    /// <summary>Append-only enforcement: any attempt to modify or delete a
    /// persisted AuditEntry is rejected before it reaches the database.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditEntry>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    "AuditEntry is append-only — modification and deletion are not permitted (FR-019).");
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
