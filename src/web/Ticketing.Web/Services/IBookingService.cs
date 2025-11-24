using Ticketing.Contracts.Bookings;

namespace Ticketing.Web.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken = default);
}

