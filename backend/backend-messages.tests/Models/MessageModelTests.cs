using System;
using System.ComponentModel.DataAnnotations;
using BackendMessages.Models;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Models
{
    public class MessageModelTests
    {
        [Fact]
        public void Message_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var message = new Message();

            // Assert
            message.Id.Should().BeEmpty(); // DatabaseGenerated Identity starts as empty
            message.MessageType.Should().Be(MessageType.Text);
            message.IsRead.Should().BeFalse();
            message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            message.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            message.DeletedAt.Should().BeNull();
            message.ReplyToMessage.Should().BeNull();
        }

        [Fact]
        public void Message_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var replyToMessageId = Guid.NewGuid();
            var content = "Test message content";
            var messageType = MessageType.Image;
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var updatedAt = DateTime.UtcNow;

            // Act
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                ReceiverId = receiverId,
                ReplyToMessageId = replyToMessageId,
                Content = content,
                MessageType = messageType,
                IsRead = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            message.ConversationId.Should().Be(conversationId);
            message.SenderId.Should().Be(senderId);
            message.ReceiverId.Should().Be(receiverId);
            message.ReplyToMessageId.Should().Be(replyToMessageId);
            message.Content.Should().Be(content);
            message.MessageType.Should().Be(messageType);
            message.IsRead.Should().BeTrue();
            message.CreatedAt.Should().Be(createdAt);
            message.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void Message_WithDeletedAt_ShouldSetDeletedAtCorrectly()
        {
            // Arrange
            var deletedAt = DateTime.UtcNow;
            var message = new Message();

            // Act
            message.DeletedAt = deletedAt;

            // Assert
            message.DeletedAt.Should().Be(deletedAt);
        }

        [Fact]
        public void Message_WithReplyToMessage_ShouldSetNavigationProperty()
        {
            // Arrange
            var replyToMessage = new Message { Id = Guid.NewGuid() };
            var message = new Message();

            // Act
            message.ReplyToMessage = replyToMessage;

            // Assert
            message.ReplyToMessage.Should().Be(replyToMessage);
        }

        [Theory]
        [InlineData(MessageType.Text)]
        [InlineData(MessageType.Image)]
        [InlineData(MessageType.File)]
        [InlineData(MessageType.Audio)]
        [InlineData(MessageType.Video)]
        [InlineData(MessageType.System)]
        public void Message_WithDifferentMessageTypes_ShouldSetCorrectly(MessageType messageType)
        {
            // Arrange
            var message = new Message();

            // Act
            message.MessageType = messageType;

            // Assert
            message.MessageType.Should().Be(messageType);
        }

        [Fact]
        public void Message_WithNullContent_ShouldBeValid()
        {
            // Arrange
            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = null
            };

            // Act & Assert
            message.Content.Should().BeNull();
        }

        [Fact]
        public void Message_WithEmptyContent_ShouldBeValid()
        {
            // Arrange
            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = ""
            };

            // Act & Assert
            message.Content.Should().Be("");
        }

        [Fact]
        public void Message_WithLongContent_ShouldBeValid()
        {
            // Arrange
            var longContent = new string('a', 1000);
            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = longContent
            };

            // Act & Assert
            message.Content.Should().Be(longContent);
        }

        [Fact]
        public void Message_WithSameSenderAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = userId,
                ReceiverId = userId
            };

            // Act & Assert
            message.SenderId.Should().Be(userId);
            message.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void Message_WithDifferentTimestamps_ShouldBeValid()
        {
            // Arrange
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow.AddDays(-1);
            var deletedAt = DateTime.UtcNow;

            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                DeletedAt = deletedAt
            };

            // Act & Assert
            message.CreatedAt.Should().Be(createdAt);
            message.UpdatedAt.Should().Be(updatedAt);
            message.DeletedAt.Should().Be(deletedAt);
        }

        [Fact]
        public void Message_WithNullReplyToMessageId_ShouldBeValid()
        {
            // Arrange
            var message = new Message
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                ReplyToMessageId = null
            };

            // Act & Assert
            message.ReplyToMessageId.Should().BeNull();
        }

        [Fact]
        public void Message_WithEmptyGuid_ShouldBeValid()
        {
            // Arrange
            var message = new Message
            {
                ConversationId = Guid.Empty,
                SenderId = Guid.Empty,
                ReceiverId = Guid.Empty
            };

            // Act & Assert
            message.ConversationId.Should().Be(Guid.Empty);
            message.SenderId.Should().Be(Guid.Empty);
            message.ReceiverId.Should().Be(Guid.Empty);
        }
    }
}
