using System.Diagnostics;
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
/// T032 — contract tests for /auth/login|refresh|logout|me per
/// contracts/auth-api.md, including the SC-009 login-latency assertion.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class AuthApiTests(OptiPulseTestFixture fixture)
{
    private async Task<string> SeedUserAsync(UserRole role, string? name = null)
    {
        var email = $"{role}-{Guid.NewGuid():N}@optipulse.test";
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Users.Add(User.Create(email, AuthTestCredentials.Password, name ?? $"Test {role}", role).Value);
        await db.SaveChangesAsync();
        return email;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenPairWithinFiveSeconds()
    {
        // SC-009: users can log in within 5 seconds.
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();

        var stopwatch = Stopwatch.StartNew();
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        stopwatch.Stop();

        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.TokenType.Should().Be("Bearer");
        body.ExpiresInSeconds.Should().BeGreaterThan(0);
        body.Role.Should().Be(nameof(UserRole.Manager));
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5), "SC-009 login latency budget");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Fails401_WithoutRevealingWhetherTheUserExists()
    {
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();

        var wrongPassword = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, "definitely-not-the-right-one"));
        var unknownUser = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest($"nobody-{Guid.NewGuid():N}@optipulse.test", "whatever"));

        wrongPassword.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        unknownUser.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "unknown-user and wrong-password must be indistinguishable (no account enumeration)");
    }

    [Fact]
    public async Task Me_WithAValidAccessToken_ReturnsThePrincipal()
    {
        var email = await SeedUserAsync(UserRole.Admin, name: "Ada Admin");
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        var response = await client.GetAsync("/api/v1/auth/me");

        response.EnsureSuccessStatusCode();
        var me = (await response.Content.ReadFromJsonAsync<MeResponse>())!;
        me.Role.Should().Be(nameof(UserRole.Admin));
        me.Name.Should().Be("Ada Admin");
        // User.Create normalizes email to lowercase (intentional, so lookups are
        // case-insensitive), so compare against the normalized form.
        me.Email.Should().Be(email.ToLowerInvariant());
        me.UserId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Me_WithoutAToken_Fails401()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithATamperedToken_Fails401()
    {
        // FR-A04: invalid/tampered sessions are rejected.
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        var tokens = (await login.Content.ReadFromJsonAsync<LoginResponse>())!;

        // Flip the signature segment so validation must fail.
        var tampered = tokens.AccessToken[..^4] + "AAAA";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tampered);

        var response = await client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_IsAuditedForBothSuccessAndFailure()
    {
        // FR-A07 / SC-006: auth-sensitive events land in the audit trail.
        var email = await SeedUserAsync(UserRole.Manager);
        var client = fixture.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, AuthTestCredentials.Password));
        await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "wrong"));

        using var scope = fixture.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<Audit.Infrastructure.AuditDbContext>();
        var entries = auditDb.AuditEntries.ToList();

        entries.Should().Contain(e =>
            e.ChangeType == Audit.Domain.AuditChangeType.LoginSucceeded &&
            e.AfterStateJson != null && e.AfterStateJson.Contains(email));
        entries.Should().Contain(e =>
            e.ChangeType == Audit.Domain.AuditChangeType.LoginFailed &&
            e.AfterStateJson != null && e.AfterStateJson.Contains(email));
    }
}
