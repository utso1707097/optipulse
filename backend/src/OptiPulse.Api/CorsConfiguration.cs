namespace OptiPulse.Api;

/// <summary>
/// Cross-origin access for the React dashboard, which is served from a different origin than the
/// API (FR-031, constitution v2.3.0).
///
/// The allowlist is explicit and comes from configuration. A wildcard origin is REFUSED rather
/// than merely discouraged: this API carries bearer credentials and an Admin-only kill-switch, so
/// `AllowAnyOrigin` would let any page on the internet drive privileged operations with a
/// victim's token. Refusing at startup means the mistake surfaces on the first boot after the
/// change, not during an incident.
/// </summary>
public static class CorsConfiguration
{
    public const string PolicyName = "OptiPulseDashboard";
    private const string ConfigKey = "Cors:AllowedOrigins";

    public static IServiceCollection AddOptiPulseCors(
        this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection(ConfigKey).Get<string[]>() ?? [];

        foreach (var origin in origins)
        {
            if (origin is "*")
            {
                throw new InvalidOperationException(
                    $"'{ConfigKey}' contains '*'. A wildcard origin is prohibited (constitution " +
                    "v2.3.0): this API carries bearer credentials and an Admin kill-switch. List " +
                    "the dashboard's exact origins instead.");
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException(
                    $"'{ConfigKey}' contains '{origin}', which is not an absolute URI. Use the " +
                    "full scheme and host, e.g. https://optipulse.vercel.app");
            }
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
        {
            if (origins.Length == 0)
            {
                // No origins configured: allow nothing. Same-origin callers are unaffected —
                // CORS only governs cross-origin requests — so a single-origin deployment needs
                // no configuration and gets no accidental exposure.
                return;
            }

            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                // Credentials are sent as an Authorization header, not a cookie, so
                // AllowCredentials is unnecessary — and omitting it keeps the browser from ever
                // attaching ambient cookies to a cross-origin call.
                .WithExposedHeaders("Location");
        }));

        return services;
    }
}
