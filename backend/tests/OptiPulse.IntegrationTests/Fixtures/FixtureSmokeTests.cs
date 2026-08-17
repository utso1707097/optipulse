using FluentAssertions;
using Xunit;

namespace OptiPulse.IntegrationTests.Fixtures;

[Collection(OptiPulseTestCollection.Name)]
public sealed class FixtureSmokeTests(OptiPulseTestFixture fixture)
{
    [Fact]
    public async Task ApiHost_StartsAgainstRealPostgresAndRedis_AndServesOpenApi()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.IsSuccessStatusCode.Should().BeTrue();
        fixture.PostgresConnectionString.Should().NotBeNullOrWhiteSpace();
        fixture.RedisConnectionString.Should().NotBeNullOrWhiteSpace();
    }
}
