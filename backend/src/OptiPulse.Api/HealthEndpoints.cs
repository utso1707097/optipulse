using Microsoft.EntityFrameworkCore;
using OptiPulse.Evaluation.Application;
using OptiPulse.Flags.Infrastructure;

namespace OptiPulse.Api;

/// <summary>
/// Liveness and readiness, deliberately separated — a hosting platform uses them for different
/// decisions and conflating them causes outages.
///
/// LIVENESS answers "is this process wedged?" It must NOT touch dependencies. If liveness failed
/// whenever Postgres blipped, the platform would kill and restart a perfectly healthy container
/// that is correctly serving last-known-good evaluations (Principle IV) — turning a dependency
/// blip into an outage of the one component that was still working.
///
/// READINESS answers "should traffic come here?" and may check dependencies. Redis is reported
/// but does NOT fail readiness: evaluation serves from the in-memory snapshot without it, so a
/// Redis outage degrades invalidation, not availability.
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
            .AllowAnonymous()
            .ExcludeFromDescription();

        app.MapGet("/health/ready", async (
            FlagsDbContext flags, ISnapshotStore snapshot, CancellationToken ct) =>
        {
            bool databaseReachable;
            try
            {
                databaseReachable = await flags.Database.CanConnectAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                databaseReachable = false;
            }

            var current = snapshot.Current;

            // Not ready without the database: the process cannot bootstrap a snapshot or serve
            // management traffic, so sending it requests would only produce errors.
            return databaseReachable
                ? Results.Ok(new
                {
                    status = "ready",
                    database = "up",
                    snapshotVersion = current.Version,
                })
                : Results.Json(new { status = "not-ready", database = "down" },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .AllowAnonymous()
        .ExcludeFromDescription();

        return app;
    }
}
