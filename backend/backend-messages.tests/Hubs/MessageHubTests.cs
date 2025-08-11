using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Hubs;
using BackendMessages.Models;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Threading;

namespace BackendMessages.Tests.Hubs
{
    public class MessageHubTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<ILogger<MessageHub>> _loggerMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IGroupManager> _groupManagerMock;
        private readonly Mock<IHubCallerClients> _hubCallerClientsMock;
        private readonly Mock<HubCallerContext> _hubCallerContextMock;
        private readonly MessageHub _hub;

        public MessageHubTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"MessageHubTests_{Guid.NewGuid()}")
                .Options;
            _context = new MessagesDbContext(options);

            // Setup mocks
            _loggerMock = new Mock<ILogger<MessageHub>>();
            _clientProxyMock = new Mock<IClientProxy>();
            _groupManagerMock = new Mock<IGroupManager>();
            _hubCallerClientsMock = new Mock<IHubCallerClients>();
            _hubCallerContextMock = new Mock<HubCallerContext>();

            // Setup hub clients
            _hubCallerClientsMock.Setup(x => x.All).Returns(_clientProxyMock.Object);
            _hubCallerClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            // Create hub
            _hub = new MessageHub(_loggerMock.Object, _context);

            // Setup hub context using reflection
            var hubContextProperty = typeof(Hub).GetProperty("Context");
            hubContextProperty?.SetValue(_hub, _hubCallerContextMock.Object);

            var clientsProperty = typeof(Hub).GetProperty("Clients");
            clientsProperty?.SetValue(_hub, _hubCallerClientsMock.Object);

            var groupsProperty = typeof(Hub).GetProperty("Groups");
            groupsProperty?.SetValue(_hub, _groupManagerMock.Object);

            // Setup connection ID
            _hubCallerContextMock.Setup(x => x.ConnectionId).Returns("test-connection-id");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task DeleteMessage_WithValidMessageAndUser_ShouldDeleteMessageAndBroadcast()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: conversation.InitiatorId,
                receiverId: conversation.ReceiverId
            );
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id.ToString();
            var userId = message.SenderId.ToString();

            // Act
            await _hub.DeleteMessage(messageId, userId);

            // Assert
            var deletedMessage = await _context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            deletedMessage.Should().NotBeNull();
            deletedMessage!.DeletedAt.Should().NotBeNull();

            // Verify SignalR broadcasts
            _hubCallerClientsMock.Verify(x => x.Group($"user_{message.ReceiverId}"), Times.AtLeastOnce);
            _hubCallerClientsMock.Verify(x => x.Group($"user_{message.SenderId}"), Times.AtLeastOnce);
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "MessageDeleted",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ), Times.AtLeast(2));
        }

        [Fact]
        public async Task DeleteMessage_WithInvalidMessageId_ShouldLogWarningAndReturn()
        {
            // Arrange
            var invalidMessageId = "invalid-guid";
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.DeleteMessage(invalidMessageId, userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid messageId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WithInvalidUserId_ShouldLogWarningAndReturn()
        {
            // Arrange
            var messageId = Guid.NewGuid().ToString();
            var invalidUserId = "invalid-guid";

            // Act
            await _hub.DeleteMessage(messageId, invalidUserId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid messageId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WithNonExistentMessage_ShouldLogWarningAndReturn()
        {
            // Arrange
            var messageId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.DeleteMessage(messageId, userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message") && v.ToString()!.Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WithNonSenderUser_ShouldLogWarningAndReturn()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: conversation.InitiatorId,
                receiverId: conversation.ReceiverId
            );
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id.ToString();
            var nonSenderUserId = Guid.NewGuid().ToString(); // Different from message.SenderId

            // Act
            await _hub.DeleteMessage(messageId, nonSenderUserId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attempted to delete message") && v.ToString()!.Contains("they didn't send")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify message was not deleted
            var unchangedMessage = await _context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            unchangedMessage.Should().NotBeNull();
            unchangedMessage!.DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task MarkMessageAsRead_WithValidMessageAndUser_ShouldMarkAsReadAndBroadcast()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: conversation.InitiatorId,
                receiverId: conversation.ReceiverId,
                isRead: false
            );
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id.ToString();
            var userId = message.ReceiverId.ToString();

            // Act
            await _hub.MarkMessageAsRead(messageId, userId);

            // Assert
            var readMessage = await _context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            readMessage.Should().NotBeNull();
            readMessage!.IsRead.Should().BeTrue();

            // Verify SignalR broadcasts
            _hubCallerClientsMock.Verify(x => x.Group($"user_{message.SenderId}"), Times.AtLeastOnce);
            _hubCallerClientsMock.Verify(x => x.Group($"user_{userId}"), Times.AtLeastOnce);
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "MessageRead",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ), Times.AtLeast(2));
        }

        [Fact]
        public async Task MarkMessageAsRead_WithAlreadyReadMessage_ShouldNotUpdateOrBroadcast()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: conversation.InitiatorId,
                receiverId: conversation.ReceiverId,
                isRead: true // Already read
            );
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id.ToString();
            var userId = message.ReceiverId.ToString();

            // Act
            await _hub.MarkMessageAsRead(messageId, userId);

            // Assert - No broadcasts should occur for already read messages
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "MessageRead",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
        }

        [Fact]
        public async Task MarkMessageAsRead_WithInvalidMessageId_ShouldLogWarningAndReturn()
        {
            // Arrange
            var invalidMessageId = "invalid-guid";
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.MarkMessageAsRead(invalidMessageId, userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid messageId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MarkMessageAsRead_WithNonReceiverUser_ShouldLogWarningAndReturn()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: conversation.InitiatorId,
                receiverId: conversation.ReceiverId,
                isRead: false
            );
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id.ToString();
            var nonReceiverUserId = Guid.NewGuid().ToString(); // Different from message.ReceiverId

            // Act
            await _hub.MarkMessageAsRead(messageId, nonReceiverUserId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found or user") && v.ToString()!.Contains("is not the receiver")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify message was not marked as read
            var unchangedMessage = await _context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            unchangedMessage.Should().NotBeNull();
            unchangedMessage!.IsRead.Should().BeFalse();
        }

        [Fact]
        public async Task JoinUserGroup_WithValidUserId_ShouldAddToGroupAndBroadcastPresence()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.JoinUserGroup(userId);

            // Assert
            _groupManagerMock.Verify(x => x.AddToGroupAsync(
                "test-connection-id",
                $"user_{userId}",
                It.IsAny<CancellationToken>()
            ), Times.Once);

            // Verify presence broadcast
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "UserPresenceUpdate",
                It.Is<object[]>(args => args.Length > 0),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task LeaveUserGroup_WithValidUserId_ShouldRemoveFromGroupAndBroadcastPresence()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            
            // First join the group
            await _hub.JoinUserGroup(userId);
            
            // Reset mocks to verify only the leave operations
            _groupManagerMock.Reset();
            _clientProxyMock.Reset();

            // Act
            await _hub.LeaveUserGroup(userId);

            // Assert
            _groupManagerMock.Verify(x => x.RemoveFromGroupAsync(
                "test-connection-id",
                $"user_{userId}",
                It.IsAny<CancellationToken>()
            ), Times.Once);

            // Verify presence broadcast (user went offline)
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "UserPresenceUpdate",
                It.Is<object[]>(args => args.Length > 0),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldCleanupConnectionsAndBroadcastPresence()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            
            // First join the group
            await _hub.JoinUserGroup(userId);
            
            // Reset mocks to verify only the disconnect operations
            _groupManagerMock.Reset();
            _clientProxyMock.Reset();

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _groupManagerMock.Verify(x => x.RemoveFromGroupAsync(
                "test-connection-id",
                $"user_{userId}",
                It.IsAny<CancellationToken>()
            ), Times.Once);

            // Verify presence broadcast (at least one user went offline)
            // Note: May be more than once due to static UserConnections shared across tests
            _clientProxyMock.Verify(x => x.SendCoreAsync(
                "UserPresenceUpdate",
                It.Is<object[]>(args => args.Length > 0),
                It.IsAny<CancellationToken>()
            ), Times.AtLeastOnce);
        }

        [Fact]
        public void IsUserOnline_WithOnlineUser_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            
            // Join user group to make them online
            _hub.JoinUserGroup(userId).Wait();

            // Act
            var result = MessageHub.IsUserOnline(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsUserOnline_WithOfflineUser_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            // Act
            var result = MessageHub.IsUserOnline(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetOnlineUsers_ShouldReturnListOfOnlineUserIds()
        {
            // Arrange
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            
            // Join user groups to make them online
            _hub.JoinUserGroup(userId1).Wait();
            _hub.JoinUserGroup(userId2).Wait();

            // Act
            var onlineUsers = MessageHub.GetOnlineUsers();

            // Assert
            onlineUsers.Should().Contain(userId1);
            onlineUsers.Should().Contain(userId2);
        }

        [Fact]
        public async Task DeleteMessage_WithDatabaseException_ShouldLogError()
        {
            // Arrange
            _context.Dispose(); // Force database exception
            var messageId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.DeleteMessage(messageId, userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error deleting message")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MarkMessageAsRead_WithDatabaseException_ShouldLogError()
        {
            // Arrange
            _context.Dispose(); // Force database exception
            var messageId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            // Act
            await _hub.MarkMessageAsRead(messageId, userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error marking message") && v.ToString()!.Contains("as read")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task JoinUserGroup_WithException_ShouldLogError()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _groupManagerMock.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _hub.JoinUserGroup(userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error adding user") && v.ToString()!.Contains("to group")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LeaveUserGroup_WithException_ShouldLogError()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _groupManagerMock.Setup(x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _hub.LeaveUserGroup(userId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error removing user") && v.ToString()!.Contains("from group")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_WithException_ShouldLogError()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            await _hub.JoinUserGroup(userId);

            _groupManagerMock.Setup(x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _hub.OnDisconnectedAsync(null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during disconnect cleanup")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
