using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OptiPulse.Api.Auth;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Endpoints;

/// <summary>
/// Authoring & control surface (contracts/management-api.md, T051).
///
/// POLICY SPLIT, and it is not arbitrary: authoring is Manager, the kill-switch is Admin.
/// A Manager who could also kill-switch would make the separation US2 tests cosmetic, and an
/// Admin who could author flags would blur "operate" into "change what ships". This mirrors the
/// spec's Role Permission Matrix exactly.
/// </summary>
public static class ManagementEndpoints
{
    public static IEndpointRouteBuilder MapManagementEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/flags")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            // Tags group the generated clients. Without them every operation lands in a
            // single god-class (OptiPulseApiApi) in the Dart client, which the mobile app
            // then has to import wholesale to call one endpoint.
            .WithTags("Flags");

        group.MapGet("/", async (FlagManagementService service, CancellationToken ct) =>
        {
            var flags = await service.ListAsync(ct);
            return Results.Ok(flags.Select(ToResponse).ToArray());
        })
        .WithName("ListFlags")
        .Produces<FlagResponse[]>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        group.MapGet("/{key}", async (string key, FlagManagementService service, CancellationToken ct) =>
        {
            var flag = await service.GetAsync(key, ct);
            return flag is null ? NotFound(key) : Results.Ok(ToResponse(flag));
        })
        .WithName("GetFlag")
        .Produces<FlagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        group.MapPost("/", async (
            CreateFlagRequest request,
            ClaimsPrincipal principal,
            FlagManagementService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateAsync(
                principal.ToActor(), request.Key, request.Name, request.DefaultOutcome,
                ToRules(request.TargetingRules), ToRollout(request.Rollout), ct);

            return result.IsFailure
                ? Problem(result.Error)
                : Results.Created($"/api/v1/flags/{result.Value.Key}", ToResponse(result.Value));
        })
        .WithName("CreateFlag")
        .Produces<FlagResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        group.MapPut("/{key}", async (
            string key,
            UpdateFlagRequest request,
            HttpRequest httpRequest,
            ClaimsPrincipal principal,
            FlagManagementService service,
            CancellationToken ct) =>
        {
            // If-Match carries the version the caller edited. Required, not optional: without it
            // a client that never read the current state could overwrite a change it never saw,
            // which is precisely the lost update FR-011 forbids.
            if (!TryReadIfMatch(httpRequest, out var expectedVersion))
            {
                return Results.Problem(
                    title: "If-Match required",
                    detail: "Provide the flag's current version in an If-Match header.",
                    statusCode: StatusCodes.Status428PreconditionRequired);
            }

            var result = await service.UpdateAsync(
                principal.ToActor(), key, expectedVersion, request.Name, request.DefaultOutcome,
                ToRules(request.TargetingRules) ?? [], ToRollout(request.Rollout), ct);

            return result.IsFailure ? Problem(result.Error) : Results.Ok(ToResponse(result.Value));
        })
        .WithName("UpdateFlag")
        .Produces<FlagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status428PreconditionRequired)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        group.MapPost("/{key}/status", async (
            string key,
            ChangeStatusRequest request,
            ClaimsPrincipal principal,
            FlagManagementService service,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<FlagStatus>(request.Status, ignoreCase: true, out var target))
                return Results.Problem(
                    title: "Invalid status",
                    detail: $"'{request.Status}' is not a known flag status.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);

            var result = await service.ChangeStatusAsync(principal.ToActor(), key, target, ct);
            return result.IsFailure ? Problem(result.Error) : Results.Ok(ToResponse(result.Value));
        })
        .WithName("ChangeFlagStatus")
        .Produces<FlagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        group.MapPost("/{key}/kill-switch", async (
            string key,
            KillSwitchRequest request,
            ClaimsPrincipal principal,
            FlagManagementService service,
            CancellationToken ct) =>
        {
            var result = await service.SetKillSwitchAsync(principal.ToActor(), key, request.Engaged, ct);
            return result.IsFailure ? Problem(result.Error) : Results.Ok(ToResponse(result.Value));
        })
        .WithName("SetKillSwitch")
        .Produces<FlagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthConfiguration.AdminPolicy);

        return app;
    }

    private static bool TryReadIfMatch(HttpRequest request, out long version)
    {
        version = 0;
        var raw = request.Headers.IfMatch.ToString().Trim('"', ' ');
        return !string.IsNullOrEmpty(raw) && long.TryParse(raw, out version);
    }

    private static IResult NotFound(string key) => Results.Problem(
        title: "Flag not found", detail: $"Flag '{key}' was not found.",
        statusCode: StatusCodes.Status404NotFound);

    /// <summary>Maps domain error types onto the status codes the contract promises, so a
    /// concurrency conflict reads as 409 rather than a generic failure.</summary>
    private static IResult Problem(Error error) => Results.Problem(
        title: error.Code,
        detail: error.Message,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest,
        });

    private static List<TargetingRule>? ToRules(TargetingRuleDto[]? rules) =>
        rules?.Select(r => new TargetingRule(
            r.Attribute,
            Enum.Parse<TargetingOperator>(r.Operator, ignoreCase: true),
            r.Values,
            r.Outcome)).ToList();

    private static Rollout? ToRollout(RolloutDto? rollout) =>
        rollout is null ? null : Rollout.FromPercentage(rollout.Percentage, rollout.Salt);

    private static FlagResponse ToResponse(Flag flag) => new(
        flag.Id, flag.Key, flag.Name, flag.DefaultOutcome, flag.Status.ToString(),
        flag.KillSwitchEngaged, flag.Version,
        flag.TargetingRules.Select(r => new TargetingRuleDto(
            r.Attribute, r.Operator.ToString(), r.Values.ToArray(), r.Outcome)).ToArray(),
        flag.Rollout is null ? null : new RolloutDto(flag.Rollout.PercentageBasisPoints / 100, flag.Rollout.Salt),
        flag.CreatedAt, flag.UpdatedAt);
}

public sealed record TargetingRuleDto(string Attribute, string Operator, string[] Values, bool Outcome);

public sealed record RolloutDto(int Percentage, string Salt);

public sealed record CreateFlagRequest(
    string Key, string Name, bool DefaultOutcome, TargetingRuleDto[]? TargetingRules, RolloutDto? Rollout);

public sealed record UpdateFlagRequest(
    string Name, bool DefaultOutcome, TargetingRuleDto[]? TargetingRules, RolloutDto? Rollout);

public sealed record ChangeStatusRequest(string Status);

public sealed record KillSwitchRequest(bool Engaged);

public sealed record FlagResponse(
    Guid Id, string Key, string Name, bool DefaultOutcome, string Status, bool KillSwitchEngaged,
    long Version, TargetingRuleDto[] TargetingRules, RolloutDto? Rollout,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

[JsonSerializable(typeof(CreateFlagRequest))]
[JsonSerializable(typeof(UpdateFlagRequest))]
[JsonSerializable(typeof(ChangeStatusRequest))]
[JsonSerializable(typeof(KillSwitchRequest))]
[JsonSerializable(typeof(FlagResponse))]
[JsonSerializable(typeof(FlagResponse[]))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class ManagementJsonContext : JsonSerializerContext;
