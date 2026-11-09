using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class ResultsApiTests
{
    [Fact]
    public async Task Organiser_can_publish_a_past_event_result_and_participant_can_view_it()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();

        var organiserLogin = await Login(client, "organiser@raceday.test", "DemoOrganiser!2026");
        Assert.Equal(HttpStatusCode.OK, organiserLogin.StatusCode);
        var publish = await client.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = 1,
            finishTimeSeconds = 3180,
            position = 4,
            totalFinishers = 65
        });
        Assert.Equal(HttpStatusCode.Created, publish.StatusCode);

        await client.PostAsync("/api/auth/logout", null);
        var participantLogin = await Login(client, "runner@raceday.test", "DemoRunner!2026");
        Assert.Equal(HttpStatusCode.OK, participantLogin.StatusCode);
        var results = await client.GetFromJsonAsync<List<ResultResponse>>("/api/results/mine");
        Assert.NotNull(results);
        Assert.Contains(results, result => result.EventName == "Hillcrest Autumn Race");

        var publicResults = await client.GetFromJsonAsync<List<ResultResponse>>("/api/events/2/results");
        Assert.NotNull(publicResults);
        Assert.Contains(publicResults, result => result.EventName == "Hillcrest Autumn Race");
    }

    [Fact]
    public async Task Organiser_cannot_publish_a_result_before_the_event_date()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        await Login(client, "runner@raceday.test", "DemoRunner!2026");
        var enrolment = await client.PostAsJsonAsync("/api/enrolments", new { eventId = 1, categoryId = 1 });
        Assert.Equal(HttpStatusCode.Created, enrolment.StatusCode);
        var entry = await enrolment.Content.ReadFromJsonAsync<EnrolmentResponse>();
        await client.PostAsync("/api/auth/logout", null);
        await Login(client, "organiser@raceday.test", "DemoOrganiser!2026");

        var response = await client.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = entry!.Id,
            finishTimeSeconds = 3180,
            position = 4,
            totalFinishers = 65
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private static Task<HttpResponseMessage> Login(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { email, password });

    private sealed record EnrolmentResponse(int Id);
    private sealed record ResultResponse(int Id, int EnrolmentId, string EventName, int Position, int TotalFinishers);
}
