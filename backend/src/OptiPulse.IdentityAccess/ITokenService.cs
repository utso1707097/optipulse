using OptiPulse.SharedKernel;

namespace OptiPulse.IdentityAccess;

public interface ITokenService
{
    /// <summary>Authenticates credentials and issues a token pair (FR-A01).</summary>
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>Rotates a refresh token, issuing a new pair (FR-A02). Reuse of an
    /// already-rotated token revokes the entire family and fails (research R10).</summary>
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes the token's whole family so it can no longer be used (US2 scenario 6).</summary>
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
