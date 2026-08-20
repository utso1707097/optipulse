using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Health;

/// <summary>
/// The health surface the deploy pipeline depends on.
///
/// <para>These are tested because CI now BLOCKS a release on them. /health/version in particular
/// exists so the pipeline can tell which build is answering: polling /health/ready alone reports
/// on whichever instance is currently serving, which during a rolling deploy is the one being
/// replaced — so the old check passed instantly against the outgoing container and announced a
/// healthy new deployment having verified nothing about it.</para>
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class HealthEndpointTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task Liveness_IsAnonymous()
    {
        // Anonymous on purpose: a readiness probe that needs a credential is a probe that fails
        // for reasons unrelated to health, and platforms call it without one.
        var response = await fixture.CreateClient().GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Readiness_ReportsDependencyState()
    {
        var response = await fixture.CreateClient().GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("database");
    }

    [Fact]
    public async Task Version_IsAnonymous_AndAlwaysReportsACommitField()
    {
        // The field must be present even when unset, because the pipeline distinguishes
        // "unknown" (cannot verify — fail loudly) from a mismatch (still deploying — keep
        // waiting). A missing field would collapse those two into a parse failure.
        var response = await fixture.CreateClient().GetAsync("/health/version");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("commit", out var commit).Should().BeTrue();
        commit.GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Version_ReportsUnknown_WhenTheHostDoesNotProvideACommit()
    {
        // The test host has no RENDER_GIT_COMMIT, so this is the shape CI must treat as
        // unverifiable rather than silently accept.
        var json = await fixture.CreateClient().GetFromJsonAsync<JsonElement>("/health/version");

        json.GetProperty("commit").GetString().Should().Be("unknown");
    }

    [Fact]
    public async Task HealthEndpoints_AreNotInThePublishedContract()
    {
        // They are operational, not part of the API surface clients are generated from. Leaving
        // them in would put deployment plumbing into the mobile and web clients.
        var spec = await fixture.CreateClient().GetStringAsync("/openapi/v1.json");

        spec.Should().NotContain("/health/version");
        spec.Should().NotContain("/health/ready");
    }
}
