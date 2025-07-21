using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
        private readonly AuthenticationService _authenticationService;

        public AuthenticationServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
            _authenticationService = new AuthenticationService(
                _mockUserRepository.Object,
                _mockOAuthProviderRepository.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AuthenticationService(null!, _mockOAuthProviderRepository.Object));
        }

        [Fact]
        public void Constructor_WithNullOAuthProviderRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new AuthenticationService(_mockUserRepository.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var service = new AuthenticationService(
                _mockUserRepository.Object,
                _mockOAuthProviderRepository.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region AuthenticateOAuthUserAsync Tests

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithValidButNonExistentProvider_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "test@example.com";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithEmptyProviderId_ShouldThrowArgumentException()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "";
            var providerEmail = "test@example.com";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail));
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithInvalidEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "invalid-email";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail));
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithNonExistentOAuthProvider_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "test@example.com";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "test@example.com";
            var userId = Guid.NewGuid();

            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(userId, provider, providerId);

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithInactiveUser_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "test@example.com";
            var userId = Guid.NewGuid();

            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(userId, provider, providerId);
            var user = TestDataFactory.CreateValidUser(isActive: false);
            user.Id = userId;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithValidCredentialsAndActiveUser_ShouldReturnUserAndUpdateLastLogin()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var providerEmail = "test@example.com";
            var userId = Guid.NewGuid();

            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(userId, provider, providerId);
            var user = TestDataFactory.CreateValidUser();
            user.Id = userId;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(user);
            _mockUserRepository.Verify(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()), Times.Once);
        }

        #endregion

        #region UpdateLastLoginAsync Tests

        [Fact]
        public async Task UpdateLastLoginAsync_WithValidUserId_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authenticationService.UpdateLastLoginAsync(userId);

            // Assert
            result.Should().BeTrue();
            _mockUserRepository.Verify(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WhenRepositoryThrowsException_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authenticationService.UpdateLastLoginAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region IsOAuthProviderLinkedAsync Tests

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithInvalidProvider_ShouldReturnFalse()
        {
            // Arrange
            var provider = (OAuthProviderType)999;
            var providerId = "test-provider-id";

            // Act
            var result = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithEmptyProviderId_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "";

            // Act
            var result = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithExistingProvider_ShouldReturnTrue()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(providerId: providerId);

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "test-provider-id";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithValidationFailure_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = ""; // Invalid - empty

            // Act
            var result = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsOAuthProviderLinkedAsync_WithRepositoryException_ShouldPropagateException()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task AuthenticationFlow_CompleteFlow_ShouldWork()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            var user = TestDataFactory.CreateValidUser();
            user.IsActive = true;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(user.Id, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var isLinked = await _authenticationService.IsOAuthProviderLinkedAsync(provider, providerId);
            var authenticatedUser = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);
            var updateResult = await _authenticationService.UpdateLastLoginAsync(user.Id);

            // Assert
            isLinked.Should().BeTrue();
            authenticatedUser.Should().NotBeNull();
            authenticatedUser!.Id.Should().Be(user.Id);
            updateResult.Should().BeTrue();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithInactiveUserSecondCase_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            var user = TestDataFactory.CreateValidUser();
            user.IsActive = false; // Inactive user

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync(user);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateOAuthUserAsync_WithNonExistentUserSecondCase_ShouldReturnNull()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WithRepositoryException_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _authenticationService.UpdateLastLoginAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AuthenticateOAuthUserAsync_WithInvalidProviderId_ShouldThrowArgumentException(string providerId)
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerEmail = "test@example.com";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _authenticationService.AuthenticateOAuthUserAsync(provider, providerId!, providerEmail));

            exception.Message.Should().Contain("Provider ID is required");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AuthenticateOAuthUserAsync_WithInvalidProviderEmail_ShouldThrowArgumentException(string providerEmail)
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _authenticationService.AuthenticateOAuthUserAsync(provider, providerId, providerEmail!));

            exception.Message.Should().Contain("Provider email is required");
        }

        [Fact]
        public async Task UpdateLastLoginAsync_WithEmptyUserId_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.Empty;

            // Act
            var result = await _authenticationService.UpdateLastLoginAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

    }
}
