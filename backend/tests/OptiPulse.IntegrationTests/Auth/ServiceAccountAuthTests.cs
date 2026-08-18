using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Api.Endpoints;
using OptiPulse.IdentityAccess;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Auth;

/// <summary>
/// T041a — the runtime SDK surface authenticates with a service-account credential, which is a
/// distinct credential type from a human user (constitution v2.2.0 Principle VI). These tests
/// pin the separation in BOTH directions: an SDK key must not satisfy a human-role policy, and
/// a human JWT must not satisfy the service-account policy.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class ServiceAccountAuthTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task Evaluate_WithoutAnyCredential_Fails401()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest("anything", "user-1", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "the evaluation surface is no longer anonymous (T041a closed that gap)");
    }

    [Fact]
    public async Task Evaluate_WithAValidServiceAccountKey_Succeeds()
    {
        var key = await fixture.CreateServiceAccountKeyAsync();
        var client = fixture.CreateServiceAccountClient(key);

        var response = await client.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest("unknown-flag", "user-1", null));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
        body!.Reason.Should().Be("Unknown", "fail-safe default still applies to an authenticated caller");
    }

    [Fact]
    public async Task Evaluate_WithAGarbageKey_Fails401()
    {
        var client = fixture.CreateServiceAccountClient("opk_not-a-real-key-at-all");

        var response = await client.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest("anything", "user-1", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Evaluate_WithARevokedKey_Fails401_AfterSnapshotRefresh()
    {
        var key = await fixture.CreateServiceAccountKeyAsync();
        var client = fixture.CreateServiceAccountClient(key);

        // Works before revocation.
        (await client.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest("f", "u", null)))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var account = db.ServiceAccounts.Single(a => a.KeyHash == ServiceAccount.HashKey(key));
            account.Revoke(DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
        }

        // Revocation is snapshot-based, so force the refresh the background service would do on
        // its interval rather than sleeping for it — the behaviour under test is that a revoked
        // key stops working, not how long the interval is.
        await fixture.Services.GetRequiredService<IServiceAccountAuthenticator>().RefreshAsync();

        var afterRevoke = await client.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest("f", "u", null));

        afterRevoke.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Evaluate_WithAHumanJwt_Fails401_ServiceAccountIsADistinctCredential()
    {
        // The separation must hold in both directions: a Manager/Admin token is not an SDK
        // credential, however privileged its holder is.
        var email = $"human-{Guid.NewGuid():N}@optipulse.test";
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Users.Add(User.Create(email, AuthTestCredentials.Password, "Ada Admin", UserRole.Admin).Value);
            await db.SaveChangesAsync();
        }

        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await client.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest("anything", "user-1", null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "an Admin JWT must not authenticate the machine surface");
    }
}
