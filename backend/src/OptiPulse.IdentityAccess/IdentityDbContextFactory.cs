using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// Design-time factory for `dotnet ef migrations add`.
///
/// Uses **Npgsql, not SQLite** (constitution v2.2.0: PostgreSQL is the single migrated
/// provider). See the note on <c>FlagsDbContextFactory</c> for why the design-time provider is
/// deliberately not the dev runtime provider (T093).
///
/// No live server is contacted to scaffold a migration, so this connection string only has to
/// be well-formed. Override with OPTIPULSE_DESIGNTIME_CONNECTION when running `database update`
/// against a real instance.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("OPTIPULSE_DESIGNTIME_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=optipulse;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new IdentityDbContext(options);
    }
}
