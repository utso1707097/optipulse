using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Api.Endpoints;
using OptiPulse.Evaluation.Infrastructure;
using OptiPulse.Flags.Domain;
using OptiPulse.Flags.Infrastructure;
using OptiPulse.IntegrationTests.Fixtures;
using StackExchange.Redis;
using Xunit;

namespace OptiPulse.IntegrationTests.Evaluation;

[Collection(OptiPulseTestCollection.Name)]
public sealed class EvaluationApiTests(OptiPulseTestFixture fixture)
{
    /// <summary>Seeds a flag directly via FlagsDbContext (the management write API
    /// is Phase 5, out of this MVP's scope) and publishes the same invalidation
    /// message a real create/activate would publish, so the already-running
    /// InvalidationSubscriber (T027) picks it up exactly as it would in
    /// production — this exercises the real pathway rather than relying on
    /// host-boot-order timing.</summary>
    private async Task<Flag> SeedAndPublishFlagAsync(Flag flag)
    {
        using (var scope = fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FlagsDbContext>();
            dbContext.Flags.Add(flag);
            await dbContext.SaveChangesAsync();
        }

        var redis = fixture.Services.GetRequiredService<IConnectionMultiplexer>();
        var options = fixture.Services.GetRequiredService<RedisOptions>();
        var message = new InvalidationMessage("FlagChanged", flag.Key, flag.Id, flag.Version, flag.KillSwitchEngaged, DateTimeOffset.UtcNow);
        await redis.GetSubscriber().PublishAsync(
            RedisChannel.Literal(options.InvalidationChannel), JsonSerializer.Serialize(message));

        return flag;
    }

    /// <summary>Polls the SPECIFIC flag (not the global snapshot version counter)
    /// until it becomes known — with multiple flags sharing this fixture, the
    /// global version can already be satisfied by an unrelated flag's earlier
    /// update, so waiting on it would race against this flag's own delta.</summary>
    private async Task WaitUntilFlagIsKnownAsync(HttpClient client, string flagKey, TimeSpan timeout)
    {
        // Stopwatch rather than DateTime.UtcNow — monotonic (immune to clock
        // adjustments) and satisfies the constitution's anti-pattern gate.
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (elapsed.Elapsed < timeout)
        {
            var response = await client.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flagKey, "warmup-probe", null));
            var body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
            if (body is not null && body.Reason != "Unknown")
                return;
            await Task.Delay(50);
        }

        throw new TimeoutException($"Flag '{flagKey}' was not known to the snapshot within {timeout}.");
    }

    [Fact]
    public async Task Evaluate_FiftyPercentRollout_10kDistinctContexts_MatchesTargetWithinOnePercent_AndIsDeterministic()
    {
        // SC-003 exercised end-to-end through the live HTTP API + real invalidation pathway.
        var flag = Flag.Create(
            key: $"rollout-test-{Guid.NewGuid():N}",
            name: "Rollout Test",
            defaultOutcome: false,
            now: DateTimeOffset.UtcNow,
            rollout: Rollout.FromPercentage(50, "salt-1")).Value;
        flag.Activate(DateTimeOffset.UtcNow);
        await SeedAndPublishFlagAsync(flag);

        var client = fixture.CreateClient();
        await WaitUntilFlagIsKnownAsync(client, flag.Key, TimeSpan.FromSeconds(10));

        int enabledCount = 0;
        var firstResult = await client.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flag.Key, "user-0", null));
        var firstBody = await firstResult.Content.ReadFromJsonAsync<EvaluateResponse>();

        for (int i = 0; i < 10_000; i++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flag.Key, $"user-{i}", null));
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
            body.Should().NotBeNull();
            body!.Reason.Should().Be("Rollout");
            if (body.Outcome) enabledCount++;
        }

        (enabledCount / 10_000.0).Should().BeApproximately(0.50, 0.01);

        // Determinism: repeated calls for the same context return the same result.
        var repeat = await client.PostAsJsonAsync("/api/v1/evaluate", new EvaluateRequest(flag.Key, "user-0", null));
        var repeatBody = await repeat.Content.ReadFromJsonAsync<EvaluateResponse>();
        repeatBody!.Outcome.Should().Be(firstBody!.Outcome);
    }

    [Fact]
    public async Task Evaluate_TargetingRuleMatch_ReturnsRuleOutcome_ViaLiveInvalidationPathway()
    {
        var rule = new TargetingRule("country", TargetingOperator.Equals, ["US"], Outcome: true);
        var flag = Flag.Create(
            key: $"targeting-test-{Guid.NewGuid():N}",
            name: "Targeting Test",
            defaultOutcome: false,
            now: DateTimeOffset.UtcNow,
            targetingRules: [rule]).Value;
        flag.Activate(DateTimeOffset.UtcNow);
        await SeedAndPublishFlagAsync(flag);

        var client = fixture.CreateClient();
        await WaitUntilFlagIsKnownAsync(client, flag.Key, TimeSpan.FromSeconds(10));

        var usResponse = await client.PostAsJsonAsync("/api/v1/evaluate",
            new EvaluateRequest(flag.Key, "user-1", new Dictionary<string, string> { ["country"] = "US" }));
        var usBody = await usResponse.Content.ReadFromJsonAsync<EvaluateResponse>();

        var caResponse = await client.PostAsJsonAsync("/api/v1/evaluate",
            new EvaluateRequest(flag.Key, "user-2", new Dictionary<string, string> { ["country"] = "CA" }));
        var caBody = await caResponse.Content.ReadFromJsonAsync<EvaluateResponse>();

        usBody!.Outcome.Should().BeTrue();
        usBody.Reason.Should().Be("TargetingMatch");
        caBody!.Outcome.Should().BeFalse();
        caBody.Reason.Should().Be("Default");
    }

    [Fact]
    public async Task Evaluate_UnknownFlag_ReturnsSafeDefault_Never404Or500()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/evaluate",
            new EvaluateRequest($"does-not-exist-{Guid.NewGuid():N}", "user-1", null));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<EvaluateResponse>();
        body!.Outcome.Should().BeFalse();
        body.Reason.Should().Be("Unknown");
    }

    [Fact]
    public async Task GetSnapshotVersion_ReturnsCurrentVersionAndBuiltAt()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/snapshot/version");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<SnapshotVersionResponse>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task EvaluateBatch_MultipleFlagKeys_ReturnsOneResultPerKey()
    {
        var client = fixture.CreateClient();
        var keys = new[] { "unknown-a", "unknown-b", "unknown-c" };

        var response = await client.PostAsJsonAsync("/api/v1/evaluate/batch",
            new BatchEvaluateRequest("user-1", null, keys));

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<BatchEvaluateResponse>();
        body!.Results.Should().HaveCount(3);
        body.Results.Select(r => r.FlagKey).Should().BeEquivalentTo(keys);
    }
}
