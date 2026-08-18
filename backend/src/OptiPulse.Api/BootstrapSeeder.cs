using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OptiPulse.IdentityAccess;

namespace OptiPulse.Api;

/// <summary>
/// Establishes the first accounts in an empty environment (FR-032 – FR-034).
///
/// Without this, a freshly provisioned deployment is unusable: there is no registration endpoint,
/// so the only way in would be direct database access — which is exactly what an operator
/// deploying to managed hosting does not have.
///
/// Three rules, all enforced below rather than documented and hoped for:
///
/// 1. NO DEFAULT CREDENTIAL. Credentials come from configuration. Outside Development the seeder
///    REFUSES to run when configuration is absent rather than inventing something — a public
///    demonstration deployment is a public deployment, and a predictable account sitting behind
///    an Admin kill-switch is the same exposure in a demo as in production.
/// 2. IDEMPOTENT. It seeds only into an empty table and never modifies an existing account. A
///    restart is not an authorization event, and a redeploy must not silently reset a password
///    someone has since changed.
/// 3. SECRETS ARE NEVER PERSISTED IN PLAINTEXT. The password is hashed by the same PBKDF2 path
///    as any other user; the generated service-account key is shown once in the startup log and
///    then only its hash is stored.
/// </summary>
public static class BootstrapSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var logger = app.Logger;
        var config = app.Configuration;
        var isDevelopment = app.Environment.IsDevelopment();

        if (await db.Users.AnyAsync())
        {
            // Not an error and not worth a warning: the environment is already established.
            logger.LogInformation("Bootstrap skipped — users already exist.");
            return;
        }

        var manager = ReadAccount(config, "Manager", isDevelopment, logger);
        var admin = ReadAccount(config, "Admin", isDevelopment, logger);

        if (manager is null || admin is null)
        {
            logger.LogWarning(
                "Bootstrap REFUSED: no Bootstrap:* credentials configured and this is not "
                + "Development. The environment has no accounts and cannot be signed into. Set "
                + "Bootstrap__Manager__Email / Bootstrap__Manager__Password (and the Admin pair) "
                + "and restart.");
            return;
        }

        var managerUser = User.Create(manager.Value.Email, manager.Value.Password, "Bootstrap Manager", UserRole.Manager);
        var adminUser = User.Create(admin.Value.Email, admin.Value.Password, "Bootstrap Admin", UserRole.Admin);

        if (managerUser.IsFailure || adminUser.IsFailure)
        {
            logger.LogError(
                "Bootstrap failed validation: {Error}",
                managerUser.IsFailure ? managerUser.Error.Message : adminUser.Error.Message);
            return;
        }

        db.Users.Add(managerUser.Value);
        db.Users.Add(adminUser.Value);

        // A service-account key for the runtime SDK surface, so /evaluate is usable immediately
        // rather than returning 401 to everything with no way to mint a credential.
        var serviceAccount = ServiceAccount.Create(
            config["Bootstrap:ServiceAccountName"] ?? "bootstrap-sdk", timeProvider.GetUtcNow());
        db.ServiceAccounts.Add(serviceAccount.Value.Account);

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Bootstrap complete — Manager {ManagerEmail}, Admin {AdminEmail}.",
            manager.Value.Email, admin.Value.Email);

        // Shown ONCE. The plaintext key is not stored anywhere, so this line is the only chance
        // to capture it; losing it means issuing a new service account, not recovering this one.
        logger.LogWarning(
            "Bootstrap service-account key (shown once, not recoverable): {Key}",
            serviceAccount.Value.PlaintextKey);
    }

    private static (string Email, string Password)? ReadAccount(
        IConfiguration config, string role, bool isDevelopment, ILogger logger)
    {
        var email = config[$"Bootstrap:{role}:Email"];
        var password = config[$"Bootstrap:{role}:Password"];

        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            return (email, password);

        if (!isDevelopment)
            return null;

        // Development convenience only. The password is RANDOM, never a fixed default, so a
        // developer machine can never accidentally become a deployment with known credentials.
        var generated = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)) + "aA1!";
        var devEmail = email ?? $"{role.ToLowerInvariant()}@optipulse.local";
        logger.LogWarning(
            "[DEV ONLY] Generated a random {Role} password for {Email}: {Password} — "
            + "regenerated on every fresh database. Set Bootstrap__{Role}__Password to pin it.",
            role, devEmail, generated, role);

        return (devEmail, generated);
    }
}
