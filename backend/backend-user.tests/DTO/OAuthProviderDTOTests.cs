using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.Models;

namespace backend_user.tests.DTO
{
    public class OAuthProviderDTOTests
    {
        #region OAuthProviderCreateRequestDto Tests

        [Fact]
        public void OAuthProviderCreateRequestDto_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123456",
                ProviderEmail = "user@google.com",
                AccessToken = "access-token-value",
                RefreshToken = "refresh-token-value",
                TokenExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderCreateRequestDto_WithEmptyUserId_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.Empty, // Empty GUID is valid for [Required] attribute
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123456",
                ProviderEmail = "user@google.com",
                AccessToken = "access-token-value"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert - Empty GUID passes Required validation in .NET
            validationResults.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void OAuthProviderCreateRequestDto_WithMissingProviderId_ShouldFailValidation(string providerId)
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = providerId!,
                ProviderEmail = "user@google.com",
                AccessToken = "access-token-value"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("ProviderId"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid-email")]
        [InlineData("@domain.com")]
        [InlineData("user@")]
        public void OAuthProviderCreateRequestDto_WithInvalidProviderEmail_ShouldFailValidation(string providerEmail)
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123456",
                ProviderEmail = providerEmail!,
                AccessToken = "access-token-value"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("ProviderEmail"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void OAuthProviderCreateRequestDto_WithMissingAccessToken_ShouldFailValidation(string accessToken)
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123456",
                ProviderEmail = "user@google.com",
                AccessToken = accessToken!
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(vr => vr.MemberNames.Contains("AccessToken"));
        }

        [Theory]
        [InlineData(OAuthProviderType.Google)]
        [InlineData(OAuthProviderType.GitHub)]
        [InlineData(OAuthProviderType.LinkedIn)]
        [InlineData(OAuthProviderType.Facebook)]
        public void OAuthProviderCreateRequestDto_WithValidProviderTypes_ShouldPassValidation(OAuthProviderType provider)
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = provider,
                ProviderId = "provider-123456",
                ProviderEmail = "user@provider.com",
                AccessToken = "access-token-value"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderCreateRequestDto_WithOptionalFields_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123456",
                ProviderEmail = "user@google.com",
                AccessToken = "access-token-value"
                // RefreshToken and TokenExpiresAt are optional
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        #endregion

        #region OAuthProviderUpdateRequestDto Tests

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                TokenExpiresAt = DateTime.UtcNow.AddHours(2)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithAllNullFields_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                // All fields are null - should be valid for partial updates
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithEmptyAccessToken_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "", // Empty but not null
                RefreshToken = "refresh-token"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithPartialUpdate_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                AccessToken = "updated-access-token"
                // Only updating access token, other fields remain null
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithFutureExpirationDate_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                TokenExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_WithPastExpirationDate_ShouldPassValidation()
        {
            // Arrange
            var request = new OAuthProviderUpdateRequestDto
            {
                TokenExpiresAt = DateTime.UtcNow.AddDays(-1) // Past date
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            // Should pass validation - business logic should handle expired tokens
            validationResults.Should().BeEmpty();
        }

        #endregion

        #region Property Tests

        [Fact]
        public void OAuthProviderCreateRequestDto_Properties_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new OAuthProviderCreateRequestDto();

            // Assert
            request.UserId.Should().Be(Guid.Empty);
            request.Provider.Should().Be(default(OAuthProviderType));
            request.ProviderId.Should().Be(string.Empty);
            request.ProviderEmail.Should().Be(string.Empty);
            request.AccessToken.Should().Be(string.Empty);
            request.RefreshToken.Should().BeNull();
            request.TokenExpiresAt.Should().BeNull();
        }

        [Fact]
        public void OAuthProviderUpdateRequestDto_Properties_ShouldHaveCorrectDefaultValues()
        {
            // Arrange & Act
            var request = new OAuthProviderUpdateRequestDto();

            // Assert
            request.AccessToken.Should().BeNull();
            request.RefreshToken.Should().BeNull();
            request.TokenExpiresAt.Should().BeNull();
        }

        [Fact]
        public void OAuthProviderCreateRequestDto_PropertiesSetAndGet_ShouldWorkCorrectly()
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto();
            var userId = Guid.NewGuid();
            var provider = OAuthProviderType.GitHub;
            var providerId = "github-123";
            var email = "user@github.com";
            var accessToken = "access-123";

            // Act
            request.UserId = userId;
            request.Provider = provider;
            request.ProviderId = providerId;
            request.ProviderEmail = email;
            request.AccessToken = accessToken;

            // Assert
            request.UserId.Should().Be(userId);
            request.Provider.Should().Be(provider);
            request.ProviderId.Should().Be(providerId);
            request.ProviderEmail.Should().Be(email);
            request.AccessToken.Should().Be(accessToken);
        }

        #endregion

        #region Helper Methods

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        #endregion

        #region Email Validation Edge Cases

        [Theory]
        [InlineData("user@google.com")]
        [InlineData("user.name@github.com")]
        [InlineData("user+tag@linkedin.com")]
        [InlineData("123@facebook.com")]
        public void OAuthProviderCreateRequestDto_WithValidEmails_ShouldPassValidation(string email)
        {
            // Arrange
            var request = new OAuthProviderCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                Provider = OAuthProviderType.Google,
                ProviderId = "provider-123",
                ProviderEmail = email,
                AccessToken = "access-token"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        #endregion
    }
}
