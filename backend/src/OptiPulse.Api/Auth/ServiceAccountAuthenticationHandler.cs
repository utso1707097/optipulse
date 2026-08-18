using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OptiPulse.IdentityAccess;

namespace OptiPulse.Api.Auth;

/// <summary>
/// Authenticates machine callers on the evaluation surface from an <c>X-OptiPulse-Key</c>
/// header (constitution v2.2.0 Principle VI: service accounts are a credential type distinct
/// from human users).
///
/// A separate scheme rather than an extra JWT claim, because the two credentials have genuinely
/// different lifecycles and blast radii: a human's JWT is short-lived, refreshable and carries a
/// role; an SDK key is long-lived, revocable, and must never satisfy a role-based policy.
/// Keeping them as separate schemes makes "an SDK key can authorize a kill-switch" not merely
/// disallowed but unrepresentable.
///
/// Validation is an in-memory lookup (see ServiceAccountAuthenticator), so this adds no
/// database round-trip to the hot path.
/// </summary>
public sealed class ServiceAccountAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceAccountAuthenticator authenticator)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ServiceAccount";
    public const string HeaderName = "X-OptiPulse-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var values))
            return Task.FromResult(AuthenticateResult.NoResult());

        var presented = values.ToString();

        if (!authenticator.TryAuthenticate(presented, out var accountId, out var name))
        {
            // Deliberately uniform: an unknown key and a revoked key are indistinguishable to
            // the caller, so the response cannot be used to enumerate valid keys.
            return Task.FromResult(AuthenticateResult.Fail("Invalid service-account key."));
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, name),
            // No ClaimTypes.Role is issued, by design — see the class remarks.
            new Claim(OptiPulseClaims.CallerType, OptiPulseClaims.ServiceAccountCallerType),
        ], SchemeName);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public static class OptiPulseClaims
{
    public const string CallerType = "optipulse:caller_type";
    public const string ServiceAccountCallerType = "service_account";
}
