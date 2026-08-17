using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                // Provider-correct DDL generated from the model — the committed
                // migration set is SQLite-authored (see BootstrapAsync's KNOWN
                // LIMITATION note). Still a REAL Postgres container, so the
                // constitution's "never in-memory" testing rule holds.
                new("Database:SchemaStrategy", "EnsureCreated"),
                new("ConnectionStrings:Flags", PostgresConnectionString),
                new("ConnectionStrings:Audit", PostgresConnectionString),
                new("ConnectionStrings:Identity", PostgresConnectionString),
                new("Redis:ConnectionString", RedisConnectionString),
            ]);
        });
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
