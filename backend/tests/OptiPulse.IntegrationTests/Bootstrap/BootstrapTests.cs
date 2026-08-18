using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.IdentityAccess;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Bootstrap;

/// <summary>
/// T099 / FR-032–FR-034 — first-run bootstrap.
///
/// Each test drives a host against its OWN database so "empty environment" is literally true;
/// the shared collection fixture already has users, which is precisely the state that must NOT
/// be re-seeded.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class BootstrapTests(OptiPulseTestFixture fixture)
{
    private const string Password = "Bootstrap-Test-Pa55!";

    static BootstrapTests()
    {
        // Set as an ENVIRONMENT VARIABLE, not through ConfigureAppConfiguration, and the reason
        // is worth recording: AddOptiPulseAuth reads builder.Configuration EAGERLY, before
        // builder.Build(), whereas WebApplicationFactory applies its configuration callbacks
        // during Build(). So an in-memory override for this key is invisible to the code that
        // consumes it — the same eager-vs-lazy trap that once desynced the database provider
        // from its connection string. Connection strings work here only because they are read
        // inside the AddDbContext lambda, which runs after Build.
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey", "bootstrap-tests-signing-key-long-enough-for-hmac-sha256!!");
    }

    /// <summary>A host pointed at a fresh database, so bootstrap sees a genuinely empty world.</summary>
    private WebApplicationFactory<Program> HostWith(
        string databaseName, bool development, params (string Key, string Value)[] settings)
    {
        return fixture.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(development ? "Development" : "Production");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var values = new List<KeyValuePair<string, string?>>
                {
                    new("Database:Provider", "Postgres"),
                    new("Database:SchemaStrategy", "EnsureCreated"),
                    new("ConnectionStrings:Flags", NewDb(databaseName)),
                    new("ConnectionStrings:Audit", NewDb(databaseName)),
                    new("ConnectionStrings:Identity", NewDb(databaseName)),
                    new("Redis:ConnectionString", fixture.RedisConnectionString),
                };
                foreach (var (key, value) in settings)
                    values.Add(new(key, value));

                config.AddInMemoryCollection(values);
            });
        });

        string NewDb(string name) =>
            System.Text.RegularExpressions.Regex.Replace(
                fixture.PostgresConnectionString, @"Database=[^;]+", $"Database={name}");
    }

    private static async Task<List<User>> UsersAsync(WebApplicationFactory<Program> host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        return await db.Users.ToListAsync();
    }

    [Fact]
    public async Task IntoAnEmptyEnvironment_SeedsAManagerAnAdminAndAServiceAccount()
    {
        var dbName = $"bootstrap_seed_{Guid.NewGuid():N}";
        using var host = HostWith(dbName, development: false,
            ("Bootstrap:Manager:Email", "mgr@example.test"),
            ("Bootstrap:Manager:Password", Password),
            ("Bootstrap:Admin:Email", "adm@example.test"),
            ("Bootstrap:Admin:Password", Password));

        host.CreateClient();

        var users = await UsersAsync(host);
        users.Select(u => u.Role).Should().BeEquivalentTo([UserRole.Manager, UserRole.Admin]);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        (await db.ServiceAccounts.CountAsync()).Should().Be(1,
            "the SDK surface is unusable without a credential, and there is no endpoint to mint one");
    }

    [Fact]
    public async Task OutsideDevelopment_WithNoConfiguredCredentials_RefusesToSeed()
    {
        // FR-033. The dangerous failure is not "no accounts" — it is a predictable account behind
        // an Admin kill-switch on a public deployment. Refusing is the correct outcome.
        var dbName = $"bootstrap_refuse_{Guid.NewGuid():N}";
        using var host = HostWith(dbName, development: false);

        host.CreateClient();

        (await UsersAsync(host)).Should().BeEmpty(
            "no credentials were configured and this is not Development, so nothing may be invented");
    }

    [Fact]
    public async Task RunTwice_IsIdempotent_AndDoesNotAlterTheExistingAccounts()
    {
        // FR-034: a restart or redeploy must not reset a password someone has since changed.
        var dbName = $"bootstrap_idem_{Guid.NewGuid():N}";
        (string, string)[] settings =
        [
            ("Bootstrap:Manager:Email", "mgr@example.test"),
            ("Bootstrap:Manager:Password", Password),
            ("Bootstrap:Admin:Email", "adm@example.test"),
            ("Bootstrap:Admin:Password", Password),
        ];

        string firstHash;
        using (var first = HostWith(dbName, development: false, settings))
        {
            first.CreateClient();
            firstHash = (await UsersAsync(first)).Single(u => u.Role == UserRole.Manager).PasswordHash;
        }

        using var second = HostWith(dbName, development: false,
            [.. settings.Select(s => (s.Item1, s.Item2 == Password ? "A-Completely-Different-Pa55!" : s.Item2))]);
        second.CreateClient();

        var users = await UsersAsync(second);
        users.Should().HaveCount(2, "a second start must not add duplicate accounts");
        users.Single(u => u.Role == UserRole.Manager).PasswordHash.Should().Be(firstHash,
            "the existing account must be left exactly as it was, even though configuration changed");
    }

    [Fact]
    public async Task InDevelopment_WithNoConfiguration_SeedsWithAGeneratedPassword_NotAFixedDefault()
    {
        var dbName = $"bootstrap_dev_{Guid.NewGuid():N}";
        using var first = HostWith(dbName, development: true);
        first.CreateClient();
        var firstHash = (await UsersAsync(first)).First().PasswordHash;

        var otherDb = $"bootstrap_dev2_{Guid.NewGuid():N}";
        using var second = HostWith(otherDb, development: true);
        second.CreateClient();
        var secondHash = (await UsersAsync(second)).First().PasswordHash;

        firstHash.Should().NotBe(secondHash,
            "each fresh Development database must get its own random password — a fixed default "
            + "is what turns a forgotten dev config into a known production credential");
    }
}
