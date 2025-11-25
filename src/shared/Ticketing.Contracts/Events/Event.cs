using System.Text.Json.Serialization;

namespace Ticketing.Contracts.Events;

public abstract class Event
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";
    
    [JsonPropertyName("source")]
    public string Source { get; set; } = "Ticketing.Api";
    
    protected Event()
    {
        // Set EventType to the class name by default
        EventType = GetType().Name;
    }
}

