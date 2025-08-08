using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendMessages.DTO.Message.Request;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using BackendMessages.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class MessageServiceTests
    {
        private readonly Mock<IMessageRepository> _messageRepositoryMock;
        private readonly Mock<IConversationRepository> _conversationRepositoryMock;
        private readonly Mock<IHubContext<MessageHub>> _hubContextMock;
        private readonly Mock<ILogger<MessageService>> _loggerMock;
        private readonly MessageService _service;

        public MessageServiceTests()
        {
            _messageRepositoryMock = new Mock<IMessageRepository>();
            _conversationRepositoryMock = new Mock<IConversationRepository>();
            _hubContextMock = new Mock<IHubContext<MessageHub>>();
            _loggerMock = new Mock<ILogger<MessageService>>();
            _service = new MessageService(
                _messageRepositoryMock.Object,
                _conversationRepositoryMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendMessageAsync_WithValidRequest_ShouldSendMessage()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            var message = TestDataFactory.CreateMessage(
                conversationId: request.ConversationId,
                senderId: request.SenderId,
                receiverId: request.ReceiverId);

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.SenderId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync(message);

            _conversationRepositoryMock
                .Setup(x => x.UpdateLastMessageAsync(request.ConversationId, message.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _messageRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Message>()), Times.Once);
            _conversationRepositoryMock.Verify(x => x.UpdateLastMessageAsync(request.ConversationId, message.Id), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_WithInvalidRequest_ShouldReturnError()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.Empty,
                SenderId = Guid.Empty,
                ReceiverId = Guid.Empty,
                Content = ""
            };

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendMessageAsync_WhenUserCannotAccessConversation_ShouldReturnError()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.SenderId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendMessageAsync_WithReplyToMessage_WhenCannotAccessReplyMessage_ShouldReturnError()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Reply",
                ReplyToMessageId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.SenderId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(request.ReplyToMessageId!.Value, request.SenderId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendMessageAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetMessageByIdAsync_WithValidRequest_ShouldReturnMessage()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var message = TestDataFactory.CreateMessage();

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(messageId, userId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.GetByIdAsync(messageId))
                .ReturnsAsync(message);

            // Act
            var result = await _service.GetMessageByIdAsync(messageId, userId);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetMessageByIdAsync_WhenUserCannotAccess_ShouldReturnNull()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(messageId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetMessageByIdAsync(messageId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationMessagesAsync_WithValidRequest_ShouldReturnMessages()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(5, conversationId);

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.GetByConversationIdAsync(conversationId, null, null, 1, 50))
                .ReturnsAsync(messages);

            _messageRepositoryMock
                .Setup(x => x.GetTotalCountByConversationIdAsync(conversationId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetConversationMessagesAsync(conversationId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().HaveCount(5);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetConversationMessagesAsync_WhenUserCannotAccess_ShouldReturnEmpty()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetConversationMessagesAsync(conversationId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WithValidRequest_ShouldMarkAsRead()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _messageRepositoryMock
                .Setup(x => x.MarkAsReadAsync(request.MessageId, request.UserId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _messageRepositoryMock.Verify(x => x.MarkAsReadAsync(request.MessageId, request.UserId), Times.Once);
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WithInvalidRequest_ShouldReturnError()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WithValidRequest_ShouldMarkAllAsRead()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.MarkConversationAsReadAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.MarkConversationAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessagesMarked.Should().Be(5);
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WhenUserCannotAccess_ShouldReturnError()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.MarkConversationAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteMessageAsync_WithValidRequest_ShouldDeleteMessage()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _messageRepositoryMock
                .Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ReturnsAsync(true);

            _messageRepositoryMock
                .Setup(x => x.SoftDeleteAsync(request.MessageId, request.UserId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _messageRepositoryMock.Verify(x => x.SoftDeleteAsync(request.MessageId, request.UserId), Times.Once);
        }

        [Fact]
        public async Task DeleteMessageAsync_WhenUserDoesNotOwnMessage_ShouldReturnError()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _messageRepositoryMock
                .Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WithValidRequest_ShouldReportMessage()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.ReportedById))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ReportMessageAsync_WhenUserCannotAccessMessage_ShouldReturnError()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.ReportedById))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetUnreadMessagesAsync_WithValidUserId_ShouldReturnUnreadMessages()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(3, Guid.NewGuid());

            _messageRepositoryMock
                .Setup(x => x.GetUnreadMessagesByUserIdAsync(userId))
                .ReturnsAsync(messages);

            // Act
            var result = await _service.GetUnreadMessagesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetUnreadCountAsync_WithValidUserId_ShouldReturnCount()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.GetUnreadCountByUserIdAsync(userId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetUnreadCountAsync(userId);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WithValidRequest_ShouldReturnResult()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(messageId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UserCanAccessMessageAsync(messageId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WithValidRequest_ShouldReturnResult()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserOwnsMessageAsync(messageId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UserOwnsMessageAsync(messageId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetMessageByIdAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(messageId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetMessageByIdAsync(messageId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationMessagesAsync_WhenExceptionOccurs_ShouldReturnEmptyResponse()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetConversationMessagesAsync(conversationId, userId);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.UserId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.MarkConversationAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteMessageAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _messageRepositoryMock
                .Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.ReportedById))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetUnreadMessagesAsync_WhenExceptionOccurs_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.GetUnreadMessagesByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetUnreadMessagesAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnreadCountAsync_WhenExceptionOccurs_ShouldReturnZero()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.GetUnreadCountByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetUnreadCountAsync(userId);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserCanAccessMessageAsync(messageId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UserCanAccessMessageAsync(messageId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _messageRepositoryMock
                .Setup(x => x.UserOwnsMessageAsync(messageId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UserOwnsMessageAsync(messageId, userId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
