using Data.Access.Repositories;
using Data.Models;
using Data.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Access.Tests
{
    public class UserRepositoryTests
    {
        private readonly Mock<IDbContext> _mockContext;
        private readonly Mock<ILogger<UserRepository>> _mockLogger;

        public UserRepositoryTests()
        {
            _mockContext = new Mock<IDbContext>();
            _mockLogger = new Mock<ILogger<UserRepository>>();

        }
        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var expectedUserId = 1;
            var expectedUser = new User { Id = expectedUserId };

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(expectedUserId))
                 .ReturnsAsync(expectedUser);
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(() => mockUsers.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);


            // Act
            var user = await userRepository.GetUserByIdAsync(expectedUserId);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(expectedUserId, user.Id);
        }


        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userIdNotExist = 100; // Assuming user with this ID doesn't exist

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users.FindAsync(userIdNotExist))
                       .ReturnsAsync((User)null); // Simulating user not found

            var mockLogger = new Mock<ILogger>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByIdAsync(userIdNotExist);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByIdAsync_ThrowsException_WhenExceptionOccurs()
        {
            // Arrange
            var expectedUserId = 1;

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users.FindAsync(expectedUserId))
                       .ThrowsAsync(new Exception("Simulated exception"));

            var mockLogger = new Mock<ILogger>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => await userRepository.GetUserByIdAsync(expectedUserId));
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var userEmail = "test@example.com";
            var expectedUser = new User { Email = userEmail };

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(userEmail)).ReturnsAsync(expectedUser);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByEmailAsync(userEmail);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(userEmail, user.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userEmail = "nonexistent@example.com";

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(userEmail)).ReturnsAsync((User)null);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByEmailAsync(userEmail);

            // Assert
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByEmailAsync_LogsUserFound_WhenUserExists()
        {
            // Arrange
            var userEmail = "test@example.com";
            var expectedUser = new User { Email = userEmail };

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(userEmail)).ReturnsAsync(expectedUser);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            await userRepository.GetUserByEmailAsync(userEmail);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"User found with email: {userEmail}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetUserByEmailAsync_LogsUserNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userEmail = "nonexistent@example.com";

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(userEmail)).ReturnsAsync((User)null);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            await userRepository.GetUserByEmailAsync(userEmail);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"User not found with email: {userEmail}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
        [Fact]
        public async Task UpdateUserAsync_SavesChanges()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };

            var mockUsers = new Mock<DbSet<User>>();
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object); // Provide the mock Users DbSet
            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, mockLogger.Object);

            // Act
            await userRepository.UpdateUserAsync(user);

            // Assert
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once); // Verify SaveChangesAsync is called
            mockUsers.Verify(x => x.Update(user), Times.Once); // Verify Update is called on the Users DbSet with the provided user
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsUser_WhenUseHashedRefreshTokenIsTrue()
        {
            // Arrange
            var refreshToken = "hashedRefreshToken";
            var expectedUser = new User { RefreshToken = refreshToken };

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(refreshToken)).ReturnsAsync(expectedUser);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken, useHashedRefreshToken: true);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(refreshToken, user.RefreshToken);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsUser_WhenUseHashedRefreshTokenIsFalse()
        {
            // Arrange
            var refreshToken = "plainTextRefreshToken";
            var expectedUser = new User { RefreshToken = refreshToken };

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(refreshToken)).ReturnsAsync(expectedUser);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken, useHashedRefreshToken: false);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(refreshToken, user.RefreshToken);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var refreshToken = "nonexistentRefreshToken";

            var mockUsers = new Mock<DbSet<User>>();
            mockUsers.Setup(x => x.FindAsync(refreshToken)).ReturnsAsync((User)null);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var user = await userRepository.GetUserByRefreshTokenAsync(refreshToken, useHashedRefreshToken: true);

            // Assert
            Assert.Null(user);
        }
        [Fact]
        public async Task CreateUserAsync_AddsUserToContextAndSavesChanges()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };

            var mockDbSet = new Mock<DbSet<User>>();
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, mockLogger.Object);

            // Act
            await userRepository.CreateUserAsync(user);

            // Assert
            mockDbSet.Verify(x => x.AddAsync(user, default), Times.Once); // Verify user is added to DbSet
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once); // Verify SaveChangesAsync is called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Adding new user") && v.ToString().Contains(user.Email)),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            ); // Verify logging of adding user
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User added successfully") && v.ToString().Contains(user.Email)),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            ); // Verify logging of successful user addition
        }
        [Fact]
        public async Task CreateUserAsync_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            User user = null;

            var mockContext = new Mock<IDbContext>();
            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => userRepository.CreateUserAsync(user));
        }
        [Fact]
        public async Task CreateUserAsync_SaveChangesAsyncThrowsException_LogsError()
        {
            // Arrange
            var user = new User { Email = "test@example.com" };

            var mockDbSet = new Mock<DbSet<User>>();
            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

            var exceptionMessage = "Error saving changes";
            mockContext.Setup(x => x.SaveChangesAsync(default))
                       .ThrowsAsync(new InvalidOperationException(exceptionMessage));

            var mockLogger = new Mock<ILogger<UserRepository>>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => userRepository.CreateUserAsync(user));
        }
        [Fact]
        public async Task DeleteUserAsync_UserExists_DeletesUser()
        {
            // Arrange
            var userId = 1;
            var user = new User { Id = userId };

            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.Setup(x => x.FindAsync(userId)).ReturnsAsync(user);

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

            var loggerMessages = new List<string>();

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            await userRepository.DeleteUserAsync(userId);

            // Assert
            mockDbSet.Verify(x => x.FindAsync(userId), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            // Arrange
            var users = new List<User>
        {
            new User { Id = 1, Email = "user1@example.com" },
            new User { Id = 2, Email = "user2@example.com" },
            new User { Id = 3, Email = "user3@example.com" }
        };

            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.As<IAsyncEnumerable<User>>()
                     .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                     .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

            var mockContext = new Mock<IDbContext>();
            mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

            var userRepository = new UserRepository(mockContext.Object, _mockLogger.Object);

            // Act
            var result = await userRepository.GetAllUsersAsync();

            // Assert
            Assert.Equal(users.Count, result.Count());
            Assert.Equal(users.Select(u => u.Id), result.Select(u => u.Id));
            Assert.Equal(users.Select(u => u.Email), result.Select(u => u.Email));
        }
    }
    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}
