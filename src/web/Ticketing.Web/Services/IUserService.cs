using Ticketing.Contracts.Users;

namespace Ticketing.Web.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}

