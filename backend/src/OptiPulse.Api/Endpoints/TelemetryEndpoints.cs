using System.Text.Json.Serialization;
using OptiPulse.Api.Auth;
using OptiPulse.Audit.Application;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Endpoints;

/// <summary>
/// Telemetry: conversion ingest (T082) and the analytics read (T055).
/// </summary>
public static class TelemetryEndpoints
{
    public static IEndpointRouteBuilder MapTelemetryEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/telemetry")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            // Tags group the generated clients. Without them every operation lands in a
            // single god-class (OptiPulseApiApi) in the Dart client, which the mobile app
            // then has to import wholesale to call one endpoint.
            .WithTags("Telemetry");

        // Reported by the host application, so it authenticates with a SERVICE-ACCOUNT
        // credential like /evaluate — a conversion is machine-reported telemetry, not a human
        // management action (Principle VI).
        group.MapPost("/conversions", async (
            ConversionRequest request,
            IConversionRecorder recorder,
            CancellationToken ct) =>
        {
            var result = await recorder.RecordAsync(
                request.FlagKey, request.Goal, request.IdempotencyKey, request.ContextKey,
                request.VariantKey, request.ExperimentId, request.Value, ct);

            if (result.IsFailure)
            {
                return Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            // A duplicate returns 200, not 409. The caller asked us to record that a conversion
            // happened, and it is recorded — reporting failure would drive host applications
            // into retry loops that can never succeed. `duplicate` tells an interested caller
            // what happened without making it an error.
            return Results.Ok(new ConversionResponse(
                Recorded: result.Value.Recorded, Duplicate: result.Value.Duplicate));
        })
        .WithName("RecordConversion")
        .Produces<ConversionResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthConfiguration.ServiceAccountPolicy);

        group.MapGet("/flags/{key}/exposures", async (
            string key, IExposureAggregator aggregator, CancellationToken ct) =>
        {
            var exposures = await aggregator.GetVariantExposureCountsAsync(key, ct);
            var conversions = await aggregator.GetVariantConversionCountsAsync(key, ct);
            var totalExposures = exposures.Sum(v => v.Exposures);

            var byVariant = exposures.Select(e =>
            {
                var converted = conversions.FirstOrDefault(c => c.VariantKey == e.VariantKey)?.Conversions ?? 0;
                return new VariantExposureDto(
                    e.VariantKey,
                    e.Exposures,
                    Share(e.Exposures, totalExposures),
                    converted,
                    // Conversion RATE, the number an experiment is actually decided on. Both
                    // the numerator and denominator are returned alongside it: a rate on its own
                    // cannot distinguish 3 conversions from 3,000, and those deserve very
                    // different confidence.
                    Rate(converted, e.Exposures));
            }).ToArray();

            return Results.Ok(new FlagExposureResponse(
                key, totalExposures, conversions.Sum(c => c.Conversions), byVariant));
        })
        .WithName("GetFlagExposures")
        .Produces<FlagExposureResponse>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        // T071 — live operational signals for the mobile app.
        //
        // Reads the in-memory snapshot rather than the database: this is what evaluation is
        // ACTUALLY serving right now, which during a Postgres blip is exactly the number an
        // operator needs and exactly the one a database query cannot give them.
        group.MapGet("/live", (
            OptiPulse.Evaluation.Application.ISnapshotStore snapshotStore,
            TimeProvider timeProvider) =>
        {
            var snapshot = snapshotStore.Current;
            var killed = snapshot.Flags.Count(f => f.KillSwitchEngaged);

            return Results.Ok(new LiveTelemetryResponse(
                SnapshotVersion: snapshot.Version,
                SnapshotBuiltAt: snapshot.BuiltAt,
                SnapshotAgeSeconds: snapshot.Version == 0
                    ? null
                    : (long)(timeProvider.GetUtcNow() - snapshot.BuiltAt).TotalSeconds,
                ActiveFlags: snapshot.Count,
                KillSwitchesEngaged: killed,
                ServerTime: timeProvider.GetUtcNow()));
        })
        .WithName("GetLiveTelemetry")
        .Produces<LiveTelemetryResponse>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthConfiguration.AdminPolicy);

        return app;
    }

    private static double Share(long part, long total) =>
        total == 0 ? 0 : Math.Round((double)part / total * 100, 2);

    /// <summary>Zero exposures yields 0, not a division by zero — a variant nobody saw has no
    /// rate, and reporting one would invent a result from no data.</summary>
    private static double Rate(long conversions, long exposures) =>
        exposures == 0 ? 0 : Math.Round((double)conversions / exposures * 100, 2);
}

public sealed record ConversionRequest(
    string FlagKey,
    string Goal,
    string IdempotencyKey,
    string? ContextKey,
    string? VariantKey,
    Guid? ExperimentId,
    decimal? Value);

public sealed record ConversionResponse(bool Recorded, bool Duplicate);

public sealed record VariantExposureDto(
    string? VariantKey, long Exposures, double SharePercent, long Conversions, double ConversionRatePercent);

public sealed record FlagExposureResponse(
    string FlagKey, long TotalExposures, long TotalConversions, VariantExposureDto[] ByVariant);

[JsonSerializable(typeof(ConversionRequest))]
[JsonSerializable(typeof(ConversionResponse))]
[JsonSerializable(typeof(FlagExposureResponse))]
internal partial class TelemetryJsonContext : JsonSerializerContext;

/// <param name="SnapshotAgeSeconds">
/// Null when nothing has been published yet. How stale the served flag set is — the single most
/// useful number during an invalidation outage, because a snapshot that stopped updating still
/// serves perfectly valid-looking answers from old rules.
/// </param>
public sealed record LiveTelemetryResponse(
    long SnapshotVersion,
    DateTimeOffset SnapshotBuiltAt,
    long? SnapshotAgeSeconds,
    int ActiveFlags,
    int KillSwitchesEngaged,
    DateTimeOffset ServerTime);
