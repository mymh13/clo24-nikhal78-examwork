using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Contracts.Users;
using Ticketing.Web.Services;
using Ticketing.Web.Helpers;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(user, cancellationToken);
            _logger.LogInformation("User created: {UserId} ({Email}) with role {Role} by admin {AdminId}", 
                createdUser.Id, createdUser.Email, createdUser.Role, User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Don't return password hash
            createdUser.PasswordHash = string.Empty;
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return ErrorHandlerHelper.HandleException(ex, _logger, "Creating user", new { Email = user?.Email, AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
        catch (Exception ex)
        {
            return ErrorHandlerHelper.HandleInternalError(ex, _logger, "Creating user", new { Email = user?.Email, AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(cancellationToken);
            // Remove password hashes from response
            foreach (var user in users)
            {
                user.PasswordHash = string.Empty;
            }
            _logger.LogInformation("All users retrieved by admin {AdminId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(users);
        }
        catch (Exception ex)
        {
            return ErrorHandlerHelper.HandleInternalError(ex, _logger, "Retrieving all users", new { AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(string id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return ErrorHandlerHelper.HandleNotFound("User", id, _logger);
            }
            
            // Don't return password hash
            user.PasswordHash = string.Empty;
            _logger.LogInformation("User {UserId} retrieved by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ErrorHandlerHelper.HandleInternalError(ex, _logger, "Retrieving user", new { UserId = id, AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {
            if (user.Id != id)
            {
                return ErrorHandlerHelper.HandleValidationError("User ID mismatch.", _logger, "Updating user");
            }

            var updatedUser = await _userService.UpdateUserAsync(user, cancellationToken);
            _logger.LogInformation("User {UserId} updated by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Don't return password hash
            updatedUser.PasswordHash = string.Empty;
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            return ErrorHandlerHelper.HandleNotFound("User", id, _logger);
        }
        catch (Exception ex)
        {
            return ErrorHandlerHelper.HandleInternalError(ex, _logger, "Updating user", new { UserId = id, AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteUserAsync(id, cancellationToken);
            _logger.LogInformation("User {UserId} deleted by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ErrorHandlerHelper.HandleNotFound("User", id, _logger);
        }
        catch (Exception ex)
        {
            return ErrorHandlerHelper.HandleInternalError(ex, _logger, "Deleting user", new { UserId = id, AdminId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }
}

