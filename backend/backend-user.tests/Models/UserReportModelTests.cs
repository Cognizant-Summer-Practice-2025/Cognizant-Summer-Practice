using Xunit;
using FluentAssertions;
using backend_user.Models;

namespace backend_user.tests.Models
{
    public class UserReportModelTests
    {
        [Fact]
        public void UserReport_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var report = new UserReport();

            // Assert
            report.Id.Should().NotBeEmpty();
            report.UserId.Should().BeEmpty();
            report.ReportedByUserId.Should().BeEmpty();
            report.Reason.Should().BeEmpty();
            report.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UserReport_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";

            // Act
            var report = new UserReport
            {
                UserId = userId,
                ReportedByUserId = reportedByUserId,
                Reason = reason
            };

            // Assert
            report.UserId.Should().Be(userId);
            report.ReportedByUserId.Should().Be(reportedByUserId);
            report.Reason.Should().Be(reason);
        }

        [Fact]
        public void UserReport_NavigationProperty_ShouldAllowUserAssignment()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var report = new UserReport
            {
                UserId = user.Id,
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Test reason"
            };

            // Act
            report.User = user;

            // Assert
            report.User.Should().Be(user);
            report.UserId.Should().Be(user.Id);
        }

        [Theory]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Fake profile")]
        [InlineData("Copyright violation")]
        public void UserReport_WithDifferentReasons_ShouldSetCorrectly(string reason)
        {
            // Act
            var report = new UserReport
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = reason
            };

            // Assert
            report.Reason.Should().Be(reason);
        }

        [Fact]
        public void UserReport_PropertySetters_ShouldWorkCorrectly()
        {
            // Arrange
            var report = new UserReport();
            var userId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Updated reason";

            // Act
            report.UserId = userId;
            report.ReportedByUserId = reportedByUserId;
            report.Reason = reason;

            // Assert
            report.UserId.Should().Be(userId);
            report.ReportedByUserId.Should().Be(reportedByUserId);
            report.Reason.Should().Be(reason);
        }

        [Fact]
        public void UserReport_WithEmptyReason_ShouldBeValid()
        {
            // Act
            var report = new UserReport
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = ""
            };

            // Assert
            report.Reason.Should().BeEmpty();
        }

        [Fact]
        public void UserReport_WithLongReason_ShouldBeValid()
        {
            // Arrange
            var longReason = new string('a', 1000); // 1000 character reason

            // Act
            var report = new UserReport
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = longReason
            };

            // Assert
            report.Reason.Should().Be(longReason);
            report.Reason.Length.Should().Be(1000);
        }

        [Fact]
        public void UserReport_SameUserReportingThemselves_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var report = new UserReport
            {
                UserId = userId,
                ReportedByUserId = userId, // Same user reporting themselves
                Reason = "Self report"
            };

            // Assert
            report.UserId.Should().Be(userId);
            report.ReportedByUserId.Should().Be(userId);
        }

        [Fact]
        public void UserReport_CreatedAt_ShouldBeSetToUtcNow()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var report = new UserReport
            {
                UserId = Guid.NewGuid(),
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Test reason"
            };

            var afterCreation = DateTime.UtcNow;

            // Assert
            report.CreatedAt.Should().BeOnOrAfter(beforeCreation);
            report.CreatedAt.Should().BeOnOrBefore(afterCreation);
        }
    }
} 
