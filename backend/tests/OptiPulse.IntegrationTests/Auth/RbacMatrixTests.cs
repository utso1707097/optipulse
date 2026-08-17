using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Api.Endpoints;
using OptiPulse.IdentityAccess;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Auth;

/// <summary>
/// T033 — RBAC matrix: the wrong role must be refused with 403 and the right role
/// admitted, with zero unauthorized successes (FR-A03/FR-A04, SC-010).
///
/// SCOPE NOTE: the Manager-only management endpoints and Admin-only kill-switch
/// endpoints are Phase 5/6 work and do not exist yet, so this suite verifies the
/// policies themselves against probe endpoints registered by the test host
/// (RbacProbeEndpoints) using the SAME policy constants the real endpoints will
/// use. When the real endpoints land, they inherit already-verified policies.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class RbacMatrixTests(OptiPulseTestFixture fixture)
{
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
        login.EnsureSuccessStatusCode();
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return client;
    }

    [Fact]
    public async Task ManagerOnlyEndpoint_AdmitsManager_AndRefusesAdminWith403()
    {
        var manager = await AuthenticatedClientAsync(UserRole.Manager);
        var admin = await AuthenticatedClientAsync(UserRole.Admin);

        (await manager.GetAsync("/api/v1/_rbac-probe/manager-only"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        (await admin.GetAsync("/api/v1/_rbac-probe/manager-only"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden,
                "an Admin must not be able to perform Manager-only authoring actions");
    }

    [Fact]
    public async Task AdminOnlyEndpoint_AdmitsAdmin_AndRefusesManagerWith403()
    {
        var manager = await AuthenticatedClientAsync(UserRole.Manager);
        var admin = await AuthenticatedClientAsync(UserRole.Admin);

        (await admin.GetAsync("/api/v1/_rbac-probe/admin-only"))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        (await manager.GetAsync("/api/v1/_rbac-probe/admin-only"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden,
                "a Manager must not be able to operate the kill-switch (US2 scenario 3)");
    }

    [Fact]
    public async Task SharedReadEndpoint_AdmitsBothRoles()
    {
        // Permission matrix: analytics/audit reads are granted to both roles.
        var manager = await AuthenticatedClientAsync(UserRole.Manager);
        var admin = await AuthenticatedClientAsync(UserRole.Admin);

        (await manager.GetAsync("/api/v1/_rbac-probe/any-role")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await admin.GetAsync("/api/v1/_rbac-probe/any-role")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/api/v1/_rbac-probe/manager-only")]
    [InlineData("/api/v1/_rbac-probe/admin-only")]
    [InlineData("/api/v1/_rbac-probe/any-role")]
    public async Task AllProtectedEndpoints_Refuse401_WithoutAnyToken(string path)
    {
        // SC-010: zero unauthorized successes across the matrix.
        var anonymous = fixture.CreateClient();

        var response = await anonymous.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
