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

/// <summary>T047/T051 — experiment authoring over HTTP, plus the analytics read (T055).</summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class ExperimentApiTests(OptiPulseTestFixture fixture)
{
    private async Task<HttpClient> ClientAsync(UserRole role)
    {
        var email = $"{role}-{Guid.NewGuid():N}@optipulse.test";
        using (var scope = fixture.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Users.Add(User.Create(email, AuthTestCredentials.Password, $"T {role}", role).Value);
            await db.SaveChangesAsync();
        }

        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }

    private async Task<string> CreateFlagAsync(HttpClient manager)
    {
        var key = $"exp-flag-{Guid.NewGuid():N}";
        var created = await manager.PostAsJsonAsync(
            "/api/v1/flags", new CreateFlagRequest(key, "Experiment host", false, null, null));
        created.EnsureSuccessStatusCode();
        return key;
    }

    [Fact]
    public async Task CreateExperiment_WithValidSplit_Returns201()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = await CreateFlagAsync(manager);

        var response = await manager.PostAsJsonAsync("/api/v1/experiments", new CreateExperimentRequest(
            flagKey, "CTA test", [new VariantDto("control", 50), new VariantDto("b", 50)], "purchase"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var experiment = (await response.Content.ReadFromJsonAsync<ExperimentResponse>())!;
        experiment.Status.Should().Be("Draft");
        experiment.Variants.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateExperiment_WhenWeightsDoNotSumTo100_Returns422()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = await CreateFlagAsync(manager);

        var response = await manager.PostAsJsonAsync("/api/v1/experiments", new CreateExperimentRequest(
            flagKey, "Bad split", [new VariantDto("a", 50), new VariantDto("b", 40)], null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateExperiment_ForAnUnknownFlag_Returns404()
    {
        var manager = await ClientAsync(UserRole.Manager);

        var response = await manager.PostAsJsonAsync("/api/v1/experiments", new CreateExperimentRequest(
            $"missing-{Guid.NewGuid():N}", "Orphan", [new VariantDto("a", 50), new VariantDto("b", 50)], null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "an experiment on a flag that does not exist could never be evaluated");
    }

    [Fact]
    public async Task CreateExperiment_AsAdmin_Returns403_AuthoringIsManagerOnly()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var admin = await ClientAsync(UserRole.Admin);
        var flagKey = await CreateFlagAsync(manager);

        var response = await admin.PostAsJsonAsync("/api/v1/experiments", new CreateExperimentRequest(
            flagKey, "Admin attempt", [new VariantDto("a", 50), new VariantDto("b", 50)], null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateExperiment_RequiresIfMatch_AndBumpsVersion()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = await CreateFlagAsync(manager);
        var created = await manager.PostAsJsonAsync("/api/v1/experiments", new CreateExperimentRequest(
            flagKey, "Reweight", [new VariantDto("a", 50), new VariantDto("b", 50)], null));
        var experiment = (await created.Content.ReadFromJsonAsync<ExperimentResponse>())!;

        var without = await manager.PutAsJsonAsync(
            $"/api/v1/experiments/{experiment.Id}",
            new UpdateExperimentRequest([new VariantDto("a", 70), new VariantDto("b", 30)]));
        without.StatusCode.Should().Be(HttpStatusCode.PreconditionRequired);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/experiments/{experiment.Id}")
        {
            Content = JsonContent.Create(
                new UpdateExperimentRequest([new VariantDto("a", 70), new VariantDto("b", 30)])),
        };
        request.Headers.TryAddWithoutValidation("If-Match", experiment.Version.ToString());
        var withMatch = await manager.SendAsync(request);

        withMatch.EnsureSuccessStatusCode();
        var updated = (await withMatch.Content.ReadFromJsonAsync<ExperimentResponse>())!;
        updated.Version.Should().Be(experiment.Version + 1);
        updated.Variants.Single(v => v.Key == "a").Weight.Should().Be(70);
    }

    [Fact]
    public async Task Analytics_ReportsExposuresPerVariant_AfterEvaluations()
    {
        // T055 end to end: evaluations recorded by the SDK surface must show up in the
        // analytics read the dashboard uses.
        var manager = await ClientAsync(UserRole.Manager);
        var sdk = fixture.CreateServiceAccountClient(await fixture.CreateServiceAccountKeyAsync());
        var flagKey = await CreateFlagAsync(manager);
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        for (int i = 0; i < 5; i++)
        {
            await sdk.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flagKey, $"user-{i}", null));
        }

        // Exposure persistence is a batched background drain, so poll rather than assume.
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        FlagExposureResponse? report = null;
        while (elapsed.Elapsed < TimeSpan.FromSeconds(15))
        {
            report = await manager.GetFromJsonAsync<FlagExposureResponse>(
                $"/api/v1/telemetry/flags/{flagKey}/exposures");
            if (report is not null && report.TotalExposures >= 5)
                break;
            await Task.Delay(100);
        }

        report!.TotalExposures.Should().BeGreaterThanOrEqualTo(5);
        report.ByVariant.Sum(v => v.SharePercent).Should().BeApproximately(100, 0.1);
    }
}
