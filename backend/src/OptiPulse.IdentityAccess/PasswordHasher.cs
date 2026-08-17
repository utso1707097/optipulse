using System.Security.Cryptography;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// PBKDF2 password hashing (Constitution Principle VI — auth is backend-contained;
/// no external identity provider). Format: {iterations}.{base64(salt)}.{base64(hash)}
/// so the work factor is stored per-hash and can be raised over time without
/// invalidating existing credentials.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int DefaultIterations = 210_000; // OWASP 2023 guidance for PBKDF2-HMAC-SHA512
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, Algorithm, HashSize);
        return $"{DefaultIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>Constant-time verification. Returns false (never throws) on a
    /// malformed stored hash, so a corrupt record cannot crash the login path.</summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var parts = storedHash.Split('.', 3);
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out var iterations) ||
            iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
