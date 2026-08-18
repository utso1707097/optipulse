using System.Security.Cryptography;
using System.Text;
using OptiPulse.SharedKernel;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// A machine caller on the runtime evaluation surface — an SDK embedded in a consuming
/// application, not a person (constitution v2.2.0 Principle VI).
///
/// Deliberately NOT a <see cref="User"/> with a role: a service account holds neither
/// Manager nor Admin, and granting it one to make authorization "work" would hand an SDK key
/// the ability to author flags or trip the kill-switch. Its scope is evaluation and telemetry
/// ingest, nothing else.
///
/// KEY HASHING — this differs from <see cref="PasswordHasher"/> on purpose. Passwords use
/// PBKDF2 with 210,000 iterations because human-chosen passwords are low-entropy and must be
/// made expensive to guess. An API key here is 256 bits of CSPRNG output, so there is nothing
/// to brute-force: a single SHA-256 is sufficient, and the collision/preimage resistance is
/// what protects a leaked database. Using PBKDF2 instead would cost ~100ms of CPU on EVERY
/// evaluation call and destroy the sub-5ms budget (Principle II) while buying no security.
/// </summary>
public sealed class ServiceAccount
{
    private const string KeyPrefix = "opk_";

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    // EF materialization constructor.
    private ServiceAccount() { }

    /// <summary>
    /// Creates an account and returns the plaintext key ONCE. The key is never stored and
    /// cannot be recovered — only its hash is persisted, so a database leak yields nothing
    /// replayable. Losing it means issuing a new account.
    /// </summary>
    public static Result<(ServiceAccount Account, string PlaintextKey)> Create(
        string name, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<(ServiceAccount, string)>.Failure(
                Error.Validation("ServiceAccount.NameRequired", "Service account name is required."));

        var secret = RandomNumberGenerator.GetBytes(32);
        var plaintextKey = KeyPrefix + Base64UrlEncode(secret);

        var account = new ServiceAccount
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            KeyHash = HashKey(plaintextKey),
            CreatedAt = now,
        };

        return Result<(ServiceAccount, string)>.Success((account, plaintextKey));
    }

    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;

    /// <summary>
    /// SHA-256 of the presented key. Deterministic (unlike a salted password hash) precisely so
    /// authentication can be a dictionary lookup on the hash rather than a per-row verify —
    /// that is what keeps the evaluation path from touching the database at all.
    /// </summary>
    public static string HashKey(string plaintextKey) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey)));

    /// <summary>Cheap shape check so obviously-malformed input never reaches a lookup.</summary>
    public static bool LooksLikeKey(string? value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(KeyPrefix, StringComparison.Ordinal);

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
