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
/// T044 / FR-011 — a concurrent edit must be refused, not silently applied. The failure this
/// guards against is the lost update: two managers open the same flag, both save, and the
/// second write erases the first with neither of them ever seeing an error.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class ConcurrencyTests(OptiPulseTestFixture fixture)
{
    private async Task<HttpClient> ManagerClientAsync()
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

    private static Task<HttpResponseMessage> EditAsync(
        HttpClient client, string key, long ifMatch, string newName)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/flags/{key}")
        {
            Content = JsonContent.Create(new UpdateFlagRequest(newName, false, null, null)),
        };
        request.Headers.TryAddWithoutValidation("If-Match", ifMatch.ToString());
        return client.SendAsync(request);
    }

    [Fact]
    public async Task TwoEditsFromTheSameVersion_SecondReturns409_AndDoesNotOverwriteTheFirst()
    {
        var first = await ManagerClientAsync();
        var second = await ManagerClientAsync();

        var created = await first.PostAsJsonAsync(
            "/api/v1/flags", new CreateFlagRequest($"race-{Guid.NewGuid():N}", "Original", false, null, null));
        var flag = (await created.Content.ReadFromJsonAsync<FlagResponse>())!;

        // Both callers read version 1 and edit from it — the classic lost-update setup.
        var firstEdit = await EditAsync(first, flag.Key, flag.Version, "First writer wins");
        var secondEdit = await EditAsync(second, flag.Key, flag.Version, "Second writer clobbers");

        firstEdit.StatusCode.Should().Be(HttpStatusCode.OK);
        secondEdit.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "the second edit was based on a version that no longer exists");

        // The decisive assertion: the first writer's change survived intact.
        var current = await first.GetFromJsonAsync<FlagResponse>($"/api/v1/flags/{flag.Key}");
        current!.Name.Should().Be("First writer wins");
        current.Version.Should().Be(flag.Version + 1, "exactly one edit was applied");
    }
}
