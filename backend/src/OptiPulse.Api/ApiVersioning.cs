using Asp.Versioning;
using Asp.Versioning.Builder;

namespace OptiPulse.Api;

/// <summary>
/// API versioning (constitution package baseline: `Asp.Versioning`, not hand-written route
/// prefixes).
///
/// Adopted at 7 endpoints rather than later by design: Principle VII requires breaking contract
/// changes to be versioned explicitly, and a version set declared centrally is what makes that
/// possible without editing every route string. Phase 5 and 6 add roughly 15 more endpoints, so
/// retrofitting this afterwards would touch a much larger surface.
///
/// The URL segment stays `/api/v{version}` so existing clients and the committed OpenAPI
/// contract are unchanged by this adoption — this is deliberately a no-op on the wire.
/// </summary>
public static class ApiVersioning
{
    /// <summary>
    /// The v1 route prefix, written as a LITERAL rather than the `v{version:apiVersion}`
    /// template. This is deliberate: with the template, the generated OpenAPI document
    /// publishes paths as `/api/v{version}/evaluate`, which forces every client to substitute
    /// the segment itself and makes the committed contract less precise than the one it
    /// replaced. A literal keeps the published contract byte-identical to what clients already
    /// consume, while the version set below still supplies the versioning metadata.
    ///
    /// Adding v2 means a new prefix constant and a group mapped to <c>V2</c> — still a single
    /// place to change, which is the property this adoption was for.
    /// </summary>
    public const string RoutePrefix = "/api/v1";

    public static readonly ApiVersion V1 = new(1, 0);

    public static IServiceCollection AddOptiPulseVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = V1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                // Report supported/deprecated versions in response headers so a client can
                // detect a pending breaking change before it is forced onto it.
                options.ReportApiVersions = true;
                // The version lives in a literal URL segment (see RoutePrefix), so no reader
                // needs to parse it; the default version applies and each group declares its
                // own via MapToApiVersion.
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            });

        return services;
    }

    /// <summary>
    /// The shared version set every endpoint group is bound to. Declared once so adding v2
    /// later is a change here plus per-endpoint <c>MapToApiVersion</c>, never a sweep of
    /// hardcoded route strings.
    /// </summary>
    public static ApiVersionSet CreateVersionSet(this IEndpointRouteBuilder app) =>
        app.NewApiVersionSet()
            .HasApiVersion(V1)
            .ReportApiVersions()
            .Build();
}
