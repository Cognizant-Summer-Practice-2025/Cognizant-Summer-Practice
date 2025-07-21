using Xunit;
using Moq;
using FluentAssertions;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class OAuthProviderServiceTests
    {
        private readonly Mock<IOAuthProviderRepository> _mockOAuthProviderRepository;
        private readonly OAuthProviderService _oauthProviderService;

        public OAuthProviderServiceTests()
        {
            _mockOAuthProviderRepository = new Mock<IOAuthProviderRepository>();
            _oauthProviderService = new OAuthProviderService(_mockOAuthProviderRepository.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OAuthProviderService(null!));
        }

        [Fact]
        public void Constructor_WithValidRepository_ShouldCreateInstance()
        {
            // Act
            var service = new OAuthProviderService(_mockOAuthProviderRepository.Object);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetUserOAuthProvidersAsync Tests

        [Fact]
        public async Task GetUserOAuthProvidersAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyUserId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.GetUserOAuthProvidersAsync(emptyUserId));
        }

        [Fact]
        public async Task GetUserOAuthProvidersAsync_WithValidUserId_ShouldReturnProviders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providers = new List<OAuthProvider>
            {
                TestDataFactory.CreateValidOAuthProvider(userId, OAuthProviderType.Google),
                TestDataFactory.CreateValidOAuthProvider(userId, OAuthProviderType.GitHub)
            };

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(providers);

            // Act
            var result = await _oauthProviderService.GetUserOAuthProvidersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _mockOAuthProviderRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserOAuthProvidersAsync_WithNoProviders_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var emptyProviders = new List<OAuthProvider>();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(emptyProviders);

            // Act
            var result = await _oauthProviderService.GetUserOAuthProvidersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockOAuthProviderRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        }

        #endregion

        #region CreateOAuthProviderAsync Tests

        [Fact]
        public async Task CreateOAuthProviderAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.CreateOAuthProviderAsync(null!));
        }

        [Fact]
        public async Task CreateOAuthProviderAsync_WithValidRequest_ShouldCreateAndReturnProvider()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123",
                ProviderEmail = "test@google.com",
                AccessToken = "access-token"
            };

            var createdProvider = TestDataFactory.CreateValidOAuthProvider(
                request.UserId,
                request.Provider,
                request.ProviderId
            );

            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(request))
                .ReturnsAsync(createdProvider);

            // Act
            var result = await _oauthProviderService.CreateOAuthProviderAsync(request);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.CreateAsync(request), Times.Once);
        }

        #endregion

        #region UpdateOAuthProviderAsync Tests

        [Fact]
        public async Task UpdateOAuthProviderAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "new-access-token"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.UpdateOAuthProviderAsync(emptyId, request));
        }

        [Fact]
        public async Task UpdateOAuthProviderAsync_WithNullRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.UpdateOAuthProviderAsync(providerId, null!));
        }

        [Fact]
        public async Task UpdateOAuthProviderAsync_WithValidParameters_ShouldUpdateAndReturnProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "new-access-token"
            };

            var updatedProvider = TestDataFactory.CreateValidOAuthProvider();
            updatedProvider.Id = providerId;

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(providerId, request))
                .ReturnsAsync(updatedProvider);

            // Act
            var result = await _oauthProviderService.UpdateOAuthProviderAsync(providerId, request);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.UpdateAsync(providerId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateOAuthProviderAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "new-access-token"
            };

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(providerId, request))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _oauthProviderService.UpdateOAuthProviderAsync(providerId, request);

            // Assert
            result.Should().BeNull();
            _mockOAuthProviderRepository.Verify(x => x.UpdateAsync(providerId, request), Times.Once);
        }

        #endregion

        #region DeleteOAuthProviderAsync Tests

        [Fact]
        public async Task DeleteOAuthProviderAsync_WithEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.DeleteOAuthProviderAsync(emptyId));
        }

        [Fact]
        public async Task DeleteOAuthProviderAsync_WithValidId_ShouldDeleteProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            _mockOAuthProviderRepository
                .Setup(x => x.DeleteAsync(providerId))
                .ReturnsAsync(true);

            // Act
            var result = await _oauthProviderService.DeleteOAuthProviderAsync(providerId);

            // Assert
            result.Should().BeTrue();
            _mockOAuthProviderRepository.Verify(x => x.DeleteAsync(providerId), Times.Once);
        }

        [Fact]
        public async Task DeleteOAuthProviderAsync_WithNonExistentProvider_ShouldReturnFalse()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            _mockOAuthProviderRepository
                .Setup(x => x.DeleteAsync(providerId))
                .ReturnsAsync(false);

            // Act
            var result = await _oauthProviderService.DeleteOAuthProviderAsync(providerId);

            // Assert
            result.Should().BeFalse();
            _mockOAuthProviderRepository.Verify(x => x.DeleteAsync(providerId), Times.Once);
        }

        #endregion

        #region CheckOAuthProviderAsync Tests

        [Fact]
        public async Task CheckOAuthProviderAsync_WithExistingProvider_ShouldReturnProviderDetails()
        {
            // Arrange
            var providerType = OAuthProviderType.Google;
            var providerId = "google-123";
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.ExistsAsync(providerType, providerId))
                .ReturnsAsync(true);
            
            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(providerType, providerId))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _oauthProviderService.CheckOAuthProviderAsync(providerType, providerId);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.ExistsAsync(providerType, providerId), Times.Once);
            _mockOAuthProviderRepository.Verify(x => x.GetByProviderAndProviderIdAsync(providerType, providerId), Times.Once);
        }

        [Fact]
        public async Task CheckOAuthProviderAsync_WithNonExistentProvider_ShouldReturnNotExists()
        {
            // Arrange
            var providerType = OAuthProviderType.Google;
            var providerId = "non-existent-123";

            _mockOAuthProviderRepository
                .Setup(x => x.ExistsAsync(providerType, providerId))
                .ReturnsAsync(false);
            
            _mockOAuthProviderRepository
                .Setup(x => x.GetByProviderAndProviderIdAsync(providerType, providerId))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _oauthProviderService.CheckOAuthProviderAsync(providerType, providerId);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.ExistsAsync(providerType, providerId), Times.Once);
            _mockOAuthProviderRepository.Verify(x => x.GetByProviderAndProviderIdAsync(providerType, providerId), Times.Once);
        }

        #endregion

        #region GetUserOAuthProviderByTypeAsync Tests

        [Fact]
        public async Task GetUserOAuthProviderByTypeAsync_WithValidUserAndExistingProvider_ShouldReturnProvider()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providerType = OAuthProviderType.Google;
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAndProviderAsync(userId, providerType))
                .ReturnsAsync(oauthProvider);

            // Act
            var result = await _oauthProviderService.GetUserOAuthProviderByTypeAsync(userId, providerType);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.GetByUserIdAndProviderAsync(userId, providerType), Times.Once);
        }

        [Fact]
        public async Task GetUserOAuthProviderByTypeAsync_WithValidUserAndNonExistentProvider_ShouldReturnNotExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providerType = OAuthProviderType.Google;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAndProviderAsync(userId, providerType))
                .ReturnsAsync((OAuthProvider?)null);

            // Act
            var result = await _oauthProviderService.GetUserOAuthProviderByTypeAsync(userId, providerType);

            // Assert
            result.Should().NotBeNull();
            _mockOAuthProviderRepository.Verify(x => x.GetByUserIdAndProviderAsync(userId, providerType), Times.Once);
        }

        [Fact]
        public async Task GetUserOAuthProviderByTypeAsync_WithEmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyUserId = Guid.Empty;
            var providerType = OAuthProviderType.Google;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _oauthProviderService.GetUserOAuthProviderByTypeAsync(emptyUserId, providerType));
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task GetUserOAuthProvidersAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.GetUserOAuthProvidersAsync(userId));
        }

        [Fact]
        public async Task CreateOAuthProviderAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123",
                ProviderEmail = "test@google.com",
                AccessToken = "access-token"
            };

            _mockOAuthProviderRepository
                .Setup(x => x.CreateAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.CreateOAuthProviderAsync(request));
        }

        [Fact]
        public async Task UpdateOAuthProviderAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "new-access-token"
            };

            _mockOAuthProviderRepository
                .Setup(x => x.UpdateAsync(providerId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.UpdateOAuthProviderAsync(providerId, request));
        }

        [Fact]
        public async Task DeleteOAuthProviderAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            _mockOAuthProviderRepository
                .Setup(x => x.DeleteAsync(providerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.DeleteOAuthProviderAsync(providerId));
        }

        [Fact]
        public async Task CheckOAuthProviderAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var providerType = OAuthProviderType.Google;
            var providerId = "google-123";

            _mockOAuthProviderRepository
                .Setup(x => x.ExistsAsync(providerType, providerId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.CheckOAuthProviderAsync(providerType, providerId));
        }

        [Fact]
        public async Task GetUserOAuthProviderByTypeAsync_WhenRepositoryThrowsException_ShouldPropagate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providerType = OAuthProviderType.Google;

            _mockOAuthProviderRepository
                .Setup(x => x.GetByUserIdAndProviderAsync(userId, providerType))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _oauthProviderService.GetUserOAuthProviderByTypeAsync(userId, providerType));
        }

        #endregion
    }
}
