using Xunit;
using FluentAssertions;
using backend_user.Services.Mappers;
using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using backend_user.Models;
using System.Net;

namespace backend_user.tests.Services
{
    public class UserAnalyticsMapperTests
    {
        private readonly IUserAnalyticsMapper _mapper;

        public UserAnalyticsMapperTests()
        {
            _mapper = new UserAnalyticsMapper();
        }

        #region MapToEntity Tests

        [Fact]
        public void MapToEntity_WithValidRequest_ShouldMapCorrectly()
        {
            // Arrange
            var request = new UserAnalyticsCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com"
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(request.UserId);
            result.SessionId.Should().Be(request.SessionId);
            result.EventType.Should().Be(request.EventType);
            result.EventData.Should().Be(request.EventData);
            result.IpAddress.Should().Be(request.IpAddress);
            result.UserAgent.Should().Be(request.UserAgent);
            result.ReferrerUrl.Should().Be(request.ReferrerUrl);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MapToEntity_WithNullOptionalProperties_ShouldMapCorrectly()
        {
            // Arrange
            var request = new UserAnalyticsCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{}",
                IpAddress = null,
                UserAgent = null,
                ReferrerUrl = null
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(request.UserId);
            result.SessionId.Should().Be(request.SessionId);
            result.EventType.Should().Be(request.EventType);
            result.EventData.Should().Be(request.EventData);
            result.IpAddress.Should().BeNull();
            result.UserAgent.Should().BeNull();
            result.ReferrerUrl.Should().BeNull();
        }

        [Fact]
        public void MapToEntity_WithEmptyEventData_ShouldMapCorrectly()
        {
            // Arrange
            var request = new UserAnalyticsCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = ""
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.EventData.Should().Be("");
        }

        #endregion

        #region MapToResponseDto Tests

        [Fact]
        public void MapToResponseDto_WithValidEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToResponseDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(entity.UserId);
            result.SessionId.Should().Be(entity.SessionId);
            result.EventType.Should().Be(entity.EventType);
            result.EventData.Should().Be(entity.EventData);
            result.IpAddress.Should().Be(entity.IpAddress);
            result.UserAgent.Should().Be(entity.UserAgent);
            result.ReferrerUrl.Should().Be(entity.ReferrerUrl);
            result.CreatedAt.Should().Be(entity.CreatedAt);
        }

        [Fact]
        public void MapToResponseDto_WithNullOptionalProperties_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{}",
                IpAddress = null,
                UserAgent = null,
                ReferrerUrl = null,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToResponseDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(entity.UserId);
            result.SessionId.Should().Be(entity.SessionId);
            result.EventType.Should().Be(entity.EventType);
            result.EventData.Should().Be(entity.EventData);
            result.IpAddress.Should().BeNull();
            result.UserAgent.Should().BeNull();
            result.ReferrerUrl.Should().BeNull();
            result.CreatedAt.Should().Be(entity.CreatedAt);
        }

        #endregion

        #region MapToSummaryDto Tests

        [Fact]
        public void MapToSummaryDto_WithValidEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToSummaryDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.SessionId.Should().Be(entity.SessionId);
            result.EventType.Should().Be(entity.EventType);
            result.CreatedAt.Should().Be(entity.CreatedAt);
        }

        [Fact]
        public void MapToSummaryDto_ShouldOnlyIncludeSummaryProperties()
        {
            // Arrange
            var entity = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToSummaryDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.SessionId.Should().Be(entity.SessionId);
            result.EventType.Should().Be(entity.EventType);
            result.CreatedAt.Should().Be(entity.CreatedAt);
            // Verify that UserId is not included in summary
            result.GetType().GetProperty("UserId").Should().BeNull();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void MapToEntity_ThenMapToResponseDto_ShouldPreserveData()
        {
            // Arrange
            var request = new UserAnalyticsCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com"
            };

            // Act
            var entity = _mapper.MapToEntity(request);
            var response = _mapper.MapToResponseDto(entity);

            // Assert
            response.UserId.Should().Be(request.UserId);
            response.SessionId.Should().Be(request.SessionId);
            response.EventType.Should().Be(request.EventType);
            response.EventData.Should().Be(request.EventData);
            response.IpAddress.Should().Be(request.IpAddress);
            response.UserAgent.Should().Be(request.UserAgent);
            response.ReferrerUrl.Should().Be(request.ReferrerUrl);
        }

        [Fact]
        public void MapToEntity_ThenMapToSummaryDto_ShouldPreserveSummaryData()
        {
            // Arrange
            var request = new UserAnalyticsCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com"
            };

            // Act
            var entity = _mapper.MapToEntity(request);
            var summary = _mapper.MapToSummaryDto(entity);

            // Assert
            summary.SessionId.Should().Be(request.SessionId);
            summary.EventType.Should().Be(request.EventType);
        }

        #endregion
    }
}
