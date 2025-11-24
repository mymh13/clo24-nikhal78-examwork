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
    public IActionResult Login([FromForm] string email, [FromForm] string password)
    {
        // Standard login is not yet implemented.
        // For now, redirect with an informative message.
        return Redirect("/login?error=Standard login is not yet implemented. Administrators and inspectors should use Azure Entra ID login.");
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword([FromForm] string email)
    {
        // Dummy implementation - password reset functionality not yet implemented
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email is required." });
        }

        // In a real implementation, this would:
        // 1. Validate the email exists
        // 2. Generate a reset token
        // 3. Send an email with reset link
        // For now, just return a success message
        return Ok(new { message = "Password reset email sent (dummy functionality - not implemented yet)." });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register([FromForm] string email, [FromForm] string password)
    {
        // Registration is currently disabled to prevent bot registrations.
        // User accounts will be managed by administrators in a later step.
        return BadRequest(new { error = "Registration is currently disabled. User accounts are managed by administrators. Please contact support if you need an account." });
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

