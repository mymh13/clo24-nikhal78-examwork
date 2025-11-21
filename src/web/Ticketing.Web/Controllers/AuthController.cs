using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        return Redirect("/login?error=Standard login is not yet implemented. Administrators and inspectors should use Azure Entra ID login.");
    }

    [HttpPost("login-entra")]
    [AllowAnonymous]
    public IActionResult LoginEntra()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/admin"
        };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return Redirect("/login?logout=success");
    }
}

