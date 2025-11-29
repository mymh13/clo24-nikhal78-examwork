using Microsoft.ApplicationInsights;

namespace Ticketing.Web.Services;

public class ApplicationInsightsTelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ApplicationInsightsTelemetryService> _logger;

    public ApplicationInsightsTelemetryService(
        TelemetryClient telemetryClient,
        ILogger<ApplicationInsightsTelemetryService> logger)
    {
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void TrackBookingCreated(string bookingId, string customerEmail, string architectureMode, bool eventDrivenEnabled)
    {
        try
        {
            _telemetryClient.TrackEvent("BookingCreated", new Dictionary<string, string>
            {
                { "BookingId", bookingId },
                { "CustomerEmail", customerEmail },
                { "ArchitectureMode", architectureMode },
                { "EventDrivenEnabled", eventDrivenEnabled.ToString() },
                { "SystemType", eventDrivenEnabled ? "Event-Driven" : "Synchronous" }
            });

            // Flush to ensure event is sent immediately (especially important for demos)
            _telemetryClient.Flush();

            _logger.LogInformation("Tracked BookingCreated event: {BookingId}, Mode: {ArchitectureMode}", bookingId, architectureMode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track BookingCreated event: {BookingId}", bookingId);
        }
    }

    public void TrackOutboxEventCreated(string outboxEventId, string bookingId, string eventType, string architectureMode)
    {
        try
        {
            _telemetryClient.TrackEvent("OutboxEventCreated", new Dictionary<string, string>
            {
                { "OutboxEventId", outboxEventId },
                { "BookingId", bookingId },
                { "EventType", eventType },
                { "ArchitectureMode", architectureMode },
                { "Status", "Pending" }
            });

            _telemetryClient.Flush();

            _logger.LogInformation("Tracked OutboxEventCreated: {OutboxEventId}, Booking: {BookingId}", outboxEventId, bookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track OutboxEventCreated: {OutboxEventId}", outboxEventId);
        }
    }

    public void TrackOutboxEventProcessed(string outboxEventId, string eventType, TimeSpan processingTime)
    {
        _telemetryClient.TrackEvent("OutboxEventProcessed", new Dictionary<string, string>
        {
            { "OutboxEventId", outboxEventId },
            { "EventType", eventType },
            { "Status", "Processed" },
            { "SystemType", "Event-Driven" }
        }, new Dictionary<string, double>
        {
            { "ProcessingTimeMs", processingTime.TotalMilliseconds }
        });

        _logger.LogDebug("Tracked OutboxEventProcessed: {OutboxEventId}, Time: {ProcessingTime}ms", 
            outboxEventId, processingTime.TotalMilliseconds);
    }

    public void TrackServiceBusEventPublished(string eventId, string eventType, string queueName)
    {
        _telemetryClient.TrackEvent("ServiceBusEventPublished", new Dictionary<string, string>
        {
            { "EventId", eventId },
            { "EventType", eventType },
            { "QueueName", queueName },
            { "SystemType", "Event-Driven" }
        });

        _logger.LogDebug("Tracked ServiceBusEventPublished: {EventId}, Type: {EventType}", eventId, eventType);
    }

    public void TrackFeatureFlagToggled(bool previousValue, bool newValue, string userId)
    {
        var fromMode = previousValue ? "Event-Driven" : "Synchronous";
        var toMode = newValue ? "Event-Driven" : "Synchronous";

        _telemetryClient.TrackEvent("FeatureFlagToggled", new Dictionary<string, string>
        {
            { "FeatureFlag", "BookingEvents_Enabled" },
            { "PreviousValue", previousValue.ToString() },
            { "NewValue", newValue.ToString() },
            { "FromMode", fromMode },
            { "ToMode", toMode },
            { "UserId", userId ?? "Unknown" }
        });

        _logger.LogInformation("Tracked FeatureFlagToggled: {FromMode} → {ToMode} by {UserId}", fromMode, toMode, userId);
    }

    public void TrackModeSwitch(string fromMode, string toMode, string userId)
    {
        _telemetryClient.TrackEvent("ModeSwitch", new Dictionary<string, string>
        {
            { "FromMode", fromMode },
            { "ToMode", toMode },
            { "UserId", userId ?? "Unknown" },
            { "SystemType", toMode }
        });

        _logger.LogInformation("Tracked ModeSwitch: {FromMode} → {ToMode} by {UserId}", fromMode, toMode, userId);
    }
}

