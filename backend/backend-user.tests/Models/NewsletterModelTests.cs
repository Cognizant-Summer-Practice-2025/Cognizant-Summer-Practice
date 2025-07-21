using Xunit;
using FluentAssertions;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Models
{
    public class NewsletterModelTests
    {
        [Fact]
        public void Newsletter_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var newsletter = new Newsletter();

            // Assert
            newsletter.Id.Should().NotBe(Guid.Empty);
            newsletter.UserId.Should().Be(Guid.Empty);
            newsletter.Type.Should().Be(string.Empty);
            newsletter.IsActive.Should().BeTrue();
            newsletter.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            newsletter.User.Should().BeNull();
        }

        [Fact]
        public void Newsletter_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var type = "weekly";
            var isActive = true;
            var createdAt = DateTime.UtcNow;

            // Act
            var newsletter = new Newsletter
            {
                Id = id,
                UserId = userId,
                Type = type,
                IsActive = isActive,
                CreatedAt = createdAt
            };

            // Assert
            newsletter.Id.Should().Be(id);
            newsletter.UserId.Should().Be(userId);
            newsletter.Type.Should().Be(type);
            newsletter.IsActive.Should().Be(isActive);
            newsletter.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void Newsletter_NavigationProperty_ShouldAllowUserAssignment()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var newsletter = TestDataFactory.CreateValidNewsletter(user.Id);

            // Act
            newsletter.User = user;

            // Assert
            newsletter.User.Should().NotBeNull();
            newsletter.User.Should().Be(user);
            newsletter.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData("daily")]
        [InlineData("weekly")]
        [InlineData("monthly")]
        [InlineData("special")]
        public void Newsletter_WithDifferentTypes_ShouldSetCorrectly(string type)
        {
            // Arrange & Act
            var newsletter = new Newsletter
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            newsletter.Type.Should().Be(type);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Newsletter_WithDifferentActiveStates_ShouldSetCorrectly(bool isActive)
        {
            // Arrange & Act
            var newsletter = new Newsletter
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = "weekly",
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            newsletter.IsActive.Should().Be(isActive);
        }

        [Fact]
        public void Newsletter_PropertySetters_ShouldWorkCorrectly()
        {
            // Arrange
            var newsletter = new Newsletter();
            var newId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            var newType = "monthly";
            var newIsActive = true;
            var newCreatedAt = DateTime.UtcNow;

            // Act
            newsletter.Id = newId;
            newsletter.UserId = newUserId;
            newsletter.Type = newType;
            newsletter.IsActive = newIsActive;
            newsletter.CreatedAt = newCreatedAt;

            // Assert
            newsletter.Id.Should().Be(newId);
            newsletter.UserId.Should().Be(newUserId);
            newsletter.Type.Should().Be(newType);
            newsletter.IsActive.Should().Be(newIsActive);
            newsletter.CreatedAt.Should().Be(newCreatedAt);
        }

        [Fact]
        public void Newsletter_WithTestDataFactory_ShouldCreateValidInstance()
        {
            // Arrange & Act
            var newsletter = TestDataFactory.CreateValidNewsletter();

            // Assert
            newsletter.Id.Should().NotBe(Guid.Empty);
            newsletter.UserId.Should().NotBe(Guid.Empty);
            newsletter.Type.Should().NotBeNullOrEmpty();
            newsletter.IsActive.Should().BeTrue();
            newsletter.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }
}
