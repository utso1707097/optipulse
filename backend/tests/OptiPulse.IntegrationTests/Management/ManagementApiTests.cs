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
/// T042 — contract tests for the management API (contracts/management-api.md), including the
/// role split: authoring is Manager, the kill-switch is Admin.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class ManagementApiTests(OptiPulseTestFixture fixture)
{
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
        login.EnsureSuccessStatusCode();
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }

    private static CreateFlagRequest NewFlag(string? key = null) => new(
        key ?? $"flag-{Guid.NewGuid():N}", "Checkout CTA", false, null, null);

    [Fact]
    public async Task CreateFlag_AsManager_Returns201_WithVersion1AndDraftStatus()
    {
        var manager = await ClientAsync(UserRole.Manager);

        var response = await manager.PostAsJsonAsync("/api/v1/flags", NewFlag());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var flag = (await response.Content.ReadFromJsonAsync<FlagResponse>())!;
        flag.Version.Should().Be(1);
        flag.Status.Should().Be("Draft", "a new flag must not serve traffic until activated");
    }

    [Fact]
    public async Task CreateFlag_WithADuplicateKey_Returns409()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var request = NewFlag();
        await manager.PostAsJsonAsync("/api/v1/flags", request);

        var duplicate = await manager.PostAsJsonAsync("/api/v1/flags", request);

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateFlag_AsAdmin_Returns403_AuthoringIsManagerOnly()
    {
        var admin = await ClientAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync("/api/v1/flags", NewFlag());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateFlag_WithoutIfMatch_Returns428()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var created = await manager.PostAsJsonAsync("/api/v1/flags", NewFlag());
        var flag = (await created.Content.ReadFromJsonAsync<FlagResponse>())!;

        var response = await manager.PutAsJsonAsync(
            $"/api/v1/flags/{flag.Key}", new UpdateFlagRequest("Renamed", true, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionRequired,
            "an edit without the version it was based on could overwrite an unseen change");
    }

    [Fact]
    public async Task UpdateFlag_WithCurrentVersion_Succeeds_AndBumpsVersion()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var created = await manager.PostAsJsonAsync("/api/v1/flags", NewFlag());
        var flag = (await created.Content.ReadFromJsonAsync<FlagResponse>())!;

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/flags/{flag.Key}")
        {
            Content = JsonContent.Create(new UpdateFlagRequest("Renamed", true, null, null)),
        };
        request.Headers.TryAddWithoutValidation("If-Match", flag.Version.ToString());

        var response = await manager.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var updated = (await response.Content.ReadFromJsonAsync<FlagResponse>())!;
        updated.Name.Should().Be("Renamed");
        updated.Version.Should().Be(flag.Version + 1);
    }

    [Fact]
    public async Task KillSwitch_AsAdmin_Engages_AndIsReflectedOnTheFlag()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var admin = await ClientAsync(UserRole.Admin);

        var created = await manager.PostAsJsonAsync("/api/v1/flags", NewFlag());
        var flag = (await created.Content.ReadFromJsonAsync<FlagResponse>())!;
        await manager.PostAsJsonAsync($"/api/v1/flags/{flag.Key}/status", new ChangeStatusRequest("Active"));

        var response = await admin.PostAsJsonAsync(
            $"/api/v1/flags/{flag.Key}/kill-switch", new KillSwitchRequest(true));

        response.EnsureSuccessStatusCode();
        var killed = (await response.Content.ReadFromJsonAsync<FlagResponse>())!;
        killed.KillSwitchEngaged.Should().BeTrue();
    }

    [Fact]
    public async Task KillSwitch_AsManager_Returns403_OperationsAreAdminOnly()
    {
        // US2 scenario 3 — a Manager must not be able to operate the kill-switch.
        var manager = await ClientAsync(UserRole.Manager);
        var created = await manager.PostAsJsonAsync("/api/v1/flags", NewFlag());
        var flag = (await created.Content.ReadFromJsonAsync<FlagResponse>())!;

        var response = await manager.PostAsJsonAsync(
            $"/api/v1/flags/{flag.Key}/kill-switch", new KillSwitchRequest(true));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ManagementEndpoints_RefuseAnonymousCallers()
    {
        var anonymous = fixture.CreateClient();

        (await anonymous.GetAsync("/api/v1/flags")).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
        (await anonymous.PostAsJsonAsync("/api/v1/flags", NewFlag())).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }
}
