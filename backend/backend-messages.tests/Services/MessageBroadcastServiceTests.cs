using System;
using System.Threading;
using System.Threading.Tasks;
using BackendMessages.Hubs;
using BackendMessages.Models;
using BackendMessages.Services;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class MessageBroadcastServiceTests
    {
        private readonly Mock<IHubContext<MessageHub>> _hubContextMock;
        private readonly Mock<ILogger<MessageBroadcastService>> _loggerMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly Mock<IHubClients> _hubCallerClientsMock;
        private readonly MessageBroadcastService _service;

        public MessageBroadcastServiceTests()
        {
            _hubContextMock = new Mock<IHubContext<MessageHub>>();
            _loggerMock = new Mock<ILogger<MessageBroadcastService>>();
            _clientProxyMock = new Mock<IClientProxy>();
            _hubCallerClientsMock = new Mock<IHubClients>();

            // Setup SignalR mocks
            _hubContextMock.Setup(x => x.Clients).Returns(_hubCallerClientsMock.Object);
            _hubCallerClientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);

            _service = new MessageBroadcastService(_hubContextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_WithValidMessage_ShouldBroadcastToSenderAndReceiver()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversationId,
                senderId: senderId,
                receiverId: receiverId,
                content: "Hello World!");

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: senderId,
                receiverId: receiverId);

            // Act
            await _service.BroadcastNewMessageAsync(message, conversation);

            // Assert
            // Verify message was sent to receiver's group
            _hubCallerClientsMock.Verify(
                x => x.Group($"user_{receiverId}"),
                Times.Once);

            // Verify message was sent to sender's group (multi-device support)
            _hubCallerClientsMock.Verify(
                x => x.Group($"user_{senderId}"),
                Times.Once);

            // Verify SendAsync was called with correct method name and message data
            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args => args.Length == 1 && ValidateMessageResponse(args[0], message)),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2)); // Once for sender, once for receiver
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_WithReplyMessage_ShouldIncludeReplyToMessageId()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var replyToMessageId = Guid.NewGuid();

            var message = TestDataFactory.CreateMessage(
                conversationId: conversationId,
                senderId: senderId,
                receiverId: receiverId,
                content: "This is a reply");
            message.ReplyToMessageId = replyToMessageId;

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: senderId,
                receiverId: receiverId);

            // Act
            await _service.BroadcastNewMessageAsync(message, conversation);

            // Assert
            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveMessage",
                    It.Is<object[]>(args => args.Length == 1 && ValidateMessageResponseWithReply(args[0], message, replyToMessageId)),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_WithDifferentMessageTypes_ShouldBroadcastCorrectly()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: senderId,
                receiverId: receiverId);

            var messageTypes = new[]
            {
                MessageType.Text,
                MessageType.Image,
                MessageType.File,
                MessageType.Audio,
                MessageType.Video,
                MessageType.System
            };

            foreach (var messageType in messageTypes)
            {
                // Arrange
                var message = TestDataFactory.CreateMessage(
                    conversationId: conversationId,
                    senderId: senderId,
                    receiverId: receiverId,
                    messageType: messageType);

                // Act
                await _service.BroadcastNewMessageAsync(message, conversation);

                // Assert
                _clientProxyMock.Verify(
                    x => x.SendCoreAsync(
                        "ReceiveMessage",
                        It.Is<object[]>(args => args.Length == 1 && ValidateMessageTypeInResponse(args[0], messageType)),
                        It.IsAny<CancellationToken>()),
                    Times.AtLeast(2)); // At least 2 because we're in a loop
            }
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_WhenSignalRThrowsException_ShouldLogError()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var message = TestDataFactory.CreateMessage(senderId: senderId, receiverId: receiverId);
            var conversation = TestDataFactory.CreateConversation();

            var expectedException = new InvalidOperationException("SignalR connection failed");
            _clientProxyMock
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            await _service.BroadcastNewMessageAsync(message, conversation);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to broadcast message {message.Id} via SignalR")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastConversationUpdateAsync_WithValidData_ShouldBroadcastToBothParticipants()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            conversation.Id = conversationId;

            var lastMessage = TestDataFactory.CreateMessage(
                conversationId: conversationId,
                senderId: initiatorId,
                receiverId: receiverId,
                content: "Latest message");

            // Act
            await _service.BroadcastConversationUpdateAsync(conversation, lastMessage);

            // Assert
            // Verify message was sent to initiator's group
            _hubCallerClientsMock.Verify(
                x => x.Group($"user_{initiatorId}"),
                Times.Once);

            // Verify message was sent to receiver's group
            _hubCallerClientsMock.Verify(
                x => x.Group($"user_{receiverId}"),
                Times.Once);

            // Verify SendAsync was called with correct method name and conversation data
            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ConversationUpdated",
                    It.Is<object[]>(args => args.Length == 1 && ValidateConversationUpdateResponse(args[0], conversation, lastMessage)),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2)); // Once for initiator, once for receiver
        }

        [Fact]
        public async Task BroadcastConversationUpdateAsync_WhenSignalRThrowsException_ShouldLogError()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var lastMessage = TestDataFactory.CreateMessage();

            var expectedException = new InvalidOperationException("SignalR connection failed");
            _clientProxyMock
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act
            await _service.BroadcastConversationUpdateAsync(conversation, lastMessage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to broadcast conversation update {conversation.Id} via SignalR")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_ShouldCreateCorrectMessageResponseStructure()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage(
                content: "Test message",
                messageType: MessageType.Text,
                isRead: false);
            var conversation = TestDataFactory.CreateConversation();

            object? capturedMessageResponse = null;
            _clientProxyMock
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((method, args, token) =>
                {
                    if (args.Length > 0)
                        capturedMessageResponse = args[0];
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.BroadcastNewMessageAsync(message, conversation);

            // Assert
            capturedMessageResponse.Should().NotBeNull();
            
            var messageResponseType = capturedMessageResponse!.GetType();
            var properties = messageResponseType.GetProperties();

            // Verify all expected properties exist
            properties.Should().Contain(p => p.Name == "Id");
            properties.Should().Contain(p => p.Name == "ConversationId");
            properties.Should().Contain(p => p.Name == "SenderId");
            properties.Should().Contain(p => p.Name == "ReceiverId");
            properties.Should().Contain(p => p.Name == "Content");
            properties.Should().Contain(p => p.Name == "MessageType");
            properties.Should().Contain(p => p.Name == "ReplyToMessageId");
            properties.Should().Contain(p => p.Name == "IsRead");
            properties.Should().Contain(p => p.Name == "CreatedAt");
            properties.Should().Contain(p => p.Name == "UpdatedAt");

            // Verify property values
            var idProperty = messageResponseType.GetProperty("Id");
            idProperty!.GetValue(capturedMessageResponse).Should().Be(message.Id);

            var contentProperty = messageResponseType.GetProperty("Content");
            contentProperty!.GetValue(capturedMessageResponse).Should().Be(message.Content);

            var messageTypeProperty = messageResponseType.GetProperty("MessageType");
            messageTypeProperty!.GetValue(capturedMessageResponse).Should().Be(message.MessageType);
        }

        [Fact]
        public async Task BroadcastConversationUpdateAsync_ShouldCreateCorrectConversationUpdateStructure()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var lastMessage = TestDataFactory.CreateMessage(content: "Last message");

            object? capturedConversationUpdate = null;
            _clientProxyMock
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Callback<string, object[], CancellationToken>((method, args, token) =>
                {
                    if (args.Length > 0)
                        capturedConversationUpdate = args[0];
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.BroadcastConversationUpdateAsync(conversation, lastMessage);

            // Assert
            capturedConversationUpdate.Should().NotBeNull();
            
            var conversationUpdateType = capturedConversationUpdate!.GetType();
            var properties = conversationUpdateType.GetProperties();

            // Verify conversation update properties
            properties.Should().Contain(p => p.Name == "Id");
            properties.Should().Contain(p => p.Name == "LastMessageTimestamp");
            properties.Should().Contain(p => p.Name == "LastMessage");
            properties.Should().Contain(p => p.Name == "UpdatedAt");

            // Verify property values
            var idProperty = conversationUpdateType.GetProperty("Id");
            idProperty!.GetValue(capturedConversationUpdate).Should().Be(conversation.Id);

            var lastMessageProperty = conversationUpdateType.GetProperty("LastMessage");
            var lastMessageValue = lastMessageProperty!.GetValue(capturedConversationUpdate);
            lastMessageValue.Should().NotBeNull();

            // Verify nested last message structure
            var lastMessageType = lastMessageValue!.GetType();
            var lastMessageProperties = lastMessageType.GetProperties();
            lastMessageProperties.Should().Contain(p => p.Name == "Id");
            lastMessageProperties.Should().Contain(p => p.Name == "Content");
            lastMessageProperties.Should().Contain(p => p.Name == "MessageType");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task BroadcastNewMessageAsync_WithEmptyOrNullContent_ShouldStillBroadcast(string content)
        {
            // Arrange
            var message = TestDataFactory.CreateMessage(content: content);
            var conversation = TestDataFactory.CreateConversation();

            // Act
            await _service.BroadcastNewMessageAsync(message, conversation);

            // Assert
            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "ReceiveMessage",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task BroadcastNewMessageAsync_WithNullHubContext_ShouldLogErrorAndNotThrow()
        {
            // Arrange
            var serviceWithNullHub = new MessageBroadcastService(null!, _loggerMock.Object);
            var message = TestDataFactory.CreateMessage();
            var conversation = TestDataFactory.CreateConversation();

            // Act & Assert - Should not throw because of try-catch in service
            var exception = await Record.ExceptionAsync(() => 
                serviceWithNullHub.BroadcastNewMessageAsync(message, conversation));
            
            exception.Should().BeNull(); // Service handles exceptions gracefully
            
            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed to broadcast message {message.Id} via SignalR")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper methods for validation
        private static bool ValidateMessageResponse(object response, Message expectedMessage)
        {
            var responseType = response.GetType();
            
            var idProperty = responseType.GetProperty("Id");
            if (idProperty?.GetValue(response)?.Equals(expectedMessage.Id) != true)
                return false;

            var contentProperty = responseType.GetProperty("Content");
            if (!Equals(contentProperty?.GetValue(response), expectedMessage.Content))
                return false;

            var messageTypeProperty = responseType.GetProperty("MessageType");
            if (messageTypeProperty?.GetValue(response)?.Equals(expectedMessage.MessageType) != true)
                return false;

            return true;
        }

        private static bool ValidateMessageResponseWithReply(object response, Message expectedMessage, Guid expectedReplyId)
        {
            if (!ValidateMessageResponse(response, expectedMessage))
                return false;

            var responseType = response.GetType();
            var replyProperty = responseType.GetProperty("ReplyToMessageId");
            return replyProperty?.GetValue(response)?.Equals(expectedReplyId) == true;
        }

        private static bool ValidateMessageTypeInResponse(object response, MessageType expectedType)
        {
            var responseType = response.GetType();
            var messageTypeProperty = responseType.GetProperty("MessageType");
            return messageTypeProperty?.GetValue(response)?.Equals(expectedType) == true;
        }

        private static bool ValidateConversationUpdateResponse(object response, Conversation expectedConversation, Message expectedLastMessage)
        {
            var responseType = response.GetType();
            
            var idProperty = responseType.GetProperty("Id");
            if (idProperty?.GetValue(response)?.Equals(expectedConversation.Id) != true)
                return false;

            var lastMessageProperty = responseType.GetProperty("LastMessage");
            var lastMessageValue = lastMessageProperty?.GetValue(response);
            if (lastMessageValue == null)
                return false;

            var lastMessageType = lastMessageValue.GetType();
            var lastMessageIdProperty = lastMessageType.GetProperty("Id");
            return lastMessageIdProperty?.GetValue(lastMessageValue)?.Equals(expectedLastMessage.Id) == true;
        }
    }
} 