namespace OptiPulse.Api;

/// <summary>
/// Converts URL-style connection strings into the key-value form the drivers expect.
///
/// Every managed platform — Render, Heroku, Railway, Fly, Neon's default copy button — hands out
/// `postgres://user:pass@host:port/db` and `redis://:pass@host:port`. Npgsql and
/// StackExchange.Redis both reject that shape, and the resulting failure is opaque: a format
/// exception at startup that says nothing about the URL being the problem. Normalising here
/// means a platform-injected connection string works as-is, which removes the single most likely
/// cause of a failed first deploy — and makes a one-click Blueprint possible at all, since the
/// platform decides the format, not us.
///
/// Anything that is not a recognised URL is passed through untouched, so an already-correct
/// key-value string keeps working.
/// </summary>
public static class ConnectionStringNormalizer
{
    public static string Postgres(string connectionString)
    {
        if (!TryParseUrl(connectionString, "postgres", "postgresql", out var uri))
            return connectionString;

        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        // Managed Postgres requires TLS, and Npgsql does not assume it. Omitting this is the
        // second most common first-deploy failure after the URL format itself.
        var sslMode = connectionString.Contains("sslmode=disable", StringComparison.OrdinalIgnoreCase)
            ? "Disable"
            : "Require";

        return $"Host={uri.Host};Port={port};Database={database};Username={user};Password={password};" +
               $"SSL Mode={sslMode};Trust Server Certificate=true";
    }

    public static string Redis(string connectionString)
    {
        if (!TryParseUrl(connectionString, "redis", "rediss", out var uri))
            return connectionString;

        var userInfo = uri.UserInfo.Split(':', 2);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var port = uri.Port > 0 ? uri.Port : 6379;

        // `rediss://` is the TLS scheme; plain `redis://` on a managed provider is still usually
        // TLS, but assuming it would break a local unencrypted instance, so only the explicit
        // scheme turns it on.
        var ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase);

        var result = $"{uri.Host}:{port}";
        if (!string.IsNullOrEmpty(password))
            result += $",password={password}";
        if (ssl)
            result += ",ssl=True";

        return result;
    }

    private static bool TryParseUrl(string value, string scheme1, string scheme2, out Uri uri)
    {
        uri = null!;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!value.StartsWith($"{scheme1}://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith($"{scheme2}://", StringComparison.OrdinalIgnoreCase))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out uri!);
    }
}
