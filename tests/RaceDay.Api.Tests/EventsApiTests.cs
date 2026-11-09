using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class EventsApiTests
{
    [Fact]
    public async Task Anyone_can_browse_seeded_published_events()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();

        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var events = await response.Content.ReadFromJsonAsync<List<EventResponse>>();
        Assert.NotNull(events);
        Assert.Contains(events, item => item.Name == "Durban Community Run");
    }

    [Fact]
    public async Task Unknown_event_id_returns_not_found()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();

        var response = await client.GetAsync("/api/events/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_an_event()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        var response = await client.PostAsJsonAsync("/api/events", NewEvent());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_can_create_an_event()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "organiser@raceday.test", password = "DemoOrganiser!2026" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var response = await client.PostAsJsonAsync("/api/events", NewEvent());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(created);
        Assert.Equal("Test Loop Run", created.Name);
    }

    private static object NewEvent() => new
    {
        name = "Test Loop Run",
        description = "A short test event created by an organiser.",
        eventDateUtc = DateTime.UtcNow.AddDays(20),
        location = "Durban",
        distanceKm = 5m,
        eventType = "Running",
        maxParticipants = 120
    };

    private sealed record EventResponse(int Id, string Name, DateTime EventDate, string Location, decimal DistanceKm, string EventType);
}
