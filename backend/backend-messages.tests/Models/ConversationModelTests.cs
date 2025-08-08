using System;
using BackendMessages.Models;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Models
{
    public class ConversationModelTests
    {
        [Fact]
        public void Conversation_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var conversation = new Conversation();

            // Assert
            conversation.Id.Should().BeEmpty(); // DatabaseGenerated Identity starts as empty
            conversation.LastMessageTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            conversation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            conversation.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            conversation.InitiatorDeletedAt.Should().BeNull();
            conversation.ReceiverDeletedAt.Should().BeNull();
            conversation.LastMessage.Should().BeNull();
            conversation.Messages.Should().NotBeNull();
            conversation.Messages.Should().BeEmpty();
        }

        [Fact]
        public void Conversation_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var lastMessageId = Guid.NewGuid();
            var lastMessageTimestamp = DateTime.UtcNow.AddDays(-1);
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow.AddDays(-1);

            // Act
            var conversation = new Conversation
            {
                InitiatorId = initiatorId,
                ReceiverId = receiverId,
                LastMessageId = lastMessageId,
                LastMessageTimestamp = lastMessageTimestamp,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            conversation.InitiatorId.Should().Be(initiatorId);
            conversation.ReceiverId.Should().Be(receiverId);
            conversation.LastMessageId.Should().Be(lastMessageId);
            conversation.LastMessageTimestamp.Should().Be(lastMessageTimestamp);
            conversation.CreatedAt.Should().Be(createdAt);
            conversation.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void Conversation_WithDeletedAtTimestamps_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var initiatorDeletedAt = DateTime.UtcNow.AddDays(-2);
            var receiverDeletedAt = DateTime.UtcNow.AddDays(-1);
            var conversation = new Conversation();

            // Act
            conversation.InitiatorDeletedAt = initiatorDeletedAt;
            conversation.ReceiverDeletedAt = receiverDeletedAt;

            // Assert
            conversation.InitiatorDeletedAt.Should().Be(initiatorDeletedAt);
            conversation.ReceiverDeletedAt.Should().Be(receiverDeletedAt);
        }

        [Fact]
        public void Conversation_WithLastMessage_ShouldSetNavigationProperty()
        {
            // Arrange
            var lastMessage = new Message { Id = Guid.NewGuid() };
            var conversation = new Conversation();

            // Act
            conversation.LastMessage = lastMessage;

            // Assert
            conversation.LastMessage.Should().Be(lastMessage);
        }

        [Fact]
        public void Conversation_WithMessages_ShouldAddToCollection()
        {
            // Arrange
            var conversation = new Conversation();
            var message1 = new Message { Id = Guid.NewGuid() };
            var message2 = new Message { Id = Guid.NewGuid() };

            // Act
            conversation.Messages.Add(message1);
            conversation.Messages.Add(message2);

            // Assert
            conversation.Messages.Should().HaveCount(2);
            conversation.Messages.Should().Contain(message1);
            conversation.Messages.Should().Contain(message2);
        }

        [Fact]
        public void Conversation_IsDeletedByUser_WithInitiatorId_ShouldReturnCorrectValue()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var conversation = new Conversation
            {
                InitiatorId = initiatorId,
                ReceiverId = Guid.NewGuid()
            };

            // Act & Assert - Not deleted
            conversation.IsDeletedByUser(initiatorId).Should().BeFalse();

            // Act - Mark as deleted
            conversation.InitiatorDeletedAt = DateTime.UtcNow;

            // Assert - Now deleted
            conversation.IsDeletedByUser(initiatorId).Should().BeTrue();
        }

        [Fact]
        public void Conversation_IsDeletedByUser_WithReceiverId_ShouldReturnCorrectValue()
        {
            // Arrange
            var receiverId = Guid.NewGuid();
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = receiverId
            };

            // Act & Assert - Not deleted
            conversation.IsDeletedByUser(receiverId).Should().BeFalse();

            // Act - Mark as deleted
            conversation.ReceiverDeletedAt = DateTime.UtcNow;

            // Assert - Now deleted
            conversation.IsDeletedByUser(receiverId).Should().BeTrue();
        }

        [Fact]
        public void Conversation_IsDeletedByUser_WithNonParticipantId_ShouldReturnFalse()
        {
            // Arrange
            var nonParticipantId = Guid.NewGuid();
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid()
            };

            // Act & Assert
            conversation.IsDeletedByUser(nonParticipantId).Should().BeFalse();
        }

        [Fact]
        public void Conversation_IsDeletedByBothUsers_WhenNeitherDeleted_ShouldReturnFalse()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid()
            };

            // Act & Assert
            conversation.IsDeletedByBothUsers().Should().BeFalse();
        }

        [Fact]
        public void Conversation_IsDeletedByBothUsers_WhenOnlyInitiatorDeleted_ShouldReturnFalse()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitiatorDeletedAt = DateTime.UtcNow
            };

            // Act & Assert
            conversation.IsDeletedByBothUsers().Should().BeFalse();
        }

        [Fact]
        public void Conversation_IsDeletedByBothUsers_WhenOnlyReceiverDeleted_ShouldReturnFalse()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                ReceiverDeletedAt = DateTime.UtcNow
            };

            // Act & Assert
            conversation.IsDeletedByBothUsers().Should().BeFalse();
        }

        [Fact]
        public void Conversation_IsDeletedByBothUsers_WhenBothDeleted_ShouldReturnTrue()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitiatorDeletedAt = DateTime.UtcNow.AddDays(-2),
                ReceiverDeletedAt = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            conversation.IsDeletedByBothUsers().Should().BeTrue();
        }

        [Fact]
        public void Conversation_WithNullLastMessageId_ShouldBeValid()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                LastMessageId = null
            };

            // Act & Assert
            conversation.LastMessageId.Should().BeNull();
        }

        [Fact]
        public void Conversation_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var conversation = new Conversation
            {
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.Empty
            };

            // Act & Assert
            conversation.InitiatorId.Should().Be(Guid.Empty);
            conversation.ReceiverId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void Conversation_WithSameInitiatorAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversation = new Conversation
            {
                InitiatorId = userId,
                ReceiverId = userId
            };

            // Act & Assert
            conversation.InitiatorId.Should().Be(userId);
            conversation.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void Conversation_WithDifferentTimestamps_ShouldBeValid()
        {
            // Arrange
            var lastMessageTimestamp = DateTime.UtcNow.AddDays(-3);
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow.AddDays(-1);

            var conversation = new Conversation
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                LastMessageTimestamp = lastMessageTimestamp,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Act & Assert
            conversation.LastMessageTimestamp.Should().Be(lastMessageTimestamp);
            conversation.CreatedAt.Should().Be(createdAt);
            conversation.UpdatedAt.Should().Be(updatedAt);
        }
    }
}
