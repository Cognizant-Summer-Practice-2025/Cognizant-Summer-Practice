using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.User.Request;
using backend_user.tests.Helpers;
using Microsoft.Extensions.Logging;

namespace backend_user.tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithValidRepository_ShouldCreateInstance()
        {
            // Act
            var service = new UserService(_mockUserRepository.Object, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_WithEmptyGuid_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByIdAsync(emptyId));
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = TestDataFactory.CreateValidUser();
            expectedUser.Id = userId;

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
            _mockUserRepository.Verify(x => x.GetUserById(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.GetUserById(userId), Times.Once);
        }

        #endregion

        #region GetUserByEmailAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetUserByEmailAsync_WithNullOrEmptyEmail_ShouldThrowArgumentException(string email)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByEmailAsync(email));
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = TestDataFactory.CreateValidUser(email: email);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
            result!.Email.Should().Be(email);
            _mockUserRepository.Verify(x => x.GetUserByEmail(email), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.GetUserByEmail(email), Times.Once);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var expectedUsers = TestDataFactory.CreateUserList(3);

            _mockUserRepository
                .Setup(x => x.GetAllUsers())
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedUsers);
            _mockUserRepository.Verify(x => x.GetAllUsers(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyUserList = new List<User>();

            _mockUserRepository
                .Setup(x => x.GetAllUsers())
                .ReturnsAsync(emptyUserList);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockUserRepository.Verify(x => x.GetAllUsers(), Times.Once);
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.CreateUserAsync(null!));
        }

        [Fact]
        public async Task CreateUserAsync_WithValidRequest_ShouldCreateAndReturnUser()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var expectedUser = TestDataFactory.CreateValidUser(
                email: request.Email
            );

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_WithExistingUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            var existingUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userService.CreateUserAsync(request));

            exception.Message.Should().Be("User already exists");
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WithValidRequest_ShouldReturnUpdatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = TestDataFactory.CreateValidUpdateUserRequest();
            var updatedUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.UpdateUser(userId, request))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(updatedUser);
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = Guid.Empty;
            var request = TestDataFactory.CreateValidUpdateUserRequest();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateUserAsync(userId, request));

            exception.Message.Should().Contain("User ID cannot be empty");
        }

        [Fact]
        public async Task UpdateUserAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.UpdateUserAsync(userId, null!));

            exception.Message.Should().Contain("Request cannot be null");
        }

        [Fact]
        public async Task UpdateUserAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = TestDataFactory.CreateValidUpdateUserRequest();

            _mockUserRepository
                .Setup(x => x.UpdateUser(userId, request))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UserExistsByEmailAsync Tests

        [Fact]
        public async Task UserExistsByEmailAsync_WithExistingUser_ShouldReturnTrue()
        {
            // Arrange
            var email = "existing@example.com";
            var existingUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _userService.UserExistsByEmailAsync(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserExistsByEmailAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.UserExistsByEmailAsync(email);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UserExistsByEmailAsync_WithInvalidEmail_ShouldReturnFalse(string email)
        {
            // Act
            var result = await _userService.UserExistsByEmailAsync(email!);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetUserPortfolioInfoAsync Tests

        [Fact]
        public async Task GetUserPortfolioInfoAsync_WithValidUserId_ShouldReturnPortfolioInfo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserPortfolioInfoAsync(userId);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserPortfolioInfoAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserPortfolioInfoAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserPortfolioInfoAsync_WithInvalidUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userService.GetUserPortfolioInfoAsync(userId));

            exception.Message.Should().Contain("User ID cannot be empty");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task GetAllUsersAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            _mockUserRepository
                .Setup(x => x.GetAllUsers())
                .ThrowsAsync(new Exception("Database connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _userService.GetAllUsersAsync());

            exception.Message.Should().Be("Database connection error");
        }

        [Fact]
        public async Task GetUserByEmailAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var email = "test@example.com";

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _userService.GetUserByEmailAsync(email));

            exception.Message.Should().Be("Database error");
        }

        [Fact]
        public async Task CreateUserAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database constraint violation"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _userService.CreateUserAsync(request));

            exception.Message.Should().Be("Database constraint violation");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task UserService_CompleteUserLifecycle_ShouldWork()
        {
            // Arrange
            var registerRequest = TestDataFactory.CreateValidRegisterUserRequest();
            var updateRequest = TestDataFactory.CreateValidUpdateUserRequest();
            var user = TestDataFactory.CreateValidUser();
            var updatedUser = TestDataFactory.CreateValidUser();

            // Setup mocks for complete lifecycle
            _mockUserRepository
                .Setup(x => x.GetUserByEmail(registerRequest.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(user.Email))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.GetUserById(user.Id))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.UpdateUser(user.Id, updateRequest))
                .ReturnsAsync(updatedUser);

            // Act - Complete user lifecycle
            var createdUser = await _userService.CreateUserAsync(registerRequest);
            var retrievedUser = await _userService.GetUserByIdAsync(createdUser.Id);
            var userExists = await _userService.UserExistsByEmailAsync(createdUser.Email);
            var portfolioInfo = await _userService.GetUserPortfolioInfoAsync(createdUser.Id);
            var finalUser = await _userService.UpdateUserAsync(createdUser.Id, updateRequest);

            // Assert
            createdUser.Should().NotBeNull();
            retrievedUser.Should().NotBeNull();
            userExists.Should().BeTrue();
            portfolioInfo.Should().NotBeNull();
            finalUser.Should().NotBeNull();

            // Verify all repository calls
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
            _mockUserRepository.Verify(x => x.GetUserById(user.Id), Times.AtLeast(1));
            _mockUserRepository.Verify(x => x.GetUserByEmail(registerRequest.Email), Times.AtLeast(1));
            _mockUserRepository.Verify(x => x.UpdateUser(user.Id, updateRequest), Times.Once);
        }

        #endregion
    }
}
