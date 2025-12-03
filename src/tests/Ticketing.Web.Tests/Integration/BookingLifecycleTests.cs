using System.Net;
using System.Net.Http.Json;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Users;
using Xunit;

namespace Ticketing.Web.Tests.Integration;

// Prerequisites:
// - No external dependencies required!
// - Tests use in-memory mock services (InMemoryUserService, InMemoryBookingService, InMemoryOutboxService)
public class BookingLifecycleTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;

    public BookingLifecycleTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateBooking_WithValidData_ReturnsCreatedBooking()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = new Booking
        {
            CustomerEmail = testUser.Email,
            CustomerName = testUser.Name,
            CustomerId = testUser.Email,
            Zone = "Zone A",
            PriceModifier = 1.0m,
            Status = TicketStatus.Created
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/bookings", booking);

        // Assert
        // POST /api/bookings returns Created (201) on success, not OK (200)
        Assert.True(response.IsSuccessStatusCode, $"Expected success status but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(createdBooking);
        var result = createdBooking!;
        Assert.NotEmpty(result.Id);
        Assert.Equal(booking.CustomerEmail, result.CustomerEmail);
        Assert.Equal(booking.Zone, result.Zone);
        Assert.Equal(TicketStatus.Created, result.Status);
    }

    [Fact]
    public async Task CreateBooking_CalculatesPriceCorrectly()
    {
        // Arrange
        // Note: When Admin creates a booking, the controller auto-creates a user without DateOfBirth/IsStudent
        // So the user gets standard pricing (1.0 modifier) by default
        var testUser = CreateTestUser();
        var booking = new Booking
        {
            CustomerEmail = testUser.Email,
            CustomerName = testUser.Name,
            CustomerId = testUser.Email,
            Zone = "Zone A, Zone B", // 2 zones
            Status = TicketStatus.Created
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/bookings", booking);

        // Assert
        // POST /api/bookings returns Created (201) on success
        Assert.True(response.IsSuccessStatusCode, $"Expected success status but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(createdBooking);
        var result = createdBooking!;
        
        // Base price: 2 zones * 20 SEK = 40 SEK
        // Auto-created user (no DateOfBirth, not student) gets standard pricing: 40 * 1.0 = 40 SEK
        Assert.Equal(40.0m, result.BasePrice);
        Assert.Equal(1.0m, result.PriceModifier); // Standard pricing (no discount)
        Assert.Equal(40.0m, result.TotalPrice);
    }

    [Fact]
    public async Task GetBookingById_WithValidId_ReturnsBooking()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);
        // Controller uses user.Id as CustomerId, not email
        var customerId = booking.CustomerId;

        // Act
        var response = await _client.GetAsync($"/api/bookings/customer/{Uri.EscapeDataString(customerId)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
        Assert.NotNull(bookings);
        Assert.Contains(bookings, b => b.Id == booking.Id);
    }

    [Fact]
    public async Task ActivateBooking_WithValidBooking_UpdatesStatus()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);
        // Controller uses user.Id as CustomerId for activation
        var customerId = booking.CustomerId;

        // Act
        var activateResponse = await _client.PostAsJsonAsync(
            $"/api/bookings/{booking.Id}/activate",
            new { CustomerId = customerId });

        // Assert
        if (!activateResponse.IsSuccessStatusCode)
        {
            var errorContent = await activateResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Activation failed: {activateResponse.StatusCode} - {errorContent}");
        }
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        var activatedBooking = await activateResponse.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(activatedBooking);
        Assert.Equal(TicketStatus.Activated, activatedBooking!.Status);
        Assert.NotNull(activatedBooking.ActivatedAt);
        Assert.NotNull(activatedBooking.ValidFrom);
        Assert.NotNull(activatedBooking.ValidTo);
        
        // Verify validity period is 90 minutes
        var validityDuration = activatedBooking.ValidTo.Value - activatedBooking.ValidFrom.Value;
        Assert.Equal(90, validityDuration.TotalMinutes);
    }

    [Fact]
    public async Task ActivateBooking_AlreadyActivated_ReturnsBadRequest()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);
        var customerId = booking.CustomerId;
        
        // Activate once
        var firstActivate = await _client.PostAsJsonAsync(
            $"/api/bookings/{booking.Id}/activate", 
            new { CustomerId = customerId });
        firstActivate.EnsureSuccessStatusCode();

        // Act - Try to activate again
        var response = await _client.PostAsJsonAsync(
            $"/api/bookings/{booking.Id}/activate",
            new { CustomerId = customerId });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBooking_WithValidId_RemovesBooking()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);
        // Controller uses user.Id as CustomerId
        var customerId = booking.CustomerId;

        // Act
        var deleteResponse = await _client.DeleteAsync(
            $"/api/bookings/{booking.Id}?customerId={Uri.EscapeDataString(customerId)}");

        // Assert
        // DELETE returns NoContent (204) on success, not OK (200)
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify booking is deleted
        var getResponse = await _client.GetAsync($"/api/bookings/customer/{Uri.EscapeDataString(customerId)}");
        var bookings = await getResponse.Content.ReadFromJsonAsync<List<Booking>>();
        Assert.NotNull(bookings);
        Assert.DoesNotContain(bookings, b => b.Id == booking.Id);
    }

    [Fact]
    public async Task GetBookingsByCustomer_ReturnsOnlyCustomerBookings()
    {
        // Arrange
        var user1 = CreateTestUser("user1@example.com");
        var user2 = CreateTestUser("user2@example.com");
        
        var booking1 = await CreateTestBooking(user1);
        var booking2 = await CreateTestBooking(user2);
        // Controller uses user.Id as CustomerId
        var customerId1 = booking1.CustomerId;
        var customerId2 = booking2.CustomerId;

        // Act
        var response = await _client.GetAsync($"/api/bookings/customer/{Uri.EscapeDataString(customerId1)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
        Assert.NotNull(bookings);
        Assert.Contains(bookings, b => b.Id == booking1.Id);
        Assert.DoesNotContain(bookings, b => b.Id == booking2.Id);
    }

    // Helper methods
    private User CreateTestUser(string? email = null)
    {
        return new User
        {
            Email = email ?? $"test-{Guid.NewGuid()}@example.com",
            Name = "Test User",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            IsStudent = false
        };
    }

    private async Task<Booking> CreateTestBooking(User user)
    {
        var booking = new Booking
        {
            CustomerEmail = user.Email,
            CustomerName = user.Name,
            CustomerId = user.Email,
            Zone = "Zone A",
            PriceModifier = 1.0m,
            Status = TicketStatus.Created
        };

        var response = await _client.PostAsJsonAsync("/api/bookings", booking);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create test booking: {response.StatusCode} - {errorContent}");
        }
        
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        return createdBooking ?? throw new InvalidOperationException("Failed to create test booking");
    }
}

