using Xunit;
using FluentAssertions;
using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using System.Net;

namespace backend_user.tests.DTO
{
    public class UserAnalyticsDTOTests
    {
        #region UserAnalyticsCreateRequestDto Tests

        [Fact]
        public void UserAnalyticsCreateRequestDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var request = new UserAnalyticsCreateRequestDto();

            // Assert
            request.UserId.Should().BeEmpty();
            request.SessionId.Should().BeEmpty();
            request.EventType.Should().BeEmpty();
            request.EventData.Should().Be("{}");
            request.IpAddress.Should().BeNull();
            request.UserAgent.Should().BeNull();
            request.ReferrerUrl.Should().BeNull();
        }

        [Fact]
        public void UserAnalyticsCreateRequestDto_WithValidData_ShouldSetPropertiesCorrectly()
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
            var request = new UserAnalyticsCreateRequestDto
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
            request.UserId.Should().Be(userId);
            request.SessionId.Should().Be(sessionId);
            request.EventType.Should().Be(eventType);
            request.EventData.Should().Be(eventData);
            request.IpAddress.Should().Be(ipAddress);
            request.UserAgent.Should().Be(userAgent);
            request.ReferrerUrl.Should().Be(referrerUrl);
        }

        [Theory]
        [InlineData("page_view")]
        [InlineData("button_click")]
        [InlineData("form_submit")]
        [InlineData("api_call")]
        public void UserAnalyticsCreateRequestDto_WithDifferentEventTypes_ShouldSetCorrectly(string eventType)
        {
            // Act
            var request = new UserAnalyticsCreateRequestDto
            {
                EventType = eventType,
                SessionId = "session-123"
            };

            // Assert
            request.EventType.Should().Be(eventType);
        }

        [Theory]
        [InlineData("{\"page\":\"/home\"}")]
        [InlineData("{\"button\":\"submit\",\"form\":\"login\"}")]
        [InlineData("{\"api\":\"users\",\"method\":\"GET\"}")]
        [InlineData("{}")]
        public void UserAnalyticsCreateRequestDto_WithDifferentEventData_ShouldSetCorrectly(string eventData)
        {
            // Act
            var request = new UserAnalyticsCreateRequestDto
            {
                EventData = eventData,
                SessionId = "session-123",
                EventType = "test"
            };

            // Assert
            request.EventData.Should().Be(eventData);
        }

        #endregion

        #region UserAnalyticsResponseDto Tests

        [Fact]
        public void UserAnalyticsResponseDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var response = new UserAnalyticsResponseDto();

            // Assert
            response.Id.Should().BeEmpty();
            response.UserId.Should().BeEmpty();
            response.SessionId.Should().BeEmpty();
            response.EventType.Should().BeEmpty();
            response.EventData.Should().Be("{}");
            response.IpAddress.Should().BeNull();
            response.UserAgent.Should().BeNull();
            response.ReferrerUrl.Should().BeNull();
            response.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void UserAnalyticsResponseDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var sessionId = "session-123";
            var eventType = "page_view";
            var eventData = "{\"page\":\"/home\"}";
            var ipAddress = IPAddress.Parse("192.168.1.1");
            var userAgent = "Mozilla/5.0";
            var referrerUrl = "https://google.com";
            var createdAt = DateTime.UtcNow;

            // Act
            var response = new UserAnalyticsResponseDto
            {
                Id = id,
                UserId = userId,
                SessionId = sessionId,
                EventType = eventType,
                EventData = eventData,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ReferrerUrl = referrerUrl,
                CreatedAt = createdAt
            };

            // Assert
            response.Id.Should().Be(id);
            response.UserId.Should().Be(userId);
            response.SessionId.Should().Be(sessionId);
            response.EventType.Should().Be(eventType);
            response.EventData.Should().Be(eventData);
            response.IpAddress.Should().Be(ipAddress);
            response.UserAgent.Should().Be(userAgent);
            response.ReferrerUrl.Should().Be(referrerUrl);
            response.CreatedAt.Should().Be(createdAt);
        }

        #endregion

        #region UserAnalyticsSummaryDto Tests

        [Fact]
        public void UserAnalyticsSummaryDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var summary = new UserAnalyticsSummaryDto();

            // Assert
            summary.Id.Should().BeEmpty();
            summary.SessionId.Should().BeEmpty();
            summary.EventType.Should().BeEmpty();
            summary.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void UserAnalyticsSummaryDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sessionId = "session-123";
            var eventType = "page_view";
            var createdAt = DateTime.UtcNow;

            // Act
            var summary = new UserAnalyticsSummaryDto
            {
                Id = id,
                SessionId = sessionId,
                EventType = eventType,
                CreatedAt = createdAt
            };

            // Assert
            summary.Id.Should().Be(id);
            summary.SessionId.Should().Be(sessionId);
            summary.EventType.Should().Be(eventType);
            summary.CreatedAt.Should().Be(createdAt);
        }

        [Theory]
        [InlineData("page_view")]
        [InlineData("button_click")]
        [InlineData("form_submit")]
        [InlineData("api_call")]
        public void UserAnalyticsSummaryDto_WithDifferentEventTypes_ShouldSetCorrectly(string eventType)
        {
            // Act
            var summary = new UserAnalyticsSummaryDto
            {
                EventType = eventType,
                SessionId = "session-123"
            };

            // Assert
            summary.EventType.Should().Be(eventType);
        }

        #endregion
    }
}
