using System.Text.Json.Serialization;
using OptiPulse.Api.Auth;
using OptiPulse.Audit.Application;

namespace OptiPulse.Api.Endpoints;

/// <summary>
/// Analytics reads for the dashboard (T055).
///
/// Readable by BOTH roles: reviewing results is not an authoring act, and an Admin diagnosing an
/// incident needs the same numbers a Manager uses to judge an experiment.
/// </summary>
public static class TelemetryEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/telemetry")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1);

        group.MapGet("/flags/{key}/exposures", async (
            string key, IExposureAggregator aggregator, CancellationToken ct) =>
        {
            var byVariant = await aggregator.GetVariantExposureCountsAsync(key, ct);
            var total = byVariant.Sum(v => v.Exposures);

            return Results.Ok(new FlagExposureResponse(
                key,
                total,
                byVariant.Select(v => new VariantExposureDto(v.VariantKey, v.Exposures, Share(v.Exposures, total)))
                    .ToArray()));
        })
        .WithName("GetFlagExposures")
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        return app;
    }

    /// <summary>Share of total exposures, rounded to 2dp. Guards the zero case explicitly rather
    /// than letting a flag with no traffic divide by zero.</summary>
    private static double Share(long part, long total) =>
        total == 0 ? 0 : Math.Round((double)part / total * 100, 2);
}

public sealed record VariantExposureDto(string? VariantKey, long Exposures, double SharePercent);

public sealed record FlagExposureResponse(
    string FlagKey, long TotalExposures, VariantExposureDto[] ByVariant);

[JsonSerializable(typeof(FlagExposureResponse))]
internal partial class TelemetryJsonContext : JsonSerializerContext;
