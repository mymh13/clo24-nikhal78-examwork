using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Contracts.Bookings;
using Ticketing.Web.Services;

namespace Ticketing.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
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
            
            // Users can only create bookings for themselves
            if (userRole == "User" && booking.CustomerId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to create booking for different customer {CustomerId}", userId, booking.CustomerId);
                return Forbid();
            }
            
            var createdBooking = await _bookingService.CreateBookingAsync(booking, cancellationToken);
            _logger.LogInformation("Booking created: {BookingId} for customer {CustomerId} by user {UserId} with role {Role}", 
                createdBooking.Id, createdBooking.CustomerId, userId, userRole);
            return CreatedAtAction(nameof(GetBookingsByCustomer), new { customerId = createdBooking.CustomerId }, createdBooking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for customer {CustomerId}", booking?.CustomerId);
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
}

