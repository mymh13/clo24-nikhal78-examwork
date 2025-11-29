using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Contracts.Events;
using Ticketing.Contracts.Outbox;

namespace Ticketing.Web.Services;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(30);
    private const int MaxRetryCount = 3;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor loop");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
        var featureFlagService = scope.ServiceProvider.GetRequiredService<IFeatureFlagService>();
        var eventPublisher = scope.ServiceProvider.GetService<IEventPublisher>();

        var isEventDrivenEnabled = await featureFlagService.IsBookingEventsEnabledAsync(cancellationToken);

        if (!isEventDrivenEnabled)
        {
            _logger.LogDebug("Event-driven mode disabled, skipping outbox processing");
            return;
        }

        if (eventPublisher == null)
        {
            _logger.LogWarning("Event publisher not available, skipping outbox processing");
            return;
        }

        var pendingEvents = await outboxService.GetPendingEventsAsync(cancellationToken);

        foreach (var outboxEvent in pendingEvents)
        {
            try
            {
                await ProcessEventAsync(outboxEvent, eventPublisher, outboxService, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox event {EventId} of type {EventType}",
                    outboxEvent.Id, outboxEvent.EventType);
            }
        }
    }

    private async Task ProcessEventAsync(
        OutboxEvent outboxEvent,
        IEventPublisher eventPublisher,
        IOutboxService outboxService,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var eventData = DeserializeEvent(outboxEvent.EventType, outboxEvent.EventData);
            
            if (eventData == null)
            {
                _logger.LogWarning("Could not deserialize event {EventId} of type {EventType}",
                    outboxEvent.Id, outboxEvent.EventType);
                return;
            }

            await eventPublisher.PublishEventAsync(eventData, cancellationToken);
            await outboxService.MarkAsProcessedAsync(outboxEvent.Id, cancellationToken);

            var processingTime = DateTime.UtcNow - startTime;
            
            // Track outbox event processed with processing time
            var telemetryService = _serviceProvider.CreateScope().ServiceProvider.GetService<ITelemetryService>();
            telemetryService?.TrackOutboxEventProcessed(outboxEvent.Id, outboxEvent.EventType, processingTime);

            _logger.LogInformation(
                "Successfully published and processed outbox event {EventId} of type {EventType} in {ProcessingTime}ms",
                outboxEvent.Id, outboxEvent.EventType, processingTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing outbox event {EventId} of type {EventType}: {ErrorMessage}",
                outboxEvent.Id, outboxEvent.EventType, ex.Message);

            if (outboxEvent.RetryCount >= MaxRetryCount)
            {
                _logger.LogWarning(
                    "Outbox event {EventId} exceeded max retry count ({MaxRetries}), marking as failed",
                    outboxEvent.Id, MaxRetryCount);
            }
        }
    }

    private Event? DeserializeEvent(string eventType, string eventDataJson)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return eventType switch
            {
                nameof(BookingCreated) => JsonSerializer.Deserialize<BookingCreated>(eventDataJson, options),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize event of type {EventType}", eventType);
            return null;
        }
    }
}

