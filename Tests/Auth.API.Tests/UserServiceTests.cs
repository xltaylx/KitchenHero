using AuthAPI.Models;
using Data.Access.Repositories;
using Data.Models;
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
        private IUserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _userService = new UserService(_userRepositoryMock.Object, _tokenServiceMock.Object, null); // Assuming no configuration injection for simplicity
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUserAndReturnUserWithTokens()
        {
            // Arrange
            var user = new User { Email = "test@example.com"};
            var expectedAccessToken = "valid_access_token";
            var expectedRefreshToken = "valid_refresh_token";
            _tokenServiceMock.Setup(s => s.GenerateAccessTokenAsync(user)).Returns(Task.FromResult(expectedAccessToken));
            _tokenServiceMock.Setup(s => s.GenerateRefreshTokenAsync(user)).Returns(Task.FromResult(expectedRefreshToken));
            _userRepositoryMock.Setup(r => r.CreateUserAsync(user)).Returns(Task.CompletedTask);

            // Act
            var userWithTokens = await _userService.CreateUserAsync(user, "password");

            // Assert
            Assert.NotNull(userWithTokens);
            Assert.Equal(user, userWithTokens.User);
            Assert.Equal(expectedAccessToken, userWithTokens.AccessToken);
            Assert.Equal(expectedRefreshToken, userWithTokens.RefreshToken);
            _userRepositoryMock.Verify(r => r.CreateUserAsync(user), Times.Once);
            _tokenServiceMock.Verify(s => s.GenerateAccessTokenAsync(user), Times.Once);
            _tokenServiceMock.Verify(s => s.GenerateRefreshTokenAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new User { Email = email };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(email)).Returns(Task.FromResult(expectedUser));

            // Act
            var user = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(expectedUser, user);
            _userRepositoryMock.Verify(r => r.GetUserByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(email)).Returns(Task.FromResult<User>(null));

            // Act
            var user = await _userService.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(user);
            _userRepositoryMock.Verify(r => r.GetUserByEmailAsync(email), Times.Once);
        }

        // Add additional tests for other UserService methods...
    }

}
