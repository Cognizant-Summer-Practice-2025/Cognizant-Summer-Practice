using Xunit;
using FluentAssertions;
using backend_user.Services.Mappers;
using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.Models;

namespace backend_user.tests.Services
{
    public class UserReportMapperTests
    {
        private readonly IUserReportMapper _mapper;

        public UserReportMapperTests()
        {
            _mapper = new UserReportMapper();
        }

        #region MapToEntity Tests

        [Fact]
        public void MapToEntity_WithValidRequest_ShouldMapCorrectly()
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(request.UserId);
            result.ReportedByUserId.Should().Be(request.ReportedByUserId);
            result.Reason.Should().Be(request.Reason);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void MapToEntity_WithEmptyReason_ShouldMapCorrectly()
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = ""
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.Reason.Should().Be("");
        }

        [Theory]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Fake profile")]
        [InlineData("Copyright violation")]
        public void MapToEntity_WithDifferentReasons_ShouldMapCorrectly(string reason)
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = reason
            };

            // Act
            var result = _mapper.MapToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.Reason.Should().Be(reason);
        }

        #endregion

        #region MapToResponseDto Tests

        [Fact]
        public void MapToResponseDto_WithValidEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToResponseDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(entity.UserId);
            result.ReportedByUserId.Should().Be(entity.ReportedByUserId);
            result.Reason.Should().Be(entity.Reason);
            result.CreatedAt.Should().Be(entity.CreatedAt);
        }

        [Fact]
        public void MapToResponseDto_WithEmptyReason_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToResponseDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Reason.Should().Be("");
        }

        #endregion

        #region MapToSummaryDto Tests

        [Fact]
        public void MapToSummaryDto_WithValidEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = _mapper.MapToSummaryDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.UserId.Should().Be(entity.UserId);
            result.ReportedByUserId.Should().Be(entity.ReportedByUserId);
            result.Reason.Should().Be(entity.Reason);
            result.CreatedAt.Should().Be(entity.CreatedAt);
        }

        #endregion

        #region MapToWithDetailsDto Tests

        [Fact]
        public void MapToWithDetailsDto_WithValidEntity_ShouldMapCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Developer",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow,
                User = user
            };

            // Act
            var result = _mapper.MapToWithDetailsDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.Reason.Should().Be(entity.Reason);
            result.CreatedAt.Should().Be(entity.CreatedAt);
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Username.Should().Be(user.Username);
            result.User.FirstName.Should().Be(user.FirstName);
            result.User.LastName.Should().Be(user.LastName);
            result.User.ProfessionalTitle.Should().Be(user.ProfessionalTitle);
            result.User.AvatarUrl.Should().Be(user.AvatarUrl);
        }

        [Fact]
        public void MapToWithDetailsDto_WithNullUser_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow,
                User = null!
            };

            // Act
            var result = _mapper.MapToWithDetailsDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);
            result.Reason.Should().Be(entity.Reason);
            result.CreatedAt.Should().Be(entity.CreatedAt);
            result.User.Should().NotBeNull();
            result.User.Id.Should().BeEmpty();
        }

        [Fact]
        public void MapToWithDetailsDto_WithEmptyUserProperties_ShouldMapCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "",
                FirstName = "",
                LastName = "",
                ProfessionalTitle = "",
                AvatarUrl = ""
            };

            var entity = new UserReport
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow,
                User = user
            };

            // Act
            var result = _mapper.MapToWithDetailsDto(entity);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Username.Should().Be("");
            result.User.FirstName.Should().Be("");
            result.User.LastName.Should().Be("");
            result.User.ProfessionalTitle.Should().Be("");
            result.User.AvatarUrl.Should().Be("");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void MapToEntity_ThenMapToResponseDto_ShouldPreserveData()
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var entity = _mapper.MapToEntity(request);
            var response = _mapper.MapToResponseDto(entity);

            // Assert
            response.UserId.Should().Be(request.UserId);
            response.ReportedByUserId.Should().Be(request.ReportedByUserId);
            response.Reason.Should().Be(request.Reason);
        }

        [Fact]
        public void MapToEntity_ThenMapToSummaryDto_ShouldPreserveData()
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var entity = _mapper.MapToEntity(request);
            var summary = _mapper.MapToSummaryDto(entity);

            // Assert
            summary.UserId.Should().Be(request.UserId);
            summary.ReportedByUserId.Should().Be(request.ReportedByUserId);
            summary.Reason.Should().Be(request.Reason);
        }

        [Fact]
        public void MapToEntity_ThenMapToWithDetailsDto_ShouldPreserveData()
        {
            // Arrange
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var entity = _mapper.MapToEntity(request);
            var details = _mapper.MapToWithDetailsDto(entity);

            // Assert
            details.Reason.Should().Be(request.Reason);
        }

        #endregion
    }
}
