using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Outbox;

public class OutboxEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("eventData")]
    public string EventData { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public OutboxEventStatus Status { get; set; } = OutboxEventStatus.Pending;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("processedAt")]
    public DateTime? ProcessedAt { get; set; }
    
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; } = 0;
    
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    // Partition key for Cosmos DB - using status for efficient querying of pending events
    [JsonPropertyName("partitionKey")]
    public string PartitionKey => Status.ToString();
}

public enum OutboxEventStatus
{
    // Event is pending processing and has not been published yet.
    Pending = 0,
    
    // Event has been successfully published to Service Bus.
    Processed = 1,
    
    // Event processing failed after maximum retry attempts.
    Failed = 2
}

