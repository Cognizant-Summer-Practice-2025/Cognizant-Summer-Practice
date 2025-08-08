using System;
using BackendMessages.Models;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Models
{
    public class MessageReportModelTests
    {
        [Fact]
        public void MessageReport_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var messageReport = new MessageReport();

            // Assert
            messageReport.Id.Should().NotBeEmpty(); // Has default Guid.NewGuid()
            messageReport.Reason.Should().BeEmpty();
            messageReport.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            messageReport.Message.Should().BeNull(); // Has default null! Message
        }

        [Fact]
        public void MessageReport_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";
            var createdAt = DateTime.UtcNow.AddDays(-1);

            // Act
            var messageReport = new MessageReport
            {
                MessageId = messageId,
                ReportedByUserId = reportedByUserId,
                Reason = reason,
                CreatedAt = createdAt
            };

            // Assert
            messageReport.MessageId.Should().Be(messageId);
            messageReport.ReportedByUserId.Should().Be(reportedByUserId);
            messageReport.Reason.Should().Be(reason);
            messageReport.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void MessageReport_WithMessage_ShouldSetNavigationProperty()
        {
            // Arrange
            var message = new Message { Id = Guid.NewGuid() };
            var messageReport = new MessageReport();

            // Act
            messageReport.Message = message;

            // Assert
            messageReport.Message.Should().Be(message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Violence")]
        [InlineData("Fake news")]
        [InlineData("Copyright violation")]
        public void MessageReport_WithDifferentReasons_ShouldSetCorrectly(string reason)
        {
            // Arrange
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = reason;

            // Assert
            messageReport.Reason.Should().Be(reason);
        }

        [Fact]
        public void MessageReport_WithLongReason_ShouldBeValid()
        {
            // Arrange
            var longReason = new string('a', 500);
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = longReason;

            // Assert
            messageReport.Reason.Should().Be(longReason);
        }

        [Fact]
        public void MessageReport_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var messageReport = new MessageReport
            {
                MessageId = Guid.Empty,
                ReportedByUserId = Guid.Empty
            };

            // Act & Assert
            messageReport.MessageId.Should().Be(Guid.Empty);
            messageReport.ReportedByUserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MessageReport_WithSameMessageAndReporter_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var messageReport = new MessageReport
            {
                MessageId = Guid.NewGuid(),
                ReportedByUserId = userId
            };

            // Act & Assert
            messageReport.ReportedByUserId.Should().Be(userId);
        }

        [Fact]
        public void MessageReport_WithDifferentTimestamps_ShouldBeValid()
        {
            // Arrange
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var messageReport = new MessageReport();

            // Act
            messageReport.CreatedAt = createdAt;

            // Assert
            messageReport.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void MessageReport_WithNullMessage_ShouldBeValid()
        {
            // Arrange
            var messageReport = new MessageReport();

            // Act
            messageReport.Message = null!;

            // Assert
            messageReport.Message.Should().BeNull();
        }

        [Fact]
        public void MessageReport_WithSpecialCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var specialReason = "Content with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = specialReason;

            // Assert
            messageReport.Reason.Should().Be(specialReason);
        }

        [Fact]
        public void MessageReport_WithUnicodeCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var unicodeReason = "Content with unicode: 🚀🌟💻📱🎉";
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = unicodeReason;

            // Assert
            messageReport.Reason.Should().Be(unicodeReason);
        }

        [Fact]
        public void MessageReport_WithWhitespaceOnlyReason_ShouldBeValid()
        {
            // Arrange
            var whitespaceReason = "   ";
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = whitespaceReason;

            // Assert
            messageReport.Reason.Should().Be(whitespaceReason);
        }

        [Fact]
        public void MessageReport_WithNewLinesInReason_ShouldBeValid()
        {
            // Arrange
            var multilineReason = "Line 1\nLine 2\nLine 3";
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = multilineReason;

            // Assert
            messageReport.Reason.Should().Be(multilineReason);
        }

        [Fact]
        public void MessageReport_WithTabCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var tabReason = "Content\twith\ttabs";
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = tabReason;

            // Assert
            messageReport.Reason.Should().Be(tabReason);
        }

        [Fact]
        public void MessageReport_WithVeryLongReason_ShouldBeValid()
        {
            // Arrange
            var veryLongReason = new string('x', 10000);
            var messageReport = new MessageReport();

            // Act
            messageReport.Reason = veryLongReason;

            // Assert
            messageReport.Reason.Should().Be(veryLongReason);
        }

        [Fact]
        public void MessageReport_WithFutureTimestamp_ShouldBeValid()
        {
            // Arrange
            var futureTimestamp = DateTime.UtcNow.AddDays(1);
            var messageReport = new MessageReport();

            // Act
            messageReport.CreatedAt = futureTimestamp;

            // Assert
            messageReport.CreatedAt.Should().Be(futureTimestamp);
        }

        [Fact]
        public void MessageReport_WithPastTimestamp_ShouldBeValid()
        {
            // Arrange
            var pastTimestamp = DateTime.UtcNow.AddDays(-30);
            var messageReport = new MessageReport();

            // Act
            messageReport.CreatedAt = pastTimestamp;

            // Assert
            messageReport.CreatedAt.Should().Be(pastTimestamp);
        }
    }
}
