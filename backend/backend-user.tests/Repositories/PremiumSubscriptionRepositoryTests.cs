using Xunit;
using FluentAssertions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO.PremiumSubscription;
using backend_user.Data;
using Microsoft.EntityFrameworkCore;

namespace backend_user.tests.Repositories
{
    public class PremiumSubscriptionRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<UserDbContext> _options;
        private readonly UserDbContext _context;
        private readonly PremiumSubscriptionRepository _repository;

        public PremiumSubscriptionRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(_options);
            _repository = new PremiumSubscriptionRepository(_context);

            // Ensure database is created
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullContext_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new PremiumSubscriptionRepository(null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var repository = new PremiumSubscriptionRepository(_context);

            // Assert
            repository.Should().NotBeNull();
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ShouldCreateSubscription()
        {
            // Arrange
            var dto = new CreatePremiumSubscriptionDto
            {
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_test123",
                StripeCustomerId = "cus_test123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false
            };

            // Act
            var result = await _repository.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.UserId.Should().Be(dto.UserId);
            result.StripeSubscriptionId.Should().Be(dto.StripeSubscriptionId);
            result.StripeCustomerId.Should().Be(dto.StripeCustomerId);
            result.Status.Should().Be(dto.Status);
            result.CurrentPeriodStart.Should().Be(dto.CurrentPeriodStart);
            result.CurrentPeriodEnd.Should().Be(dto.CurrentPeriodEnd);
            result.CancelAtPeriodEnd.Should().Be(dto.CancelAtPeriodEnd);

            // Verify it was saved to database
            var savedSubscription = await _context.PremiumSubscriptions.FindAsync(result.Id);
            savedSubscription.Should().NotBeNull();
            savedSubscription!.UserId.Should().Be(dto.UserId);
        }

        [Fact]
        public async Task CreateAsync_WithNullDto_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _repository.CreateAsync(null!));
        }

        #endregion

        #region GetByUserIdAsync Tests

        [Fact]
        public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnSubscription()
        {
            // Arrange
            var subscription = await CreateTestSubscription();

            // Act
            var result = await _repository.GetByUserIdAsync(subscription.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(subscription.Id);
            result.UserId.Should().Be(subscription.UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithNonExistentUserId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByUserIdAsync(nonExistentUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Arrange
            var emptyUserId = Guid.Empty;

            // Act
            var result = await _repository.GetByUserIdAsync(emptyUserId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByStripeSubscriptionIdAsync Tests

        [Fact]
        public async Task GetByStripeSubscriptionIdAsync_WithValidStripeId_ShouldReturnSubscription()
        {
            // Arrange
            var subscription = await CreateTestSubscription();

            // Act
            var result = await _repository.GetByStripeSubscriptionIdAsync(subscription.StripeSubscriptionId!);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(subscription.Id);
            result.StripeSubscriptionId.Should().Be(subscription.StripeSubscriptionId);
        }

        [Fact]
        public async Task GetByStripeSubscriptionIdAsync_WithNonExistentStripeId_ShouldReturnNull()
        {
            // Arrange
            var nonExistentStripeId = "sub_nonexistent";

            // Act
            var result = await _repository.GetByStripeSubscriptionIdAsync(nonExistentStripeId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByStripeSubscriptionIdAsync_WithNullStripeId_ShouldReturnNull()
        {
            // Arrange
            string? nullStripeId = null;

            // Act
            var result = await _repository.GetByStripeSubscriptionIdAsync(nullStripeId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByStripeSubscriptionIdAsync_WithEmptyStripeId_ShouldReturnNull()
        {
            // Arrange
            var emptyStripeId = "";

            // Act
            var result = await _repository.GetByStripeSubscriptionIdAsync(emptyStripeId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ShouldUpdateSubscription()
        {
            // Arrange
            var subscription = await CreateTestSubscription();
            var updateDto = new UpdatePremiumSubscriptionDto
            {
                StripeSubscriptionId = "sub_updated123",
                StripeCustomerId = "cus_updated123",
                Status = "canceled",
                CurrentPeriodStart = DateTime.UtcNow.AddDays(1),
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(31),
                CancelAtPeriodEnd = true
            };

            // Act
            var result = await _repository.UpdateAsync(subscription.Id, updateDto);

            // Assert
            result.Should().NotBeNull();

            // Verify the update in database
            var updatedSubscription = await _context.PremiumSubscriptions.FindAsync(subscription.Id);
            updatedSubscription.Should().NotBeNull();
            updatedSubscription!.StripeSubscriptionId.Should().Be(updateDto.StripeSubscriptionId);
            updatedSubscription.StripeCustomerId.Should().Be(updateDto.StripeCustomerId);
            updatedSubscription.Status.Should().Be(updateDto.Status);
            updatedSubscription.CurrentPeriodStart.Should().Be(updateDto.CurrentPeriodStart);
            updatedSubscription.CurrentPeriodEnd.Should().Be(updateDto.CurrentPeriodEnd);
            if (updateDto.CancelAtPeriodEnd.HasValue)
            {
                updatedSubscription.CancelAtPeriodEnd.Should().Be(updateDto.CancelAtPeriodEnd.Value);
            }
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new UpdatePremiumSubscriptionDto
            {
                Status = "canceled"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.UpdateAsync(nonExistentId, updateDto));
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ShouldThrowArgumentNullException()
        {
            // Arrange
            var subscription = await CreateTestSubscription();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _repository.UpdateAsync(subscription.Id, null!));
        }

        [Fact]
        public async Task UpdateAsync_WithPartialUpdate_ShouldUpdateOnlySpecifiedFields()
        {
            // Arrange
            var subscription = await CreateTestSubscription();
            var originalStatus = subscription.Status;
            var updateDto = new UpdatePremiumSubscriptionDto
            {
                Status = "canceled"
            };

            // Act
            var result = await _repository.UpdateAsync(subscription.Id, updateDto);

            // Assert
            result.Should().NotBeNull();

            // Verify only status was updated
            var updatedSubscription = await _context.PremiumSubscriptions.FindAsync(subscription.Id);
            updatedSubscription.Should().NotBeNull();
            updatedSubscription!.Status.Should().Be("canceled");
            updatedSubscription.StripeSubscriptionId.Should().Be(subscription.StripeSubscriptionId); // Unchanged
            updatedSubscription.StripeCustomerId.Should().Be(subscription.StripeCustomerId); // Unchanged
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteSubscription()
        {
            // Arrange
            var subscription = await CreateTestSubscription();

            // Act
            var result = await _repository.DeleteAsync(subscription.Id);

            // Assert
            result.Should().BeTrue();

            // Verify it was deleted from database
            var deletedSubscription = await _context.PremiumSubscriptions.FindAsync(subscription.Id);
            deletedSubscription.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WithExistingUserId_ShouldReturnTrue()
        {
            // Arrange
            var subscription = await CreateTestSubscription();

            // Act
            var result = await _repository.ExistsAsync(subscription.UserId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentUserId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _repository.ExistsAsync(nonExistentUserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyGuid_ShouldReturnFalse()
        {
            // Arrange
            var emptyUserId = Guid.Empty;

            // Act
            var result = await _repository.ExistsAsync(emptyUserId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region IsActiveAsync Tests

        [Fact]
        public async Task IsActiveAsync_WithActiveSubscription_ShouldReturnTrue()
        {
            // Arrange
            var subscription = await CreateTestSubscription();
            subscription.Status = "active";
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsActiveAsync(subscription.UserId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsActiveAsync_WithInactiveSubscription_ShouldReturnFalse()
        {
            // Arrange
            var subscription = await CreateTestSubscription();
            subscription.Status = "canceled";
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsActiveAsync(subscription.UserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsActiveAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _repository.IsActiveAsync(nonExistentUserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsActiveAsync_WithEmptyGuid_ShouldReturnFalse()
        {
            // Arrange
            var emptyUserId = Guid.Empty;

            // Act
            var result = await _repository.IsActiveAsync(emptyUserId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private async Task<PremiumSubscription> CreateTestSubscription()
        {
            var subscription = new PremiumSubscription
            {
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = $"sub_test_{Guid.NewGuid():N}",
                StripeCustomerId = $"cus_test_{Guid.NewGuid():N}",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PremiumSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        #endregion
    }
}
