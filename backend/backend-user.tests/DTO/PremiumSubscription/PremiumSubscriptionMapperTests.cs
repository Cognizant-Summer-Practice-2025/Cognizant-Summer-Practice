using Xunit;
using FluentAssertions;
using backend_user.DTO.PremiumSubscription;
using backend_user.Models;

namespace backend_user.tests.DTO.PremiumSubscription
{
    public class PremiumSubscriptionMapperTests
    {
        #region ToDto Tests

        [Fact]
        public void ToDto_WithValidModel_ShouldMapCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_test123",
                StripeCustomerId = "cus_test123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = model.ToDto();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(model.Id);
            result.UserId.Should().Be(model.UserId);
            result.StripeSubscriptionId.Should().Be(model.StripeSubscriptionId);
            result.StripeCustomerId.Should().Be(model.StripeCustomerId);
            result.Status.Should().Be(model.Status);
            result.CurrentPeriodStart.Should().Be(model.CurrentPeriodStart);
            result.CurrentPeriodEnd.Should().Be(model.CurrentPeriodEnd);
            result.CancelAtPeriodEnd.Should().Be(model.CancelAtPeriodEnd);
            result.CreatedAt.Should().Be(model.CreatedAt);
            result.UpdatedAt.Should().Be(model.UpdatedAt);
        }

        [Fact]
        public void ToDto_WithNullModel_ShouldReturnNull()
        {
            // Arrange
            backend_user.Models.PremiumSubscription? model = null;

            // Act
            var result = model.ToDto();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ToDto_WithModelWithNullValues_ShouldMapCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = null,
                StripeCustomerId = null,
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = model.ToDto();

            // Assert
            result.Should().NotBeNull();
            result.StripeSubscriptionId.Should().BeNull();
            result.StripeCustomerId.Should().BeNull();
        }

        #endregion

        #region ToModel Tests

        [Fact]
        public void ToModel_WithValidDto_ShouldMapCorrectly()
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
            var result = dto.ToModel();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty); // New model should have empty ID
            result.UserId.Should().Be(dto.UserId);
            result.StripeSubscriptionId.Should().Be(dto.StripeSubscriptionId);
            result.StripeCustomerId.Should().Be(dto.StripeCustomerId);
            result.Status.Should().Be(dto.Status);
            result.CurrentPeriodStart.Should().Be(dto.CurrentPeriodStart);
            result.CurrentPeriodEnd.Should().Be(dto.CurrentPeriodEnd);
            result.CancelAtPeriodEnd.Should().Be(dto.CancelAtPeriodEnd);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ToModel_WithNullDto_ShouldReturnNull()
        {
            // Arrange
            CreatePremiumSubscriptionDto? dto = null;

            // Act
            var result = dto.ToModel();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ToModel_WithDtoWithNullValues_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new CreatePremiumSubscriptionDto
            {
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = null,
                StripeCustomerId = null,
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false
            };

            // Act
            var result = dto.ToModel();

            // Assert
            result.Should().NotBeNull();
            result.StripeSubscriptionId.Should().BeNull();
            result.StripeCustomerId.Should().BeNull();
        }

        #endregion

        #region UpdateModel Tests

        [Fact]
        public void UpdateModel_WithValidDto_ShouldUpdateModelCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_old123",
                StripeCustomerId = "cus_old123",
                Status = "inactive",
                CurrentPeriodStart = DateTime.UtcNow.AddDays(-30),
                CurrentPeriodEnd = DateTime.UtcNow,
                CancelAtPeriodEnd = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var updateDto = new UpdatePremiumSubscriptionDto
            {
                StripeSubscriptionId = "sub_new123",
                StripeCustomerId = "cus_new123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false
            };

            var originalId = model.Id;
            var originalUserId = model.UserId;
            var originalCreatedAt = model.CreatedAt;

            // Act
            model.UpdateModel(updateDto);

            // Assert
            model.Id.Should().Be(originalId); // ID should not change
            model.UserId.Should().Be(originalUserId); // UserId should not change
            model.CreatedAt.Should().Be(originalCreatedAt); // CreatedAt should not change
            model.StripeSubscriptionId.Should().Be(updateDto.StripeSubscriptionId);
            model.StripeCustomerId.Should().Be(updateDto.StripeCustomerId);
            model.Status.Should().Be(updateDto.Status);
            model.CurrentPeriodStart.Should().Be(updateDto.CurrentPeriodStart);
            model.CurrentPeriodEnd.Should().Be(updateDto.CurrentPeriodEnd);
            if (updateDto.CancelAtPeriodEnd.HasValue)
            {
                model.CancelAtPeriodEnd.Should().Be(updateDto.CancelAtPeriodEnd.Value);
            }
            model.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateModel_WithNullDto_ShouldNotUpdateModel()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_old123",
                StripeCustomerId = "cus_old123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var originalStripeSubscriptionId = model.StripeSubscriptionId;
            var originalStripeCustomerId = model.StripeCustomerId;
            var originalStatus = model.Status;
            var originalUpdatedAt = model.UpdatedAt;

            UpdatePremiumSubscriptionDto? updateDto = null;

            // Act
            model.UpdateModel(updateDto);

            // Assert
            model.StripeSubscriptionId.Should().Be(originalStripeSubscriptionId);
            model.StripeCustomerId.Should().Be(originalStripeCustomerId);
            model.Status.Should().Be(originalStatus);
            model.UpdatedAt.Should().Be(originalUpdatedAt);
        }

        [Fact]
        public void UpdateModel_WithPartialDto_ShouldUpdateOnlySpecifiedFields()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_old123",
                StripeCustomerId = "cus_old123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var updateDto = new UpdatePremiumSubscriptionDto
            {
                Status = "canceled"
                // Only status is specified, other fields should remain unchanged
            };

            var originalStripeSubscriptionId = model.StripeSubscriptionId;
            var originalStripeCustomerId = model.StripeCustomerId;
            var originalCurrentPeriodStart = model.CurrentPeriodStart;
            var originalCurrentPeriodEnd = model.CurrentPeriodEnd;
            var originalCancelAtPeriodEnd = model.CancelAtPeriodEnd;

            // Act
            model.UpdateModel(updateDto);

            // Assert
            model.Status.Should().Be("canceled");
            model.StripeSubscriptionId.Should().Be(originalStripeSubscriptionId);
            model.StripeCustomerId.Should().Be(originalStripeCustomerId);
            model.CurrentPeriodStart.Should().Be(originalCurrentPeriodStart);
            model.CurrentPeriodEnd.Should().Be(originalCurrentPeriodEnd);
            model.CancelAtPeriodEnd.Should().Be(originalCancelAtPeriodEnd);
            model.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateModel_WithDtoWithNullValues_ShouldHandleNullValuesCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_old123",
                StripeCustomerId = "cus_old123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var updateDto = new UpdatePremiumSubscriptionDto
            {
                StripeSubscriptionId = null,
                StripeCustomerId = null,
                Status = "canceled"
            };

            // Act
            model.UpdateModel(updateDto);

            // Assert
            model.Status.Should().Be("canceled");
            model.StripeSubscriptionId.Should().BeNull();
            model.StripeCustomerId.Should().BeNull();
        }

        [Fact]
        public void UpdateModel_WithDtoWithEmptyStrings_ShouldHandleEmptyStringsCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StripeSubscriptionId = "sub_old123",
                StripeCustomerId = "cus_old123",
                Status = "active",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var updateDto = new UpdatePremiumSubscriptionDto
            {
                StripeSubscriptionId = "",
                StripeCustomerId = "",
                Status = "canceled"
            };

            // Act
            model.UpdateModel(updateDto);

            // Assert
            model.Status.Should().Be("canceled");
            model.StripeSubscriptionId.Should().Be("");
            model.StripeCustomerId.Should().Be("");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void ToDto_WithModelWithMinimalValues_ShouldMapCorrectly()
        {
            // Arrange
            var model = new backend_user.Models.PremiumSubscription
            {
                Id = Guid.Empty,
                UserId = Guid.Empty,
                StripeSubscriptionId = "",
                StripeCustomerId = "",
                Status = "",
                CurrentPeriodStart = DateTime.MinValue,
                CurrentPeriodEnd = DateTime.MinValue,
                CancelAtPeriodEnd = false,
                CreatedAt = DateTime.MinValue,
                UpdatedAt = DateTime.MinValue
            };

            // Act
            var result = model.ToDto();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty);
            result.UserId.Should().Be(Guid.Empty);
            result.StripeSubscriptionId.Should().Be("");
            result.StripeCustomerId.Should().Be("");
            result.Status.Should().Be("");
            result.CurrentPeriodStart.Should().Be(DateTime.MinValue);
            result.CurrentPeriodEnd.Should().Be(DateTime.MinValue);
            result.CancelAtPeriodEnd.Should().BeFalse();
            result.CreatedAt.Should().Be(DateTime.MinValue);
            result.UpdatedAt.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public void ToModel_WithDtoWithMinimalValues_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new CreatePremiumSubscriptionDto
            {
                UserId = Guid.Empty,
                StripeSubscriptionId = "",
                StripeCustomerId = "",
                Status = "",
                CurrentPeriodStart = DateTime.MinValue,
                CurrentPeriodEnd = DateTime.MinValue,
                CancelAtPeriodEnd = false
            };

            // Act
            var result = dto.ToModel();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty);
            result.UserId.Should().Be(Guid.Empty);
            result.StripeSubscriptionId.Should().Be("");
            result.StripeCustomerId.Should().Be("");
            result.Status.Should().Be("");
            result.CurrentPeriodStart.Should().Be(DateTime.MinValue);
            result.CurrentPeriodEnd.Should().Be(DateTime.MinValue);
            result.CancelAtPeriodEnd.Should().BeFalse();
        }

        #endregion
    }
}
