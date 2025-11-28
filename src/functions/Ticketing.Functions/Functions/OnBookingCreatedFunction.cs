using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Ticketing.Contracts.Events;

namespace Ticketing.Functions.Functions;

public class OnBookingCreatedFunction
{
    private readonly ILogger<OnBookingCreatedFunction> _logger;

    public OnBookingCreatedFunction(ILogger<OnBookingCreatedFunction> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("OnBookingCreated")]
    public async Task Run(
        [ServiceBusTrigger("booking-events", Connection = "AzureWebJobsServiceBus")] 
        string messageBody,
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Received BookingCreated event from Service Bus");

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var bookingCreated = JsonSerializer.Deserialize<BookingCreated>(messageBody, options);

            if (bookingCreated == null)
            {
                _logger.LogError("Failed to deserialize BookingCreated event from message body");
                throw new InvalidOperationException("Failed to deserialize BookingCreated event");
            }

            _logger.LogInformation(
                "Processing BookingCreated event: BookingId={BookingId}, CustomerId={CustomerId}, CustomerEmail={CustomerEmail}, TotalPrice={TotalPrice}",
                bookingCreated.BookingId,
                bookingCreated.CustomerId,
                bookingCreated.CustomerEmail,
                bookingCreated.TotalPrice);

            _logger.LogInformation(
                "BookingCreated event processed successfully: BookingId={BookingId}, EventId={EventId}, Timestamp={Timestamp}",
                bookingCreated.BookingId,
                bookingCreated.Id,
                bookingCreated.Timestamp);

            await Task.CompletedTask;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error processing BookingCreated event: {ErrorMessage}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BookingCreated event: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}

