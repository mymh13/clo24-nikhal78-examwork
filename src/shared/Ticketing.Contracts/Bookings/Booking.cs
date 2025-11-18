namespace Ticketing.Contracts.Bookings;

/// <summary>
/// Minimal Booking entity for Cosmos DB storage
/// </summary>
public class Booking
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
}

