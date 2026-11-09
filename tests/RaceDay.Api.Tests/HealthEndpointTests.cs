using System.Net;

namespace RaceDay.Api.Tests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Health_returns_ok_json()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        using var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", await response.Content.ReadAsStringAsync());
    }
}
