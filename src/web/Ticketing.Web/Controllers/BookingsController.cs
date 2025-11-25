using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Users;
using Ticketing.Web.Services;
using Ticketing.Web.Helpers;
using TicketingUser = Ticketing.Contracts.Users.User;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, IUserService userService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Inspector,User")]
    public async Task<ActionResult<Booking>> CreateBooking([FromBody] Booking booking, CancellationToken cancellationToken)
    {
        try
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Email);
            
            // Determine customer email - use provided email or current user's email
            string customerEmail;
            TicketingUser? targetUser = null;
            
            if (userRole == "User")
            {
                // Users can only create bookings for themselves - ignore any email/name from request
                customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    return BadRequest(new { error = "User email not found in claims." });
                }
                
                // Get the current user's account
                targetUser = await _userService.GetUserByEmailAsync(customerEmail, cancellationToken);
                if (targetUser == null)
                {
                    return BadRequest(new { error = "User account not found." });
                }
            }
            else
            {
                // Admin/Inspector can create bookings for any user
                customerEmail = booking.CustomerEmail ?? booking.CustomerId;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    return BadRequest(new { error = "Customer email is required." });
                }

                // Validate email format
                if (!System.Text.RegularExpressions.Regex.IsMatch(customerEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    return BadRequest(new { error = "Invalid email format." });
                }

                // Get or create user
                targetUser = await _userService.GetUserByEmailAsync(customerEmail, cancellationToken);
                if (targetUser == null)
                {
                    // Create user if doesn't exist with temporary password
                    // Password will be hashed by UserService
                    var newUser = new TicketingUser
                    {
                        Email = customerEmail,
                        Name = booking.CustomerName ?? customerEmail.Split('@')[0], // Use email prefix as name
                        Role = "User",
                        PasswordHash = "TempPassword123!" // Temporary password, admin should reset this
                    };
                    targetUser = await _userService.CreateUserAsync(newUser, cancellationToken);
                    _logger.LogInformation("Auto-created user {Email} for booking. Admin should reset password.", customerEmail);
                }
            }

            // Set booking customer ID to user ID and email - always use user's actual data
            booking.CustomerId = targetUser.Id;
            booking.CustomerEmail = targetUser.Email;
            booking.CustomerName = targetUser.Name ?? targetUser.Email; // Always use user's actual name, ignore form input
            
            // Calculate price modifier based on user's age and student status
            booking.PriceModifier = PriceCalculationHelper.CalculatePriceModifier(targetUser);
            
            // Calculate prices (base price per zone is 20 SEK)
            // Zone can be comma-separated list (e.g., "Zone A, Zone B")
            int numberOfZones = string.IsNullOrEmpty(booking.Zone) 
                ? 0 
                : booking.Zone.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            booking.BasePrice = 20.0m * numberOfZones; // Base price per zone (20 SEK)
            booking.TotalPrice = PriceCalculationHelper.CalculateTotalPrice(booking.PriceModifier, numberOfZones, 20.0m);
            
            var createdBooking = await _bookingService.CreateBookingAsync(booking, cancellationToken);
            _logger.LogInformation("Booking created: {BookingId} for customer {CustomerEmail} (ID: {CustomerId}) by user {UserId} with role {Role}", 
                createdBooking.Id, createdBooking.CustomerEmail, createdBooking.CustomerId, userId, userRole);
            return CreatedAtAction(nameof(GetBookingsByCustomer), new { customerId = createdBooking.CustomerId }, createdBooking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for customer {CustomerEmail}", booking?.CustomerEmail);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByCustomer(string customerId, CancellationToken cancellationToken)
    {
        try
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            _logger.LogInformation("Bookings retrieved for customer {CustomerId} by user {UserId} with role {Role}", 
                customerId, userId, userRole);
            
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId, cancellationToken);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for customer {CustomerId}", customerId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings(CancellationToken cancellationToken)
    {
        try
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            _logger.LogInformation("All bookings retrieved by user {UserId} with role {Role}", userId, userRole);
            
            var bookings = await _bookingService.GetAllBookingsAsync(cancellationToken);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bookings");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings(CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { error = "User ID not found in claims." });
            }
            
            _logger.LogInformation("Bookings retrieved for user {UserId} ({UserEmail})", userId, userEmail);
            
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(userId, cancellationToken);
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{bookingId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBooking(string bookingId, [FromQuery] string customerId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest(new { error = "Booking ID is required." });
            }

            if (string.IsNullOrEmpty(customerId))
            {
                return BadRequest(new { error = "Customer ID is required." });
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            _logger.LogInformation("Booking deletion requested: {BookingId} for customer {CustomerId} by admin {AdminId}", 
                bookingId, customerId, adminId);
            
            await _bookingService.DeleteBookingAsync(bookingId, customerId, cancellationToken);
            
            _logger.LogInformation("Booking deleted: {BookingId} by admin {AdminId}", bookingId, adminId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Booking deletion failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {BookingId}", bookingId);
            return BadRequest(new { error = "An unexpected error occurred." });
        }
    }
}

