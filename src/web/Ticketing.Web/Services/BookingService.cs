using Microsoft.Azure.Cosmos;
using Ticketing.Contracts.Bookings;

namespace Ticketing.Web.Services;

public class BookingService : IBookingService
{
    private readonly CosmosClient _cosmosClient;
    private const string DatabaseName = "ticketing";
    private const string ContainerName = "bookings";

    public BookingService(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
    }

    public async Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking == null)
            throw new ArgumentNullException(nameof(booking));

        if (string.IsNullOrEmpty(booking.CustomerId))
            throw new ArgumentException("CustomerId is required", nameof(booking));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        
        if (string.IsNullOrWhiteSpace(booking.Id))
        {
            booking.Id = Guid.NewGuid().ToString();
        }
        
        if (booking.BookingDate == default)
        {
            booking.BookingDate = DateTime.UtcNow;
        }

        var response = await container.CreateItemAsync(
            booking,
            new PartitionKey(booking.CustomerId),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerId))
            throw new ArgumentException("CustomerId is required", nameof(customerId));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.customerId = @customerId")
            .WithParameter("@customerId", customerId);

        var iterator = container.GetItemQueryIterator<Booking>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(customerId)
            });

        var bookings = new List<Booking>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            bookings.AddRange(response);
        }

        return bookings;
    }
}

