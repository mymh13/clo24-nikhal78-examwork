using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
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
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect("/login?error=Please provide both username and password");
        }

        if (username == "admin" && password == "admin")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-001"),
                new Claim(ClaimTypes.Name, "Admin User"),
                new Claim(ClaimTypes.Email, "admin@test.local"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "Hardcoded");
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync("Hardcoded", principal, authProperties);

            return Redirect("/test-auth");
        }

        return Redirect("/login?error=Invalid username or password");
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Hardcoded");
        return Redirect("/test-auth");
    }
}

