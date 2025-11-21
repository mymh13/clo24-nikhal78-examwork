using System.Security.Claims;

namespace Ticketing.Web.Helpers;

public static class NavigationHelper
{
    public static string GetLandingPageUrl(ClaimsPrincipal? user)
    {
        if (user == null || !user.Identity?.IsAuthenticated == true)
        {
            return "/login";
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        return role switch
        {
            "Admin" => "/admin",
            "Inspector" => "/inspector",
            "User" => "/user",
            _ => "/login"
        };
    }
}

