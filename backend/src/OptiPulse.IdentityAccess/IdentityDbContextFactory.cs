using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.IdentityAccess;

/// <summary>Design-time factory for `dotnet ef migrations add`.</summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlite("Data Source=optipulse-identity.dev.db")
            .Options;
        return new IdentityDbContext(options);
    }
}
