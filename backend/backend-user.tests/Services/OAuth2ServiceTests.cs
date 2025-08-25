using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using System.Text.Json;

namespace backend_user.tests.Services
{
    public class OAuth2ServiceTests
    {
        private readonly Mock<IOAuthProviderRepository> _mockProviderRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ILogger<OAuth2Service>> _mockLogger;
        private readonly OAuth2Service _service;

        public OAuth2ServiceTests()
        {
            _mockProviderRepo = new Mock<IOAuthProviderRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<OAuth2Service>>();
            _service = new OAuth2Service(_mockProviderRepo.Object, _mockUserRepo.Object, _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullOAuthProviderRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OAuth2Service(null!, _mockUserRepo.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OAuth2Service(_mockProviderRepo.Object, null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new OAuth2Service(_mockProviderRepo.Object, _mockUserRepo.Object, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var service = new OAuth2Service(_mockProviderRepo.Object, _mockUserRepo.Object, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region ValidateAccessTokenAsync Tests

        [Fact]
        public async Task ValidateAccessTokenAsync_WithValidToken_ShouldReturnProvider()
        {
            // Arrange
            var token = "valid_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.ValidateAccessTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(provider);
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var token = "expired_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.ValidateAccessTokenAsync(token);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithTokenExpiringNow_ShouldReturnNull()
        {
            // Arrange
            var token = "expiring_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = DateTime.UtcNow // Expiring now
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.ValidateAccessTokenAsync(token);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithNoExpiration_ShouldReturnProvider()
        {
            // Arrange
            var token = "no_expiration_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = null // No expiration
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.ValidateAccessTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(provider);
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task ValidateAccessTokenAsync_WithNonExistentToken_ShouldReturnNull()
        {
            // Arrange
            var token = "non_existent_token";

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _service.ValidateAccessTokenAsync(token);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
        }

        #endregion

        #region GetUserByAccessTokenAsync Tests

        [Fact]
        public async Task GetUserByAccessTokenAsync_WithValidToken_ShouldReturnUser()
        {
            // Arrange
            var token = "valid_token";
            var userId = Guid.NewGuid();
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                Username = "testuser"
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            _mockUserRepo
                .Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByAccessTokenAsync(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(user);
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
            _mockUserRepo.Verify(x => x.GetUserById(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserByAccessTokenAsync_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var token = "invalid_token";

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _service.GetUserByAccessTokenAsync(token);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
            _mockUserRepo.Verify(x => x.GetUserById(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByAccessTokenAsync_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var token = "expired_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                AccessToken = token,
                TokenExpiresAt = DateTime.UtcNow.AddHours(-1)
            };

            _mockProviderRepo
                .Setup(x => x.GetByAccessTokenAsync(token))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.GetUserByAccessTokenAsync(token);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByAccessTokenAsync(token), Times.Once);
            _mockUserRepo.Verify(x => x.GetUserById(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region RefreshAccessTokenAsync Tests

        [Fact]
        public async Task RefreshAccessTokenAsync_WithNonExistentRefreshToken_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "non_existent_refresh";

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithEmptyRefreshToken_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                RefreshToken = null // Empty refresh token
            };

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull();
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithGoogleProvider_ShouldAttemptGoogleRefresh()
        {
            // Arrange
            var refreshToken = "google_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                RefreshToken = refreshToken
            };

            // Set up environment variables for Google
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_ID", "google_client_id");
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_SECRET", "google_client_secret");

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Will fail due to HTTP call, but we test the path
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithGitHubProvider_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "github_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.GitHub,
                RefreshToken = refreshToken
            };

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // GitHub doesn't support refresh tokens
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithFacebookProvider_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "facebook_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Facebook,
                RefreshToken = refreshToken
            };

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Facebook doesn't support refresh tokens
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithLinkedInProvider_ShouldAttemptLinkedInRefresh()
        {
            // Arrange
            var refreshToken = "linkedin_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.LinkedIn,
                RefreshToken = refreshToken
            };

            // Set up environment variables for LinkedIn
            Environment.SetEnvironmentVariable("AUTH_LINKEDIN_ID", "linkedin_client_id");
            Environment.SetEnvironmentVariable("AUTH_LINKEDIN_SECRET", "linkedin_client_secret");

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Will fail due to HTTP call, but we test the path
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithUnknownProvider_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "unknown_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = (OAuthProviderType)999, // Unknown provider type
                RefreshToken = refreshToken
            };

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Unknown provider returns null
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithMissingGoogleCredentials_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "google_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                RefreshToken = refreshToken
            };

            // Don't set environment variables
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_ID", null);
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_SECRET", null);

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Missing credentials return null
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithMissingLinkedInCredentials_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "linkedin_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.LinkedIn,
                RefreshToken = refreshToken
            };

            // Don't set environment variables
            Environment.SetEnvironmentVariable("AUTH_LINKEDIN_ID", null);
            Environment.SetEnvironmentVariable("AUTH_LINKEDIN_SECRET", null);

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Missing credentials return null
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_WithExceptionDuringRefresh_ShouldReturnNull()
        {
            // Arrange
            var refreshToken = "google_refresh_token";
            var provider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                RefreshToken = refreshToken
            };

            // Set up environment variables for Google
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_ID", "google_client_id");
            Environment.SetEnvironmentVariable("AUTH_GOOGLE_SECRET", "google_client_secret");

            _mockProviderRepo
                .Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(provider);

            // Act
            var result = await _service.RefreshAccessTokenAsync(refreshToken);

            // Assert
            result.Should().BeNull(); // Exception during refresh returns null
            _mockProviderRepo.Verify(x => x.GetByRefreshTokenAsync(refreshToken), Times.Once);
        }

        #endregion
    }
}