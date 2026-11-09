using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class AuthFlowTests
{
    [Fact]
    public async Task Registration_rejects_a_weak_password()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Weak Runner",
            email = "weak@example.test",
            password = "runner123",
            role = "Participant",
            contactNumber = "0821234567",
            city = "Durban"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Strong_registration_can_log_in_and_read_its_session()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        const string email = "new.runner@example.test";
        const string password = "RiverPark!2027";

        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "New Runner",
            email,
            password,
            role = "Participant",
            contactNumber = "0821234567",
            city = "Durban"
        });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var session = await client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
        var payload = await session.Content.ReadFromJsonAsync<AuthPayload>();
        Assert.Equal(email, payload!.Email);
        Assert.Equal("Participant", payload.Role);
    }

    private sealed record AuthPayload(int UserId, string FullName, string Email, string Role);
}
