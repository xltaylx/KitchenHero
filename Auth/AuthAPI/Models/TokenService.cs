using Data.Access.Repositories;
using Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthAPI.Models
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;
        private readonly IUserService _userService;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger,IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            if (user == null)
            {
                _logger.LogError("Attempted to generate access token with null user");
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            _logger.LogInformation("Generating access token for user: {Email}", user.Email);

            // JWT access token generation logic
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["AccessTokenExpirationTimeInMinutes"]));
            var notBefore = expires.Subtract(TimeSpan.FromSeconds(10)); // Adjust seconds as needed

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // Add other relevant user claims
        }),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Expires = expires,
                NotBefore = notBefore,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            try
            {
                var token = tokenHandler.CreateToken(tokenDescriptor);
                _logger.LogInformation("Access token generated successfully for user: {Email}", user.Email);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate access token for user: {Email}, Exception: {errorMessage}", user.Email, ex.Message);
                throw;
            }
        }


        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                }
                // Generate a secure refresh token
                var randomNumber = RandomNumberGenerator.GetBytes(32);
                var refreshToken = Convert.ToBase64String(randomNumber);

                // Set appropriate expiration time (adjust as needed)
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(30);

                // Hash the refresh token before storing (replace HashRefreshToken with your implementation)
                user.RefreshTokenHash = HashRefreshToken(refreshToken);

                // Update user's RefreshTokenExpiration property
                user.RefreshTokenExpiration = refreshTokenExpiration;

                // Save changes to the database
                await _userService.UpdateUserAsync(user);

                _logger.LogInformation("Generated refresh token for user {userId} with expiration {expiration}", user.Id, refreshTokenExpiration);

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to generate refresh token: {errorMessage}", ex.Message);
                throw; // Re-throw to allow higher-level exception handling
            }
        }


        private string HashRefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            // Use BCrypt for secure hashing
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12); // Adjust rounds as needed
            return BCrypt.Net.BCrypt.HashPassword(refreshToken, salt);
        }

        public async Task<bool> ValidateAccessTokenAsync(string accessToken) // Optional, for future token validation
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero // Consider setting a small clock skew for tolerance
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (SecurityTokenException ex)
            {
                // Log or handle specific token validation exceptions (e.g., expired token, invalid signature)
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return false;
            }
        }
    }

}
