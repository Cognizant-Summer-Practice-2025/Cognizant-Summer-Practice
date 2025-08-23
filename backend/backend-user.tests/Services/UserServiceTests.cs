using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.User.Request;
using backend_user.tests.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;

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
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UserService(_mockUserRepository.Object, null!));
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

        [Fact]
        public async Task GetUserByEmailAsync_WithNullEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByEmailAsync(null!));
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithEmptyEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByEmailAsync(""));
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithWhitespaceEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByEmailAsync("   "));
        }

        [Fact]
        public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = TestDataFactory.CreateValidUser();
            expectedUser.Email = email;

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(email))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
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

        #region GetUserByUsernameAsync Tests

        [Fact]
        public async Task GetUserByUsernameAsync_WithNullUsername_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByUsernameAsync(null!));
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithEmptyUsername_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByUsernameAsync(""));
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithWhitespaceUsername_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetUserByUsernameAsync("   "));
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithValidUsername_ShouldReturnUser()
        {
            // Arrange
            var username = "testuser";
            var expectedUser = TestDataFactory.CreateValidUser();
            expectedUser.Username = username;

            _mockUserRepository
                .Setup(x => x.GetUserByUsername(username))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
            _mockUserRepository.Verify(x => x.GetUserByUsername(username), Times.Once);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_WithNonExistentUsername_ShouldReturnNull()
        {
            // Arrange
            var username = "nonexistentuser";

            _mockUserRepository
                .Setup(x => x.GetUserByUsername(username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            result.Should().BeNull();
            _mockUserRepository.Verify(x => x.GetUserByUsername(username), Times.Once);
        }

        #endregion

        #region SearchUsersAsync Tests

        [Fact]
        public async Task SearchUsersAsync_WithNullSearchTerm_ShouldReturnEmptyList()
        {
            // Act
            var result = await _userService.SearchUsersAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithEmptySearchTerm_ShouldReturnEmptyList()
        {
            // Act
            var result = await _userService.SearchUsersAsync("");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithWhitespaceSearchTerm_ShouldReturnEmptyList()
        {
            // Act
            var result = await _userService.SearchUsersAsync("   ");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithValidSearchTerm_ShouldReturnUsers()
        {
            // Arrange
            var searchTerm = "test";
            var expectedUsers = new List<User>
            {
                TestDataFactory.CreateValidUser(),
                TestDataFactory.CreateValidUser()
            };

            _mockUserRepository
                .Setup(x => x.SearchUsers(searchTerm.ToLowerInvariant().Trim()))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUsers);
            _mockUserRepository.Verify(x => x.SearchUsers(searchTerm.ToLowerInvariant().Trim()), Times.Once);
        }

        [Fact]
        public async Task SearchUsersAsync_WithSearchTermContainingWhitespace_ShouldTrimAndSearch()
        {
            // Arrange
            var searchTerm = "  test user  ";
            var trimmedTerm = "test user";
            var expectedUsers = new List<User> { TestDataFactory.CreateValidUser() };

            _mockUserRepository
                .Setup(x => x.SearchUsers(trimmedTerm))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUsers);
            _mockUserRepository.Verify(x => x.SearchUsers(trimmedTerm), Times.Once);
        }

        [Fact]
        public async Task SearchUsersAsync_WithRepositoryException_ShouldReturnEmptyList()
        {
            // Arrange
            var searchTerm = "test";

            _mockUserRepository
                .Setup(x => x.SearchUsers(searchTerm))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _userService.SearchUsersAsync(searchTerm);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                TestDataFactory.CreateValidUser(),
                TestDataFactory.CreateValidUser(),
                TestDataFactory.CreateValidUser()
            };

            _mockUserRepository
                .Setup(x => x.GetAllUsers())
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUsers);
            _mockUserRepository.Verify(x => x.GetAllUsers(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var expectedUsers = new List<User>();

            _mockUserRepository
                .Setup(x => x.GetAllUsers())
                .ReturnsAsync(expectedUsers);

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
        public async Task CreateUserAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidRequest = new RegisterUserRequest
            {
                Email = "", // Invalid email
                FirstName = "", // Invalid first name
                LastName = "" // Invalid last name
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.CreateUserAsync(invalidRequest));
        }

        [Fact]
        public async Task CreateUserAsync_WithExistingUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User"
            };

            var existingUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _userService.CreateUserAsync(request));
        }

        [Fact]
        public async Task CreateUserAsync_WithValidRequest_ShouldCreateUser()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "new@example.com",
                FirstName = "New",
                LastName = "User"
            };

            var createdUser = TestDataFactory.CreateValidUser();
            createdUser.Email = request.Email;
            createdUser.FirstName = request.FirstName;
            createdUser.LastName = request.LastName;

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(createdUser);
            _mockUserRepository.Verify(x => x.GetUserByEmail(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_WithInvalidUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidId = Guid.Empty;
            var request = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "User"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateUserAsync(invalidId, request));
        }

        [Fact]
        public async Task UpdateUserAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var validId = Guid.NewGuid();
            var invalidRequest = new UpdateUserRequest
            {
                FirstName = new string('A', 101), // Exceeds 100 character limit
                LastName = new string('B', 101)   // Exceeds 100 character limit
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateUserAsync(validId, invalidRequest));
        }

        [Fact]
        public async Task UpdateUserAsync_WithValidParameters_ShouldUpdateUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "User"
            };

            var updatedUser = TestDataFactory.CreateValidUser();
            updatedUser.Id = userId;
            updatedUser.FirstName = request.FirstName;
            updatedUser.LastName = request.LastName;

            _mockUserRepository
                .Setup(x => x.UpdateUser(userId, request))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(updatedUser);
            _mockUserRepository.Verify(x => x.UpdateUser(userId, request), Times.Once);
        }

        #endregion

        #region UserExistsByEmailAsync Tests

        [Fact]
        public async Task UserExistsByEmailAsync_WithNullEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _userService.UserExistsByEmailAsync(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserExistsByEmailAsync_WithEmptyEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _userService.UserExistsByEmailAsync("");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserExistsByEmailAsync_WithWhitespaceEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _userService.UserExistsByEmailAsync("   ");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
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
            _mockUserRepository.Verify(x => x.GetUserByEmail(email), Times.Once);
        }

        [Fact]
        public async Task UserExistsByEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
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
            _mockUserRepository.Verify(x => x.GetUserByEmail(email), Times.Once);
        }

        #endregion

        #region GetUserPortfolioInfoAsync Tests

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
            _mockUserRepository.Verify(x => x.GetUserById(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserPortfolioInfoAsync_WithExistingUser_ShouldReturnPortfolioInfo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataFactory.CreateValidUser();
            user.Id = userId;

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserPortfolioInfoAsync(userId);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.GetUserById(userId), Times.Once);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_WithInvalidUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.DeleteUserAsync(invalidId));
        }

        [Fact]
        public async Task DeleteUserAsync_WithValidUserId_ShouldDeleteUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeFalse();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithPortfolioServiceFailure_ShouldContinueAndLogWarning()
        {
            // Arrange
            var userId = Guid.NewGuid();
            Environment.SetEnvironmentVariable("PORTFOLIO_SERVICE_URL", "http://localhost:5201");

            _mockUserRepository
                .Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithMessagesServiceFailure_ShouldContinueAndLogWarning()
        {
            // Arrange
            var userId = Guid.NewGuid();
            Environment.SetEnvironmentVariable("MESSAGES_SERVICE_URL", "http://localhost:5003");

            _mockUserRepository
                .Setup(x => x.DeleteUserAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_WithRepositoryException_ShouldThrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.DeleteUserAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _userService.DeleteUserAsync(userId));
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
