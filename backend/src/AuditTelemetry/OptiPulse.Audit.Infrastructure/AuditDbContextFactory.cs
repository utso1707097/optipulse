using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>Design-time factory for `dotnet ef migrations add`.</summary>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseSqlite("Data Source=optipulse-audit.dev.db")
            .Options;
        return new AuditDbContext(options);
    }
}
