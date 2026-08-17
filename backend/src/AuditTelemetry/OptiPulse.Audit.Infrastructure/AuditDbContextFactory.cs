using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// Design-time factory for `dotnet ef migrations add`.
///
/// Uses **Npgsql, not SQLite** (constitution v2.2.0: PostgreSQL is the single migrated
/// provider). EF Core generates provider-specific DDL from whichever provider this factory
/// names, so a SQLite factory silently produces migrations that are invalid against Postgres —
/// which is exactly what happened before T093: `MigrateAsync` against Postgres failed with
/// `42804` (a boolean expression assigned to an integer column). SQLite remains supported as a
/// dev/edge convenience, provisioned by schema creation rather than by these migrations.
///
/// No live server is contacted to scaffold a migration, so this connection string only has to
/// be well-formed. Override with OPTIPULSE_DESIGNTIME_CONNECTION when running `database update`
/// against a real instance.
/// </summary>
public sealed class AuditDbContextFactory : IDesignTimeDbContextFactory<AuditDbContext>
{
    public AuditDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("OPTIPULSE_DESIGNTIME_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=optipulse;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AuditDbContext(options);
    }
}
