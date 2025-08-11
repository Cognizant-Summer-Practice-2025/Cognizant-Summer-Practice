using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Repositories
{
    public class MessageRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<MessagesDbContext> _options;
        private readonly Mock<ILogger<MessageRepository>> _loggerMock;
        private readonly MessagesDbContext _context;
        private readonly MessageRepository _repository;

        public MessageRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"MessageRepositoryTests_{Guid.NewGuid()}")
                .Options;

            _context = new MessagesDbContext(_options);
            _loggerMock = new Mock<ILogger<MessageRepository>>();
            _repository = new MessageRepository(_context, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task GetByIdAsync_WhenMessageExists_ShouldReturnMessage()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(message.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(message.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenMessageDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetByIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateMessage()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();

            // Act
            var result = await _repository.CreateAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(message.Id);
            result.CreatedAt.Should().NotBe(default);
            result.UpdatedAt.Should().NotBe(default);

            var savedMessage = await _context.Messages.FindAsync(message.Id);
            savedMessage.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.CreateAsync(message));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMessage()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = message.UpdatedAt;
            message.Content = "Updated content";

            // Act
            var result = await _repository.UpdateAsync(message);

            // Assert
            result.Should().NotBeNull();
            result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

            var updatedMessage = await _context.Messages.FindAsync(message.Id);
            updatedMessage.Should().NotBeNull();
            updatedMessage!.Content.Should().Be("Updated content");
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UpdateAsync(message));
        }

        [Fact]
        public async Task DeleteAsync_WhenMessageExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(message.Id);

            // Assert
            result.Should().BeTrue();

            var deletedMessage = await _context.Messages.FindAsync(message.Id);
            deletedMessage.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task SoftDeleteAsync_WhenMessageExists_ShouldSoftDeleteAndReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var userId = message.SenderId;

            // Act
            var result = await _repository.SoftDeleteAsync(message.Id, userId);

            // Assert
            result.Should().BeTrue();

            var softDeletedMessage = await _context.Messages.FindAsync(message.Id);
            softDeletedMessage.Should().NotBeNull();
            softDeletedMessage!.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.SoftDeleteAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.SoftDeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetByConversationIdAsync_ShouldReturnMessages()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(5, conversationId);
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByConversationIdAsync(conversationId, 1, 50);

            // Assert
            result.Should().HaveCount(5);
            result.Should().OnlyContain(m => m.ConversationId == conversationId);
        }

        [Fact]
        public async Task GetByConversationIdAsync_WithPagination_ShouldReturnPaginatedResults()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(10, conversationId);
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByConversationIdAsync(conversationId, 1, 5);

            // Assert
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetByConversationIdAsync_WithDateRange_ShouldReturnFilteredMessages()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            
            // Create messages with specific timestamps to ensure they fall within our date range
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(conversationId: conversationId, createdAt: DateTime.UtcNow.AddHours(-12)), // 12 hours ago
                TestDataFactory.CreateMessage(conversationId: conversationId, createdAt: DateTime.UtcNow.AddHours(-6)),  // 6 hours ago
                TestDataFactory.CreateMessage(conversationId: conversationId, createdAt: DateTime.UtcNow.AddHours(-2))   // 2 hours ago
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            var before = DateTime.UtcNow; // Now
            var after = DateTime.UtcNow.AddDays(-1); // 24 hours ago

            // Act
            var result = await _repository.GetByConversationIdAsync(conversationId, before, after);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(3); // All 3 messages should be within the date range
            result.Should().OnlyContain(m => m.CreatedAt < before && m.CreatedAt > after);
        }

        [Fact]
        public async Task GetByConversationIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetByConversationIdAsync(Guid.NewGuid(), 1, 50));
        }

        [Fact]
        public async Task GetTotalCountByConversationIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(5, conversationId);
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTotalCountByConversationIdAsync(conversationId);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task GetTotalCountByConversationIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetTotalCountByConversationIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetUnreadCountByConversationIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: true)
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadCountByConversationIdAsync(conversationId, userId);

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task GetUnreadCountByConversationIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetUnreadCountByConversationIdAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetUnreadCountByUserIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(receiverId: userId, isRead: true)
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadCountByUserIdAsync(userId);

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task GetUnreadCountByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetUnreadCountByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenMessageExists_ShouldMarkAsReadAndReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage(isRead: false);
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var userId = message.ReceiverId;

            // Act
            var result = await _repository.MarkAsReadAsync(message.Id, userId);

            // Assert
            result.Should().BeTrue();

            var updatedMessage = await _context.Messages.FindAsync(message.Id);
            updatedMessage.Should().NotBeNull();
            updatedMessage!.IsRead.Should().BeTrue();
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.MarkAsReadAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_ShouldMarkAllMessagesAsRead()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(conversationId: conversationId, receiverId: userId, isRead: true)
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.MarkConversationAsReadAsync(conversationId, userId);

            // Assert
            result.Should().Be(2);

            var updatedMessages = await _context.Messages.Where(m => m.ConversationId == conversationId).ToListAsync();
            updatedMessages.Should().OnlyContain(m => m.IsRead);
        }

        [Fact]
        public async Task MarkConversationAsReadAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.MarkConversationAsReadAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ExistsAsync_WhenMessageExists_ShouldReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(message.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.ExistsAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.ExistsAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WhenUserIsParticipant_ShouldReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var userId = message.SenderId;

            // Act
            var result = await _repository.UserCanAccessMessageAsync(message.Id, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WhenUserIsNotParticipant_ShouldReturnFalse()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var nonParticipantId = Guid.NewGuid();

            // Act
            var result = await _repository.UserCanAccessMessageAsync(message.Id, nonParticipantId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.UserCanAccessMessageAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserCanAccessMessageAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UserCanAccessMessageAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetUnreadMessagesByUserIdAsync_ShouldReturnUnreadMessages()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(receiverId: userId, isRead: false),
                TestDataFactory.CreateMessage(receiverId: userId, isRead: true)
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadMessagesByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => !m.IsRead && m.ReceiverId == userId);
        }

        [Fact]
        public async Task GetUnreadMessagesByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetUnreadMessagesByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetLatestMessageInConversationAsync_WhenMessagesExist_ShouldReturnLatestMessage()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messages = TestDataFactory.CreateMessages(5, conversationId);
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestMessageInConversationAsync(conversationId);

            // Assert
            result.Should().NotBeNull();
            result!.ConversationId.Should().Be(conversationId);
        }

        [Fact]
        public async Task GetLatestMessageInConversationAsync_WhenNoMessagesExist_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();

            // Act
            var result = await _repository.GetLatestMessageInConversationAsync(conversationId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLatestMessageInConversationAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetLatestMessageInConversationAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WhenUserIsSender_ShouldReturnTrue()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var userId = message.SenderId;

            // Act
            var result = await _repository.UserOwnsMessageAsync(message.Id, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WhenUserIsNotSender_ShouldReturnFalse()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var nonSenderId = Guid.NewGuid();

            // Act
            var result = await _repository.UserOwnsMessageAsync(message.Id, nonSenderId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WhenMessageDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.UserOwnsMessageAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserOwnsMessageAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UserOwnsMessageAsync(Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}
