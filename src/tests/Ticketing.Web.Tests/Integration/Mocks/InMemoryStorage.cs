using System.Collections.Concurrent;
using Ticketing.Contracts.Bookings;
using Ticketing.Contracts.Outbox;
using Ticketing.Contracts.Users;
using TicketingUser = Ticketing.Contracts.Users.User;

namespace Ticketing.Web.Tests.Integration.Mocks;

// Shared in-memory storage for test services.
// This is a singleton that persists data across HTTP requests in tests.
public class InMemoryStorage
{
    public ConcurrentDictionary<string, TicketingUser> UsersByEmail { get; } = new();
    public ConcurrentDictionary<string, TicketingUser> UsersById { get; } = new();
    public ConcurrentDictionary<string, Booking> Bookings { get; } = new();
    public ConcurrentDictionary<string, OutboxEvent> OutboxEvents { get; } = new();

    public void Clear()
    {
        UsersByEmail.Clear();
        UsersById.Clear();
        Bookings.Clear();
        OutboxEvents.Clear();
    }
}

