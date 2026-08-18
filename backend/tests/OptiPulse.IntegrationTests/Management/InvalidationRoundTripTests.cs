using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Api.Endpoints;
using OptiPulse.IdentityAccess;
using OptiPulse.IntegrationTests.Auth;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Management;

/// <summary>
/// The management API and the evaluation engine live in separate bounded contexts and each owns
/// its own copy of the invalidation wire schema. That duplication is deliberate (contexts must
/// not reference each other), but it means a field rename on one side would silently stop
/// invalidation working — the publisher would keep publishing and the subscriber would keep
/// discarding, with every test still green.
///
/// This test is what makes that impossible: it drives the REAL path end to end — create a flag
/// through the API, activate it, and assert the evaluation surface starts serving it — so a
/// drift between the two shapes fails here.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class InvalidationRoundTripTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task ActivatingAFlagThroughTheApi_MakesItEvaluable_ViaTheRealInvalidationPathway()
    {
        var manager = await ManagerClientAsync();
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        var key = $"roundtrip-{Guid.NewGuid():N}";

        // Before it exists, evaluation must fail safe rather than error.
        var before = await sdk.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(key, "user-1", null));
        (await before.Content.ReadFromJsonAsync<EvaluateResponse>())!.Reason.Should().Be("Unknown");

        var created = await manager.PostAsJsonAsync(
            "/api/v1/flags", new CreateFlagRequest(key, "Round trip", DefaultOutcome: true, null, null));
        created.EnsureSuccessStatusCode();

        await manager.PostAsJsonAsync($"/api/v1/flags/{key}/status", new ChangeStatusRequest("Active"));

        // Invalidation is asynchronous (publish → Redis → subscriber → snapshot swap), so poll
        // rather than assume it has landed. Stopwatch, not wall-clock arithmetic.
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        EvaluateResponse? body = null;
        while (elapsed.Elapsed < TimeSpan.FromSeconds(10))
        {
            var response = await sdk.PostAsJsonAsync(
                "/api/v1/evaluate", new EvaluateRequest(key, "user-1", null));
            body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
            if (body is not null && body.Reason != "Unknown")
                break;
            await Task.Delay(50);
        }

        body!.Reason.Should().NotBe("Unknown",
            "the flag was created and activated through the management API, so the publisher's "
            + "message must have been understood by the evaluation subscriber");
        body.Outcome.Should().BeTrue("the flag's default outcome is true");
    }

    [Fact]
    public async Task EngagingTheKillSwitch_TurnsTheFlagOff_OnTheEvaluationSurface()
    {
        // SC-002's operator promise, exercised through the real HTTP surface rather than by
        // poking the snapshot directly.
        var manager = await ManagerClientAsync();
        var admin = await AdminClientAsync();
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        var key = $"killswitch-{Guid.NewGuid():N}";

        await manager.PostAsJsonAsync(
            "/api/v1/flags", new CreateFlagRequest(key, "Kill me", DefaultOutcome: true, null, null));
        await manager.PostAsJsonAsync($"/api/v1/flags/{key}/status", new ChangeStatusRequest("Active"));
        await WaitUntilAsync(sdk, key, r => r.Reason != "Unknown");

        await admin.PostAsJsonAsync($"/api/v1/flags/{key}/kill-switch", new KillSwitchRequest(true));

        var killed = await WaitUntilAsync(sdk, key, r => r.Outcome == false);
        killed.Outcome.Should().BeFalse("an engaged kill-switch overrides the flag's own default");
    }

    private async Task<EvaluateResponse> WaitUntilAsync(
        HttpClient sdk, string key, Func<EvaluateResponse, bool> predicate)
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        EvaluateResponse? last = null;
        while (elapsed.Elapsed < TimeSpan.FromSeconds(10))
        {
            var response = await sdk.PostAsJsonAsync(
                "/api/v1/evaluate", new EvaluateRequest(key, "user-1", null));
            last = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
            if (last is not null && predicate(last))
                return last;
            await Task.Delay(50);
        }

        throw new TimeoutException($"Flag '{key}' never reached the expected state; last was {last?.Reason}.");
    }

    private Task<HttpClient> ManagerClientAsync() => AuthenticatedClientAsync(UserRole.Manager);

    private Task<HttpClient> AdminClientAsync() => AuthenticatedClientAsync(UserRole.Admin);

    private async Task<HttpClient> AuthenticatedClientAsync(UserRole role)
    {
        var email = $"{role}-{Guid.NewGuid():N}@optipulse.test";
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Users.Add(User.Create(email, AuthTestCredentials.Password, $"Test {role}", role).Value);
            await db.SaveChangesAsync();
        }

        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }
}
