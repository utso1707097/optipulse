using Microsoft.EntityFrameworkCore;

namespace OptiPulse.IdentityAccess;

/// <summary>Per-context DbContext for Identity & Access, over the shared physical
/// database (Principle I context isolation). Entities only in this MVP slice —
/// see User.cs / RefreshToken.cs for scope notes.</summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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
    }
}
