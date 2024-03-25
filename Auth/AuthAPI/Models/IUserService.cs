using Data.Access.Repositories;
using Data.Models;
using Data.Models.Models.Auth;

namespace AuthAPI.Models
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task UpdateUserAsync(User user);
        Task<UserWithTokens?> CreateUserAsync(User user, string password);  // New method
    }
}
