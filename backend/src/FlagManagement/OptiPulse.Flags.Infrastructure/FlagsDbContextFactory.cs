using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.Flags.Infrastructure;

/// <summary>Design-time factory for `dotnet ef migrations add` — SQLite is the
/// dev-time provider per constitution (SQLite dev / PostgreSQL prod).</summary>
public sealed class FlagsDbContextFactory : IDesignTimeDbContextFactory<FlagsDbContext>
{
    public FlagsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FlagsDbContext>()
            .UseSqlite("Data Source=optipulse-flags.dev.db")
            .Options;
        return new FlagsDbContext(options);
    }
}
