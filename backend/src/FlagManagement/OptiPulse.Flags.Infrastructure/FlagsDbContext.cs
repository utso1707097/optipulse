using Microsoft.EntityFrameworkCore;
using OptiPulse.Flags.Domain;

namespace OptiPulse.Flags.Infrastructure;

/// <summary>
/// Per-context DbContext for Flag Management, over the shared physical database
/// (constitution v2.1.0 / research R5: no single shared AppDbContext — each
/// bounded context owns its own DbContext for isolation, Principle I).
/// </summary>
public sealed class FlagsDbContext(DbContextOptions<FlagsDbContext> options) : DbContext(options)
{
    public DbSet<Flag> Flags => Set<Flag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flag>(builder =>
        {
            builder.ToTable("Flags");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Key).IsRequired().HasMaxLength(120);
            builder.HasIndex(f => f.Key).IsUnique();
            builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
            builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(f => f.Version).IsConcurrencyToken();

            // TargetingRules and Rollout are value objects — stored as owned JSON columns
            // rather than a normalized table, adequate for this MVP's read-mostly bootstrap use.
            builder.Property(f => f.TargetingRules)
                .HasConversion(TargetingRuleListConverter.Instance, TargetingRuleListConverter.Comparer)
                .HasColumnName("TargetingRulesJson");

            builder.OwnsOne(f => f.Rollout, rollout =>
            {
                rollout.Property(r => r.PercentageBasisPoints).HasColumnName("Rollout_PercentageBasisPoints");
                rollout.Property(r => r.Salt).HasColumnName("Rollout_Salt");
            });
        });
    }
}
