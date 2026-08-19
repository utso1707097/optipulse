using System.Diagnostics;
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
/// T063 — a kill switch engaged by an Admin must reach evaluation fast enough to be an incident
/// control rather than a configuration change (SC-005).
///
/// <para>The measurement is deliberately of PROPAGATION, not of the HTTP round trip. What the
/// budget is really about is the gap between "the switch was thrown" and "no evaluation returns
/// the feature any more" — the window in which the thing being killed is still serving. That is
/// measured from the moment the write commits, by polling the evaluation surface until it
/// changes.</para>
///
/// <para>Timing assertions are the classic source of CI flake, so the bound here is the SC-005
/// budget applied to the invalidation path with headroom, and the test reports the observed
/// figure on failure rather than only that it was too slow.</para>
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class KillSwitchPropagationTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task EngagingAKillSwitch_StopsEvaluationsServingTheFlag_Promptly()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = $"killprop-{Guid.NewGuid():N}";

        await manager.PostAsJsonAsync("/api/v1/flags",
            new CreateFlagRequest(flagKey, "Kill propagation", DefaultOutcome: false, null, null));
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        await WaitUntilServedAsync(sdk, flagKey);

        var admin = await ClientAsync(UserRole.Admin);

        var stopwatch = Stopwatch.StartNew();
        var engaged = await admin.PostAsJsonAsync(
            $"/api/v1/flags/{flagKey}/kill-switch", new KillSwitchRequest(true));
        engaged.EnsureSuccessStatusCode();

        var elapsed = await PollUntilKilledAsync(sdk, flagKey, stopwatch);

        elapsed.Should().BeLessThan(
            TimeSpan.FromSeconds(2),
            "a kill switch is an incident control; if it took seconds to take effect an operator "
            + "could not rely on it to stop a bad release");
    }

    [Fact]
    public async Task ReleasingAKillSwitch_RestoresEvaluation()
    {
        // The recovery direction matters as much as the kill: an admin who cannot un-kill is
        // stuck waiting for a deploy to restore a feature they disabled by mistake.
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = $"killprop-rel-{Guid.NewGuid():N}";

        await manager.PostAsJsonAsync("/api/v1/flags",
            new CreateFlagRequest(flagKey, "Kill release", DefaultOutcome: false, null, null));
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        await WaitUntilServedAsync(sdk, flagKey);

        var admin = await ClientAsync(UserRole.Admin);
        await admin.PostAsJsonAsync($"/api/v1/flags/{flagKey}/kill-switch", new KillSwitchRequest(true));
        await PollUntilKilledAsync(sdk, flagKey, Stopwatch.StartNew());

        await admin.PostAsJsonAsync($"/api/v1/flags/{flagKey}/kill-switch", new KillSwitchRequest(false));

        var restored = await PollAsync(sdk, flagKey, r => r != "KillSwitch", TimeSpan.FromSeconds(5));
        restored.Should().NotBe("KillSwitch", "releasing must restore normal evaluation");
    }

    private static async Task<TimeSpan> PollUntilKilledAsync(
        HttpClient sdk, string flagKey, Stopwatch stopwatch)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var reason = await EvaluateReasonAsync(sdk, flagKey);
            if (reason == "KillSwitch") return stopwatch.Elapsed;
            await Task.Delay(10);
        }

        throw new TimeoutException(
            $"'{flagKey}' was still being evaluated normally {stopwatch.Elapsed.TotalSeconds:F1}s "
            + "after the kill switch was engaged.");
    }

    private static async Task WaitUntilServedAsync(HttpClient sdk, string flagKey) =>
        await PollAsync(sdk, flagKey, reason => reason is not null and not "FlagNotFound",
            TimeSpan.FromSeconds(10));

    private static async Task<string?> PollAsync(
        HttpClient sdk, string flagKey, Func<string?, bool> predicate, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        string? last = null;
        while (DateTime.UtcNow < deadline)
        {
            last = await EvaluateReasonAsync(sdk, flagKey);
            if (predicate(last)) return last;
            await Task.Delay(20);
        }

        throw new TimeoutException($"'{flagKey}' never reached the expected state; last was '{last}'.");
    }

    private static async Task<string?> EvaluateReasonAsync(HttpClient sdk, string flagKey)
    {
        var response = await sdk.PostAsJsonAsync(
            "/api/v1/evaluate", new EvaluateRequest(flagKey, "user-1", null));
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
        return body?.Reason;
    }

    private async Task<HttpClient> ClientAsync(UserRole role)
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
