using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.Flags.Infrastructure;

/// <summary>
/// Design-time factory for `dotnet ef migrations add`.
///
/// Uses **Npgsql, not SQLite** (constitution v2.2.0: PostgreSQL is the single migrated
/// provider). The provider named here decides the DDL dialect of every generated migration,
/// which is separate from the provider the app runs on. Reading "SQLite dev / PostgreSQL prod"
/// as "author migrations with SQLite" is what produced migrations that could not apply to
/// Postgres at all (`42804`, boolean expression into an integer column) — see T093.
///
/// No live server is contacted to scaffold a migration, so this connection string only has to
/// be well-formed. Override with OPTIPULSE_DESIGNTIME_CONNECTION when running `database update`
/// against a real instance.
/// </summary>
public sealed class FlagsDbContextFactory : IDesignTimeDbContextFactory<FlagsDbContext>
{
    public FlagsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("OPTIPULSE_DESIGNTIME_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=optipulse;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<FlagsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new FlagsDbContext(options);
    }
}
