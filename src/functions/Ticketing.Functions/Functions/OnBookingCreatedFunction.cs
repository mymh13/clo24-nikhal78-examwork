using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Ticketing.Contracts.Events;

namespace Ticketing.Functions.Functions;

public class OnBookingCreatedFunction
{
    private readonly ILogger<OnBookingCreatedFunction> _logger;
    private readonly TelemetryClient _telemetryClient;

    public OnBookingCreatedFunction(
        ILogger<OnBookingCreatedFunction> logger,
        TelemetryClient telemetryClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function("OnBookingCreated")]
    public async Task Run(
        [ServiceBusTrigger("booking-events", Connection = "AzureWebJobsServiceBus")] 
        string messageBody,
        int deliveryCount,
        DateTime enqueuedTimeUtc,
        string messageId,
        FunctionContext context)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation(
                "Received BookingCreated event from Service Bus: MessageId={MessageId}, DeliveryCount={DeliveryCount}, EnqueuedTime={EnqueuedTime}",
                messageId, deliveryCount, enqueuedTimeUtc);

            if (string.IsNullOrWhiteSpace(messageBody))
            {
                _logger.LogError("Received empty message body for BookingCreated event: MessageId={MessageId}", messageId);
                throw new ArgumentException("Message body cannot be empty", nameof(messageBody));
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            BookingCreated? bookingCreated;
            try
            {
                bookingCreated = JsonSerializer.Deserialize<BookingCreated>(messageBody, options);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx,
                    "JSON deserialization error processing BookingCreated event: MessageId={MessageId}, DeliveryCount={DeliveryCount}, MessageBody={MessageBody}",
                    messageId, deliveryCount, messageBody);
                throw new InvalidOperationException($"Failed to deserialize BookingCreated event: {jsonEx.Message}", jsonEx);
            }

            if (bookingCreated == null)
            {
                _logger.LogError("Deserialized BookingCreated event is null: MessageId={MessageId}, DeliveryCount={DeliveryCount}", messageId, deliveryCount);
                throw new InvalidOperationException("Failed to deserialize BookingCreated event: result is null");
            }

            if (string.IsNullOrEmpty(bookingCreated.BookingId))
            {
                _logger.LogWarning("BookingCreated event missing BookingId: MessageId={MessageId}, EventId={EventId}", messageId, bookingCreated.Id);
            }

            _logger.LogInformation(
                "Processing BookingCreated event: BookingId={BookingId}, CustomerId={CustomerId}, CustomerEmail={CustomerEmail}, TotalPrice={TotalPrice}, EventId={EventId}",
                bookingCreated.BookingId,
                bookingCreated.CustomerId,
                bookingCreated.CustomerEmail,
                bookingCreated.TotalPrice,
                bookingCreated.Id);

            await ProcessBookingCreatedEventAsync(bookingCreated);

            var processingTime = DateTime.UtcNow - startTime;
            
            // Track custom event for Application Insights dashboard
            _telemetryClient.TrackEvent("FunctionBookingCreatedProcessed", new Dictionary<string, string>
            {
                { "BookingId", bookingCreated.BookingId },
                { "EventId", bookingCreated.Id },
                { "CustomerEmail", bookingCreated.CustomerEmail },
                { "EventType", "BookingCreated" },
                { "SystemType", "Event-Driven" },
                { "FunctionName", "OnBookingCreated" },
                { "DeliveryCount", deliveryCount.ToString() }
            }, new Dictionary<string, double>
            {
                { "ProcessingTimeMs", processingTime.TotalMilliseconds }
            });
            
            _logger.LogInformation(
                "BookingCreated event processed successfully: BookingId={BookingId}, EventId={EventId}, ProcessingTime={ProcessingTime}ms, DeliveryCount={DeliveryCount}",
                bookingCreated.BookingId,
                bookingCreated.Id,
                processingTime.TotalMilliseconds,
                deliveryCount);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx,
                "Invalid argument error processing BookingCreated event: MessageId={MessageId}, DeliveryCount={DeliveryCount}, Error={ErrorMessage}",
                messageId, deliveryCount, argEx.Message);
            throw;
        }
        catch (InvalidOperationException invOpEx)
        {
            _logger.LogError(invOpEx,
                "Invalid operation error processing BookingCreated event: MessageId={MessageId}, DeliveryCount={DeliveryCount}, Error={ErrorMessage}",
                messageId, deliveryCount, invOpEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogError(ex,
                "Unexpected error processing BookingCreated event: MessageId={MessageId}, DeliveryCount={DeliveryCount}, ProcessingTime={ProcessingTime}ms, Error={ErrorMessage}. " +
                "After {MaxDeliveryCount} delivery attempts, message will be moved to dead letter queue.",
                messageId, deliveryCount, processingTime.TotalMilliseconds, ex.Message, 10);
            throw;
        }
    }

    private async Task ProcessBookingCreatedEventAsync(BookingCreated bookingCreated)
    {
        await Task.CompletedTask;
    }
}

