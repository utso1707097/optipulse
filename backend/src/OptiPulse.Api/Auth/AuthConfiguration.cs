using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OptiPulse.IdentityAccess;

namespace OptiPulse.Api.Auth;

/// <summary>
/// JWT bearer authentication + RBAC authorization policies (Constitution
/// Principle VI: all auth decisions are server-side; clients hold no auth logic).
/// </summary>
public static class AuthConfiguration
{
    /// <summary>Policy requiring the Manager role — flag/experiment authoring,
    /// micro-copy generation and approval (spec Role Permission Matrix).</summary>
    public const string ManagerPolicy = "RequireManager";

    /// <summary>Policy requiring the Admin/DevOps role — telemetry monitoring,
    /// alerts, and kill-switch operation.</summary>
    public const string AdminPolicy = "RequireAdmin";

    /// <summary>Policy for capabilities shared by both roles (analytics and audit
    /// reads) — overlaps default to read-only per the permission matrix.</summary>
    public const string AnyRolePolicy = "RequireAnyRole";

    public static IServiceCollection AddOptiPulseAuth(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
    {
        var signingKey = ResolveSigningKey(configuration, isDevelopment);
        var issuer = configuration[$"{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}"] ?? "optipulse";
        var audience = configuration[$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] ?? "optipulse-clients";

        // Manual binding (no reflection-based Bind) to stay consistent with the
        // rest of the codebase's AOT-friendly options pattern.
        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection(JwtOptions.SectionName);
            options.SigningKey = signingKey;
            options.Issuer = section[nameof(JwtOptions.Issuer)] ?? options.Issuer;
            options.Audience = section[nameof(JwtOptions.Audience)] ?? options.Audience;
            if (int.TryParse(section[nameof(JwtOptions.AccessTokenMinutes)], out var accessMinutes))
                options.AccessTokenMinutes = accessMinutes;
            if (int.TryParse(section[nameof(JwtOptions.RefreshTokenDays)], out var refreshDays))
                options.RefreshTokenDays = refreshDays;
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ManagerPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(nameof(UserRole.Manager)));

            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(nameof(UserRole.Admin)));

            options.AddPolicy(AnyRolePolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(nameof(UserRole.Manager), nameof(UserRole.Admin)));
        });

        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<ITokenService, TokenService>();
        services.TryAddSingletonTimeProvider();

        return services;
    }

    private static void TryAddSingletonTimeProvider(this IServiceCollection services)
    {
        // TimeProvider (not DateTime.UtcNow) per the constitution's anti-pattern
        // gate — injectable so tests can control time deterministically.
        if (services.All(s => s.ServiceType != typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);
    }

    /// <summary>
    /// Resolves the JWT signing key. It is deliberately NOT committed to
    /// appsettings.json (Principle VI) — supply it in real environments via
    /// environment variable (`Jwt__SigningKey`), user-secrets, or a secret store.
    ///
    /// In Development only, an ephemeral random key is generated when none is
    /// configured, so `dotnet run` works out of the box without anyone being
    /// tempted to commit one. Consequence: tokens do not survive a restart, and
    /// multiple instances cannot validate each other's tokens — acceptable for
    /// local dev, never for anything shared. Outside Development a missing key is
    /// a hard startup failure rather than a silent insecure default.
    /// </summary>
    private static string ResolveSigningKey(IConfiguration configuration, bool isDevelopment)
    {
        var configured = configuration[$"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)}"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        if (!isDevelopment)
        {
            throw new InvalidOperationException(
                $"Configuration '{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)}' is required. " +
                "Supply it via the Jwt__SigningKey environment variable or a secret store — " +
                "never commit it (Constitution Principle VI).");
        }

        var ephemeral = Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        Console.WriteLine(
            "[WARN] No Jwt:SigningKey configured — generated an EPHEMERAL development key. " +
            "Tokens will be invalidated on restart. Set Jwt__SigningKey for anything beyond local dev.");
        return ephemeral;
    }
}
