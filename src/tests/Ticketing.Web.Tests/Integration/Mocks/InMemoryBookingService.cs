using System.Collections.Concurrent;
using Ticketing.Contracts.Bookings;
using Ticketing.Web.Services;

namespace Ticketing.Web.Tests.Integration.Mocks;

// In-memory implementation of IBookingService for testing.
// Stores bookings in memory instead of Cosmos DB.
public class InMemoryBookingService : IBookingService
{
    private readonly InMemoryStorage _storage;

    public InMemoryBookingService(InMemoryStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking == null)
            throw new ArgumentNullException(nameof(booking));

        if (string.IsNullOrEmpty(booking.Id))
        {
            booking.Id = Guid.NewGuid().ToString();
        }

        if (booking.BookingDate == default)
        {
            booking.BookingDate = DateTime.UtcNow;
        }

        // Create a copy to avoid external modifications
        var bookingCopy = new Booking
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            CustomerEmail = booking.CustomerEmail,
            CustomerName = booking.CustomerName,
            Zone = booking.Zone,
            Region = booking.Region,
            BasePrice = booking.BasePrice,
            PriceModifier = booking.PriceModifier,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingDate = booking.BookingDate,
            ActivatedAt = booking.ActivatedAt,
            ValidFrom = booking.ValidFrom,
            ValidTo = booking.ValidTo,
            QrCodeData = booking.QrCodeData
        };

        _storage.Bookings[booking.Id] = bookingCopy;
        return Task.FromResult(bookingCopy);
    }

    public Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(customerId))
            return Task.FromResult(Enumerable.Empty<Booking>());

        var bookings = _storage.Bookings.Values
            .Where(b => b.CustomerId == customerId)
            .ToList();

        return Task.FromResult<IEnumerable<Booking>>(bookings);
    }

    public Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Booking>>(_storage.Bookings.Values.ToList());
    }

    public Task<Booking?> GetBookingByIdAsync(string bookingId, string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bookingId))
            return Task.FromResult<Booking?>(null);

        if (_storage.Bookings.TryGetValue(bookingId, out var booking))
        {
            // Verify customer ID matches if provided
            if (!string.IsNullOrEmpty(customerId) && booking.CustomerId != customerId)
            {
                return Task.FromResult<Booking?>(null);
            }
            return Task.FromResult<Booking?>(booking);
        }

        return Task.FromResult<Booking?>(null);
    }

    public Task<Booking> UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        if (booking == null)
            throw new ArgumentNullException(nameof(booking));

        if (string.IsNullOrEmpty(booking.Id))
            throw new ArgumentException("Booking Id is required", nameof(booking));

        if (!_storage.Bookings.ContainsKey(booking.Id))
        {
            throw new InvalidOperationException($"Booking with id {booking.Id} not found");
        }

        // Create updated copy
        var updatedBooking = new Booking
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            CustomerEmail = booking.CustomerEmail,
            CustomerName = booking.CustomerName,
            Zone = booking.Zone,
            Region = booking.Region,
            BasePrice = booking.BasePrice,
            PriceModifier = booking.PriceModifier,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            BookingDate = booking.BookingDate,
            ActivatedAt = booking.ActivatedAt,
            ValidFrom = booking.ValidFrom,
            ValidTo = booking.ValidTo,
            QrCodeData = booking.QrCodeData
        };

        _storage.Bookings[booking.Id] = updatedBooking;
        return Task.FromResult(updatedBooking);
    }

    public Task DeleteBookingAsync(string bookingId, string customerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(bookingId))
            return Task.CompletedTask;

        if (_storage.Bookings.TryGetValue(bookingId, out var booking))
        {
            // Verify customer ID matches if provided
            if (!string.IsNullOrEmpty(customerId) && booking.CustomerId != customerId)
            {
                throw new InvalidOperationException($"Booking {bookingId} does not belong to customer {customerId}");
            }

            _storage.Bookings.TryRemove(bookingId, out _);
        }

        return Task.CompletedTask;
    }
}

