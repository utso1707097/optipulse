using System.Security.Cryptography;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// Revocable, server-side refresh token (data-model.md, research R10). Only the
/// HASH is persisted — the raw token exists solely in the response to the client,
/// so a database leak cannot be replayed as a valid credential.
///
/// Rotation: each refresh issues a new token in the same FamilyId and revokes the
/// presented one. Presenting an ALREADY-rotated token means it leaked, so the
/// whole family is revoked (reuse detection).
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public Guid FamilyId { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>EF Core materialization constructor.</summary>
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(Guid id, Guid userId, string tokenHash, Guid familyId, DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        ExpiresAt = expiresAt;
    }

    /// <summary>Issues a new token. Returns the entity plus the RAW token value,
    /// which is returned to the client once and never persisted.</summary>
    public static (RefreshToken Token, string RawValue) Issue(
        Guid userId, DateTimeOffset now, TimeSpan lifetime, Guid? familyId = null)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var token = new RefreshToken(
            Guid.NewGuid(),
            userId,
            ComputeHash(raw),
            familyId ?? Guid.NewGuid(),
            now.Add(lifetime));
        return (token, raw);
    }

    public static string ComputeHash(string rawValue) =>
        Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawValue)));

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
