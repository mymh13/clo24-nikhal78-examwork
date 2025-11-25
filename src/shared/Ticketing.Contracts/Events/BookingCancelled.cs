using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Events;

public class BookingCancelled : Event
{
    [JsonPropertyName("bookingId")]
    public string BookingId { get; set; } = string.Empty;
    
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;
    
    [JsonPropertyName("customerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;
    
    [JsonPropertyName("cancellationReason")]
    public string CancellationReason { get; set; } = string.Empty;
    
    [JsonPropertyName("cancelledBy")]
    public string CancelledBy { get; set; } = string.Empty;
    
    [JsonPropertyName("originalBookingDate")]
    public DateTime OriginalBookingDate { get; set; }
    
    [JsonPropertyName("refundAmount")]
    public decimal? RefundAmount { get; set; } // Optional refund amount
}

