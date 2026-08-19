using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OptiPulse.Api.Auth;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Endpoints;

/// <summary>
/// A/B/n experiment authoring (contracts/management-api.md). Manager-only, like flag authoring:
/// designing an experiment is an authoring act, not an operational one.
/// </summary>
public static class ExperimentEndpoints
{
    public static IEndpointRouteBuilder MapExperimentEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/experiments")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            // Tags group the generated clients. Without them every operation lands in a
            // single god-class (OptiPulseApiApi) in the Dart client, which the mobile app
            // then has to import wholesale to call one endpoint.
            .WithTags("Experiments");

        group.MapGet("/", async (string? flagKey, ExperimentService service, CancellationToken ct) =>
        {
            var experiments = await service.ListAsync(flagKey, ct);
            return Results.Ok(experiments.Select(ToResponse).ToArray());
        })
        .WithName("ListExperiments")
        .Produces<ExperimentResponse[]>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        group.MapGet("/{id:guid}", async (Guid id, ExperimentService service, CancellationToken ct) =>
        {
            var experiment = await service.GetAsync(id, ct);
            return experiment is null
                ? Results.Problem(title: "Experiment not found", statusCode: StatusCodes.Status404NotFound)
                : Results.Ok(ToResponse(experiment));
        })
        .WithName("GetExperiment")
        .Produces<ExperimentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        group.MapPost("/", async (
            CreateExperimentRequest request,
            ClaimsPrincipal principal,
            ExperimentService service,
            CancellationToken ct) =>
        {
            var variants = ToVariants(request.Variants);
            if (variants.IsFailure)
                return Problem(variants.Error);

            var result = await service.CreateAsync(
                principal.ToActor(), request.FlagKey, request.Name, variants.Value, request.ConversionGoal, ct);

            return result.IsFailure
                ? Problem(result.Error)
                : Results.Created($"/api/v1/experiments/{result.Value.Id}", ToResponse(result.Value));
        })
        .WithName("CreateExperiment")
        .Produces<ExperimentResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateExperimentRequest request,
            HttpRequest httpRequest,
            ClaimsPrincipal principal,
            ExperimentService service,
            CancellationToken ct) =>
        {
            var raw = httpRequest.Headers.IfMatch.ToString().Trim('"', ' ');
            if (!long.TryParse(raw, out var expectedVersion))
                return Results.Problem(
                    title: "If-Match required",
                    detail: "Provide the experiment's current version in an If-Match header.",
                    statusCode: StatusCodes.Status428PreconditionRequired);

            var variants = ToVariants(request.Variants);
            if (variants.IsFailure)
                return Problem(variants.Error);

            var result = await service.UpdateVariantsAsync(
                principal.ToActor(), id, expectedVersion, variants.Value, ct);

            return result.IsFailure ? Problem(result.Error) : Results.Ok(ToResponse(result.Value));
        })
        .WithName("UpdateExperiment")
        .Produces<ExperimentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status428PreconditionRequired)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        group.MapPost("/{id:guid}/status", async (
            Guid id,
            ChangeStatusRequest request,
            ClaimsPrincipal principal,
            ExperimentService service,
            CancellationToken ct) =>
        {
            if (!Enum.TryParse<ExperimentStatus>(request.Status, ignoreCase: true, out var target))
                return Results.Problem(
                    title: "Invalid status",
                    detail: $"'{request.Status}' is not a known experiment status.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);

            var result = await service.ChangeStatusAsync(principal.ToActor(), id, target, ct);
            return result.IsFailure ? Problem(result.Error) : Results.Ok(ToResponse(result.Value));
        })
        .WithName("ChangeExperimentStatus")
        .Produces<ExperimentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization(AuthConfiguration.ManagerPolicy);

        return app;
    }

    private static Result<IReadOnlyList<Variant>> ToVariants(VariantDto[] dtos)
    {
        var variants = new List<Variant>(dtos.Length);
        foreach (var dto in dtos)
        {
            var variant = Variant.FromPercentage(dto.Key, dto.Weight);
            if (variant.IsFailure)
                return variant.Error;
            variants.Add(variant.Value);
        }

        return variants;
    }

    private static IResult Problem(Error error) => Results.Problem(
        title: error.Code,
        detail: error.Message,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest,
        });

    private static ExperimentResponse ToResponse(Experiment e) => new(
        e.Id, e.FlagKey, e.Name, e.Status.ToString(), e.ConversionGoal, e.Version,
        e.Variants.Select(v => new VariantDto(v.Key, v.WeightBasisPoints / 100)).ToArray(),
        e.CreatedAt, e.UpdatedAt);
}

public sealed record VariantDto(string Key, int Weight);

public sealed record CreateExperimentRequest(
    string FlagKey, string Name, VariantDto[] Variants, string? ConversionGoal);

public sealed record UpdateExperimentRequest(VariantDto[] Variants);

public sealed record ExperimentResponse(
    Guid Id, string FlagKey, string Name, string Status, string? ConversionGoal, long Version,
    VariantDto[] Variants, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

[JsonSerializable(typeof(CreateExperimentRequest))]
[JsonSerializable(typeof(UpdateExperimentRequest))]
[JsonSerializable(typeof(ExperimentResponse))]
[JsonSerializable(typeof(ExperimentResponse[]))]
internal partial class ExperimentJsonContext : JsonSerializerContext;
