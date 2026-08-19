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

namespace OptiPulse.IntegrationTests.Alerts;

/// <summary>
/// The alerts contract and the guarantee underneath it (T064, T065, FR-026).
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class AlertsApiTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task EngagingAKillSwitch_RaisesACriticalAlert_VisibleToOtherAdmins()
    {
        // The whole point of the feature: an Admin somewhere else finds out that a kill switch
        // moved, without the person who moved it having to tell them.
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = $"alert-e2e-{Guid.NewGuid():N}";

        var created = await manager.PostAsJsonAsync("/api/v1/flags",
            new CreateFlagRequest(flagKey, "Alert e2e", false, null, null));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        var admin = await ClientAsync(UserRole.Admin);
        var engaged = await admin.PostAsJsonAsync(
            $"/api/v1/flags/{flagKey}/kill-switch", new KillSwitchRequest(true));
        engaged.StatusCode.Should().Be(HttpStatusCode.OK);

        // A DIFFERENT admin reads the history.
        var otherAdmin = await ClientAsync(UserRole.Admin);
        var alerts = await otherAdmin.GetFromJsonAsync<List<AlertResponse>>("/api/v1/alerts?limit=200");

        var alert = alerts!.FirstOrDefault(a => a.FlagKey == flagKey);
        alert.Should().NotBeNull("engaging a kill switch must be alertable");
        alert!.Kind.Should().Be("KillSwitchChanged");
        alert.Severity.Should().Be("Critical");
        alert.AcknowledgedAt.Should().BeNull();
    }

    [Fact]
    public async Task Acknowledging_IsRecorded_AndIsIdempotent()
    {
        var (flagKey, alertId) = await RaiseKillSwitchAlertAsync();
        var admin = await ClientAsync(UserRole.Admin);

        var first = await admin.PostAsync($"/api/v1/alerts/{alertId}/ack", null);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var acked = (await first.Content.ReadFromJsonAsync<AlertResponse>())!;
        acked.AcknowledgedAt.Should().NotBeNull();
        acked.AcknowledgedBy.Should().NotBeNullOrWhiteSpace();

        // A second acknowledgement keeps the FIRST responder. Two admins opening the app at once
        // must not make the record of who responded depend on request ordering.
        var second = await admin.PostAsync($"/api/v1/alerts/{alertId}/ack", null);
        var reacked = (await second.Content.ReadFromJsonAsync<AlertResponse>())!;
        reacked.AcknowledgedAt.Should().Be(acked.AcknowledgedAt);
        reacked.AcknowledgedBy.Should().Be(acked.AcknowledgedBy);

        flagKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UnacknowledgedFilter_ExcludesWhatHasBeenSeen()
    {
        var (_, alertId) = await RaiseKillSwitchAlertAsync();
        var admin = await ClientAsync(UserRole.Admin);

        await admin.PostAsync($"/api/v1/alerts/{alertId}/ack", null);

        var outstanding = await admin.GetFromJsonAsync<List<AlertResponse>>(
            "/api/v1/alerts?unacknowledgedOnly=true&limit=200");

        outstanding!.Should().NotContain(a => a.Id == alertId);
    }

    [Fact]
    public async Task Alerts_AreAdminOnly()
    {
        // Managers author experiments; alerts are operational. A Manager reading them is not
        // catastrophic, but acknowledging one tells other Admins "someone is on this", and that
        // is not a claim the authoring role should be able to make.
        var manager = await ClientAsync(UserRole.Manager);

        (await manager.GetAsync("/api/v1/alerts")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await manager.PostAsJsonAsync("/api/v1/alerts/devices",
            new RegisterDeviceRequest("Ios", "token"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Alerts_RejectAnonymousCallers()
    {
        var anonymous = fixture.CreateClient();
        (await anonymous.GetAsync("/api/v1/alerts")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisteringTheSameToken_DoesNotCreateASecondDevice()
    {
        // One alert must not become several notifications to one phone. An app that registers on
        // every launch would otherwise accumulate a row per launch.
        var admin = await ClientAsync(UserRole.Admin);
        var token = $"tok-{Guid.NewGuid():N}";

        var first = await admin.PostAsJsonAsync("/api/v1/alerts/devices",
            new RegisterDeviceRequest("Android", token));
        var second = await admin.PostAsJsonAsync("/api/v1/alerts/devices",
            new RegisterDeviceRequest("Android", token));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var a = (await first.Content.ReadFromJsonAsync<RegisterDeviceResponse>())!;
        var b = (await second.Content.ReadFromJsonAsync<RegisterDeviceResponse>())!;
        b.Id.Should().Be(a.Id, "re-registration is a touch, not a new device");
    }

    [Fact]
    public async Task RegisterDevice_DoesNotEchoTheTokenBack()
    {
        // A response body is the easiest place for a credential-shaped value to end up in a log
        // or a crash report, and the caller already has it.
        var admin = await ClientAsync(UserRole.Admin);
        var token = $"tok-{Guid.NewGuid():N}";

        var response = await admin.PostAsJsonAsync("/api/v1/alerts/devices",
            new RegisterDeviceRequest("Ios", token));

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain(token);
    }

    [Fact]
    public async Task RegisterDevice_RejectsAnUnknownPlatform()
    {
        var admin = await ClientAsync(UserRole.Admin);

        var response = await admin.PostAsJsonAsync("/api/v1/alerts/devices",
            new RegisterDeviceRequest("blackberry", "tok"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task LiveTelemetry_ReportsTheSnapshotActuallyBeingServed()
    {
        // T071. Reads the in-memory snapshot rather than the database, because during a Postgres
        // blip that is the number an operator needs and the one a query cannot give them.
        var admin = await ClientAsync(UserRole.Admin);

        var live = await admin.GetFromJsonAsync<LiveTelemetryResponse>("/api/v1/telemetry/live");

        live.Should().NotBeNull();
        live!.ActiveFlags.Should().BeGreaterThanOrEqualTo(0);
        live.KillSwitchesEngaged.Should().BeLessThanOrEqualTo(live.ActiveFlags);
        live.ServerTime.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    private async Task<(string FlagKey, Guid AlertId)> RaiseKillSwitchAlertAsync()
    {
        var manager = await ClientAsync(UserRole.Manager);
        var flagKey = $"alert-{Guid.NewGuid():N}";

        await manager.PostAsJsonAsync("/api/v1/flags",
            new CreateFlagRequest(flagKey, "Alert source", false, null, null));
        await manager.PostAsJsonAsync($"/api/v1/flags/{flagKey}/status", new ChangeStatusRequest("Active"));

        var admin = await ClientAsync(UserRole.Admin);
        await admin.PostAsJsonAsync($"/api/v1/flags/{flagKey}/kill-switch", new KillSwitchRequest(true));

        var alerts = await admin.GetFromJsonAsync<List<AlertResponse>>("/api/v1/alerts?limit=200");
        var alert = alerts!.Single(a => a.FlagKey == flagKey);
        return (flagKey, alert.Id);
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
