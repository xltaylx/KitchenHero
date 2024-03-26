using Data.Access;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


namespace Data.Access.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                // Retrieve the user with their refresh token expiration
                var user = await _context.Users
                                   .FindAsync(userId);

                // Return the user if found
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve user by ID: {UserId}, Exception: {ExceptionMessage}", userId, ex.Message);
                throw; // Re-throw to allow higher-level exception handling
            }
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("Retrieving user with email: {Email}", email);

            var user = await _context.Users.FindAsync(email);

            if (user != null)
            {
                _logger.LogInformation("User found with email: {Email}", email);
            }
            else
            {
                _logger.LogWarning("User not found with email: {Email}", email);
            }

            return user;
        }
        public async Task<User> GetUserByRefreshTokenAsync(string refreshToken, bool useHashedRefreshToken)
        {
            if (useHashedRefreshToken)
            {
                return await _context.Users.FindAsync(refreshToken) ?? null;
            }
            else
            {
                return await _context.Users.FindAsync(refreshToken) ?? null;
            }
        }
        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user with email: {email}", user.Email);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully: {email}", user.Email);
        }
        public async Task CreateUserAsync(User user)
        {
            if (user == null)
            {
                _logger.LogError("User cannot be null.");
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            _logger.LogInformation("Adding new user: {Email}", user.Email);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User added successfully: {Email}", user.Email);
        }
        public async Task DeleteUserAsync(int userId)
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", userId);

            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User deleted successfully: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("User not found for deletion: {UserId}", userId);
            }
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Retrieving all users");

            var users = await _context.Users.ToListAsync();
            _logger.LogInformation("Retrieved {UserCount} users", users.Count);

            return users;
        }
    }
}
