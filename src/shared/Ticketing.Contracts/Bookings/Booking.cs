using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Bookings;

public class Booking
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty; // User's email or ID
    
    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty; // User's email for display
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;
    
    [JsonPropertyName("bookingDate")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
}

