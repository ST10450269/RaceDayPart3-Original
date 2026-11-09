using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class ProfilesApiTests
{
    [Fact]
    public async Task Profile_requires_login_and_can_be_updated_by_its_owner()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);

        await client.PostAsJsonAsync("/api/auth/login", new { email = "runner@raceday.test", password = "DemoRunner!2026" });
        var current = await client.GetFromJsonAsync<ProfileResponse>("/api/profile");
        Assert.Equal("Demo Participant", current!.FullName);

        var update = await client.PutAsJsonAsync("/api/profile", new
        {
            fullName = "Ayesha Runner",
            contactNumber = "0827654321",
            city = "Pietermaritzburg"
        });
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var changed = await client.GetFromJsonAsync<ProfileResponse>("/api/profile");
        Assert.Equal("Pietermaritzburg", changed!.City);
    }

    private sealed record ProfileResponse(int Id, string FullName, string Email, string ContactNumber, string City, string Role);
}
