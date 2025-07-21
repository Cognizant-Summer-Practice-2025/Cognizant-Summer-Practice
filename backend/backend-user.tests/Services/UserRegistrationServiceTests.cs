using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.User.Request;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class UserRegistrationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
        private readonly UserRegistrationService _userRegistrationService;

        public UserRegistrationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
            _userRegistrationService = new UserRegistrationService(
                _mockUserRepository.Object,
                _mockOAuthProviderRepository.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_ShouldInitialize()
        {
            // Arrange & Act
            var service = new UserRegistrationService(_mockUserRepository.Object, _mockOAuthProviderRepository.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserRegistrationService(null!, _mockOAuthProviderRepository.Object));
        }

        [Fact]
        public void Constructor_WithNullOAuthProviderRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new UserRegistrationService(_mockUserRepository.Object, null!));
        }

        #endregion

        #region RegisterUserAsync Tests

        [Fact]
        public async Task RegisterUserAsync_WithValidRequest_ShouldReturnUser()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            var expectedUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _userRegistrationService.RegisterUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedUser);
            _mockUserRepository.Verify(x => x.GetUserByEmail(request.Email), Times.Once);
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new RegisterUserRequest
            {
                Email = "", // Invalid - empty email
                FirstName = "John",
                LastName = "Doe"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterUserAsync(request));

            exception.Message.Should().Contain("Email cannot be null or empty");
        }

        [Fact]
        public async Task RegisterUserAsync_WithExistingUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            var existingUser = TestDataFactory.CreateValidUser();

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userRegistrationService.RegisterUserAsync(request));

            exception.Message.Should().Be("User already exists");
        }

        [Fact]
        public async Task RegisterUserAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterUserAsync(null!));

            exception.Message.Should().Contain("Request cannot be null");
        }

        #endregion

        #region RegisterOAuthUserAsync Tests

        [Fact]
        public async Task RegisterOAuthUserAsync_WithValidRequest_ShouldReturnUserResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            var expectedUser = TestDataFactory.CreateValidUser();
            expectedUser.Email = request.Email;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            var createdProvider = TestDataFactory.CreateValidOAuthProvider();
            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()))
                .ReturnsAsync(createdProvider);

            // Act
            var result = await _userRegistrationService.RegisterOAuthUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
            _mockOAuthProviderRepository.Verify(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new RegisterOAuthUserRequest
            {
                Email = "", // Invalid - empty email
                FirstName = "John",
                LastName = "Doe",
                Provider = OAuthProviderType.Google,
                ProviderId = "google123",
                ProviderEmail = "john@example.com",
                AccessToken = "token"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(request));

            exception.Message.Should().Contain("Email cannot be null or empty");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithExistingOAuthProvider_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            var existingProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync(existingProvider);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(request));

            exception.Message.Should().Be("OAuth provider already exists");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithExistingUserByEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            var existingUser = TestDataFactory.CreateValidUser();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(request));

            exception.Message.Should().Be("User with this email already exists");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(null!));

            exception.Message.Should().Contain("Request cannot be null");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithUserCreationFailure_ShouldHandleGracefully()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(request));

            exception.Message.Should().Be("Database error");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithOAuthProviderCreationFailure_ShouldHandleGracefully()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            var expectedUser = TestDataFactory.CreateValidUser();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()))
                .ThrowsAsync(new Exception("OAuth creation failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _userRegistrationService.RegisterOAuthUserAsync(request));

            exception.Message.Should().Be("OAuth creation failed");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task UserRegistration_CompleteFlow_ShouldWork()
        {
            // Arrange
            var regularRequest = TestDataFactory.CreateValidRegisterUserRequest();
            var oauthRequest = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            
            var regularUser = TestDataFactory.CreateValidUser();
            var oauthUser = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            // Setup regular user registration
            _mockUserRepository
                .Setup(x => x.GetUserByEmail(regularRequest.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(regularUser);

            // Setup OAuth user registration
            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(oauthRequest.Provider, oauthRequest.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(oauthRequest.Email))
                .ReturnsAsync((User?)null);

            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()))
                .ReturnsAsync(oauthProvider);

            // Act
            var regularResult = await _userRegistrationService.RegisterUserAsync(regularRequest);
            var oauthResult = await _userRegistrationService.RegisterOAuthUserAsync(oauthRequest);

            // Assert
            regularResult.Should().NotBeNull();
            regularResult.Should().Be(regularUser);

            oauthResult.Should().NotBeNull();

            // Verify all interactions
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.AtLeast(1));
            _mockOAuthProviderRepository.Verify(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()), Times.Once);
        }

        #endregion

        #region Edge Case Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RegisterUserAsync_WithInvalidEmail_ShouldThrowArgumentException(string email)
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            request.Email = email!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterUserAsync(request));

            exception.Message.Should().Contain("Email cannot be null or empty");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RegisterUserAsync_WithInvalidFirstName_ShouldThrowArgumentException(string firstName)
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            request.FirstName = firstName!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterUserAsync(request));

            exception.Message.Should().Contain("FirstName cannot be null or empty");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RegisterUserAsync_WithInvalidLastName_ShouldThrowArgumentException(string lastName)
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            request.LastName = lastName!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _userRegistrationService.RegisterUserAsync(request));

            exception.Message.Should().Contain("LastName cannot be null or empty");
        }

        [Fact]
        public async Task RegisterOAuthUserAsync_WithMismatchedProviderEmails_ShouldHandleCorrectly()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterOAuthUserRequest();
            request.Email = "user@example.com";
            request.ProviderEmail = "different@example.com"; // Different from Email

            var expectedUser = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            _mockUserRepository
                .Setup(x => x.GetUserByEmail(request.Email))
                .ReturnsAsync((User?)null);

            _mockUserRepository
                .Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(expectedUser);

            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(It.IsAny<backend_user.DTO.OAuthProvider.Request.OAuthProviderCreateRequestDto>()))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _userRegistrationService.RegisterOAuthUserAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockUserRepository.Verify(x => x.CreateUser(It.IsAny<User>()), Times.Once);
        }

        #endregion
    }
}
