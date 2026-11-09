using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class EnrolmentsApiTests
{
    [Fact]
    public async Task Participant_can_enrol_once_and_view_the_entry()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "runner@raceday.test", password = "DemoRunner!2026" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var enrol = await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);

        var entries = await client.GetFromJsonAsync<List<EnrolmentResponse>>("/api/enrolments/mine");
        Assert.NotNull(entries);
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, item => item.EventId == 1 && item.EventName == "Durban Community Run");
    }

    [Fact]
    public async Task Participant_cannot_enrol_twice_in_the_same_event()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        await client.PostAsJsonAsync("/api/auth/login", new { email = "runner@raceday.test", password = "DemoRunner!2026" });

        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,
            (await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 })).StatusCode);
    }

    private sealed record EnrolmentResponse(int Id, int EventId, string EventName, int CategoryId, string CategoryName, string Status, DateTime EnteredAtUtc);
}
