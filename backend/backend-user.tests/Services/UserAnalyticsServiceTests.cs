using Xunit;
using FluentAssertions;
using Moq;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Repositories;
using backend_user.DTO.UserAnalytics.Request;
using backend_user.DTO.UserAnalytics.Response;
using backend_user.Models;
using System.Net;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class UserAnalyticsServiceTests
    {
        private readonly Mock<IUserAnalyticsRepository> _mockRepository;
        private readonly Mock<IUserAnalyticsMapper> _mockMapper;
        private readonly UserAnalyticsService _service;

        public UserAnalyticsServiceTests()
        {
            _mockRepository = new Mock<IUserAnalyticsRepository>();
            _mockMapper = new Mock<IUserAnalyticsMapper>();
            _service = new UserAnalyticsService(_mockRepository.Object, _mockMapper.Object);
        }

        #region CreateUserAnalyticsAsync Tests

        [Fact]
        public async Task CreateUserAnalyticsAsync_WithValidRequest_ShouldCreateAndReturnResponse()
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

            var entity = new UserAnalytics
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                SessionId = request.SessionId,
                EventType = request.EventType,
                EventData = request.EventData,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                ReferrerUrl = request.ReferrerUrl,
                CreatedAt = DateTime.UtcNow
            };

            var response = new UserAnalyticsResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                SessionId = entity.SessionId,
                EventType = entity.EventType,
                EventData = entity.EventData,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                ReferrerUrl = entity.ReferrerUrl,
                CreatedAt = entity.CreatedAt
            };

            _mockMapper.Setup(m => m.MapToEntity(request)).Returns(entity);
            _mockRepository.Setup(r => r.CreateUserAnalyticsAsync(entity)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);

            // Act
            var result = await _service.CreateUserAnalyticsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(response);
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(request.UserId);
            result.SessionId.Should().Be(request.SessionId);
            result.EventType.Should().Be(request.EventType);
            result.EventData.Should().Be(request.EventData);
            result.IpAddress.Should().Be(request.IpAddress);
            result.UserAgent.Should().Be(request.UserAgent);
            result.ReferrerUrl.Should().Be(request.ReferrerUrl);

            _mockMapper.Verify(m => m.MapToEntity(request), Times.Once);
            _mockRepository.Verify(r => r.CreateUserAnalyticsAsync(entity), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(entity), Times.Once);
        }

        #endregion

        #region GetUserAnalyticsByIdAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByIdAsync_WithExistingId_ShouldReturnResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = TestDataFactory.CreateValidUserAnalytics();
            entity.Id = id;

            var response = new UserAnalyticsResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                SessionId = entity.SessionId,
                EventType = entity.EventType,
                EventData = entity.EventData,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                ReferrerUrl = entity.ReferrerUrl,
                CreatedAt = entity.CreatedAt
            };

            _mockRepository.Setup(r => r.GetUserAnalyticsByIdAsync(id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);

            // Act
            var result = await _service.GetUserAnalyticsByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(response);
            result!.Id.Should().Be(id);

            _mockRepository.Verify(r => r.GetUserAnalyticsByIdAsync(id), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(entity), Times.Once);
        }

        [Fact]
        public async Task GetUserAnalyticsByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetUserAnalyticsByIdAsync(id)).ReturnsAsync((UserAnalytics?)null);

            // Act
            var result = await _service.GetUserAnalyticsByIdAsync(id);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(r => r.GetUserAnalyticsByIdAsync(id), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserAnalytics>()), Times.Never);
        }

        #endregion

        #region GetUserAnalyticsAsync Tests

        [Fact]
        public async Task GetUserAnalyticsAsync_WithExistingUser_ShouldReturnResponses()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(userId),
                TestDataFactory.CreateValidUserAnalytics(userId)
            };

            var responses = entities.Select(e => new UserAnalyticsResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                SessionId = e.SessionId,
                EventType = e.EventType,
                EventData = e.EventData,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                ReferrerUrl = e.ReferrerUrl,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserAnalyticsAsync(userId)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetUserAnalyticsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetUserAnalyticsAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserAnalytics>()), Times.Exactly(2));
        }

        #endregion

        #region GetUserAnalyticsBySessionAsync Tests

        [Fact]
        public async Task GetUserAnalyticsBySessionAsync_WithExistingSession_ShouldReturnResponses()
        {
            // Arrange
            var sessionId = "session-123";
            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(),
                TestDataFactory.CreateValidUserAnalytics()
            };

            var responses = entities.Select(e => new UserAnalyticsResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                SessionId = e.SessionId,
                EventType = e.EventType,
                EventData = e.EventData,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                ReferrerUrl = e.ReferrerUrl,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserAnalyticsBySessionAsync(sessionId)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetUserAnalyticsBySessionAsync(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetUserAnalyticsBySessionAsync(sessionId), Times.Once);
        }

        #endregion

        #region GetUserAnalyticsByEventTypeAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByEventTypeAsync_WithExistingEventType_ShouldReturnResponses()
        {
            // Arrange
            var eventType = "page_view";
            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(),
                TestDataFactory.CreateValidUserAnalytics()
            };

            var responses = entities.Select(e => new UserAnalyticsResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                SessionId = e.SessionId,
                EventType = e.EventType,
                EventData = e.EventData,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                ReferrerUrl = e.ReferrerUrl,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserAnalyticsByEventTypeAsync(eventType)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetUserAnalyticsByEventTypeAsync(eventType);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetUserAnalyticsByEventTypeAsync(eventType), Times.Once);
        }

        #endregion

        #region GetAllUserAnalyticsAsync Tests

        [Fact]
        public async Task GetAllUserAnalyticsAsync_WithExistingAnalytics_ShouldReturnResponses()
        {
            // Arrange
            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(),
                TestDataFactory.CreateValidUserAnalytics()
            };

            var responses = entities.Select(e => new UserAnalyticsResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                SessionId = e.SessionId,
                EventType = e.EventType,
                EventData = e.EventData,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                ReferrerUrl = e.ReferrerUrl,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetAllUserAnalyticsAsync()).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetAllUserAnalyticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetAllUserAnalyticsAsync(), Times.Once);
        }

        #endregion

        #region GetUserAnalyticsByDateRangeAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByDateRangeAsync_WithAnalyticsInRange_ShouldReturnResponses()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-2);
            var endDate = DateTime.UtcNow.AddDays(1);

            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(userId),
                TestDataFactory.CreateValidUserAnalytics(userId)
            };

            var responses = entities.Select(e => new UserAnalyticsResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                SessionId = e.SessionId,
                EventType = e.EventType,
                EventData = e.EventData,
                IpAddress = e.IpAddress,
                UserAgent = e.UserAgent,
                ReferrerUrl = e.ReferrerUrl,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserAnalyticsByDateRangeAsync(userId, startDate, endDate)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetUserAnalyticsByDateRangeAsync(userId, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetUserAnalyticsByDateRangeAsync(userId, startDate, endDate), Times.Once);
        }

        #endregion

        #region GetUserAnalyticsCountAsync Tests

        [Fact]
        public async Task GetUserAnalyticsCountAsync_WithExistingAnalytics_ShouldReturnCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedCount = 5;

            _mockRepository.Setup(r => r.GetUserAnalyticsCountAsync(userId)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetUserAnalyticsCountAsync(userId);

            // Assert
            result.Should().Be(expectedCount);

            _mockRepository.Verify(r => r.GetUserAnalyticsCountAsync(userId), Times.Once);
        }

        #endregion

        #region DeleteUserAnalyticsAsync Tests

        [Fact]
        public async Task DeleteUserAnalyticsAsync_WithExistingAnalytics_ShouldReturnTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteUserAnalyticsAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteUserAnalyticsAsync(id);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(r => r.DeleteUserAnalyticsAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAnalyticsAsync_WithNonExistentAnalytics_ShouldReturnFalse()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.DeleteUserAnalyticsAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _service.DeleteUserAnalyticsAsync(id);

            // Assert
            result.Should().BeFalse();

            _mockRepository.Verify(r => r.DeleteUserAnalyticsAsync(id), Times.Once);
        }

        #endregion

        #region GetUserAnalyticsSummaryAsync Tests

        [Fact]
        public async Task GetUserAnalyticsSummaryAsync_WithExistingAnalytics_ShouldReturnSummaries()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var entities = new List<UserAnalytics>
            {
                TestDataFactory.CreateValidUserAnalytics(userId),
                TestDataFactory.CreateValidUserAnalytics(userId)
            };

            var summaries = entities.Select(e => new UserAnalyticsSummaryDto
            {
                Id = e.Id,
                SessionId = e.SessionId,
                EventType = e.EventType,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserAnalyticsAsync(userId)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToSummaryDto(entities[0])).Returns(summaries[0]);
            _mockMapper.Setup(m => m.MapToSummaryDto(entities[1])).Returns(summaries[1]);

            // Act
            var result = await _service.GetUserAnalyticsSummaryAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(summaries);

            _mockRepository.Verify(r => r.GetUserAnalyticsAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.MapToSummaryDto(It.IsAny<UserAnalytics>()), Times.Exactly(2));
        }

        #endregion
    }
}
