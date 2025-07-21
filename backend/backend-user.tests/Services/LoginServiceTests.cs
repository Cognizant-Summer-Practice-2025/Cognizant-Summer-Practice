using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.Authentication.Request;
using backend_user.DTO.Authentication.Response;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class LoginServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
        private readonly LoginService _loginService;

        public LoginServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
            _loginService = new LoginService(
                _mockUserRepository.Object,
                _mockOAuthProviderRepository.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_ShouldInitialize()
        {
            // Arrange & Act
            var service = new LoginService(_mockUserRepository.Object, _mockOAuthProviderRepository.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new LoginService(null!, _mockOAuthProviderRepository.Object));
        }

        [Fact]
        public void Constructor_WithNullOAuthProviderRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new LoginService(_mockUserRepository.Object, null!));
        }

        #endregion

        #region LoginWithOAuthAsync Tests

        [Fact]
        public async Task LoginWithOAuthAsync_WithValidRequest_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            var user = TestDataFactory.CreateValidUser();
            user.IsActive = true;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(user.Id, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Id.Should().Be(user.Id);
            result.Message.Should().Be("Login successful");
        }

        [Fact]
        public async Task LoginWithOAuthAsync_WithInvalidRequest_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = new OAuthLoginRequestDto
            {
                Provider = OAuthProviderType.Google,
                ProviderId = "", // Invalid - empty
                ProviderEmail = "test@example.com",
                AccessToken = "access_token"
            };

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
            result.Message.Should().Contain("Provider ID cannot be null or empty");
        }

        [Fact]
        public async Task LoginWithOAuthAsync_WithNonExistentOAuthProvider_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
            result.Message.Should().Be("OAuth provider not found");
        }

        [Fact]
        public async Task LoginWithOAuthAsync_WithInactiveUser_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            var user = TestDataFactory.CreateValidUser();
            user.IsActive = false; // Inactive user

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync(user);

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
            result.Message.Should().Be("User account is inactive");
        }

        [Fact]
        public async Task LoginWithOAuthAsync_WithNonExistentUser_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
            result.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task LoginWithOAuthAsync_WithRepositoryException_ShouldReturnFailureResponse()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _loginService.LoginWithOAuthAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.User.Should().BeNull();
            result.Message.Should().Be("An error occurred during login");
        }

        #endregion

        #region ValidateOAuthCredentialsAsync Tests

        [Fact]
        public async Task ValidateOAuthCredentialsAsync_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            oauthProvider.ProviderEmail = providerEmail;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _loginService.ValidateOAuthCredentialsAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateOAuthCredentialsAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _loginService.ValidateOAuthCredentialsAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateOAuthCredentialsAsync_WithMismatchedEmail_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            oauthProvider.ProviderEmail = "different@example.com"; // Different email

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _loginService.ValidateOAuthCredentialsAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateOAuthCredentialsAsync_WithRepositoryException_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var providerEmail = "test@example.com";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _loginService.ValidateOAuthCredentialsAsync(provider, providerId, providerEmail);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateOAuthTokenAsync Tests

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var accessToken = "new_access_token";
            var refreshToken = "new_refresh_token";
            var tokenExpiresAt = DateTime.UtcNow.AddHours(1);
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(oauthProvider.Id, It.IsAny<OAuthProviderUpdateRequestDto>()))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId, accessToken, refreshToken, tokenExpiresAt);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithNullProviderId_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            string? providerId = null;
            var accessToken = "new_access_token";

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId!, accessToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithEmptyAccessToken_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var accessToken = "";

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId, accessToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var accessToken = "new_access_token";

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId, accessToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithRepositoryException_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var accessToken = "new_access_token";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(oauthProvider.Id, It.IsAny<OAuthProviderUpdateRequestDto>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId, accessToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateOAuthTokenAsync_WithFailedUpdate_ShouldReturnFalse()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google123";
            var accessToken = "new_access_token";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(provider, providerId))
                .ReturnsAsync(oauthProvider);

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(oauthProvider.Id, It.IsAny<OAuthProviderUpdateRequestDto>()))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _loginService.UpdateOAuthTokenAsync(provider, providerId, accessToken);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task LoginFlow_EndToEnd_ShouldWork()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthLoginRequest();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();
            var user = TestDataFactory.CreateValidUser();
            user.IsActive = true;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(request.Provider, request.ProviderId))
                .ReturnsAsync(oauthProvider);

            _mockUserRepository
                .Setup(x => x.GetUserById(oauthProvider.UserId))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.UpdateLastLoginAsync(user.Id, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var loginResult = await _loginService.LoginWithOAuthAsync(request);

            // Verify credentials
            var verifyResult = await _loginService.ValidateOAuthCredentialsAsync(
                request.Provider, request.ProviderId, oauthProvider.ProviderEmail);

            // Update tokens
            var updateResult = await _loginService.UpdateOAuthTokenAsync(
                request.Provider, request.ProviderId, "new_token");

            // Assert
            loginResult.Success.Should().BeTrue();
            verifyResult.Should().BeTrue();
            updateResult.Should().BeTrue();
        }

        #endregion
    }
}
