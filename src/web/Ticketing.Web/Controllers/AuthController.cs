using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Web.Services;
using Ticketing.Web.Helpers;
using BCrypt.Net;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect("/login?error=" + Uri.EscapeDataString("Email and password are required."));
        }

        try
        {
            // Get user by email
            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
                return Redirect("/login?error=" + Uri.EscapeDataString("Invalid email or password."));
            }

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", email);
                return Redirect("/login?error=" + Uri.EscapeDataString("Invalid email or password."));
            }

            // Create claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name ?? user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User logged in successfully: {Email} with role {Role}", user.Email, user.Role);

            // Redirect to appropriate landing page based on role
            var redirectUrl = NavigationHelper.GetLandingPageUrl(new ClaimsPrincipal(claimsIdentity));
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            ErrorHandlerHelper.HandleInternalError(ex, _logger, "User login", new { Email = email });
            return Redirect("/login?error=" + Uri.EscapeDataString("An error occurred during login. Please try again."));
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public IActionResult ForgotPassword([FromForm] string email)
    {
        // Dummy implementation - password reset functionality not yet implemented
        if (string.IsNullOrWhiteSpace(email))
        {
            return ErrorHandlerHelper.HandleValidationError("Email is required.", _logger, "Password reset");
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
        return ErrorHandlerHelper.HandleValidationError("Registration is currently disabled. User accounts are managed by administrators. Please contact support if you need an account.", _logger, "User registration");
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

