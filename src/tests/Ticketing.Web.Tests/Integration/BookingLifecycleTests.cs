using System.Net;
using System.Net.Http.Json;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Users;
using Xunit;

namespace Ticketing.Web.Tests.Integration;

// Prerequisites:
// - Cosmos DB Emulator must be running locally, OR
// - Set TEST_COSMOS_CONNECTION_STRING environment variable to a test Cosmos DB account connection string
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        var testUser = CreateTestUser();
        var booking = new Booking
        {
            CustomerEmail = testUser.Email,
            CustomerName = testUser.Name,
            CustomerId = testUser.Email,
            Zone = "Zone A, Zone B", // 2 zones
            PriceModifier = 0.5m, // Student discount
            Status = TicketStatus.Created
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/bookings", booking);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(createdBooking);
        var result = createdBooking!;
        
        // Base price: 2 zones * 20 SEK = 40 SEK
        // With 50% modifier: 40 * 0.5 = 20 SEK
        Assert.Equal(40.0m, result.BasePrice);
        Assert.Equal(20.0m, result.TotalPrice);
    }

    [Fact]
    public async Task GetBookingById_WithValidId_ReturnsBooking()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);

        // Act
        var response = await _client.GetAsync($"/api/bookings/customer/{testUser.Email}");

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

        // Act
        var activateResponse = await _client.PostAsJsonAsync(
            $"/api/bookings/{booking.Id}/activate",
            new { });

        // Assert
        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);
        var activatedBooking = await activateResponse.Content.ReadFromJsonAsync<Booking>();
        Assert.NotNull(activatedBooking);
        Assert.Equal(TicketStatus.Activated, activatedBooking.Status);
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
        
        // Activate once
        await _client.PostAsJsonAsync($"/api/bookings/{booking.Id}/activate", new { });

        // Act - Try to activate again
        var response = await _client.PostAsJsonAsync(
            $"/api/bookings/{booking.Id}/activate",
            new { });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBooking_WithValidId_RemovesBooking()
    {
        // Arrange
        var testUser = CreateTestUser();
        var booking = await CreateTestBooking(testUser);

        // Act
        var deleteResponse = await _client.DeleteAsync(
            $"/api/bookings/{booking.Id}?customerId={Uri.EscapeDataString(testUser.Email)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Verify booking is deleted
        var getResponse = await _client.GetAsync($"/api/bookings/customer/{testUser.Email}");
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

        // Act
        var response = await _client.GetAsync($"/api/bookings/customer/{user1.Email}");

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
        response.EnsureSuccessStatusCode();
        
        var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
        return createdBooking ?? throw new InvalidOperationException("Failed to create test booking");
    }
}

