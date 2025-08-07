using Xunit;
using FluentAssertions;
using backend_user.Models;
using System.Net;

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
            analytics.Id.Should().NotBeEmpty();
            analytics.UserId.Should().BeEmpty();
            analytics.SessionId.Should().BeEmpty();
            analytics.EventType.Should().BeEmpty();
            analytics.EventData.Should().Be("{}");
            analytics.IpAddress.Should().BeNull();
            analytics.UserAgent.Should().BeNull();
            analytics.ReferrerUrl.Should().BeNull();
            analytics.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UserAnalytics_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessionId = "session-123";
            var eventType = "page_view";
            var eventData = "{\"page\":\"/home\"}";
            var ipAddress = IPAddress.Parse("192.168.1.1");
            var userAgent = "Mozilla/5.0";
            var referrerUrl = "https://google.com";

            // Act
            var analytics = new UserAnalytics
            {
                UserId = userId,
                SessionId = sessionId,
                EventType = eventType,
                EventData = eventData,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ReferrerUrl = referrerUrl
            };

            // Assert
            analytics.UserId.Should().Be(userId);
            analytics.SessionId.Should().Be(sessionId);
            analytics.EventType.Should().Be(eventType);
            analytics.EventData.Should().Be(eventData);
            analytics.IpAddress.Should().Be(ipAddress);
            analytics.UserAgent.Should().Be(userAgent);
            analytics.ReferrerUrl.Should().Be(referrerUrl);
        }

        [Fact]
        public void UserAnalytics_NavigationProperty_ShouldAllowUserAssignment()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var analytics = new UserAnalytics
            {
                UserId = user.Id,
                SessionId = "session-123",
                EventType = "page_view"
            };

            // Act
            analytics.User = user;

            // Assert
            analytics.User.Should().Be(user);
            analytics.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData("page_view")]
        [InlineData("button_click")]
        [InlineData("form_submit")]
        [InlineData("api_call")]
        public void UserAnalytics_WithDifferentEventTypes_ShouldSetCorrectly(string eventType)
        {
            // Act
            var analytics = new UserAnalytics
            {
                EventType = eventType,
                SessionId = "session-123"
            };

            // Assert
            analytics.EventType.Should().Be(eventType);
        }

        [Theory]
        [InlineData("{\"page\":\"/home\"}")]
        [InlineData("{\"button\":\"submit\",\"form\":\"login\"}")]
        [InlineData("{\"api\":\"users\",\"method\":\"GET\"}")]
        [InlineData("{}")]
        public void UserAnalytics_WithDifferentEventData_ShouldSetCorrectly(string eventData)
        {
            // Act
            var analytics = new UserAnalytics
            {
                EventData = eventData,
                SessionId = "session-123",
                EventType = "test"
            };

            // Assert
            analytics.EventData.Should().Be(eventData);
        }

        [Fact]
        public void UserAnalytics_PropertySetters_ShouldWorkCorrectly()
        {
            // Arrange
            var analytics = new UserAnalytics();
            var userId = Guid.NewGuid();
            var sessionId = "new-session";
            var eventType = "new-event";
            var eventData = "{\"new\":\"data\"}";
            var ipAddress = IPAddress.Parse("10.0.0.1");
            var userAgent = "New Browser";
            var referrerUrl = "https://bing.com";

            // Act
            analytics.UserId = userId;
            analytics.SessionId = sessionId;
            analytics.EventType = eventType;
            analytics.EventData = eventData;
            analytics.IpAddress = ipAddress;
            analytics.UserAgent = userAgent;
            analytics.ReferrerUrl = referrerUrl;

            // Assert
            analytics.UserId.Should().Be(userId);
            analytics.SessionId.Should().Be(sessionId);
            analytics.EventType.Should().Be(eventType);
            analytics.EventData.Should().Be(eventData);
            analytics.IpAddress.Should().Be(ipAddress);
            analytics.UserAgent.Should().Be(userAgent);
            analytics.ReferrerUrl.Should().Be(referrerUrl);
        }

        [Fact]
        public void UserAnalytics_WithNullOptionalProperties_ShouldBeValid()
        {
            // Act
            var analytics = new UserAnalytics
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "test",
                IpAddress = null,
                UserAgent = null,
                ReferrerUrl = null
            };

            // Assert
            analytics.IpAddress.Should().BeNull();
            analytics.UserAgent.Should().BeNull();
            analytics.ReferrerUrl.Should().BeNull();
        }
    }
} 
