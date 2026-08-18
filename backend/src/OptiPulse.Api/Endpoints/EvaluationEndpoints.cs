using System.Text.Json.Serialization;
using OptiPulse.Api.Auth;
using OptiPulse.Audit.Application;
using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Api.Endpoints;

public static class EvaluationEndpoints
{
    public static IEndpointRouteBuilder MapEvaluationEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        // The runtime SDK surface: authenticated by a SERVICE-ACCOUNT credential, never by a
        // human Manager/Admin role (constitution v2.2.0 Principle VI). T041a — these endpoints
        // were anonymous until this policy existed, because no credential type fitted a machine
        // caller and binding them to a human role would have been wrong rather than safer.
        var group = app.MapGroup(ApiVersioning.RoutePrefix)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            .RequireAuthorization(AuthConfiguration.ServiceAccountPolicy);

        group.MapPost("/evaluate", (
            EvaluateRequest request,
            IEvaluator evaluator,
            IExposureRecorder exposureRecorder) =>
        {
            var context = new EvaluationContext(request.FlagKey, request.ContextKey, request.Attributes);
            var result = evaluator.Evaluate(context);

            // MVP scope: no Experiments/Variants exist yet (Phase 5), so every
            // evaluation is recorded as a flag-level exposure with a null
            // variant/experiment — FR-020's full "exposure within an experiment"
            // semantics apply once those aggregates exist.
            exposureRecorder.Record(request.FlagKey, result.VariantKey, request.ContextKey, result.SnapshotVersion);

            return Results.Ok(ToResponse(request.FlagKey, result));
        })
        .WithName("Evaluate");

        group.MapPost("/evaluate/batch", (
            BatchEvaluateRequest request,
            IEvaluator evaluator,
            IExposureRecorder exposureRecorder) =>
        {
            var results = new EvaluateResponse[request.FlagKeys.Length];
            long snapshotVersion = 0;

            for (int i = 0; i < request.FlagKeys.Length; i++)
            {
                var flagKey = request.FlagKeys[i];
                var context = new EvaluationContext(flagKey, request.ContextKey, request.Attributes);
                var result = evaluator.Evaluate(context);
                exposureRecorder.Record(flagKey, result.VariantKey, request.ContextKey, result.SnapshotVersion);
                results[i] = ToResponse(flagKey, result);
                snapshotVersion = result.SnapshotVersion;
            }

            return Results.Ok(new BatchEvaluateResponse(results, snapshotVersion));
        })
        .WithName("EvaluateBatch");

        group.MapGet("/snapshot/version", (ISnapshotStore snapshotStore) =>
        {
            var snapshot = snapshotStore.Current;
            return Results.Ok(new SnapshotVersionResponse(snapshot.Version, snapshot.BuiltAt));
        })
        .WithName("GetSnapshotVersion");

        // T041 / T041a — REQUIRED CREDENTIAL for this group: a service-account key
        // (X-OptiPulse-Key), enforced by AuthConfiguration.ServiceAccountPolicy above.
        //
        // NOT the Manager/Admin RBAC policies, deliberately: this is the runtime SDK surface
        // consumed by client applications, and an SDK holds neither role. The spec's Role
        // Permission Matrix covers human capabilities only; FR-A05's "all protected actions"
        // refers to the management, kill-switch and AI-approval surfaces, which are
        // role-bound separately.
        //
        // The policy is bound to the ServiceAccount authentication scheme, so a human JWT
        // cannot satisfy it either — the separation runs in both directions.

        return app;
    }

    private static EvaluateResponse ToResponse(string flagKey, EvaluationResult result) => new(
        flagKey, result.Outcome, result.VariantKey, result.Reason.ToString(), result.SnapshotVersion);
}

public sealed record EvaluateRequest(string FlagKey, string? ContextKey, Dictionary<string, string>? Attributes);

public sealed record EvaluateResponse(string FlagKey, bool Outcome, string? VariantKey, string Reason, long SnapshotVersion);

public sealed record BatchEvaluateRequest(string? ContextKey, Dictionary<string, string>? Attributes, string[] FlagKeys);

public sealed record BatchEvaluateResponse(EvaluateResponse[] Results, long SnapshotVersion);

public sealed record SnapshotVersionResponse(long Version, DateTimeOffset BuiltAt);

[JsonSerializable(typeof(EvaluateRequest))]
[JsonSerializable(typeof(EvaluateResponse))]
[JsonSerializable(typeof(BatchEvaluateRequest))]
[JsonSerializable(typeof(BatchEvaluateResponse))]
[JsonSerializable(typeof(SnapshotVersionResponse))]
internal partial class EvaluationJsonContext : JsonSerializerContext;
