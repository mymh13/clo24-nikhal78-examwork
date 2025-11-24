using Microsoft.Azure.Cosmos;
using Ticketing.Contracts.Users;
using BCrypt.Net;
using TicketingUser = Ticketing.Contracts.Users.User;

namespace Ticketing.Web.Services;

public class UserService : IUserService
{
    private readonly CosmosClient _cosmosClient;
    private const string DatabaseName = "ticketing";
    private const string ContainerName = "users";

    public UserService(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
    }

    public async Task<TicketingUser> CreateUserAsync(TicketingUser user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(user.Email))
            throw new ArgumentException("Email is required", nameof(user));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        
        // Check if user with this email already exists
        var existingUser = await GetUserByEmailAsync(user.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {user.Email} already exists");
        }
        
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = Guid.NewGuid().ToString();
        }
        
        if (user.CreatedDate == default)
        {
            user.CreatedDate = DateTime.UtcNow;
        }

        // Simple password hashing (for MVP - should use proper hashing in production)
        if (!string.IsNullOrEmpty(user.PasswordHash) && !user.PasswordHash.StartsWith("$2a$") && !user.PasswordHash.StartsWith("$2b$"))
        {
            // If password is not already hashed, hash it (simple hash for MVP)
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }

        var response = await container.CreateItemAsync(
            user,
            new PartitionKey(user.Email),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<TicketingUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email is required", nameof(email));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
            .WithParameter("@email", email);

        var iterator = container.GetItemQueryIterator<TicketingUser>(
            query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(email)
            });

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<TicketingUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        // Query across partitions to find user by ID
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", userId);

        var iterator = container.GetItemQueryIterator<TicketingUser>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }

        return null;
    }

    public async Task<IEnumerable<TicketingUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdDate DESC");

        var iterator = container.GetItemQueryIterator<TicketingUser>(query);

        var users = new List<TicketingUser>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            users.AddRange(response);
        }

        return users;
    }

    public async Task<TicketingUser> UpdateUserAsync(TicketingUser user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(user.Id))
            throw new ArgumentException("User Id is required", nameof(user));

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        // Get existing user to preserve partition key
        var existingUser = await GetUserByIdAsync(user.Id, cancellationToken);
        if (existingUser == null)
        {
            throw new InvalidOperationException($"User with id {user.Id} not found");
        }

        // If password is being updated and not already hashed, hash it
        if (!string.IsNullOrEmpty(user.PasswordHash) && 
            user.PasswordHash != existingUser.PasswordHash &&
            !user.PasswordHash.StartsWith("$2a$") && 
            !user.PasswordHash.StartsWith("$2b$"))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }
        else if (string.IsNullOrEmpty(user.PasswordHash))
        {
            // Keep existing password if not provided
            user.PasswordHash = existingUser.PasswordHash;
        }

        // Preserve email and created date
        user.Email = existingUser.Email;
        user.CreatedDate = existingUser.CreatedDate;

        var response = await container.ReplaceItemAsync(
            user,
            user.Id,
            new PartitionKey(user.Email),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        var userEntity = await GetUserByIdAsync(userId, cancellationToken);
        if (userEntity == null)
        {
            throw new InvalidOperationException($"User with id {userId} not found");
        }

        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

        await container.DeleteItemAsync<TicketingUser>(
            userId,
            new PartitionKey(userEntity.Email),
            cancellationToken: cancellationToken);
    }
}

