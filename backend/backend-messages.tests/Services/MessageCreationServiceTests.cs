using System;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Services;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class MessageCreationServiceTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<ILogger<MessageCreationService>> _loggerMock;
        private readonly MessageCreationService _service;

        public MessageCreationServiceTests()
        {
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MessagesDbContext(options);
            _loggerMock = new Mock<ILogger<MessageCreationService>>();
            _service = new MessageCreationService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateMessageAsync_WithValidData_ShouldCreateMessageAndUpdateConversation()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var content = "Hello, this is a test message!";
            var messageType = MessageType.Text;

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                content,
                messageType);

            // Assert
            result.message.Should().NotBeNull();
            result.conversation.Should().NotBeNull();

            // Verify message properties
            result.message.ConversationId.Should().Be(conversation.Id);
            result.message.SenderId.Should().Be(initiatorId);
            result.message.ReceiverId.Should().Be(receiverId);
            result.message.Content.Should().Be(content);
            result.message.MessageType.Should().Be(messageType);
            result.message.IsRead.Should().BeFalse();
            result.message.ReplyToMessageId.Should().BeNull();
            result.message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.message.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify conversation was updated
            result.conversation.LastMessageId.Should().Be(result.message.Id);
            result.conversation.LastMessageTimestamp.Should().Be(result.message.CreatedAt);
            result.conversation.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify message was saved to database
            var savedMessage = await _context.Messages.FindAsync(result.message.Id);
            savedMessage.Should().NotBeNull();
            savedMessage!.Content.Should().Be(content);
        }

        [Fact]
        public async Task CreateMessageAsync_WithReceiverAsSender_ShouldSetCorrectReceiverId()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var content = "Reply message";

            // Act - receiver sends message back to initiator
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                receiverId,
                content);

            // Assert
            result.message.SenderId.Should().Be(receiverId);
            result.message.ReceiverId.Should().Be(initiatorId);
        }

        [Fact]
        public async Task CreateMessageAsync_WithReplyToMessageId_ShouldSetReplyToMessageId()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var replyToMessageId = Guid.NewGuid();
            var content = "This is a reply";

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                content,
                MessageType.Text,
                replyToMessageId);

            // Assert
            result.message.ReplyToMessageId.Should().Be(replyToMessageId);
        }

        [Fact]
        public async Task CreateMessageAsync_WithDifferentMessageTypes_ShouldSetCorrectMessageType()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var testCases = new[]
            {
                MessageType.Text,
                MessageType.Image,
                MessageType.File,
                MessageType.Audio,
                MessageType.Video,
                MessageType.System
            };

            foreach (var messageType in testCases)
            {
                // Act
                var result = await _service.CreateMessageAsync(
                    conversation.Id,
                    initiatorId,
                    $"Content for {messageType}",
                    messageType);

                // Assert
                result.message.MessageType.Should().Be(messageType);
            }
        }

        [Fact]
        public async Task CreateMessageAsync_WithNullMessageType_ShouldDefaultToText()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                "Test content");

            // Assert
            result.message.MessageType.Should().Be(MessageType.Text);
        }

        [Fact]
        public async Task CreateMessageAsync_WithNonExistentConversation_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentConversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var content = "Test message";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateMessageAsync(nonExistentConversationId, senderId, content));

            exception.Message.Should().Contain($"Conversation with ID {nonExistentConversationId} not found");
            exception.ParamName.Should().Be("conversationId");
        }

        [Fact]
        public async Task CreateMessageAsync_WithUnauthorizedSender_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var unauthorizedSenderId = Guid.NewGuid(); // Not part of conversation
            
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var content = "Unauthorized message";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CreateMessageAsync(conversation.Id, unauthorizedSenderId, content));

            exception.Message.Should().Be("User is not part of this conversation");
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldUpdateConversationTimestamps()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var originalTimestamp = DateTime.UtcNow.AddDays(-1);
            
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId,
                lastMessageTimestamp: originalTimestamp,
                updatedAt: originalTimestamp);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var content = "New message";

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                content);

            // Assert
            result.conversation.LastMessageTimestamp.Should().BeAfter(originalTimestamp);
            result.conversation.UpdatedAt.Should().BeAfter(originalTimestamp);
            result.conversation.LastMessageId.Should().Be(result.message.Id);
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldPersistChangesToDatabase()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var content = "Persistent message";

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                content);

            // Assert - Verify data persisted by checking the same context
            var persistedMessage = await _context.Messages.FindAsync(result.message.Id);
            var persistedConversation = await _context.Conversations.FindAsync(conversation.Id);

            persistedMessage.Should().NotBeNull();
            persistedMessage!.Content.Should().Be(content);
            
            persistedConversation.Should().NotBeNull();
            persistedConversation!.LastMessageId.Should().Be(result.message.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateMessageAsync_WithEmptyOrNullContent_ShouldStillCreateMessage(string content)
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                content);

            // Assert
            result.message.Should().NotBeNull();
            result.message.Content.Should().Be(content);
        }

        [Fact]
        public async Task CreateMessageAsync_ShouldSetDateTimeKindToUnspecified()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: initiatorId,
                receiverId: receiverId);
            
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CreateMessageAsync(
                conversation.Id,
                initiatorId,
                "Test content");

            // Assert
            result.message.CreatedAt.Kind.Should().Be(DateTimeKind.Unspecified);
            result.message.UpdatedAt.Kind.Should().Be(DateTimeKind.Unspecified);
            result.conversation.UpdatedAt.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 