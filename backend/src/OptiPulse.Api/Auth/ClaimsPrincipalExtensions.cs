using System.Security.Claims;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Projects the authenticated principal into the cross-context attribution reference used by
    /// the audit trail (FR-A06: every management action attributed to the acting user AND role).
    ///
    /// Falls back to the raw JWT claim names because the bearer handler rewrites some short
    /// names to WS-Federation URIs and not others — the same mismatch that made /auth/me return
    /// an empty email during Phase 4.
    /// </summary>
    public static ActorReference ToActor(this ClaimsPrincipal principal)
    {
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        var roleValue = principal.FindFirstValue(ClaimTypes.Role) ?? principal.FindFirstValue("role");
        var name = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name") ?? "unknown";

        var actorId = Guid.TryParse(idValue, out var parsed) ? parsed : Guid.Empty;
        var role = Enum.TryParse<ActorRole>(roleValue, ignoreCase: true, out var parsedRole)
            ? parsedRole
            : ActorRole.Service;

        return new ActorReference(actorId, role, name);
    }
}
