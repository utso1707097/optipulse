namespace OptiPulse.IdentityAccess;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Signing key — MUST be supplied via configuration/secret store and
    /// never committed or shipped to clients (Principle VI). Validated at startup.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "optipulse";
    public string Audience { get; set; } = "optipulse-clients";

    /// <summary>Short-lived access token (~15 min per research R10).</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>Longer-lived rotating refresh token (~7 days per research R10).</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
