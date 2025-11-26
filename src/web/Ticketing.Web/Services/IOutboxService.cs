using Ticketing.Contracts.Outbox;

namespace Ticketing.Web.Services;

public interface IOutboxService
{
    Task<OutboxEvent> AddEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : Contracts.Events.Event;

    Task<IEnumerable<OutboxEvent>> GetPendingEventsAsync(CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default);
}

