namespace OptiPulse.IdentityAccess;

/// <summary>Token pair returned by login/refresh (contracts/auth-api.md).
/// Both values are OPAQUE to clients — they must not parse the access token for
/// authorization decisions (Principle VI).</summary>
public sealed record AuthTokens(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserRole Role)
{
    public string TokenType => "Bearer";
}
