using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Contracts.Users;
using Ticketing.Web.Services;

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
            _logger.LogWarning(ex, "Failed to create user {Email} by admin {AdminId}", user?.Email, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user by admin {AdminId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
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
            _logger.LogError(ex, "Error retrieving users by admin {AdminId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
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
                return NotFound(new { error = $"User with id {id} not found" });
            }
            
            // Don't return password hash
            user.PasswordHash = string.Empty;
            _logger.LogInformation("User {UserId} retrieved by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId} by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] User user, CancellationToken cancellationToken)
    {
        try
        {
            if (user.Id != id)
            {
                return BadRequest(new { error = "User ID mismatch" });
            }

            var updatedUser = await _userService.UpdateUserAsync(user, cancellationToken);
            _logger.LogInformation("User {UserId} updated by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Don't return password hash
            updatedUser.PasswordHash = string.Empty;
            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update user {UserId} by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId} by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
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
            _logger.LogWarning(ex, "Failed to delete user {UserId} by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId} by admin {AdminId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
        }
    }
}

