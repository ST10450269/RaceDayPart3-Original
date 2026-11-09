using System.Net.Http;

namespace RaceDay.Mvc.Services;

public static class ApiCookieParser
{
    public const string SessionCookieName = "RaceDay.Api.Session";

    public static string? FromResponse(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers)) return null;

        foreach (var header in headers)
        {
            var cookie = header.Split(';', 2)[0].Trim();
            if (cookie.StartsWith(SessionCookieName + "=", StringComparison.Ordinal)) return cookie;
        }

        return null;
    }
}
