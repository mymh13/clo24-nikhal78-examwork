using System.Text.Json.Serialization;
using Ticketing.Contracts.Bookings;

namespace Ticketing.Contracts.Events;

public class BookingCreated : Event
{
    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = string.Empty;
    
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;
    
    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;
    
    [JsonPropertyName("zone")]
    public string Zone { get; set; } = string.Empty;
    
    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;
    
    [JsonPropertyName("priceModifier")]
    public decimal PriceModifier { get; set; } = 1.0m;
    
    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; } = 0m;
    
    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; } = 0m;
    
    [JsonPropertyName("bookingDate")]
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Creates a BookingCreated event from a Booking entity.
    /// </summary>
    public static BookingCreated FromBooking(Booking booking)
    {
        return new BookingCreated
        {
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            CustomerEmail = booking.CustomerEmail,
            CustomerName = booking.CustomerName,
            Zone = booking.Zone,
            Region = booking.Region,
            PriceModifier = booking.PriceModifier,
            BasePrice = booking.BasePrice,
            TotalPrice = booking.TotalPrice,
            BookingDate = booking.BookingDate,
            Timestamp = DateTime.UtcNow
        };
    }
}

