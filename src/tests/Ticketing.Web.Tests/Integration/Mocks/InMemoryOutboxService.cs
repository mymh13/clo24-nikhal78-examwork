using System.Collections.Concurrent;
using Ticketing.Contracts.Events;
using Ticketing.Contracts.Outbox;
using Ticketing.Web.Services;
using OutboxEventStatus = Ticketing.Contracts.Outbox.OutboxEventStatus;

namespace Ticketing.Web.Tests.Integration.Mocks;

// In-memory implementation of IOutboxService for testing.
// Stores outbox events in memory instead of Cosmos DB.
public class InMemoryOutboxService : IOutboxService
{
    private readonly InMemoryStorage _storage;

    public InMemoryOutboxService(InMemoryStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public Task<OutboxEvent> AddEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : Event
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid().ToString(),
            EventType = typeof(T).Name,
            EventData = System.Text.Json.JsonSerializer.Serialize(eventData),
            Status = OutboxEventStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _storage.OutboxEvents[outboxEvent.Id] = outboxEvent;
        return Task.FromResult(outboxEvent);
    }

    public Task<IEnumerable<OutboxEvent>> GetPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        var pendingEvents = _storage.OutboxEvents.Values
            .Where(e => e.Status == OutboxEventStatus.Pending)
            .ToList();

        return Task.FromResult<IEnumerable<OutboxEvent>>(pendingEvents);
    }

    public Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return Task.CompletedTask;

        if (_storage.OutboxEvents.TryGetValue(eventId, out var outboxEvent))
        {
            var updatedEvent = new OutboxEvent
            {
                Id = outboxEvent.Id,
                EventType = outboxEvent.EventType,
                EventData = outboxEvent.EventData,
                Status = OutboxEventStatus.Processed,
                CreatedAt = outboxEvent.CreatedAt,
                ProcessedAt = DateTime.UtcNow,
                RetryCount = outboxEvent.RetryCount,
                ErrorMessage = outboxEvent.ErrorMessage
            };

            _storage.OutboxEvents[eventId] = updatedEvent;
        }

        return Task.CompletedTask;
    }
}

