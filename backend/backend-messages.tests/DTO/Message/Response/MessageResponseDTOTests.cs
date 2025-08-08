using System;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Models;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

namespace BackendMessages.Tests.DTO.Message.Response
{
    public class MessageResponseDTOTests
    {
        [Fact]
        public void MessageResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new MessageResponse();

            // Assert
            response.Id.Should().BeEmpty();
            response.ConversationId.Should().BeEmpty();
            response.SenderId.Should().BeEmpty();
            response.ReceiverId.Should().BeEmpty();
            response.Content.Should().BeEmpty();
            response.MessageType.Should().Be(MessageType.Text);
            response.IsRead.Should().BeFalse();
            response.CreatedAt.Should().Be(default(DateTime));
            response.UpdatedAt.Should().Be(default(DateTime));
            response.ReplyToMessageId.Should().BeNull();
            response.ReplyToMessage.Should().BeNull();
        }

        [Fact]
        public void MessageResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var content = "Hello, world!";
            var messageType = MessageType.Image;
            var isRead = true;
            var createdAt = DateTime.UtcNow.AddDays(-1);
            var updatedAt = DateTime.UtcNow;
            var replyToMessageId = Guid.NewGuid();

            // Act
            var response = new MessageResponse
            {
                Id = id,
                ConversationId = conversationId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                MessageType = messageType,
                IsRead = isRead,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ReplyToMessageId = replyToMessageId
            };

            // Assert
            response.Id.Should().Be(id);
            response.ConversationId.Should().Be(conversationId);
            response.SenderId.Should().Be(senderId);
            response.ReceiverId.Should().Be(receiverId);
            response.Content.Should().Be(content);
            response.MessageType.Should().Be(messageType);
            response.IsRead.Should().Be(isRead);
            response.CreatedAt.Should().Be(createdAt);
            response.UpdatedAt.Should().Be(updatedAt);
            response.ReplyToMessageId.Should().Be(replyToMessageId);
        }

        [Fact]
        public void MessageResponse_WithReplyToMessage_ShouldSetNavigationProperty()
        {
            // Arrange
            var replyToMessage = new MessageResponse { Id = Guid.NewGuid() };
            var response = new MessageResponse();

            // Act
            response.ReplyToMessage = replyToMessage;

            // Assert
            response.ReplyToMessage.Should().Be(replyToMessage);
        }

        [Theory]
        [InlineData(MessageType.Text)]
        [InlineData(MessageType.Image)]
        [InlineData(MessageType.File)]
        [InlineData(MessageType.Audio)]
        [InlineData(MessageType.Video)]
        [InlineData(MessageType.System)]
        public void MessageResponse_WithDifferentMessageTypes_ShouldSetCorrectly(MessageType messageType)
        {
            // Arrange
            var response = new MessageResponse();

            // Act
            response.MessageType = messageType;

            // Assert
            response.MessageType.Should().Be(messageType);
        }

        [Fact]
        public void MessageSummaryResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new MessageSummaryResponse();

            // Assert
            response.Id.Should().BeEmpty();
            response.ConversationId.Should().BeEmpty();
            response.SenderId.Should().BeEmpty();
            response.Content.Should().BeEmpty();
            response.IsRead.Should().BeFalse();
            response.CreatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void MessageSummaryResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var content = "Summary message";
            var isRead = true;
            var createdAt = DateTime.UtcNow.AddDays(-1);

            // Act
            var response = new MessageSummaryResponse
            {
                Id = id,
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                IsRead = isRead,
                CreatedAt = createdAt
            };

            // Assert
            response.Id.Should().Be(id);
            response.ConversationId.Should().Be(conversationId);
            response.SenderId.Should().Be(senderId);
            response.Content.Should().Be(content);
            response.IsRead.Should().Be(isRead);
            response.CreatedAt.Should().Be(createdAt);
        }

        [Fact]
        public void SendMessageResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new SendMessageResponse();

            // Assert
            response.Message.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Error.Should().BeNull();
        }

        [Fact]
        public void SendMessageResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var message = new MessageResponse { Id = Guid.NewGuid() };
            var success = true;
            var error = "Test error";

            // Act
            var response = new SendMessageResponse
            {
                Message = message,
                Success = success,
                Error = error
            };

            // Assert
            response.Message.Should().Be(message);
            response.Success.Should().Be(success);
            response.Error.Should().Be(error);
        }

        [Fact]
        public void SendMessageResponse_WithNullError_ShouldBeValid()
        {
            // Arrange
            var response = new SendMessageResponse
            {
                Message = new MessageResponse(),
                Success = true,
                Error = null
            };

            // Act & Assert
            response.Error.Should().BeNull();
        }

        [Fact]
        public void MessagesPagedResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new MessagesPagedResponse();

            // Assert
            response.Messages.Should().NotBeNull();
            response.Messages.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
            response.PageNumber.Should().Be(0);
            response.PageSize.Should().Be(0);
            response.HasNextPage.Should().BeFalse();
            response.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public void MessagesPagedResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var messages = new List<MessageResponse>
            {
                new MessageResponse { Id = Guid.NewGuid() },
                new MessageResponse { Id = Guid.NewGuid() }
            };
            var totalCount = 10;
            var pageNumber = 2;
            var pageSize = 5;
            var hasNextPage = true;
            var hasPreviousPage = true;

            // Act
            var response = new MessagesPagedResponse
            {
                Messages = messages,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };

            // Assert
            response.Messages.Should().BeEquivalentTo(messages);
            response.TotalCount.Should().Be(totalCount);
            response.PageNumber.Should().Be(pageNumber);
            response.PageSize.Should().Be(pageSize);
            response.HasNextPage.Should().Be(hasNextPage);
            response.HasPreviousPage.Should().Be(hasPreviousPage);
        }

        [Fact]
        public void MessagesPagedResponse_WithEmptyMessages_ShouldBeValid()
        {
            // Arrange
            var response = new MessagesPagedResponse
            {
                Messages = new List<MessageResponse>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                HasNextPage = false,
                HasPreviousPage = false
            };

            // Act & Assert
            response.Messages.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
        }

        [Fact]
        public void MarkAsReadResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new MarkAsReadResponse();

            // Assert
            response.Success.Should().BeFalse();
            response.MessagesMarked.Should().Be(0);
            response.Error.Should().BeNull();
        }

        [Fact]
        public void MarkAsReadResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var success = true;
            var messagesMarked = 5;
            var error = "Test error";

            // Act
            var response = new MarkAsReadResponse
            {
                Success = success,
                MessagesMarked = messagesMarked,
                Error = error
            };

            // Assert
            response.Success.Should().Be(success);
            response.MessagesMarked.Should().Be(messagesMarked);
            response.Error.Should().Be(error);
        }

        [Fact]
        public void MarkAsReadResponse_WithNullError_ShouldBeValid()
        {
            // Arrange
            var response = new MarkAsReadResponse
            {
                Success = true,
                MessagesMarked = 3,
                Error = null
            };

            // Act & Assert
            response.Error.Should().BeNull();
        }

        [Fact]
        public void DeleteMessageResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new DeleteMessageResponse();

            // Assert
            response.Success.Should().BeFalse();
            response.Error.Should().BeNull();
        }

        [Fact]
        public void DeleteMessageResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var success = true;
            var error = "Test error";

            // Act
            var response = new DeleteMessageResponse
            {
                Success = success,
                Error = error
            };

            // Assert
            response.Success.Should().Be(success);
            response.Error.Should().Be(error);
        }

        [Fact]
        public void DeleteMessageResponse_WithNullError_ShouldBeValid()
        {
            // Arrange
            var response = new DeleteMessageResponse
            {
                Success = true,
                Error = null
            };

            // Act & Assert
            response.Error.Should().BeNull();
        }

        [Fact]
        public void MessageResponse_WithNullContent_ShouldBeValid()
        {
            // Arrange
            var response = new MessageResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = null!
            };

            // Act & Assert
            response.Content.Should().BeNull();
        }

        [Fact]
        public void MessageResponse_WithEmptyContent_ShouldBeValid()
        {
            // Arrange
            var response = new MessageResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = ""
            };

            // Act & Assert
            response.Content.Should().BeEmpty();
        }

        [Fact]
        public void MessageResponse_WithLongContent_ShouldBeValid()
        {
            // Arrange
            var longContent = new string('a', 1000);
            var response = new MessageResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = longContent
            };

            // Act & Assert
            response.Content.Should().Be(longContent);
        }

        [Fact]
        public void MessageResponse_WithNullReplyToMessage_ShouldBeValid()
        {
            // Arrange
            var response = new MessageResponse();

            // Act
            response.ReplyToMessage = null;

            // Assert
            response.ReplyToMessage.Should().BeNull();
        }

        [Fact]
        public void MessageResponse_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var response = new MessageResponse
            {
                Id = Guid.Empty,
                ConversationId = Guid.Empty,
                SenderId = Guid.Empty,
                ReceiverId = Guid.Empty
            };

            // Act & Assert
            response.Id.Should().Be(Guid.Empty);
            response.ConversationId.Should().Be(Guid.Empty);
            response.SenderId.Should().Be(Guid.Empty);
            response.ReceiverId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MessageResponse_WithSameSenderAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new MessageResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = userId,
                ReceiverId = userId,
                Content = "Test message"
            };

            // Act & Assert
            response.SenderId.Should().Be(userId);
            response.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void MessageSummaryResponse_WithNullContent_ShouldBeValid()
        {
            // Arrange
            var response = new MessageSummaryResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                Content = null!
            };

            // Act & Assert
            response.Content.Should().BeNull();
        }

        [Fact]
        public void MessageSummaryResponse_WithEmptyContent_ShouldBeValid()
        {
            // Arrange
            var response = new MessageSummaryResponse
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                Content = ""
            };

            // Act & Assert
            response.Content.Should().BeEmpty();
        }

        [Fact]
        public void MessageSummaryResponse_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var response = new MessageSummaryResponse
            {
                Id = Guid.Empty,
                ConversationId = Guid.Empty,
                SenderId = Guid.Empty
            };

            // Act & Assert
            response.Id.Should().Be(Guid.Empty);
            response.ConversationId.Should().Be(Guid.Empty);
            response.SenderId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MessagesPagedResponse_WithNullMessages_ShouldBeValid()
        {
            // Arrange
            var response = new MessagesPagedResponse
            {
                Messages = null!,
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            // Act & Assert
            response.Messages.Should().BeNull();
        }

        [Fact]
        public void MessagesPagedResponse_WithNegativeValues_ShouldBeValid()
        {
            // Arrange
            var response = new MessagesPagedResponse
            {
                Messages = new List<MessageResponse>(),
                TotalCount = -1,
                PageNumber = -1,
                PageSize = -1,
                HasNextPage = false,
                HasPreviousPage = false
            };

            // Act & Assert
            response.TotalCount.Should().Be(-1);
            response.PageNumber.Should().Be(-1);
            response.PageSize.Should().Be(-1);
        }
    }
}
