using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Services;

public sealed record ApiReply<T>(bool Success, T? Value, string Message, int StatusCode);

public sealed class ApiGateway(IHttpClientFactory factory, IHttpContextAccessor contextAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<(ApiReply<T> Reply, string? Cookie)> LoginAsync<T>(object body, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "api/auth/login", JsonContent.Create(body));
        try
        {
            var response = await factory.CreateClient("RaceDayApi").SendAsync(request, cancellationToken);
            var cookie = ApiCookieParser.FromResponse(response);
            var reply = await ParseAsync<T>(response, cancellationToken);
            return (reply, cookie);
        }
        catch (HttpRequestException)
        {
            return (Unavailable<T>(), null);
        }
    }

    public Task<ApiReply<T>> GetAsync<T>(string route, CancellationToken cancellationToken = default) =>
        SendAsync<T>(HttpMethod.Get, route, null, cancellationToken);

    public async Task<ApiReply<T>> SendAsync<T>(HttpMethod method, string route, object? body = null, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(method, route, body is null ? null : JsonContent.Create(body));
        try
        {
            var response = await factory.CreateClient("RaceDayApi").SendAsync(request, cancellationToken);
            return await ParseAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Unavailable<T>();
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string route, HttpContent? content)
    {
        var request = new HttpRequestMessage(method, route) { Content = content };
        var cookie = contextAccessor.HttpContext?.Session.GetString("RaceDay.Api.Cookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
        return request;
    }

    private static async Task<ApiReply<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using (response)
        {
            var statusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return new(true, default, string.Empty, statusCode);
                var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
                return new(true, value, string.Empty, statusCode);
            }

            var message = "The request could not be completed.";
            try
            {
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                if (json.RootElement.TryGetProperty("message", out var property))
                    message = property.GetString() ?? message;
                else if (json.RootElement.TryGetProperty("title", out var title))
                    message = title.GetString() ?? message;
            }
            catch (JsonException)
            {
                // Keep the safe generic message when the API returns non-JSON content.
            }

            return new(false, default, message, statusCode);
        }
    }

    private static ApiReply<T> Unavailable<T>() =>
        new(false, default, "RaceDay API is unavailable. Start the API service and try again.", 503);
}
