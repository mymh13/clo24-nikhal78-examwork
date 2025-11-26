using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Ticketing.Contracts.Events;
using Ticketing.Contracts.Outbox;

namespace Ticketing.Web.Services;

public class OutboxService : IOutboxService
{
    private readonly CosmosClient _cosmosClient;
    private const string DatabaseName = "ticketing";
    private const string ContainerName = "outbox";
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(CosmosClient cosmosClient, ILogger<OutboxService> logger)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OutboxEvent> AddEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : Event
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var eventJson = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid().ToString(),
            EventType = typeof(T).Name,
            EventData = eventJson,
            Status = OutboxEventStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var response = await container.CreateItemAsync(
            outboxEvent,
            new PartitionKey(outboxEvent.PartitionKey),
            cancellationToken: cancellationToken);

        _logger.LogInformation("Outbox event created: {EventId} of type {EventType} with status {Status}",
            outboxEvent.Id, outboxEvent.EventType, outboxEvent.Status);

        return response.Resource;
    }

    public async Task<IEnumerable<OutboxEvent>> GetPendingEventsAsync(CancellationToken cancellationToken = default)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.status = @status ORDER BY c.createdAt")
            .WithParameter("@status", OutboxEventStatus.Pending.ToString());

        var iterator = container.GetItemQueryIterator<OutboxEvent>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(OutboxEventStatus.Pending.ToString())
            });

        var events = new List<OutboxEvent>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            events.AddRange(response);
        }

        return events;
    }

    public async Task MarkAsProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(eventId))
            throw new ArgumentException("EventId is required", nameof(eventId));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var readResponse = await container.ReadItemAsync<OutboxEvent>(
            eventId,
            new PartitionKey(OutboxEventStatus.Pending.ToString()),
            cancellationToken: cancellationToken);

        var outboxEvent = readResponse.Resource;

        outboxEvent.Status = OutboxEventStatus.Processed;
        outboxEvent.ProcessedAt = DateTime.UtcNow;

        // Cosmos DB doesn't allow changing partition key - must delete and recreate
        await container.DeleteItemAsync<OutboxEvent>(
            eventId,
            new PartitionKey(OutboxEventStatus.Pending.ToString()),
            cancellationToken: cancellationToken);

        await container.CreateItemAsync(
            outboxEvent,
            new PartitionKey(outboxEvent.PartitionKey), // Now uses "Processed" as partition key
            cancellationToken: cancellationToken);

        _logger.LogInformation("Outbox event marked as processed: {EventId}", eventId);
    }
}

