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
        private Mock<IUserService> _userServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ILogger<TokenService>> _loggerMock;
        private ITokenService _tokenService;

        public TokenServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<TokenService>>();
            _tokenService = new TokenService(_configurationMock.Object, _loggerMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ValidUser_ReturnsValidJwtToken()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            var expectedIssuer = "AuthAPI";
            var expectedAudience = "KitchenHeroBlazor";
            var expectedSigningKey = "jMwYL6FZDDqGJv20d&7@S1kZKqe$eeOhm?xQV9h5GAc2";
            var expectedAccessTokenExpirationTimeInMinutes = "120";

            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns(expectedIssuer);
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns(expectedAudience);
            _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns(expectedSigningKey);
            _configurationMock.Setup(c => c["AccessTokenExpirationTimeInMinutes"]).Returns(expectedAccessTokenExpirationTimeInMinutes);

            // Act
            var token = await _tokenService.GenerateAccessTokenAsync(user);

            // Assert
            Assert.NotNull(token);
            // Additional assertions for token structure, claims, etc.
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _tokenService.GenerateAccessTokenAsync(null));
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_ValidUser_ReturnsValidRefreshToken()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@example.com" };
            var expectedRefreshTokenLength = 44; // Assuming SHA256 hash and Base64 encoding

            _userServiceMock.Setup(us => us.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

            // Assert
            Assert.NotNull(refreshToken);
            Assert.Equal(expectedRefreshTokenLength, refreshToken.Length);
            _userServiceMock.Verify(us => us.UpdateUserAsync(It.Is<User>(u => u.RefreshTokenExpiration.HasValue)), Times.Once);
        }

        [Fact]
        public async Task GenerateRefreshTokenAsync_NullUser_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _tokenService.GenerateRefreshTokenAsync(null));
        }

        // Additional test methods for other scenarios can be added here
    }

}
