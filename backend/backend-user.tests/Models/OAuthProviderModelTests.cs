using Xunit;
using FluentAssertions;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Models
{
    public class OAuthProviderModelTests
    {
        [Fact]
        public void OAuthProvider_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var oauthProvider = new OAuthProvider();

            // Assert
            oauthProvider.Id.Should().NotBe(Guid.Empty);
            oauthProvider.UserId.Should().Be(Guid.Empty);
            oauthProvider.Provider.Should().Be(default(OAuthProviderType));
            oauthProvider.ProviderId.Should().Be(string.Empty);
            oauthProvider.ProviderEmail.Should().Be(string.Empty);
            oauthProvider.AccessToken.Should().Be(string.Empty);
            oauthProvider.RefreshToken.Should().BeNull();
            oauthProvider.TokenExpiresAt.Should().BeNull();
            oauthProvider.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            oauthProvider.User.Should().BeNull();
        }

        [Fact]
        public void OAuthProvider_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providerId = "google-123456";
            var providerEmail = "test@google.com";
            var accessToken = "access-token-123";
            var refreshToken = "refresh-token-123";
            var tokenExpiresAt = DateTime.UtcNow.AddHours(1);
            var createdAt = DateTime.UtcNow;

            // Act
            var oauthProvider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = OAuthProviderType.Google,
                ProviderId = providerId,
                ProviderEmail = providerEmail,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiresAt = tokenExpiresAt,
                CreatedAt = createdAt
            };

            // Assert
            oauthProvider.Id.Should().NotBe(Guid.Empty);
            oauthProvider.UserId.Should().Be(userId);
            oauthProvider.Provider.Should().Be(OAuthProviderType.Google);
            oauthProvider.ProviderId.Should().Be(providerId);
            oauthProvider.ProviderEmail.Should().Be(providerEmail);
            oauthProvider.AccessToken.Should().Be(accessToken);
            oauthProvider.RefreshToken.Should().Be(refreshToken);
            oauthProvider.TokenExpiresAt.Should().Be(tokenExpiresAt);
            oauthProvider.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void OAuthProvider_NavigationProperty_ShouldAllowUserAssignment()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(user.Id);

            // Act
            oauthProvider.User = user;

            // Assert
            oauthProvider.User.Should().NotBeNull();
            oauthProvider.User.Should().Be(user);
            oauthProvider.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData(OAuthProviderType.Google)]
        [InlineData(OAuthProviderType.Facebook)]
        [InlineData(OAuthProviderType.GitHub)]
        [InlineData(OAuthProviderType.LinkedIn)]
        public void OAuthProvider_WithDifferentProviderTypes_ShouldSetCorrectly(OAuthProviderType providerType)
        {
            // Arrange & Act
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(providerType: providerType);

            // Assert
            oauthProvider.Provider.Should().Be(providerType);
        }

        [Fact]
        public void OAuthProvider_WithNullRefreshToken_ShouldBeValid()
        {
            // Arrange & Act
            var oauthProvider = new OAuthProvider
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123",
                ProviderEmail = "test@google.com",
                AccessToken = "access-token",
                RefreshToken = null,
                TokenExpiresAt = null,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            oauthProvider.RefreshToken.Should().BeNull();
            oauthProvider.TokenExpiresAt.Should().BeNull();
        }

        [Fact]
        public void OAuthProvider_PropertySetters_ShouldWorkCorrectly()
        {
            // Arrange
            var oauthProvider = new OAuthProvider();
            var newId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            var newProviderId = "new-provider-id";
            var newEmail = "new@email.com";
            var newAccessToken = "new-access-token";
            var newRefreshToken = "new-refresh-token";
            var newExpiresAt = DateTime.UtcNow.AddDays(1);
            var newCreatedAt = DateTime.UtcNow;

            // Act
            oauthProvider.Id = newId;
            oauthProvider.UserId = newUserId;
            oauthProvider.Provider = OAuthProviderType.LinkedIn;
            oauthProvider.ProviderId = newProviderId;
            oauthProvider.ProviderEmail = newEmail;
            oauthProvider.AccessToken = newAccessToken;
            oauthProvider.RefreshToken = newRefreshToken;
            oauthProvider.TokenExpiresAt = newExpiresAt;
            oauthProvider.CreatedAt = newCreatedAt;

            // Assert
            oauthProvider.Id.Should().Be(newId);
            oauthProvider.UserId.Should().Be(newUserId);
            oauthProvider.Provider.Should().Be(OAuthProviderType.LinkedIn);
            oauthProvider.ProviderId.Should().Be(newProviderId);
            oauthProvider.ProviderEmail.Should().Be(newEmail);
            oauthProvider.AccessToken.Should().Be(newAccessToken);
            oauthProvider.RefreshToken.Should().Be(newRefreshToken);
            oauthProvider.TokenExpiresAt.Should().Be(newExpiresAt);
            oauthProvider.CreatedAt.Should().Be(newCreatedAt);
        }
    }
}
