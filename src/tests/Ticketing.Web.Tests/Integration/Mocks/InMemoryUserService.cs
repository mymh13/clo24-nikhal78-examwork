using System.Collections.Concurrent;
using Ticketing.Contracts.Users;
using Ticketing.Web.Services;
using TicketingUser = Ticketing.Contracts.Users.User;

namespace Ticketing.Web.Tests.Integration.Mocks;

// In-memory implementation of IUserService for testing.
// Stores users in memory instead of Cosmos DB.
public class InMemoryUserService : IUserService
{
    private readonly InMemoryStorage _storage;

    public InMemoryUserService(InMemoryStorage storage)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    public Task<TicketingUser> CreateUserAsync(TicketingUser user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(user.Email))
            throw new ArgumentException("Email is required", nameof(user));

        // Check if user already exists
        if (_storage.UsersByEmail.ContainsKey(user.Email.ToLowerInvariant()))
        {
            throw new InvalidOperationException($"User with email {user.Email} already exists");
        }

        // Set defaults
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }

        if (user.CreatedDate == default)
        {
            user.CreatedDate = DateTime.UtcNow;
        }

        // Create a copy to avoid external modifications
        var userCopy = new TicketingUser
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            DateOfBirth = user.DateOfBirth,
            IsStudent = user.IsStudent,
            CreatedDate = user.CreatedDate
        };

        _storage.UsersByEmail[user.Email.ToLowerInvariant()] = userCopy;
        _storage.UsersById[user.Id] = userCopy;

        return Task.FromResult(userCopy);
    }

    public Task<TicketingUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            return Task.FromResult<TicketingUser?>(null);

        _storage.UsersByEmail.TryGetValue(email.ToLowerInvariant(), out var user);
        return Task.FromResult<TicketingUser?>(user);
    }

    public Task<TicketingUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult<TicketingUser?>(null);

        _storage.UsersById.TryGetValue(userId, out var user);
        return Task.FromResult<TicketingUser?>(user);
    }

    public Task<IEnumerable<TicketingUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<TicketingUser>>(_storage.UsersByEmail.Values.ToList());
    }

    public Task<TicketingUser> UpdateUserAsync(TicketingUser user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(user.Id))
            throw new ArgumentException("User Id is required", nameof(user));

        if (!_storage.UsersById.ContainsKey(user.Id))
        {
            throw new InvalidOperationException($"User with id {user.Id} not found");
        }

        // Update both dictionaries
        var existingUser = _storage.UsersById[user.Id];
        var updatedUser = new TicketingUser
        {
            Id = user.Id,
            Email = !string.IsNullOrEmpty(user.Email) ? user.Email : existingUser.Email,
            Name = user.Name ?? existingUser.Name,
            PasswordHash = !string.IsNullOrEmpty(user.PasswordHash) ? user.PasswordHash : existingUser.PasswordHash,
            Role = !string.IsNullOrEmpty(user.Role) ? user.Role : existingUser.Role,
            DateOfBirth = user.DateOfBirth ?? existingUser.DateOfBirth,
            IsStudent = user.IsStudent, // bool is not nullable, use the provided value
            CreatedDate = existingUser.CreatedDate
        };

        _storage.UsersById[user.Id] = updatedUser;
        if (!string.IsNullOrEmpty(updatedUser.Email))
        {
            _storage.UsersByEmail[updatedUser.Email.ToLowerInvariant()] = updatedUser;
        }

        return Task.FromResult(updatedUser);
    }

    public Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.CompletedTask;

        if (_storage.UsersById.TryRemove(userId, out var user) && !string.IsNullOrEmpty(user.Email))
        {
            _storage.UsersByEmail.TryRemove(user.Email.ToLowerInvariant(), out _);
        }

        return Task.CompletedTask;
    }
}

