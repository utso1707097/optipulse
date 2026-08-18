using Microsoft.EntityFrameworkCore;

namespace OptiPulse.IdentityAccess;

/// <summary>Per-context DbContext for Identity & Access, over the shared physical
/// database (Principle I context isolation). Entities only in this MVP slice —
/// see User.cs / RefreshToken.cs for scope notes.</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ServiceAccount> ServiceAccounts => Set<ServiceAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(320);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.TokenHash).IsUnique();
            builder.HasIndex(t => t.FamilyId);
        });

        modelBuilder.Entity<ServiceAccount>(builder =>
        {
            builder.ToTable("ServiceAccounts");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.Property(a => a.KeyHash).IsRequired().HasMaxLength(64);
            // Unique: the hash is the lookup key for authentication, so a duplicate would make
            // one credential resolve to two accounts.
            builder.HasIndex(a => a.KeyHash).IsUnique();
        });
    }
}
