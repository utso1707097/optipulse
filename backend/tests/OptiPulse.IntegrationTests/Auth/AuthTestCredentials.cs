using System.Security.Cryptography;

namespace OptiPulse.IntegrationTests.Auth;

/// <summary>
/// Test credential generated at runtime rather than written as a literal in the
/// repository. Keeps the codebase free of hardcoded credential literals (the
/// repo's block-secrets guard enforces this, and Principle VI is the reason),
/// while still giving tests a stable value for the duration of the run.
/// </summary>
internal static class AuthTestCredentials
{
    public static readonly string Password =
        $"Test-{Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))}-1aA!";
}
