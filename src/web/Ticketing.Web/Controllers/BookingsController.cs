using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Events;
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
    private readonly IOutboxService _outboxService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ITelemetryService _telemetryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        IBookingService bookingService, 
        IUserService userService, 
        IOutboxService outboxService,
        IFeatureFlagService featureFlagService,
        ITelemetryService telemetryService,
        IConfiguration configuration,
        ILogger<BookingsController> logger)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _outboxService = outboxService ?? throw new ArgumentNullException(nameof(outboxService));
        _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
        _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

                if (!System.Text.RegularExpressions.Regex.IsMatch(customerEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    return BadRequest(new { error = "Invalid email format." });
                }

                targetUser = await _userService.GetUserByEmailAsync(customerEmail, cancellationToken);
                if (targetUser == null)
                {
                    // Create user if doesn't exist with temporary password
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

            booking.CustomerId = targetUser.Id;
            booking.CustomerEmail = targetUser.Email;
            booking.CustomerName = targetUser.Name ?? targetUser.Email;
            
            booking.PriceModifier = PriceCalculationHelper.CalculatePriceModifier(targetUser);
            
            int numberOfZones = string.IsNullOrEmpty(booking.Zone) 
                ? 0 
                : booking.Zone.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
            
            // Get base price from configuration with fallback to default
            var basePricePerZone = _configuration.GetValue<decimal>("Pricing:BasePricePerZone", 20.0m);
            booking.BasePrice = basePricePerZone * numberOfZones;
            booking.TotalPrice = PriceCalculationHelper.CalculateTotalPrice(booking.PriceModifier, numberOfZones, basePricePerZone);
            
            var createdBooking = await _bookingService.CreateBookingAsync(booking, cancellationToken);
            
            // Check feature flag to determine which path to take
            var isEventDrivenEnabled = await _featureFlagService.IsBookingEventsEnabledAsync(cancellationToken);
            var architecturePath = isEventDrivenEnabled ? "Event-Driven" : "Synchronous";
            
            // Track booking creation with architecture mode
            _telemetryService.TrackBookingCreated(
                createdBooking.Id, 
                createdBooking.CustomerEmail, 
                architecturePath, 
                isEventDrivenEnabled);
            
            _logger.LogInformation("Booking created: {BookingId} for customer {CustomerEmail} (ID: {CustomerId}) by user {UserId} with role {Role}. Architecture: {ArchitecturePath}",
                createdBooking.Id, createdBooking.CustomerEmail, createdBooking.CustomerId, userId, userRole, architecturePath);
            
            // Always write to outbox for audit and future activation (dual-system coexistence)
            _logger.LogInformation("Creating outbox event for booking {BookingId} (Architecture: {ArchitecturePath})", createdBooking.Id, architecturePath);
            try
            {
                var bookingCreatedEvent = BookingCreated.FromBooking(createdBooking);
                var outboxEvent = await _outboxService.AddEventAsync(bookingCreatedEvent, cancellationToken);
                
                // Track outbox event creation
                _telemetryService.TrackOutboxEventCreated(
                    outboxEvent.Id, 
                    createdBooking.Id, 
                    outboxEvent.EventType, 
                    architecturePath);
                
                _logger.LogInformation("Outbox event created: {OutboxEventId} for booking {BookingId} (EventType: {EventType}, Architecture: {ArchitecturePath})",
                    outboxEvent.Id, createdBooking.Id, outboxEvent.EventType, architecturePath);
                
                // Event-driven path: Publish to Service Bus when feature flag is enabled
                // TODO: Phase 5 - Implement Service Bus publishing
                if (isEventDrivenEnabled)
                {
                    _logger.LogInformation("Event-driven architecture enabled - Service Bus publishing will be implemented in Phase 5");
                    // Phase 5: await _eventPublisher.PublishEventAsync(bookingCreatedEvent, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Synchronous architecture - booking processed via chained API calls");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the booking creation
                // In production, consider whether to rollback booking or handle outbox failure differently
                _logger.LogError(ex, "Failed to create outbox event for booking {BookingId}. Booking was created successfully. Architecture: {ArchitecturePath}. Error: {ErrorMessage}", 
                    createdBooking.Id, architecturePath, ex.Message);
            }
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

