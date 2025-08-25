using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using BackendMessages.DTO.Conversation.Request;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class ConversationServiceAdditionalTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<IConversationRepository> _conversationRepositoryMock;
        private readonly Mock<IMessageRepository> _messageRepositoryMock;
        private readonly Mock<IUserSearchService> _userSearchServiceMock;
        private readonly Mock<ILogger<ConversationServiceRefactored>> _loggerMock;
        private readonly ConversationServiceRefactored _service;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testConversationId = Guid.NewGuid();

        public ConversationServiceAdditionalTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"ConversationServiceAdditionalTests_{Guid.NewGuid()}")
                .Options;
            _context = new MessagesDbContext(options);

            // Setup mocks
            _conversationRepositoryMock = new Mock<IConversationRepository>();
            _messageRepositoryMock = new Mock<IMessageRepository>();
            _userSearchServiceMock = new Mock<IUserSearchService>();
            _loggerMock = new Mock<ILogger<ConversationServiceRefactored>>();

            // Create service
            _service = new ConversationServiceRefactored(
                _conversationRepositoryMock.Object,
                _messageRepositoryMock.Object,
                _userSearchServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateConversationAsync_WithNullRequest_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _service.CreateConversationAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Request cannot be null");
        }

        [Fact]
        public async Task CreateConversationAsync_WithSameUserIds_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = _testUserId,
                ReceiverId = _testUserId
            };

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Cannot create conversation with yourself");
        }

        [Fact]
        public async Task CreateConversationAsync_WithEmptyInitiatorId_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.NewGuid()
            };

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("InitiatorId is required");
        }

        [Fact]
        public async Task CreateConversationAsync_WithEmptyReceiverId_ShouldReturnErrorResponse()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.Empty
            };

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("ReceiverId is required");
        }

        [Fact]
        public async Task GetConversationByIdAsync_WithNonExistentConversation_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.GetConversationByIdAsync(nonExistentId, _testUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WithNonExistentConversation_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.GetConversationDetailAsync(nonExistentId, _testUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(conversation.Id))
                .ReturnsAsync(conversation);

            var nonExistentUserId = Guid.NewGuid();
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(nonExistentUserId))
                .ReturnsAsync((SearchUser?)null);

            // Act
            var result = await _service.GetConversationDetailAsync(conversation.Id, nonExistentUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserConversationsAsync_WithNonExistentUser_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var request = new GetConversationsRequest
            {
                UserId = nonExistentUserId
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(nonExistentUserId))
                .ReturnsAsync((SearchUser?)null);

            // Act
            var result = await _service.GetUserConversationsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserConversationsAsync_WithNoConversations_ShouldReturnEmptyList()
        {
            // Arrange
            var user = new SearchUser
            {
                Id = _testUserId,
                Username = "testuser",
                Email = "test@example.com"
            };

            var request = new GetConversationsRequest
            {
                UserId = _testUserId
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId))
                .ReturnsAsync(user);

            _conversationRepositoryMock.Setup(x => x.GetByUserIdAsync(_testUserId, 1, 20, false))
                .ReturnsAsync(new List<Conversation>());

            // Act
            var result = await _service.GetUserConversationsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().BeEmpty();
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_WithSameUserIds_ShouldReturnNull()
        {
            // Arrange
            var user1Id = _testUserId;
            var user2Id = _testUserId; // Same as user1Id

            // Act
            var result = await _service.GetOrCreateConversationAsync(user1Id, user2Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_WithExistingConversation_ShouldReturnExisting()
        {
            // Arrange
            var existingConversation = TestDataFactory.CreateConversation();
            existingConversation.InitiatorId = _testUserId;
            existingConversation.ReceiverId = Guid.NewGuid();

            _conversationRepositoryMock.Setup(x => x.GetConversationBetweenUsersAsync(_testUserId, existingConversation.ReceiverId))
                .ReturnsAsync(existingConversation);

            // Act
            var result = await _service.GetOrCreateConversationAsync(_testUserId, existingConversation.ReceiverId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(existingConversation.Id);
            _conversationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Conversation>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConversationAsync_WithNullRequest_ShouldReturnErrorResponse()
        {
            // Act
            var result = await _service.DeleteConversationAsync(null!);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Request cannot be null");
        }

        [Fact]
        public async Task DeleteConversationAsync_WithNonExistentConversation_ShouldReturnFalse()
        {
            // Arrange
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = _testUserId
            };

            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(request.ConversationId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("not found");
        }

        [Fact]
        public async Task DeleteConversationAsync_WithUserNotInConversation_ShouldReturnFalse()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var userNotInConversation = Guid.NewGuid();
            var request = new DeleteConversationRequest
            {
                ConversationId = conversation.Id,
                UserId = userNotInConversation
            };

            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(conversation.Id))
                .ReturnsAsync(conversation);

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Conversation not found or access denied");
        }

        [Fact]
        public async Task DeleteConversationAsync_WithAlreadyDeletedConversation_ShouldReturnFalse()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var request = new DeleteConversationRequest
            {
                ConversationId = conversation.Id,
                UserId = _testUserId
            };

            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(conversation.Id))
                .ReturnsAsync(conversation);
            _conversationRepositoryMock.Setup(x => x.UserCanAccessConversationAsync(conversation.Id, _testUserId))
                .ReturnsAsync(true);
            _conversationRepositoryMock.Setup(x => x.SoftDeleteAsync(conversation.Id, _testUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("Failed to delete conversation");
        }

        [Fact]
        public async Task GetConversationStatsAsync_WithNonExistentUser_ShouldReturnZeroStats()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(nonExistentUserId))
                .ReturnsAsync((SearchUser?)null);

            // Act
            var result = await _service.GetConversationStatsAsync(nonExistentUserId);

            // Assert
            result.Should().NotBeNull();
            result.TotalConversations.Should().Be(0);
            result.UnreadConversations.Should().Be(0);
            result.TotalMessages.Should().Be(0);
            result.UnreadMessages.Should().Be(0);
        }

        [Fact]
        public async Task GetConversationStatsAsync_WithNoConversations_ShouldReturnZeroStats()
        {
            // Arrange
            var user = new SearchUser
            {
                Id = _testUserId,
                Username = "testuser",
                Email = "test@example.com"
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId))
                .ReturnsAsync(user);

            _conversationRepositoryMock.Setup(x => x.GetByUserIdAsync(_testUserId, 1, 20, false))
                .ReturnsAsync(new List<Conversation>());
            _messageRepositoryMock.Setup(x => x.GetUnreadCountByUserIdAsync(_testUserId))
                .ReturnsAsync(0);

            // Act
            var result = await _service.GetConversationStatsAsync(_testUserId);

            // Assert
            result.Should().NotBeNull();
            result.TotalConversations.Should().Be(0);
            result.UnreadConversations.Should().Be(0);
            result.TotalMessages.Should().Be(0);
            result.UnreadMessages.Should().Be(0);
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WithNonExistentConversation_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Conversation?)null);

            // Act
            await _service.UpdateLastMessageAsync(nonExistentId, messageId);

            // Assert
            // Method returns void, so we just verify it doesn't throw
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WithEmptyMessageId_ShouldReturnFalse()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(conversation.Id))
                .ReturnsAsync(conversation);

            // Act
            await _service.UpdateLastMessageAsync(conversation.Id, Guid.Empty);

            // Assert
            // Method returns void, so we just verify it doesn't throw
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WithNonExistentConversation_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.UserCanAccessConversationAsync(nonExistentId, _testUserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WithNonExistentConversation_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _conversationRepositoryMock.Setup(x => x.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.IsConversationDeletedByUserAsync(nonExistentId, _testUserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUnreadConversationCountAsync_WithNonExistentUser_ShouldReturnZero()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(nonExistentUserId))
                .ReturnsAsync((SearchUser?)null);

            // Act
            var result = await _service.GetUnreadConversationCountAsync(nonExistentUserId);

            // Assert
            result.Should().Be(0);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 