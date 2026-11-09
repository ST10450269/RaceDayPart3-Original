using System.Net;
using RaceDay.Mvc.Services;

namespace RaceDay.Api.Tests;

public sealed class ApiCookieParserTests
{
    [Fact]
    public void Extracts_only_the_session_cookie_name_and_value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.TryAddWithoutValidation("Set-Cookie", "RaceDay.Api.Session=opaque-token; path=/; httponly; samesite=lax");

        Assert.Equal("RaceDay.Api.Session=opaque-token", ApiCookieParser.FromResponse(response));
    }

    [Fact]
    public void Returns_null_when_the_api_did_not_issue_a_session_cookie()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Headers.TryAddWithoutValidation("Set-Cookie", "OtherCookie=value; path=/");

        Assert.Null(ApiCookieParser.FromResponse(response));
    }
}
