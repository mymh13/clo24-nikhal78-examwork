namespace Ticketing.Web.Services;

public interface ITelemetryService
{
    void TrackBookingCreated(string bookingId, string customerEmail, string architectureMode, bool eventDrivenEnabled);
    void TrackOutboxEventCreated(string outboxEventId, string bookingId, string eventType, string architectureMode);
    void TrackOutboxEventProcessed(string outboxEventId, string eventType, TimeSpan processingTime);
    void TrackServiceBusEventPublished(string eventId, string eventType, string queueName);
    void TrackFeatureFlagToggled(bool previousValue, bool newValue, string userId);
    void TrackModeSwitch(string fromMode, string toMode, string userId);
}

