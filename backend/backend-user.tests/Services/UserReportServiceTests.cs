using Xunit;
using FluentAssertions;
using Moq;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Repositories;
using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.DTO.User.Response;
using backend_user.Models;
using backend_user.tests.Helpers;

namespace backend_user.tests.Services
{
    public class UserReportServiceTests
    {
        private readonly Mock<IUserReportRepository> _mockRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserReportMapper> _mockMapper;
        private readonly UserReportService _service;

        public UserReportServiceTests()
        {
            _mockRepository = new Mock<IUserReportRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IUserReportMapper>();
            _service = new UserReportService(_mockRepository.Object, _mockUserRepository.Object, _mockMapper.Object);
        }

        #region CreateUserReportAsync Tests

        [Fact]
        public async Task CreateUserReportAsync_WithValidData_ShouldCreateAndReturnResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content"
            };

            var reportedUser = TestDataFactory.CreateValidUser();
            reportedUser.Id = userId;
            var reporter = TestDataFactory.CreateValidUser();
            reporter.Id = reporterId;

            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow
            };

            var response = new UserReportResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ReportedByUserId = entity.ReportedByUserId,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt
            };

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(reportedUser);
            _mockUserRepository.Setup(r => r.GetUserById(reporterId)).ReturnsAsync(reporter);
            _mockRepository.Setup(r => r.HasUserReportedUserAsync(userId, reporterId)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.MapToEntity(request)).Returns(entity);
            _mockRepository.Setup(r => r.CreateUserReportAsync(entity)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);

            // Act
            var result = await _service.CreateUserReportAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(response);
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(userId);
            result.ReportedByUserId.Should().Be(reporterId);
            result.Reason.Should().Be("Inappropriate content");

            _mockUserRepository.Verify(r => r.GetUserById(userId), Times.Once);
            _mockUserRepository.Verify(r => r.GetUserById(reporterId), Times.Once);
            _mockRepository.Verify(r => r.HasUserReportedUserAsync(userId, reporterId), Times.Once);
            _mockMapper.Verify(m => m.MapToEntity(request), Times.Once);
            _mockRepository.Verify(r => r.CreateUserReportAsync(entity), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(entity), Times.Once);
        }

        [Fact]
        public async Task CreateUserReportAsync_WithNonExistentReportedUser_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content"
            };

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateUserReportAsync(userId, request));

            exception.Message.Should().Be($"User with ID {userId} not found");

            _mockUserRepository.Verify(r => r.GetUserById(userId), Times.Once);
            _mockUserRepository.Verify(r => r.GetUserById(reporterId), Times.Never);
            _mockRepository.Verify(r => r.HasUserReportedUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _mockMapper.Verify(m => m.MapToEntity(It.IsAny<UserReportCreateRequestDto>()), Times.Never);
            _mockRepository.Verify(r => r.CreateUserReportAsync(It.IsAny<UserReport>()), Times.Never);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserReportAsync_WithNonExistentReporter_ShouldThrowArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content"
            };

            var reportedUser = TestDataFactory.CreateValidUser();
            reportedUser.Id = userId;

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(reportedUser);
            _mockUserRepository.Setup(r => r.GetUserById(reporterId)).ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateUserReportAsync(userId, request));

            exception.Message.Should().Be($"Reporter with ID {reporterId} not found");

            _mockUserRepository.Verify(r => r.GetUserById(userId), Times.Once);
            _mockUserRepository.Verify(r => r.GetUserById(reporterId), Times.Once);
            _mockRepository.Verify(r => r.HasUserReportedUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _mockMapper.Verify(m => m.MapToEntity(It.IsAny<UserReportCreateRequestDto>()), Times.Never);
            _mockRepository.Verify(r => r.CreateUserReportAsync(It.IsAny<UserReport>()), Times.Never);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserReportAsync_WithAlreadyReportedUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content"
            };

            var reportedUser = TestDataFactory.CreateValidUser();
            reportedUser.Id = userId;
            var reporter = TestDataFactory.CreateValidUser();
            reporter.Id = reporterId;

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(reportedUser);
            _mockUserRepository.Setup(r => r.GetUserById(reporterId)).ReturnsAsync(reporter);
            _mockRepository.Setup(r => r.HasUserReportedUserAsync(userId, reporterId)).ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateUserReportAsync(userId, request));

            exception.Message.Should().Be("You have already reported this user");

            _mockUserRepository.Verify(r => r.GetUserById(userId), Times.Once);
            _mockUserRepository.Verify(r => r.GetUserById(reporterId), Times.Once);
            _mockRepository.Verify(r => r.HasUserReportedUserAsync(userId, reporterId), Times.Once);
            _mockMapper.Verify(m => m.MapToEntity(It.IsAny<UserReportCreateRequestDto>()), Times.Never);
            _mockRepository.Verify(r => r.CreateUserReportAsync(It.IsAny<UserReport>()), Times.Never);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserReportAsync_WithSelfReporting_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = userId, // Same user
                Reason = "Inappropriate content"
            };

            var user = TestDataFactory.CreateValidUser();
            user.Id = userId;

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(user);
            _mockRepository.Setup(r => r.HasUserReportedUserAsync(userId, userId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateUserReportAsync(userId, request));

            exception.Message.Should().Be("You cannot report yourself");

            _mockUserRepository.Verify(r => r.GetUserById(userId), Times.Exactly(2)); // Called twice for same user
            _mockRepository.Verify(r => r.HasUserReportedUserAsync(userId, userId), Times.Once); // Called before self-report check
            _mockMapper.Verify(m => m.MapToEntity(It.IsAny<UserReportCreateRequestDto>()), Times.Never);
            _mockRepository.Verify(r => r.CreateUserReportAsync(It.IsAny<UserReport>()), Times.Never);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        #endregion

        #region GetUserReportByIdAsync Tests

        [Fact]
        public async Task GetUserReportByIdAsync_WithExistingId_ShouldReturnResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = TestDataFactory.CreateValidUserReport();
            entity.Id = id;

            var response = new UserReportResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ReportedByUserId = entity.ReportedByUserId,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt
            };

            _mockRepository.Setup(r => r.GetUserReportByIdAsync(id)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);

            // Act
            var result = await _service.GetUserReportByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(response);
            result!.Id.Should().Be(id);

            _mockRepository.Verify(r => r.GetUserReportByIdAsync(id), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(entity), Times.Once);
        }

        [Fact]
        public async Task GetUserReportByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetUserReportByIdAsync(id)).ReturnsAsync((UserReport?)null);

            // Act
            var result = await _service.GetUserReportByIdAsync(id);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(r => r.GetUserReportByIdAsync(id), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        #endregion

        #region GetUserReportsAsync Tests

        [Fact]
        public async Task GetUserReportsAsync_WithExistingUser_ShouldReturnResponses()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var entities = new List<UserReport>
            {
                TestDataFactory.CreateValidUserReport(userId),
                TestDataFactory.CreateValidUserReport(userId)
            };

            var responses = entities.Select(e => new UserReportResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                ReportedByUserId = e.ReportedByUserId,
                Reason = e.Reason,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetUserReportsAsync(userId)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetUserReportsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetUserReportsAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetUserReportsAsync_WithNonExistentUser_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetUserReportsAsync(userId)).ReturnsAsync(new List<UserReport>());

            // Act
            var result = await _service.GetUserReportsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockRepository.Verify(r => r.GetUserReportsAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        #endregion

        #region GetReportsByReporterAsync Tests

        [Fact]
        public async Task GetReportsByReporterAsync_WithExistingReporter_ShouldReturnResponses()
        {
            // Arrange
            var reporterId = Guid.NewGuid();
            var entities = new List<UserReport>
            {
                TestDataFactory.CreateValidUserReport(Guid.NewGuid(), reporterId),
                TestDataFactory.CreateValidUserReport(Guid.NewGuid(), reporterId)
            };

            var responses = entities.Select(e => new UserReportResponseDto
            {
                Id = e.Id,
                UserId = e.UserId,
                ReportedByUserId = e.ReportedByUserId,
                Reason = e.Reason,
                CreatedAt = e.CreatedAt
            }).ToList();

            _mockRepository.Setup(r => r.GetReportsByReporterAsync(reporterId)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToResponseDto(entities[1])).Returns(responses[1]);

            // Act
            var result = await _service.GetReportsByReporterAsync(reporterId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetReportsByReporterAsync(reporterId), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetReportsByReporterAsync_WithNonExistentReporter_ShouldReturnEmptyList()
        {
            // Arrange
            var reporterId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetReportsByReporterAsync(reporterId)).ReturnsAsync(new List<UserReport>());

            // Act
            var result = await _service.GetReportsByReporterAsync(reporterId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockRepository.Verify(r => r.GetReportsByReporterAsync(reporterId), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        #endregion

        #region GetAllUserReportsAsync Tests

        [Fact]
        public async Task GetAllUserReportsAsync_WithExistingReports_ShouldReturnResponses()
        {
            // Arrange
            var entities = new List<UserReport>
            {
                TestDataFactory.CreateValidUserReport(),
                TestDataFactory.CreateValidUserReport()
            };

            var responses = entities.Select(e => new UserReportWithDetailsDto
            {
                Id = e.Id,
                UserId = e.UserId,
                ReportedByUserId = e.ReportedByUserId,
                Reason = e.Reason,
                CreatedAt = e.CreatedAt,
                User = new UserSummaryDto(),
                ReportedByUser = new UserSummaryDto()
            }).ToList();

            var reporterUser = TestDataFactory.CreateValidUser();
            reporterUser.Id = entities[0].ReportedByUserId;
            var reporterUser2 = TestDataFactory.CreateValidUser();
            reporterUser2.Id = entities[1].ReportedByUserId;

            _mockRepository.Setup(r => r.GetAllUserReportsAsync()).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.MapToWithDetailsDto(entities[0])).Returns(responses[0]);
            _mockMapper.Setup(m => m.MapToWithDetailsDto(entities[1])).Returns(responses[1]);
            _mockUserRepository.Setup(r => r.GetUserById(entities[0].ReportedByUserId)).ReturnsAsync(reporterUser);
            _mockUserRepository.Setup(r => r.GetUserById(entities[1].ReportedByUserId)).ReturnsAsync(reporterUser2);

            // Act
            var result = await _service.GetAllUserReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(responses);

            _mockRepository.Verify(r => r.GetAllUserReportsAsync(), Times.Once);
            _mockMapper.Verify(m => m.MapToWithDetailsDto(It.IsAny<UserReport>()), Times.Exactly(2));
            _mockUserRepository.Verify(r => r.GetUserById(It.IsAny<Guid>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetAllUserReportsAsync_WithNoReports_ShouldReturnEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllUserReportsAsync()).ReturnsAsync(new List<UserReport>());

            // Act
            var result = await _service.GetAllUserReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockRepository.Verify(r => r.GetAllUserReportsAsync(), Times.Once);
            _mockMapper.Verify(m => m.MapToResponseDto(It.IsAny<UserReport>()), Times.Never);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateAndRetrieve_ShouldWorkCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content"
            };

            var reportedUser = TestDataFactory.CreateValidUser();
            reportedUser.Id = userId;
            var reporter = TestDataFactory.CreateValidUser();
            reporter.Id = reporterId;

            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow
            };

            var response = new UserReportResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ReportedByUserId = entity.ReportedByUserId,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt
            };

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(reportedUser);
            _mockUserRepository.Setup(r => r.GetUserById(reporterId)).ReturnsAsync(reporter);
            _mockRepository.Setup(r => r.HasUserReportedUserAsync(userId, reporterId)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.MapToEntity(request)).Returns(entity);
            _mockRepository.Setup(r => r.CreateUserReportAsync(entity)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);
            _mockRepository.Setup(r => r.GetUserReportByIdAsync(entity.Id)).ReturnsAsync(entity);

            // Act
            var createdReport = await _service.CreateUserReportAsync(userId, request);
            var retrievedReport = await _service.GetUserReportByIdAsync(entity.Id);

            // Assert
            createdReport.Should().NotBeNull();
            retrievedReport.Should().NotBeNull();
            retrievedReport!.Id.Should().Be(createdReport.Id);
            retrievedReport.UserId.Should().Be(createdReport.UserId);
            retrievedReport.ReportedByUserId.Should().Be(createdReport.ReportedByUserId);
            retrievedReport.Reason.Should().Be(createdReport.Reason);
        }

        #endregion

        #region Edge Cases

        [Theory]
        [InlineData("")]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Fake profile")]
        [InlineData("Copyright violation")]
        public async Task CreateUserReportAsync_WithDifferentReasons_ShouldWorkCorrectly(string reason)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reporterId = Guid.NewGuid();
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = reason
            };

            var reportedUser = TestDataFactory.CreateValidUser();
            reportedUser.Id = userId;
            var reporter = TestDataFactory.CreateValidUser();
            reporter.Id = reporterId;

            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReportedByUserId = reporterId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };

            var response = new UserReportResponseDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ReportedByUserId = entity.ReportedByUserId,
                Reason = entity.Reason,
                CreatedAt = entity.CreatedAt
            };

            _mockUserRepository.Setup(r => r.GetUserById(userId)).ReturnsAsync(reportedUser);
            _mockUserRepository.Setup(r => r.GetUserById(reporterId)).ReturnsAsync(reporter);
            _mockRepository.Setup(r => r.HasUserReportedUserAsync(userId, reporterId)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.MapToEntity(request)).Returns(entity);
            _mockRepository.Setup(r => r.CreateUserReportAsync(entity)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.MapToResponseDto(entity)).Returns(response);

            // Act
            var result = await _service.CreateUserReportAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.Reason.Should().Be(reason);
        }

        #endregion
    }
}
