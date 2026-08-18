using System.Net;
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
/// T082 / FR-021 — conversion ingest, and the analytics read that finally answers which variant
/// performed better. Before this, telemetry recorded who SAW each variant and nothing about
/// what they then did, so no experiment could ever be decided.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class ConversionTests(OptiPulseTestFixture fixture)
{
    private async Task<HttpClient> ManagerAsync()
    {
        var email = $"mgr-{Guid.NewGuid():N}@optipulse.test";
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Users.Add(User.Create(email, AuthTestCredentials.Password, "Mgr", UserRole.Manager).Value);
            await db.SaveChangesAsync();
        }

        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }

    [Fact]
    public async Task RecordingTheSameConversionTwice_CountsItOnce()
    {
        // The failure this prevents: a host application retries after a network timeout, the
        // conversion is counted twice, and one arm's numerator is silently inflated — which is
        // exactly how an experiment reports the wrong winner while looking perfectly healthy.
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        var manager = await ManagerAsync();
        var flagKey = $"conv-{Guid.NewGuid():N}";
        var idempotencyKey = $"order-{Guid.NewGuid():N}";

        var request = new ConversionRequest(flagKey, "purchase", idempotencyKey, "user-1", "b", null, 49.99m);

        var first = await sdk.PostAsJsonAsync("/api/v1/telemetry/conversions", request);
        var retry = await sdk.PostAsJsonAsync("/api/v1/telemetry/conversions", request);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.Content.ReadFromJsonAsync<ConversionResponse>())!.Duplicate.Should().BeFalse();

        retry.StatusCode.Should().Be(HttpStatusCode.OK, "a retry is not an error — the caller's intent is satisfied");
        (await retry.Content.ReadFromJsonAsync<ConversionResponse>())!.Duplicate.Should().BeTrue();

        var report = await manager.GetFromJsonAsync<FlagExposureResponse>(
            $"/api/v1/telemetry/flags/{flagKey}/exposures");
        report!.TotalConversions.Should().Be(1, "the retry must not have added a second conversion");
    }

    [Fact]
    public async Task WithoutAnIdempotencyKey_TheRequestIsRejected()
    {
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());

        var response = await sdk.PostAsJsonAsync("/api/v1/telemetry/conversions",
            new ConversionRequest("some-flag", "purchase", "", "user-1", "b", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Conversions_RequireAServiceAccount_NotAHumanToken()
    {
        var manager = await ManagerAsync();

        var response = await manager.PostAsJsonAsync("/api/v1/telemetry/conversions",
            new ConversionRequest("f", "purchase", Guid.NewGuid().ToString(), "u", "b", null, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "conversions are machine-reported telemetry, like evaluation");
    }

    [Fact]
    public async Task Analytics_ReportsAConversionRatePerVariant_SoAWinnerCanBeRead()
    {
        // The end-to-end payoff: evaluate to generate exposures, report conversions against one
        // variant, and read back a rate that distinguishes the arms.
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        var manager = await ManagerAsync();
        var flagKey = $"rate-{Guid.NewGuid():N}";

        await manager.PostAsJsonAsync("/api/v1/flags",
            new CreateFlagRequest(flagKey, "Rate test", false, null, null));
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        for (int i = 0; i < 10; i++)
            await sdk.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flagKey, $"user-{i}", null));

        for (int i = 0; i < 3; i++)
        {
            await sdk.PostAsJsonAsync("/api/v1/telemetry/conversions", new ConversionRequest(
                flagKey, "purchase", $"{flagKey}-order-{i}", $"user-{i}", null, null, 10m));
        }

        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        FlagExposureResponse? report = null;
        while (elapsed.Elapsed < TimeSpan.FromSeconds(15))
        {
            report = await manager.GetFromJsonAsync<FlagExposureResponse>(
                $"/api/v1/telemetry/flags/{flagKey}/exposures");
            if (report is not null && report.TotalExposures >= 10)
                break;
            await Task.Delay(100);
        }

        report!.TotalConversions.Should().Be(3);
        var arm = report.ByVariant.Single();
        arm.Exposures.Should().BeGreaterThanOrEqualTo(10);
        arm.Conversions.Should().Be(3);
        arm.ConversionRatePercent.Should().BeGreaterThan(0,
            "a rate is what an experiment is decided on, and it was previously impossible to compute");
    }
}
