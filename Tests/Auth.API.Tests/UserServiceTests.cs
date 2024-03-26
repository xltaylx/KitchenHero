using AuthAPI.Models;
using Data.Access.Repositories;
using Data.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.API.Tests
{
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IConfiguration> _configurationMock;
        private IUserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _configurationMock = new Mock<IConfiguration>();
            _userService = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object, _configurationMock.Object); // Assuming no configuration injection for simplicity
        }
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = 1;

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userEmail = "test@example.com";
            var expectedUser = new User { Email = userEmail };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(userEmail)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(userEmail);

            // Assert
            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userEmail = "nonexistent@example.com";

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(userEmail)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByEmailAsync(userEmail);

            // Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var refreshToken = "someRefreshToken";
            var expectedUser = new User { RefreshToken = refreshToken };

            // Load test-specific configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var userService = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object, configuration);

            // Mock the repository to return the expected user
            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshToken, It.IsAny<bool>())).ReturnsAsync(expectedUser);

            // Act
            var result = await userService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var refreshToken = "nonexistentRefreshToken";

            // Load test-specific configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var userService = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object, configuration);

            // Mock the repository to return null
            _userRepositoryMock.Setup(repo => repo.GetUserByRefreshTokenAsync(refreshToken, It.IsAny<bool>())).ReturnsAsync((User)null);

            // Act
            var result = await userService.GetUserByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.Null(result);
        }


    }
}
