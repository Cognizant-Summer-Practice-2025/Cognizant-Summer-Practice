using System;
using System.ComponentModel.DataAnnotations;
using BackendMessages.DTO.Message.Request;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.DTO.Message.Request
{
    public class MessageRequestDTOTests
    {
        [Fact]
        public void SendMessageRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new SendMessageRequest();

            // Assert
            request.ConversationId.Should().BeEmpty();
            request.SenderId.Should().BeEmpty();
            request.ReceiverId.Should().BeEmpty();
            request.Content.Should().BeEmpty();
            request.ReplyToMessageId.Should().BeNull();
        }

        [Fact]
        public void SendMessageRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var content = "Hello, world!";
            var replyToMessageId = Guid.NewGuid();

            // Act
            var request = new SendMessageRequest
            {
                ConversationId = conversationId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ReplyToMessageId = replyToMessageId
            };

            // Assert
            request.ConversationId.Should().Be(conversationId);
            request.SenderId.Should().Be(senderId);
            request.ReceiverId.Should().Be(receiverId);
            request.Content.Should().Be(content);
            request.ReplyToMessageId.Should().Be(replyToMessageId);
        }

        [Fact]
        public void SendMessageRequest_WithNullReplyToMessageId_ShouldBeValid()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Test message",
                ReplyToMessageId = null
            };

            // Act & Assert
            request.ReplyToMessageId.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("This is a longer message with more content")]
        [InlineData("Message with special chars: !@#$%^&*()")]
        [InlineData("Message with unicode: 🚀🌟💻")]
        public void SendMessageRequest_WithDifferentContent_ShouldSetCorrectly(string content)
        {
            // Arrange
            var request = new SendMessageRequest();

            // Act
            request.Content = content;

            // Assert
            request.Content.Should().Be(content);
        }

        [Fact]
        public void MarkMessageAsReadRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new MarkMessageAsReadRequest();

            // Assert
            request.MessageId.Should().BeEmpty();
            request.UserId.Should().BeEmpty();
        }

        [Fact]
        public void MarkMessageAsReadRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var request = new MarkMessageAsReadRequest
            {
                MessageId = messageId,
                UserId = userId
            };

            // Assert
            request.MessageId.Should().Be(messageId);
            request.UserId.Should().Be(userId);
        }

        [Fact]
        public void MarkMessagesAsReadRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new MarkMessagesAsReadRequest();

            // Assert
            request.ConversationId.Should().BeEmpty();
            request.UserId.Should().BeEmpty();
        }

        [Fact]
        public void MarkMessagesAsReadRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = conversationId,
                UserId = userId
            };

            // Assert
            request.ConversationId.Should().Be(conversationId);
            request.UserId.Should().Be(userId);
        }

        [Fact]
        public void DeleteMessageRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new DeleteMessageRequest();

            // Assert
            request.MessageId.Should().BeEmpty();
            request.UserId.Should().BeEmpty();
        }

        [Fact]
        public void DeleteMessageRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var request = new DeleteMessageRequest
            {
                MessageId = messageId,
                UserId = userId
            };

            // Assert
            request.MessageId.Should().Be(messageId);
            request.UserId.Should().Be(userId);
        }

        [Fact]
        public void ReportMessageRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new ReportMessageRequest();

            // Assert
            request.MessageId.Should().BeEmpty();
            request.ReportedById.Should().BeEmpty();
            request.Reason.Should().BeEmpty();
        }

        [Fact]
        public void ReportMessageRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var reportedById = Guid.NewGuid();
            var reason = "Inappropriate content";

            // Act
            var request = new ReportMessageRequest
            {
                MessageId = messageId,
                ReportedById = reportedById,
                Reason = reason
            };

            // Assert
            request.MessageId.Should().Be(messageId);
            request.ReportedById.Should().Be(reportedById);
            request.Reason.Should().Be(reason);
        }

        [Theory]
        [InlineData("Spam")]
        [InlineData("Harassment")]
        [InlineData("Inappropriate content")]
        [InlineData("Violence")]
        [InlineData("Fake news")]
        [InlineData("Copyright violation")]
        public void ReportMessageRequest_WithDifferentReasons_ShouldSetCorrectly(string reason)
        {
            // Arrange
            var request = new ReportMessageRequest();

            // Act
            request.Reason = reason;

            // Assert
            request.Reason.Should().Be(reason);
        }

        [Fact]
        public void ReportMessageRequest_WithLongReason_ShouldBeValid()
        {
            // Arrange
            var longReason = new string('a', 500);
            var request = new ReportMessageRequest();

            // Act
            request.Reason = longReason;

            // Assert
            request.Reason.Should().Be(longReason);
        }

        [Fact]
        public void ReportMessageRequest_WithSpecialCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var specialReason = "Content with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";
            var request = new ReportMessageRequest();

            // Act
            request.Reason = specialReason;

            // Assert
            request.Reason.Should().Be(specialReason);
        }

        [Fact]
        public void ReportMessageRequest_WithUnicodeCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var unicodeReason = "Content with unicode: 🚀🌟💻📱🎉";
            var request = new ReportMessageRequest();

            // Act
            request.Reason = unicodeReason;

            // Assert
            request.Reason.Should().Be(unicodeReason);
        }

        [Fact]
        public void ReportMessageRequest_WithNewLinesInReason_ShouldBeValid()
        {
            // Arrange
            var multilineReason = "Line 1\nLine 2\nLine 3";
            var request = new ReportMessageRequest();

            // Act
            request.Reason = multilineReason;

            // Assert
            request.Reason.Should().Be(multilineReason);
        }

        [Fact]
        public void ReportMessageRequest_WithTabCharactersInReason_ShouldBeValid()
        {
            // Arrange
            var tabReason = "Content\twith\ttabs";
            var request = new ReportMessageRequest();

            // Act
            request.Reason = tabReason;

            // Assert
            request.Reason.Should().Be(tabReason);
        }

        [Fact]
        public void ReportMessageRequest_WithWhitespaceOnlyReason_ShouldBeValid()
        {
            // Arrange
            var whitespaceReason = "   ";
            var request = new ReportMessageRequest();

            // Act
            request.Reason = whitespaceReason;

            // Assert
            request.Reason.Should().Be(whitespaceReason);
        }

        [Fact]
        public void ReportMessageRequest_WithVeryLongReason_ShouldBeValid()
        {
            // Arrange
            var veryLongReason = new string('x', 500);
            var request = new ReportMessageRequest();

            // Act
            request.Reason = veryLongReason;

            // Assert
            request.Reason.Should().Be(veryLongReason);
        }

        [Fact]
        public void SendMessageRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.Empty,
                SenderId = Guid.Empty,
                ReceiverId = Guid.Empty,
                Content = "Test message"
            };

            // Act & Assert
            request.ConversationId.Should().Be(Guid.Empty);
            request.SenderId.Should().Be(Guid.Empty);
            request.ReceiverId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MarkMessageAsReadRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act & Assert
            request.MessageId.Should().Be(Guid.Empty);
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MarkMessagesAsReadRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act & Assert
            request.ConversationId.Should().Be(Guid.Empty);
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void DeleteMessageRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act & Assert
            request.MessageId.Should().Be(Guid.Empty);
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void ReportMessageRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.Empty,
                ReportedById = Guid.Empty,
                Reason = "Test reason"
            };

            // Act & Assert
            request.MessageId.Should().Be(Guid.Empty);
            request.ReportedById.Should().Be(Guid.Empty);
        }

        [Fact]
        public void SendMessageRequest_WithSameSenderAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = userId,
                ReceiverId = userId,
                Content = "Test message"
            };

            // Act & Assert
            request.SenderId.Should().Be(userId);
            request.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void ReportMessageRequest_WithSameMessageAndReporter_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = userId,
                Reason = "Test reason"
            };

            // Act & Assert
            request.ReportedById.Should().Be(userId);
        }
    }
}
