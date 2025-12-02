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
    
    [JsonPropertyName("zone")]
    public string Zone { get; set; } = string.Empty; // Zone name (e.g., "Zone A", "Zone B")
    
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty; // Region name (reserved for future use, empty for now)
    
    [JsonPropertyName("priceModifier")]
    public decimal PriceModifier { get; set; } = 1.0m; // 0.0 (child), 0.5 (student/pensioner), 1.0 (standard)
    
    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; } = 0m; // Base price per zone (will be calculated)
    
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; } = 0m; // Final calculated price
    
    [JsonPropertyName("bookingDate")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("activatedAt")]
    public DateTime? ActivatedAt { get; set; } // When ticket was activated by user (nullable for backward compatibility)
    
    [JsonPropertyName("validFrom")]
    public DateTime? ValidFrom { get; set; } // Start of validity period (nullable for backward compatibility)
    
    [JsonPropertyName("validTo")]
    public DateTime? ValidTo { get; set; } // End of validity period (nullable for backward compatibility)
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = TicketStatus.Created; // Default to Created for backward compatibility
}

