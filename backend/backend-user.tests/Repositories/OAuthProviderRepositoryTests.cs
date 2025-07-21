using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using backend_user.tests.Helpers;

namespace backend_user.tests.Repositories
{
    public class OAuthProviderRepositoryTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly OAuthProviderRepository _repository;

        public OAuthProviderRepositoryTests()
        {
            _context = TestDbContextHelper.CreateInMemoryContext();
            _repository = new OAuthProviderRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProviderWithUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var oauthProvider = TestDataFactory.CreateValidOAuthProvider(user.Id);
            
            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddAsync(oauthProvider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(oauthProvider.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(oauthProvider.Id);
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByUserIdAsync Tests

        [Fact]
        public async Task GetByUserIdAsync_WithMultipleProviders_ShouldReturnAllUserProviders()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var provider1 = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);
            var provider2 = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.GitHub);
            var otherUserProvider = TestDataFactory.CreateValidOAuthProvider();

            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddRangeAsync(provider1, provider2, otherUserProvider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Provider == OAuthProviderType.Google);
            result.Should().Contain(p => p.Provider == OAuthProviderType.GitHub);
            result.Should().NotContain(p => p.Id == otherUserProvider.Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithNoProviders_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByUserIdAndProviderAsync Tests

        [Fact]
        public async Task GetByUserIdAndProviderAsync_WithExistingProvider_ShouldReturnProvider()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var provider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);

            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddAsync(provider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAndProviderAsync(user.Id, OAuthProviderType.Google);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.Id);
            result.Provider.Should().Be(OAuthProviderType.Google);
        }

        [Fact]
        public async Task GetByUserIdAndProviderAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByUserIdAndProviderAsync(userId, OAuthProviderType.Google);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserIdAndProviderAsync_WithDifferentProvider_ShouldReturnNull()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var provider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);

            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddAsync(provider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAndProviderAsync(user.Id, OAuthProviderType.GitHub);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByProviderAndProviderIdAsync Tests

        [Fact]
        public async Task GetByProviderAndProviderIdAsync_WithExistingProvider_ShouldReturnProviderWithUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var provider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google, "google-123");

            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddAsync(provider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "google-123");

            // Assert
            result.Should().NotBeNull();
            result!.Provider.Should().Be(OAuthProviderType.Google);
            result.ProviderId.Should().Be("google-123");
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByProviderAndProviderIdAsync_WithNonExistentProvider_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "nonexistent-123");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByProviderAndEmailAsync Tests

        [Fact]
        public async Task GetByProviderAndEmailAsync_WithExistingProvider_ShouldReturnProviderWithUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var provider = TestDataFactory.CreateValidOAuthProvider(user.Id, OAuthProviderType.Google);
            provider.ProviderEmail = "test@google.com";

            await _context.Users.AddAsync(user);
            await _context.OAuthProviders.AddAsync(provider);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByProviderAndEmailAsync(OAuthProviderType.Google, "test@google.com");

            // Assert
            result.Should().NotBeNull();
            result!.Provider.Should().Be(OAuthProviderType.Google);
            result.ProviderEmail.Should().Be("test@google.com");
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByProviderAndEmailAsync_WithNonExistentEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByProviderAndEmailAsync(OAuthProviderType.Google, "nonexistent@google.com");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task Repository_WithComplexScenario_ShouldWorkCorrectly()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser(email: "user1@test.com");
            var user2 = TestDataFactory.CreateValidUser(email: "user2@test.com");
            
            var provider1 = TestDataFactory.CreateValidOAuthProvider(user1.Id, OAuthProviderType.Google, "google-user1");
            var provider2 = TestDataFactory.CreateValidOAuthProvider(user1.Id, OAuthProviderType.GitHub, "github-user1");
            var provider3 = TestDataFactory.CreateValidOAuthProvider(user2.Id, OAuthProviderType.Google, "google-user2");

            await _context.Users.AddRangeAsync(user1, user2);
            await _context.OAuthProviders.AddRangeAsync(provider1, provider2, provider3);
            await _context.SaveChangesAsync();

            // Act & Assert - User1 should have 2 providers
            var user1Providers = await _repository.GetByUserIdAsync(user1.Id);
            user1Providers.Should().HaveCount(2);

            // Act & Assert - User2 should have 1 provider
            var user2Providers = await _repository.GetByUserIdAsync(user2.Id);
            user2Providers.Should().HaveCount(1);

            // Act & Assert - Should find specific provider by provider and providerId
            var foundProvider = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "google-user1");
            foundProvider.Should().NotBeNull();
            foundProvider!.UserId.Should().Be(user1.Id);

            // Act & Assert - Should not find provider with wrong providerId
            var notFoundProvider = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, "wrong-id");
            notFoundProvider.Should().BeNull();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task GetByUserIdAsync_WithInvalidGuid_ShouldReturnEmptyList()
        {
            // Arrange
            var invalidUserId = Guid.Empty;

            // Act
            var result = await _repository.GetByUserIdAsync(invalidUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByProviderAndProviderIdAsync_WithNullProviderId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByProviderAndProviderIdAsync(OAuthProviderType.Google, null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByProviderAndEmailAsync_WithNullEmail_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByProviderAndEmailAsync(OAuthProviderType.Google, null!);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}
