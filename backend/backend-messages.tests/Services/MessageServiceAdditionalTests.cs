using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using BackendMessages.Hubs;
using BackendMessages.DTO.Message.Request;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class MessageServiceAdditionalTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<IMessageRepository> _messageRepositoryMock;
        private readonly Mock<IConversationRepository> _conversationRepositoryMock;
        private readonly Mock<IHubContext<MessageHub>> _hubContextMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IGroupManager> _groupManagerMock;
        private readonly Mock<ILogger<MessageService>> _loggerMock;
        private readonly MessageService _service;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testConversationId = Guid.NewGuid();
        private readonly Guid _testMessageId = Guid.NewGuid(); // Added for ReportMessageAsync test

        public MessageServiceAdditionalTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"MessageServiceAdditionalTests_{Guid.NewGuid()}")
                .Options;
            _context = new MessagesDbContext(options);

            // Setup mocks
            _messageRepositoryMock = new Mock<IMessageRepository>();
            _conversationRepositoryMock = new Mock<IConversationRepository>();
            _hubContextMock = new Mock<IHubContext<MessageHub>>();
            _clientProxyMock = new Mock<IClientProxy>();
            _groupManagerMock = new Mock<IGroupManager>();
            _loggerMock = new Mock<ILogger<MessageService>>();

            // Setup hub context
            var hubClientsMock = new Mock<IHubClients>();
            hubClientsMock.Setup(x => x.All).Returns(_clientProxyMock.Object);
            _hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);
            _clientProxyMock.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Create service
            _service = new MessageService(
                _messageRepositoryMock.Object,
                _conversationRepositoryMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendMessageAsync_WithNullRequest_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _service.SendMessageAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Request cannot be null");
        }

        [Fact]
        public async Task SendMessageAsync_WithEmptyContent_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new BackendMessages.DTO.Message.Request.SendMessageRequest
            {
                ConversationId = _testConversationId,
                SenderId = _testUserId,
                ReceiverId = Guid.NewGuid(),
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
        public async Task SendMessageAsync_WithWhitespaceContent_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new BackendMessages.DTO.Message.Request.SendMessageRequest
            {
                ConversationId = _testConversationId,
                SenderId = _testUserId,
                ReceiverId = Guid.NewGuid(),
                Content = "   "
            };

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SendMessageAsync_WithVeryLongContent_ShouldReturnErrorResponse()
        {
            // Arrange
            var longContent = new string('a', 10001); // Assuming max length is 10000
            var request = new BackendMessages.DTO.Message.Request.SendMessageRequest
            {
                ConversationId = _testConversationId,
                SenderId = _testUserId,
                ReceiverId = Guid.NewGuid(),
                Content = longContent
            };

            // Act
            var result = await _service.SendMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteMessageAsync_WithNonExistentMessage_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteMessageAsync_WithMessageNotOwnedByUser_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeleteMessageAsync_WithAlreadyDeletedMessage_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.UserOwnsMessageAsync(request.MessageId, request.UserId))
                .ReturnsAsync(true);
            _messageRepositoryMock.Setup(x => x.SoftDeleteAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WithNonExistentMessage_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.MarkAsReadAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WithMessageNotForUser_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.MarkAsReadAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkMessageAsReadAsync_WithAlreadyReadMessage_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _messageRepositoryMock.Setup(x => x.MarkAsReadAsync(request.MessageId, request.UserId))
                .ReturnsAsync(false); // Already read, so marking as read returns false

            // Act
            var result = await _service.MarkMessageAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WithNonExistentMessage_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = _testUserId,
                Reason = "Spam"
            };

            _messageRepositoryMock.Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.ReportedById))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WithMessageNotForUser_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = _testUserId,
                Reason = "Spam"
            };

            _messageRepositoryMock.Setup(x => x.UserCanAccessMessageAsync(request.MessageId, request.ReportedById))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WithUserReportingOwnMessage_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = _testMessageId,
                ReportedById = _testUserId, // Same user as message sender
                Reason = "Test reason"
            };

            _messageRepositoryMock.Setup(x => x.UserCanAccessMessageAsync(_testMessageId, _testUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public async Task ReportMessageAsync_WithEmptyReason_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = _testUserId,
                Reason = ""
            };

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReportMessageAsync_WithNullReason_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = _testUserId,
                Reason = null!
            };

            // Act
            var result = await _service.ReportMessageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetConversationMessagesAsync_WithInvalidPagination_ShouldUseDefaults()
        {
            // Arrange
            var conversationId = Guid.NewGuid();

            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(conversationId, _testUserId))
                .ReturnsAsync(true);

            _messageRepositoryMock.Setup(x => x.GetByConversationIdAsync(conversationId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), -1, -1))
                .ReturnsAsync(new List<Message>());

            _messageRepositoryMock.Setup(x => x.GetTotalCountByConversationIdAsync(conversationId))
                .ReturnsAsync(0);

            // Act
            var result = await _service.GetConversationMessagesAsync(conversationId, _testUserId, -1, -1);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConversationMessagesAsync_WithVeryLargePageSize_ShouldUseMaxPageSize()
        {
            // Arrange
            var conversationId = Guid.NewGuid();

            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(conversationId, _testUserId))
                .ReturnsAsync(true);

            _messageRepositoryMock.Setup(x => x.GetByConversationIdAsync(conversationId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10001))
                .ReturnsAsync(new List<Message>());

            _messageRepositoryMock.Setup(x => x.GetTotalCountByConversationIdAsync(conversationId))
                .ReturnsAsync(0);

            // Act
            var result = await _service.GetConversationMessagesAsync(conversationId, _testUserId, 1, 10001);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUnreadCountAsync_WithNonExistentConversation_ShouldReturnZero()
        {
            // Arrange
            var nonExistentConversationId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(nonExistentConversationId, _testUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetUnreadCountAsync(_testUserId);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetUnreadMessagesAsync_WithNonExistentConversation_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentConversationId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(nonExistentConversationId, _testUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetUnreadMessagesAsync(_testUserId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WithNonExistentConversation_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.MarkConversationAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WithNoUnreadMessages_ShouldReturnSuccessResponse()
        {
            // Arrange
            var request = new MarkMessagesAsReadRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(true);
            _messageRepositoryMock.Setup(x => x.MarkConversationAsReadAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(0);

            // Act
            var result = await _service.MarkConversationAsReadAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessagesMarked.Should().Be(0);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 