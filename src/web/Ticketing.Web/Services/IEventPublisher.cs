using Ticketing.Contracts.Events;

namespace Ticketing.Web.Services;

public interface IEventPublisher
{
    Task PublishEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : Event;
}

