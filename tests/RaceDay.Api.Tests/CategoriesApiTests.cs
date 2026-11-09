using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public sealed class CategoriesApiTests
{
    [Fact]
    public async Task Public_event_categories_are_listed()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();

        var response = await client.GetAsync("/api/events/1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(categories);
        Assert.Contains(categories, item => item.Name == "Open 10K");
    }

    [Fact]
    public async Task Organiser_can_add_a_category_to_their_event()
    {
        using var factory = new ApiTestFactory();
        using var client = factory.CreateInitializedClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "organiser@raceday.test", password = "DemoOrganiser!2026" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var response = await client.PostAsJsonAsync("/api/events/1/categories", new
        {
            name = "Junior 10K",
            minimumAge = 12,
            maximumAge = 17,
            fee = 15m
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.Equal("Junior 10K", category!.Name);
    }

    private sealed record CategoryResponse(int Id, int EventId, string Name, int? MinimumAge, int? MaximumAge, decimal Fee);
}
