using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Bookings;

public class Booking
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;
    
    [JsonPropertyName("bookingDate")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
}

