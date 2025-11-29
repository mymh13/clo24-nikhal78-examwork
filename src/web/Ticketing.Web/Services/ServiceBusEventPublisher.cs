using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Ticketing.Contracts.Events;

namespace Ticketing.Web.Services;

public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;
    private readonly ITelemetryService? _telemetryService;
    private readonly ILogger<ServiceBusEventPublisher> _logger;

    public ServiceBusEventPublisher(
        ServiceBusClient serviceBusClient,
        IConfiguration configuration,
        ITelemetryService? telemetryService,
        ILogger<ServiceBusEventPublisher> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryService = telemetryService;
        
        _queueName = configuration["ServiceBus:QueueName"] 
            ?? configuration["ServiceBus--QueueName"] 
            ?? "booking-events";
    }

    public async Task PublishEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : Event
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        try
        {
            await using var sender = _serviceBusClient.CreateSender(_queueName);

            var eventJson = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var message = new ServiceBusMessage(eventJson)
            {
                ContentType = "application/json",
                Subject = eventData.EventType,
                MessageId = eventData.Id,
                CorrelationId = eventData.Id
            };

            await sender.SendMessageAsync(message, cancellationToken);

            // Track Service Bus event published
            _telemetryService?.TrackServiceBusEventPublished(eventData.Id, eventData.EventType, _queueName);

            _logger.LogInformation(
                "Event published to Service Bus: {EventType} with ID {EventId} to queue {QueueName}",
                eventData.EventType, eventData.Id, _queueName);
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(ex,
                "Service Bus error publishing event {EventType} with ID {EventId}: {ErrorMessage}",
                eventData.EventType, eventData.Id, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error publishing event {EventType} with ID {EventId}: {ErrorMessage}",
                eventData.EventType, eventData.Id, ex.Message);
            throw;
        }
    }
}

