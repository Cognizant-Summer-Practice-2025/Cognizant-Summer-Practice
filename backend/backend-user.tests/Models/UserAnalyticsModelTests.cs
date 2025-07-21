using Xunit;
using FluentAssertions;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Models
{
    public class UserAnalyticsModelTests
    {
        [Fact]
        public void UserAnalytics_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var analytics = new UserAnalytics();

            // Assert
            analytics.Id.Should().NotBe(Guid.Empty);
            analytics.UserId.Should().Be(Guid.Empty);
            analytics.SessionId.Should().Be(string.Empty);
            analytics.EventType.Should().Be(string.Empty);
            analytics.EventData.Should().Be("{}");
            analytics.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            analytics.User.Should().BeNull();
        }

        [Fact]
        public void UserAnalytics_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var sessionId = "session-123";
            var eventType = "login";
            var eventData = "{\"ip\":\"192.168.1.1\"}";
            var createdAt = DateTime.UtcNow;

            // Act
            var analytics = new UserAnalytics
            {
                Id = id,
                UserId = userId,
                SessionId = sessionId,
                EventType = eventType,
                EventData = eventData,
                CreatedAt = createdAt
            };

            // Assert
            analytics.Id.Should().Be(id);
            analytics.UserId.Should().Be(userId);
            analytics.SessionId.Should().Be(sessionId);
            analytics.EventType.Should().Be(eventType);
            analytics.EventData.Should().Be(eventData);
            analytics.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void UserAnalytics_NavigationProperty_ShouldAllowUserAssignment()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var analytics = TestDataFactory.CreateValidUserAnalytics(user.Id);

            // Act
            analytics.User = user;

            // Assert
            analytics.User.Should().NotBeNull();
            analytics.User.Should().Be(user);
            analytics.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData("login")]
        [InlineData("logout")]
        [InlineData("page_view")]
        [InlineData("button_click")]
        [InlineData("form_submit")]
        public void UserAnalytics_WithDifferentEventTypes_ShouldSetCorrectly(string eventType)
        {
            // Arrange & Act
            var analytics = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = eventType,
                EventData = "{}",
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            analytics.EventType.Should().Be(eventType);
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"page\":\"/dashboard\"}")]
        [InlineData("{\"button\":\"save\",\"form\":\"profile\"}")]
        [InlineData("null")]
        public void UserAnalytics_WithDifferentEventData_ShouldSetCorrectly(string eventData)
        {
            // Arrange & Act
            var analytics = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "test_event",
                EventData = eventData,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            analytics.EventData.Should().Be(eventData);
        }

        [Fact]
        public void UserAnalytics_PropertySetters_ShouldWorkCorrectly()
        {
            // Arrange
            var analytics = new UserAnalytics();
            var newId = Guid.NewGuid();
            var newUserId = Guid.NewGuid();
            var newSessionId = "new-session-456";
            var newEventType = "profile_update";
            var newEventData = "{\"field\":\"bio\"}";
            var newCreatedAt = DateTime.UtcNow;

            // Act
            analytics.Id = newId;
            analytics.UserId = newUserId;
            analytics.SessionId = newSessionId;
            analytics.EventType = newEventType;
            analytics.EventData = newEventData;
            analytics.CreatedAt = newCreatedAt;

            // Assert
            analytics.Id.Should().Be(newId);
            analytics.UserId.Should().Be(newUserId);
            analytics.SessionId.Should().Be(newSessionId);
            analytics.EventType.Should().Be(newEventType);
            analytics.EventData.Should().Be(newEventData);
            analytics.CreatedAt.Should().Be(newCreatedAt);
        }

        [Fact]
        public void UserAnalytics_WithTestDataFactory_ShouldCreateValidInstance()
        {
            // Arrange & Act
            var analytics = TestDataFactory.CreateValidUserAnalytics();

            // Assert
            analytics.Id.Should().NotBe(Guid.Empty);
            analytics.UserId.Should().NotBe(Guid.Empty);
            analytics.SessionId.Should().NotBeNullOrEmpty();
            analytics.EventType.Should().NotBeNullOrEmpty();
            analytics.EventData.Should().NotBeNullOrEmpty();
            analytics.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void UserAnalytics_WithEmptyEventData_ShouldBeValid()
        {
            // Arrange & Act
            var analytics = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "test_event",
                EventData = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            analytics.EventData.Should().Be(string.Empty);
        }
    }
}
