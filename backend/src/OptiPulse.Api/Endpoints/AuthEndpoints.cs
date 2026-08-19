using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OptiPulse.Api.Auth;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.IdentityAccess;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet versionSet)
    {
        var group = app.MapGroup($"{ApiVersioning.RoutePrefix}/auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersioning.V1)
            // Tags group the generated clients. Without them every operation lands in a
            // single god-class (OptiPulseApiApi) in the Dart client, which the mobile app
            // then has to import wholesale to call one endpoint.
            .WithTags("Authentication");

        group.MapPost("/login", async (
            LoginRequest request,
            ITokenService tokenService,
            IAuditLog auditLog,
            CancellationToken cancellationToken) =>
        {
            var result = await tokenService.LoginAsync(request.Email, request.Password, cancellationToken);

            if (result.IsFailure)
            {
                // Audit the failure without recording the attempted credential
                // (FR-A07). Actor is unknown, so attribution is by email only.
                await auditLog.AppendAsync(
                    ActorReference.System, AuditChangeType.LoginFailed, Guid.Empty,
                    afterStateJson: $"{{\"email\":\"{request.Email}\"}}",
                    cancellationToken: cancellationToken);

                return Results.Problem(
                    title: "Invalid credentials",
                    detail: "Invalid email or password.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var tokens = result.Value;
            await auditLog.AppendAsync(
                ActorReference.System, AuditChangeType.LoginSucceeded, Guid.Empty,
                afterStateJson: $"{{\"email\":\"{request.Email}\",\"role\":\"{tokens.Role}\"}}",
                cancellationToken: cancellationToken);

            return Results.Ok(ToResponse(tokens));
        })
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapPost("/refresh", async (
            RefreshRequest request,
            ITokenService tokenService,
            CancellationToken cancellationToken) =>
        {
            var result = await tokenService.RefreshAsync(request.RefreshToken, cancellationToken);

            return result.IsFailure
                ? Results.Problem(
                    title: "Refresh failed",
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized)
                : Results.Ok(ToResponse(result.Value));
        })
        .WithName("Refresh")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapPost("/logout", async (
            RefreshRequest request,
            ITokenService tokenService,
            CancellationToken cancellationToken) =>
        {
            await tokenService.LogoutAsync(request.RefreshToken, cancellationToken);
            return Results.NoContent();
        })
        .WithName("Logout")
        .Produces(StatusCodes.Status204NoContent)
        .AllowAnonymous(); // the refresh token itself is the credential here

        group.MapGet("/me", (ClaimsPrincipal principal) =>
        {
            // The JWT bearer handler's inbound claim mapping rewrites several
            // short JWT claim names to the long WS-Federation URIs (e.g. `sub` ->
            // NameIdentifier, `email` -> ClaimTypes.Email), but not all of them.
            // Read the mapped form first and fall back to the raw JWT name so
            // this works whether or not MapInboundClaims is enabled.
            static string Claim(ClaimsPrincipal p, string mapped, string raw) =>
                p.FindFirstValue(mapped) ?? p.FindFirstValue(raw) ?? string.Empty;

            return Results.Ok(new MeResponse(
                Claim(principal, ClaimTypes.NameIdentifier, "sub"),
                Claim(principal, ClaimTypes.Name, "name"),
                Claim(principal, ClaimTypes.Role, "role"),
                Claim(principal, ClaimTypes.Email, "email")));
        })
        .WithName("Me")
        .Produces<MeResponse>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthConfiguration.AnyRolePolicy);

        return app;
    }

    private static LoginResponse ToResponse(AuthTokens tokens) => new(
        tokens.AccessToken, tokens.RefreshToken, tokens.TokenType, tokens.ExpiresInSeconds, tokens.Role.ToString());
}

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LoginResponse(
    string AccessToken, string RefreshToken, string TokenType, int ExpiresInSeconds, string Role);

public sealed record MeResponse(string UserId, string Name, string Role, string Email);

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(RefreshRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(MeResponse))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AuthJsonContext : JsonSerializerContext;
