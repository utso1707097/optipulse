using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.IdentityAccess;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace OptiPulse.IntegrationTests.Fixtures;

/// <summary>
/// Shared Testcontainers fixture (Postgres + Redis) for integration tests
/// (tasks.md T016). Spins up real containers once per test collection rather
/// than per test — constitution testing baseline: integration tests use real
/// dependencies, never an in-memory database.
/// </summary>
public sealed class OptiPulseTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("optipulse")
        .WithUsername("optipulse")
        .WithPassword("optipulse")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        // Probe endpoints bound to the REAL RBAC policy constants, so the policies
        // themselves are verified now (T033) even though the Manager-only
        // management and Admin-only kill-switch endpoints are Phase 5/6 work.
        // An IStartupFilter COMPOSES with the app's real pipeline (builder.Configure
        // would replace it wholesale, losing auth middleware and every real route).
        builder.ConfigureTestServices(services =>
            services.AddSingleton<IStartupFilter, RbacProbeStartupFilter>());

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(
            [
                new("Database:Provider", "Postgres"),
                // "Migrate", not "EnsureCreated" (T093). The suite previously generated
                // DDL from the model because the committed migrations were SQLite-authored
                // and could not apply to Postgres — which meant the production schema path
                // was never exercised, and its breakage (`42804`) stayed invisible while
                // every test passed. Applying the real migrations against the real Postgres
                // container makes every CI run a regression test for them, so a future
                // model change that lacks a migration fails here instead of at deploy.
                new("Database:SchemaStrategy", "Migrate"),
                new("ConnectionStrings:Flags", PostgresConnectionString),
                new("ConnectionStrings:Audit", PostgresConnectionString),
                new("ConnectionStrings:Identity", PostgresConnectionString),
                new("Redis:ConnectionString", RedisConnectionString),
            ]);
        });
    }

    /// <summary>
    /// Creates an active service account and returns its plaintext key (returned once, never
    /// stored). Refreshes the authenticator snapshot immediately so the key is usable now
    /// rather than after the background refresh interval.
    /// </summary>
    public async Task<string> CreateServiceAccountKeyAsync(string? name = null)
    {
        string plaintextKey;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var created = ServiceAccount.Create(name ?? $"test-sdk-{Guid.NewGuid():N}", DateTimeOffset.UtcNow).Value;
            db.ServiceAccounts.Add(created.Account);
            await db.SaveChangesAsync();
            plaintextKey = created.PlaintextKey;
        }

        await Services.GetRequiredService<IServiceAccountAuthenticator>().RefreshAsync();
        return plaintextKey;
    }

    /// <summary>An HttpClient presenting a service-account key on every request.</summary>
    public HttpClient CreateServiceAccountClient(string plaintextKey)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-OptiPulse-Key", plaintextKey);
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }
}

/// <summary>xUnit collection so all integration tests share one set of containers.</summary>
[CollectionDefinition(Name)]
public sealed class OptiPulseTestCollection : ICollectionFixture<OptiPulseTestFixture>
{
    public const string Name = "OptiPulse integration tests";
}
