using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Users;
using Ticketing.Web.Services;
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
            if (userRole == "User")
            {
                // Users can only create bookings for themselves
                customerEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                if (string.IsNullOrEmpty(customerEmail))
                {
                    return BadRequest(new { error = "User email not found in claims." });
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
            }

            // Validate email format
            if (!System.Text.RegularExpressions.Regex.IsMatch(customerEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return BadRequest(new { error = "Invalid email format." });
            }

            // Get or create user
            var user = await _userService.GetUserByEmailAsync(customerEmail, cancellationToken);
            if (user == null)
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
                user = await _userService.CreateUserAsync(newUser, cancellationToken);
                _logger.LogInformation("Auto-created user {Email} for booking. Admin should reset password.", customerEmail);
            }

            // Set booking customer ID to user ID and email
            booking.CustomerId = user.Id;
            booking.CustomerEmail = user.Email;
            if (string.IsNullOrEmpty(booking.CustomerName))
            {
                booking.CustomerName = user.Name ?? user.Email;
            }
            
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
}

