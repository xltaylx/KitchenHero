using Azure.Core;
using Data.Access.Repositories;
using Data.Models;
using Data.Models.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Models
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ITokenService tokenService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {   
            var useHashedRefreshToken = _configuration.GetValue<bool>("UseHashedRefreshToken");
            return await _userRepository.GetUserByRefreshTokenAsync(refreshToken, useHashedRefreshToken);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<UserWithTokens?> CreateUserAsync(User user, string password)
        {
            if(user != null)
            {
                user.PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt();
                user.PasswordHash = HashPassword(password, user.PasswordSalt); 
                await _userRepository.CreateUserAsync(user);

                var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

                return new UserWithTokens
                {
                    User = user,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            return null;
        }

        private string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            // Delegate refresh token generation to TokenService
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            // Update user's refresh token hash (optional, for tighter integration)
            user.RefreshTokenHash = refreshToken; // Assuming the token is already hashed in TokenService

            return refreshToken;
        }
    }
}
