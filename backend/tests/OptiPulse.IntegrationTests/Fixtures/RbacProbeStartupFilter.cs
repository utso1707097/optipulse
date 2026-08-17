using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OptiPulse.Api.Auth;

namespace OptiPulse.IntegrationTests.Fixtures;

/// <summary>
/// Registers RBAC probe endpoints bound to the production policy constants
/// (T033). Implemented as an IStartupFilter so it composes with the real
/// application pipeline rather than replacing it — the app's own authentication
/// and authorization middleware and all real routes stay intact.
/// </summary>
internal sealed class RbacProbeStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        app.UseEndpoints(endpoints =>
        {
            var probe = endpoints.MapGroup("/api/v1/_rbac-probe");

            probe.MapGet("/manager-only", () => Results.Ok(new ProbeResult(true)))
                .RequireAuthorization(AuthConfiguration.ManagerPolicy);

            probe.MapGet("/admin-only", () => Results.Ok(new ProbeResult(true)))
                .RequireAuthorization(AuthConfiguration.AdminPolicy);

            probe.MapGet("/any-role", () => Results.Ok(new ProbeResult(true)))
                .RequireAuthorization(AuthConfiguration.AnyRolePolicy);
        });
    };

    private sealed record ProbeResult(bool Ok);
}
