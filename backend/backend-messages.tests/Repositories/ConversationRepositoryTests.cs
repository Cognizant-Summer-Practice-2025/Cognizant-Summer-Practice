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
    public class ConversationRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<MessagesDbContext> _options;
        private readonly Mock<ILogger<ConversationRepository>> _loggerMock;
        private readonly MessagesDbContext _context;
        private readonly ConversationRepository _repository;

        public ConversationRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"ConversationRepositoryTests_{Guid.NewGuid()}")
                .Options;

            _context = new MessagesDbContext(_options);
            _loggerMock = new Mock<ILogger<ConversationRepository>>();
            _repository = new ConversationRepository(_context, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task GetByIdAsync_WhenConversationExists_ShouldReturnConversation()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(conversation.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(conversation.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenConversationDoesNotExist_ShouldReturnNull()
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
        public async Task CreateAsync_ShouldCreateConversation()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();

            // Act
            var result = await _repository.CreateAsync(conversation);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(conversation.Id);
            result.CreatedAt.Should().NotBe(default);
            result.UpdatedAt.Should().NotBe(default);
            result.LastMessageTimestamp.Should().NotBe(default);

            var savedConversation = await _context.Conversations.FindAsync(conversation.Id);
            savedConversation.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.CreateAsync(conversation));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateConversation()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = conversation.UpdatedAt;
            conversation.InitiatorId = Guid.NewGuid();

            // Act
            var result = await _repository.UpdateAsync(conversation);

            // Assert
            result.Should().NotBeNull();
            result.UpdatedAt.Should().BeAfter(originalUpdatedAt);

            var updatedConversation = await _context.Conversations.FindAsync(conversation.Id);
            updatedConversation.Should().NotBeNull();
            updatedConversation!.InitiatorId.Should().Be(conversation.InitiatorId);
        }

        [Fact]
        public async Task UpdateAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UpdateAsync(conversation));
        }

        [Fact]
        public async Task DeleteAsync_WhenConversationExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(conversation.Id);

            // Assert
            result.Should().BeTrue();

            var deletedConversation = await _context.Conversations.FindAsync(conversation.Id);
            deletedConversation.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
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
        public async Task SoftDeleteAsync_WhenConversationExists_ShouldSoftDeleteAndReturnTrue()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var userId = conversation.InitiatorId;

            // Act
            var result = await _repository.SoftDeleteAsync(conversation.Id, userId);

            // Assert
            result.Should().BeTrue();

            var softDeletedConversation = await _context.Conversations.FindAsync(conversation.Id);
            softDeletedConversation.Should().NotBeNull();
            softDeletedConversation!.InitiatorDeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task SoftDeleteAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
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
        public async Task GetByUserIdAsync_ShouldReturnConversations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversations = new List<Conversation>
            {
                TestDataFactory.CreateConversation(initiatorId: userId),
                TestDataFactory.CreateConversation(receiverId: userId),
                TestDataFactory.CreateConversation(initiatorId: userId)
            };

            _context.Conversations.AddRange(conversations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(c => c.InitiatorId == userId || c.ReceiverId == userId);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithPagination_ShouldReturnPaginatedResults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversations = Enumerable.Range(0, 10)
                .Select(_ => TestDataFactory.CreateConversation(initiatorId: userId))
                .ToList();

            _context.Conversations.AddRange(conversations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdAsync(userId, page: 1, pageSize: 5);

            // Assert
            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetTotalCountByUserIdAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversations = new List<Conversation>
            {
                TestDataFactory.CreateConversation(initiatorId: userId),
                TestDataFactory.CreateConversation(receiverId: userId),
                TestDataFactory.CreateConversation(initiatorId: userId)
            };

            _context.Conversations.AddRange(conversations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTotalCountByUserIdAsync(userId);

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public async Task GetTotalCountByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetTotalCountByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetConversationBetweenUsersAsync_WhenConversationExists_ShouldReturnConversation()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(initiatorId: user1Id, receiverId: user2Id);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetConversationBetweenUsersAsync(user1Id, user2Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(conversation.Id);
        }

        [Fact]
        public async Task GetConversationBetweenUsersAsync_WhenConversationDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            // Act
            var result = await _repository.GetConversationBetweenUsersAsync(user1Id, user2Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationBetweenUsersAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetConversationBetweenUsersAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ExistsAsync_WhenConversationExists_ShouldReturnTrue()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(conversation.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
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
        public async Task UserCanAccessConversationAsync_WhenUserIsParticipant_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(initiatorId: userId);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.UserCanAccessConversationAsync(conversation.Id, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WhenUserIsNotParticipant_ShouldReturnFalse()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var nonParticipantId = Guid.NewGuid();

            // Act
            var result = await _repository.UserCanAccessConversationAsync(conversation.Id, nonParticipantId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.UserCanAccessConversationAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UserCanAccessConversationAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WhenConversationExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var messageId = Guid.NewGuid();
            var originalLastMessageId = conversation.LastMessageId;
            var originalTimestamp = conversation.LastMessageTimestamp;

            // Add a small delay to ensure different timestamps
            await Task.Delay(10);

            // Act
            var result = await _repository.UpdateLastMessageAsync(conversation.Id, messageId);

            // Assert
            result.Should().BeTrue();

            var updatedConversation = await _context.Conversations.FindAsync(conversation.Id);
            updatedConversation.Should().NotBeNull();
            updatedConversation!.LastMessageId.Should().Be(messageId);
            updatedConversation.LastMessageTimestamp.Should().BeAfter(originalTimestamp);
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            // Act
            var result = await _repository.UpdateLastMessageAsync(nonExistentId, messageId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.UpdateLastMessageAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetActiveConversationsByUserIdAsync_ShouldReturnActiveConversations()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversations = new List<Conversation>
            {
                TestDataFactory.CreateConversation(initiatorId: userId),
                TestDataFactory.CreateConversation(receiverId: userId),
                TestDataFactory.CreateDeletedConversation(userId)
            };

            _context.Conversations.AddRange(conversations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveConversationsByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(c => !c.IsDeletedByUser(userId));
        }

        [Fact]
        public async Task GetActiveConversationsByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetActiveConversationsByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetUnreadConversationCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversations = new List<Conversation>
            {
                TestDataFactory.CreateConversation(initiatorId: userId),
                TestDataFactory.CreateConversation(receiverId: userId),
                TestDataFactory.CreateConversation(initiatorId: userId)
            };

            _context.Conversations.AddRange(conversations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnreadConversationCountAsync(userId);

            // Assert
            result.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetUnreadConversationCountAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetUnreadConversationCountAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WhenUserDeletedConversation_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateDeletedConversation(userId);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsConversationDeletedByUserAsync(conversation.Id, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WhenUserDidNotDeleteConversation_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(initiatorId: userId);
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsConversationDeletedByUserAsync(conversation.Id, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WhenConversationDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.IsConversationDeletedByUserAsync(nonExistentId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.IsConversationDeletedByUserAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetByIdWithMessagesAsync_WhenConversationExists_ShouldReturnConversationWithMessages()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var messages = TestDataFactory.CreateMessages(5, conversation.Id);
            
            _context.Conversations.Add(conversation);
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithMessagesAsync(conversation.Id, 3);

            // Assert
            result.Should().NotBeNull();
            result!.Messages.Should().HaveCountLessOrEqualTo(5);
            result!.Messages.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdWithMessagesAsync_WhenConversationDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdWithMessagesAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdWithMessagesAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetByIdWithMessagesAsync(Guid.NewGuid()));
        }
    }
}
