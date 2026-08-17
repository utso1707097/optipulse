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
/// T034 — refresh-token rotation and reuse detection (research R10, US2
/// scenarios 2, 5 and 6). Runs against the real Postgres container so the
/// revocation/family logic is exercised through actual persistence.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class RefreshRotationTests(OptiPulseTestFixture fixture)
{
    private async Task<string> SeedUserAsync(UserRole role)
    {
        var email = $"{role}-{Guid.NewGuid():N}@optipulse.test";
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Users.Add(User.Create(email, AuthTestCredentials.Password, $"Test {role}", role).Value);
        await db.SaveChangesAsync();
        return email;
    }

    private static async Task<LoginResponse> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    [Fact]
    public async Task Refresh_RotatesTheToken_IssuingANewPairWithoutReLogin()
    {
        // US2 scenario 2 / SC-009: seamless session continuation.
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();
        var login = await LoginAsync(client, email);

        var refreshResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh", new RefreshRequest(login.RefreshToken));

        refreshResponse.EnsureSuccessStatusCode();
        var rotated = (await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>())!;
        rotated.RefreshToken.Should().NotBe(login.RefreshToken, "the refresh token must rotate");
        rotated.AccessToken.Should().NotBeNullOrWhiteSpace();
        rotated.Role.Should().Be(nameof(UserRole.Manager));
    }

    [Fact]
    public async Task Refresh_WithAnAlreadyRotatedToken_Fails401_AndRevokesTheWholeFamily()
    {
        // US2 scenario 5 — reuse detection. Presenting an already-rotated token
        // means it leaked, so BOTH it and the newest token must stop working.
        var email = await SeedUserAsync(UserRole.Admin);
        var client = fixture.CreateClient();
        var login = await LoginAsync(client, email);

        // First rotation succeeds and yields the current (newest) token.
        var first = await client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(login.RefreshToken));
        first.EnsureSuccessStatusCode();
        var current = (await first.Content.ReadFromJsonAsync<LoginResponse>())!;

        // Replaying the ORIGINAL (now rotated) token is the leak signal.
        var reuse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshRequest(login.RefreshToken));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // ...and the family revocation must also invalidate the newest token.
        var afterRevocation = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh", new RefreshRequest(current.RefreshToken));
        afterRevocation.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "reuse detection must revoke the entire token family, not just the replayed token");
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshToken_SoItCannotBeReused()
    {
        // US2 scenario 6.
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();
        var login = await LoginAsync(client, email);

        var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new RefreshRequest(login.RefreshToken));
        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterLogout = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh", new RefreshRequest(login.RefreshToken));
        afterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithAnUnknownToken_Fails401()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh", new RefreshRequest("this-token-was-never-issued"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
