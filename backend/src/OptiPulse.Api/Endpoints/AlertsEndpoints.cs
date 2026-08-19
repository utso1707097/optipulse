using System.Security.Claims;
using System.Text.Json.Serialization;
using OptiPulse.Api.Auth;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;

namespace OptiPulse.Api.Endpoints;

/// <summary>
/// Alert history, acknowledgement and device registration (T070, FR-026).
///
/// <para>Admin-only throughout. Alerts describe operational conditions and carry flag keys and
/// actor names; acknowledging one is an operational act that other Admins will read as "someone
/// is on this". Neither belongs to the Manager role, whose job is authoring experiments.</para>
/// </summary>
public static class AlertsEndpoints
{
    public static IEndpointRouteBuilder MapAlertsEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/alerts")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            .WithTags("Alerts")
            .RequireAuthorization(AuthConfiguration.AdminPolicy);

        group.MapGet("/", async (
            IAlertStore store,
            bool? unacknowledgedOnly,
            int? limit,
            CancellationToken ct) =>
        {
            var alerts = await store.ListAsync(unacknowledgedOnly ?? false, limit ?? 50, ct);
            return Results.Ok(alerts.Select(ToResponse).ToList());
        })
        .WithName("ListAlerts")
        .Produces<List<AlertResponse>>(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/ack", async (
            Guid id,
            ClaimsPrincipal principal,
            IAlertStore store,
            CancellationToken ct) =>
        {
            var alert = await store.AcknowledgeAsync(id, principal.ToActor().DisplayName, ct);
            return alert is null ? Results.NotFound() : Results.Ok(ToResponse(alert));
        })
        .WithName("AcknowledgeAlert")
        .Produces<AlertResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/devices", async (
            RegisterDeviceRequest request,
            ClaimsPrincipal principal,
            IAlertStore store,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return Results.Problem(title: "token is required.", statusCode: 422);

            if (!Enum.TryParse<DevicePlatform>(request.Platform, ignoreCase: true, out var platform))
                return Results.Problem(
                    title: $"platform must be one of: {string.Join(", ", Enum.GetNames<DevicePlatform>())}.",
                    statusCode: 422);

            var actor = principal.ToActor();
            var device = await store.RegisterDeviceAsync(actor.ActorId, platform, request.Token, ct);

            // The token is deliberately NOT echoed back. It is already known to the caller, and
            // a response body is the easiest place for a credential-shaped value to end up in a
            // log or a crash report.
            return Results.Ok(new RegisterDeviceResponse(device.Id, device.Platform.ToString(), device.RegisteredAt));
        })
        .WithName("RegisterPushDevice")
        .Produces<RegisterDeviceResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // POST rather than DELETE, for two reasons. Minimal APIs refuse to infer a body on
        // DELETE at all — mapping it throws at startup, which took the whole host down the first
        // time this was written — and a DELETE body is poorly supported by proxies and HTTP
        // clients regardless. The token does not belong in the URL either: it would land in
        // access logs and browser history.
        group.MapPost("/devices/revoke", async (
            RegisterDeviceRequest request, IAlertStore store, CancellationToken ct) =>
        {
            await store.RevokeDeviceAsync(request.Token, ct);
            // 204 whether or not the token was known. Reporting "no such device" would let a
            // caller probe which tokens are registered, and there is nothing to fix either way.
            return Results.NoContent();
        })
        .WithName("RevokePushDevice")
        .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static AlertResponse ToResponse(Alert alert) => new(
        alert.Id,
        alert.RaisedAt,
        alert.Kind.ToString(),
        alert.Severity.ToString(),
        alert.FlagKey,
        alert.Title,
        alert.Detail,
        alert.AcknowledgedAt,
        alert.AcknowledgedBy);
}

public sealed record AlertResponse(
    Guid Id,
    DateTimeOffset RaisedAt,
    string Kind,
    string Severity,
    string? FlagKey,
    string Title,
    string Detail,
    DateTimeOffset? AcknowledgedAt,
    string? AcknowledgedBy);

public sealed record RegisterDeviceRequest(string Platform, string Token);

public sealed record RegisterDeviceResponse(Guid Id, string Platform, DateTimeOffset RegisteredAt);

[JsonSerializable(typeof(List<AlertResponse>))]
[JsonSerializable(typeof(AlertResponse))]
[JsonSerializable(typeof(RegisterDeviceRequest))]
[JsonSerializable(typeof(RegisterDeviceResponse))]
internal sealed partial class AlertsJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
