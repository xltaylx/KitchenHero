using AuthAPI.Models;
using Data.Models;
using Meziantou.Extensions.Logging.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.API.Tests
{
    public class TokenServiceTests
    {
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<TokenService>> _loggerMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IUserService> _userServiceMock;
        private ITokenService _tokenService;

        public TokenServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _tokenServiceMock = new Mock<ITokenService>(); 
            _userServiceMock = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<TokenService>>();
            _loggerMock = new Mock<ILogger<TokenService>>();
            _tokenService = new TokenService(_configurationMock.Object, _loggerMock.Object, _userServiceMock.Object);

        }
        [Fact]
        public async Task GenerateAccessTokenAsync_ShouldReturnValidJwtToken_WhenUserIsValid()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            var expectedIssuer = "AuthAPI";
            var expectedAudience = "KitchenHeroBlazor";
            var expectedSigningKey = "jMwYL6FZDDqGJv20d&7@S1kZKqe$eeOhm?xQV9h5GAc2"; // Replace with a valid secret key
            var expectedAccessTokenExpirationTimeInMinutes = "120";

            // Mock dependencies (assuming you use Moq)
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(expectedIssuer);
            mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(expectedAudience);
            mockConfiguration.Setup(c => c["Jwt:SecretKey"]).Returns(expectedSigningKey);
            mockConfiguration.Setup(c => c["AccessTokenExpirationTimeInMinutes"]).Returns(expectedAccessTokenExpirationTimeInMinutes);

            var mockTokenService = new Mock<ITokenService>();
            mockTokenService.Setup(s => s.GenerateAccessTokenAsync(user))
                            .ReturnsAsync("test_token"); // Or use Callback for more control

            var mockLogger = new Mock<ILogger<TokenService>>(); // Optional for logging

            // Act
            var tokenService = new TokenService(mockConfiguration.Object, mockLogger.Object, _userServiceMock.Object);
            var tokenTask = tokenService.GenerateAccessTokenAsync(user);
            var token = await tokenTask;

            // Assert
            Assert.NotNull(token);

            // Validate basic JWT structure using JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            Assert.NotNull(securityToken);

            // Assert specific claims (issuer, audience, expiry, etc.)
            var claims = securityToken.Claims;
            var issuerClaim = claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss);
            var audienceClaim = claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud);
            var expiryClaim = claims.SingleOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            Assert.Equal(expectedIssuer, issuerClaim?.Value);
            Assert.Equal(expectedAudience, audienceClaim?.Value);

            if (expiryClaim == null)
            {
                Assert.Fail("Expiry claim not found in JWT token");
            }
            else
            {
                long expiryTimestamp = long.Parse(expiryClaim?.Value); // Assuming it's a string representation of a long
                long utcTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                Assert.True(expiryTimestamp > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            }
        }


        [Fact]
        public async Task GenerateAccessTokenAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            // Act
           await Assert.ThrowsAsync<ArgumentNullException>(async () => await _tokenService.GenerateAccessTokenAsync(null));
        }

        [Fact]
        public async void GenerateRefreshTokenAsync_ShouldReturnValidRefreshToken_WhenUserIsValid()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            var expectedRefreshTokenLength = 44; // Assuming SHA256 hash and Base64 encoding

            var expectedRefreshTokenExpiration = DateTime.UtcNow.AddDays(30); // Based on your expiration logic
            _userServiceMock.Setup(us => us.GetUserByIdAsync(user.Id)).Returns(Task.FromResult(user)); // Mock user retrieval

            // Act
            var token = await _tokenService.GenerateRefreshTokenAsync(user);

            // Assert
            Assert.NotNull(token);
            Assert.Equal(expectedRefreshTokenLength, token.Length);

            // Verify user was updated with expiration (assuming UserService updates user)
            _userServiceMock.Verify(
                us => us.UpdateUserAsync(It.Is<User>(u => u.RefreshTokenExpiration.HasValue)),
                Times.Once);
        }


        [Fact]
        public async Task GenerateRefreshTokenAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _tokenService.GenerateRefreshTokenAsync(null));
        }
    }
}
