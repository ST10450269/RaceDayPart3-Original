using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAuthorizeAttribute(string? requiredRole = null) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetInt32("RaceDayUserId") is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Please log in first." });
            return;
        }

        if (requiredRole is not null && !string.Equals(session.GetString("RaceDayRole"), requiredRole, StringComparison.Ordinal))
        {
            context.Result = new ObjectResult(new { message = "Your account role cannot perform this action." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

public static class SessionIdentity
{
    public static int? UserId(HttpContext context) => context.Session.GetInt32("RaceDayUserId");
    public static string Role(HttpContext context) => context.Session.GetString("RaceDayRole") ?? string.Empty;
}
