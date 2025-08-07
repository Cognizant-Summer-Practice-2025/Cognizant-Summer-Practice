using Xunit;
using FluentAssertions;
using backend_user.DTO.UserReport.Request;
using backend_user.DTO.UserReport.Response;
using backend_user.DTO.User.Response;

namespace backend_user.tests.DTO
{
    public class UserReportDTOTests
    {
        #region UserReportCreateRequestDto Tests

        [Fact]
        public void UserReportCreateRequestDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var request = new UserReportCreateRequestDto();

            // Assert
            request.UserId.Should().BeEmpty();
            request.ReportedByUserId.Should().BeEmpty();
            request.Reason.Should().BeEmpty();
        }

        [Fact]
        public void UserReportCreateRequestDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";

            // Act
            var request = new UserReportCreateRequestDto
            {
                UserId = userId,
                ReportedByUserId = reportedByUserId,
                Reason = reason
            };

            // Assert
            request.UserId.Should().Be(userId);
            request.ReportedByUserId.Should().Be(reportedByUserId);
            request.Reason.Should().Be(reason);
        }

        [Theory]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Fake profile")]
        [InlineData("Copyright violation")]
        public void UserReportCreateRequestDto_WithDifferentReasons_ShouldSetCorrectly(string reason)
        {
            // Act
            var request = new UserReportCreateRequestDto
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = reason
            };

            // Assert
            request.Reason.Should().Be(reason);
        }

        #endregion

        #region UserReportResponseDto Tests

        [Fact]
        public void UserReportResponseDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var response = new UserReportResponseDto();

            // Assert
            response.Id.Should().BeEmpty();
            response.UserId.Should().BeEmpty();
            response.ReportedByUserId.Should().BeEmpty();
            response.Reason.Should().BeEmpty();
            response.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void UserReportResponseDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";
            var createdAt = DateTime.UtcNow;

            // Act
            var response = new UserReportResponseDto
            {
                Id = id,
                UserId = userId,
                ReportedByUserId = reportedByUserId,
                Reason = reason,
                CreatedAt = createdAt
            };

            // Assert
            response.Id.Should().Be(id);
            response.UserId.Should().Be(userId);
            response.ReportedByUserId.Should().Be(reportedByUserId);
            response.Reason.Should().Be(reason);
            response.CreatedAt.Should().Be(createdAt);
        }

        #endregion

        #region UserReportSummaryDto Tests

        [Fact]
        public void UserReportSummaryDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var summary = new UserReportSummaryDto();

            // Assert
            summary.Id.Should().BeEmpty();
            summary.UserId.Should().BeEmpty();
            summary.ReportedByUserId.Should().BeEmpty();
            summary.Reason.Should().BeEmpty();
            summary.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void UserReportSummaryDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";
            var createdAt = DateTime.UtcNow;

            // Act
            var summary = new UserReportSummaryDto
            {
                Id = id,
                UserId = userId,
                ReportedByUserId = reportedByUserId,
                Reason = reason,
                CreatedAt = createdAt
            };

            // Assert
            summary.Id.Should().Be(id);
            summary.UserId.Should().Be(userId);
            summary.ReportedByUserId.Should().Be(reportedByUserId);
            summary.Reason.Should().Be(reason);
            summary.CreatedAt.Should().Be(createdAt);
        }

        #endregion

        #region UserReportWithDetailsDto Tests

        [Fact]
        public void UserReportWithDetailsDto_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var details = new UserReportWithDetailsDto();

            // Assert
            details.Id.Should().BeEmpty();
            details.User.Should().NotBeNull();
            details.ReportedByUser.Should().NotBeNull();
            details.Reason.Should().BeEmpty();
            details.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void UserReportWithDetailsDto_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var reason = "Inappropriate content";
            var createdAt = DateTime.UtcNow;
            var user = new UserSummaryDto
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                FirstName = "John",
                LastName = "Doe",
                ProfessionalTitle = "Developer",
                AvatarUrl = "https://example.com/avatar.jpg"
            };
            var reportedByUser = new UserSummaryDto
            {
                Id = Guid.NewGuid(),
                Username = "reporter",
                FirstName = "Jane",
                LastName = "Smith",
                ProfessionalTitle = "Manager",
                AvatarUrl = "https://example.com/reporter.jpg"
            };

            // Act
            var details = new UserReportWithDetailsDto
            {
                Id = id,
                User = user,
                ReportedByUser = reportedByUser,
                Reason = reason,
                CreatedAt = createdAt
            };

            // Assert
            details.Id.Should().Be(id);
            details.User.Should().Be(user);
            details.ReportedByUser.Should().Be(reportedByUser);
            details.Reason.Should().Be(reason);
            details.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void UserReportWithDetailsDto_WithNullUser_ShouldHandleCorrectly()
        {
            // Act
            var details = new UserReportWithDetailsDto
            {
                Id = Guid.NewGuid(),
                User = null!,
                ReportedByUser = new UserSummaryDto(),
                Reason = "Test reason",
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            details.User.Should().BeNull();
        }

        #endregion
    }
}
