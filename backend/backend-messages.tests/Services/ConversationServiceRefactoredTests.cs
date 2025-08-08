using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.DTO.Conversation.Response;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class ConversationServiceRefactoredTests
    {
        private readonly Mock<IConversationRepository> _conversationRepositoryMock;
        private readonly Mock<IMessageRepository> _messageRepositoryMock;
        private readonly Mock<IUserSearchService> _userSearchServiceMock;
        private readonly Mock<ILogger<ConversationServiceRefactored>> _loggerMock;
        private readonly ConversationServiceRefactored _service;

        public ConversationServiceRefactoredTests()
        {
            _conversationRepositoryMock = new Mock<IConversationRepository>();
            _messageRepositoryMock = new Mock<IMessageRepository>();
            _userSearchServiceMock = new Mock<IUserSearchService>();
            _loggerMock = new Mock<ILogger<ConversationServiceRefactored>>();
            _service = new ConversationServiceRefactored(
                _conversationRepositoryMock.Object,
                _messageRepositoryMock.Object,
                _userSearchServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateConversationAsync_WithValidRequest_ShouldCreateConversation()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: request.InitiatorId,
                receiverId: request.ReceiverId);

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId))
                .ReturnsAsync((Conversation?)null);

            _conversationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Conversation>()))
                .ReturnsAsync(conversation);

            _messageRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Message>()))
                .ReturnsAsync(TestDataFactory.CreateMessage());

            _conversationRepositoryMock
                .Setup(x => x.UpdateLastMessageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _conversationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Conversation>()), Times.Once);
            _messageRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public async Task CreateConversationAsync_WithExistingConversation_ShouldReturnExistingConversation()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid()
            };

            var existingConversation = TestDataFactory.CreateConversation(
                initiatorId: request.InitiatorId,
                receiverId: request.ReceiverId);

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId))
                .ReturnsAsync(existingConversation);

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _conversationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Conversation>()), Times.Never);
        }

        [Fact]
        public async Task CreateConversationAsync_WithDeletedConversation_ShouldRestoreConversation()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid()
            };

            var existingConversation = TestDataFactory.CreateConversation(
                initiatorId: request.InitiatorId,
                receiverId: request.ReceiverId);
            existingConversation.InitiatorDeletedAt = DateTime.UtcNow;

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId))
                .ReturnsAsync(existingConversation);

            _conversationRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Conversation>()))
                .ReturnsAsync(existingConversation);

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _conversationRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Conversation>()), Times.Once);
        }

        [Fact]
        public async Task CreateConversationAsync_WithInvalidRequest_ShouldReturnError()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.Empty
            };

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateConversationAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetConversationByIdAsync_WithValidRequest_ShouldReturnConversation()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation();

            _conversationRepositoryMock
                .Setup(x => x.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.GetConversationByIdAsync(conversationId, userId);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetConversationByIdAsync_WhenConversationDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetByIdAsync(conversationId))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.GetConversationByIdAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationByIdAsync_WhenUserCannotAccess_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation();

            _conversationRepositoryMock
                .Setup(x => x.GetByIdAsync(conversationId))
                .ReturnsAsync(conversation);

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetConversationByIdAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WithValidRequest_ShouldReturnConversationDetail()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation();
            var messages = TestDataFactory.CreateMessages(5, conversationId);

            _conversationRepositoryMock
                .Setup(x => x.GetByIdWithMessagesAsync(conversationId, It.IsAny<int>()))
                .ReturnsAsync(conversation);

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.GetConversationDetailAsync(conversationId, userId);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WhenConversationDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetByIdWithMessagesAsync(conversationId, It.IsAny<int>()))
                .ReturnsAsync((Conversation?)null);

            // Act
            var result = await _service.GetConversationDetailAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserConversationsAsync_WithValidRequest_ShouldReturnConversations()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20
            };

            var conversations = TestDataFactory.CreateConversations(5);

            _conversationRepositoryMock
                .Setup(x => x.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, It.IsAny<bool>()))
                .ReturnsAsync(conversations);

            _conversationRepositoryMock
                .Setup(x => x.GetTotalCountByUserIdAsync(request.UserId, It.IsAny<bool>()))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetUserConversationsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().HaveCount(5);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task DeleteConversationAsync_WithValidRequest_ShouldDeleteConversation()
        {
            // Arrange
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(true);

            _conversationRepositoryMock
                .Setup(x => x.SoftDeleteAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _conversationRepositoryMock.Verify(x => x.SoftDeleteAsync(request.ConversationId, request.UserId), Times.Once);
        }

        [Fact]
        public async Task DeleteConversationAsync_WhenUserCannotAccess_ShouldReturnError()
        {
            // Arrange
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_WithExistingConversation_ShouldReturnExisting()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(initiatorId: user1Id, receiverId: user2Id);

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(user1Id, user2Id))
                .ReturnsAsync(conversation);

            // Act
            var result = await _service.GetOrCreateConversationAsync(user1Id, user2Id);

            // Assert
            result.Should().NotBeNull();
            _conversationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Conversation>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_WithNewConversation_ShouldCreateNew()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var conversation = TestDataFactory.CreateConversation(initiatorId: user1Id, receiverId: user2Id);

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(user1Id, user2Id))
                .ReturnsAsync((Conversation?)null);

            _conversationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Conversation>()))
                .ReturnsAsync(conversation);

            // Act
            var result = await _service.GetOrCreateConversationAsync(user1Id, user2Id);

            // Assert
            result.Should().NotBeNull();
            _conversationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Conversation>()), Times.Once);
        }

        [Fact]
        public async Task GetConversationStatsAsync_WithValidUserId_ShouldReturnStats()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetTotalCountByUserIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(10);

            _conversationRepositoryMock
                .Setup(x => x.GetUnreadConversationCountAsync(userId))
                .ReturnsAsync(3);

            // Act
            var result = await _service.GetConversationStatsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalConversations.Should().Be(10);
            result.UnreadConversations.Should().Be(3);
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WithValidRequest_ShouldReturnTrue()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UserCanAccessConversationAsync(conversationId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WithValidRequest_ShouldReturnResult()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.IsConversationDeletedByUserAsync(conversationId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IsConversationDeletedByUserAsync(conversationId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUnreadConversationCountAsync_WithValidUserId_ShouldReturnCount()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetUnreadConversationCountAsync(userId))
                .ReturnsAsync(5);

            // Act
            var result = await _service.GetUnreadConversationCountAsync(userId);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WithValidRequest_ShouldUpdateLastMessage()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UpdateLastMessageAsync(conversationId, messageId))
                .ReturnsAsync(true);

            // Act
            await _service.UpdateLastMessageAsync(conversationId, messageId);

            // Assert
            _conversationRepositoryMock.Verify(x => x.UpdateLastMessageAsync(conversationId, messageId), Times.Once);
        }

        [Fact]
        public async Task CreateConversationAsync_WhenDatabaseExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetConversationByIdAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetConversationByIdAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetConversationDetailAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserConversationsAsync_WhenExceptionOccurs_ShouldReturnEmptyResponse()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 10
            };

            _conversationRepositoryMock
                .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetUserConversationsAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task DeleteConversationAsync_WhenExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(request.ConversationId, request.UserId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.DeleteConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetOrCreateConversationAsync_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(user1Id, user2Id))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetOrCreateConversationAsync(user1Id, user2Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationStatsAsync_WhenExceptionOccurs_ShouldReturnZeroStats()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetActiveConversationsByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetConversationStatsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.TotalConversations.Should().Be(0);
            result.UnreadConversations.Should().Be(0);
        }

        [Fact]
        public async Task UserCanAccessConversationAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.UserCanAccessConversationAsync(conversationId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsConversationDeletedByUserAsync_WhenExceptionOccurs_ShouldReturnFalse()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.IsConversationDeletedByUserAsync(conversationId, userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.IsConversationDeletedByUserAsync(conversationId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUnreadConversationCountAsync_WhenExceptionOccurs_ShouldReturnZero()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.GetUnreadConversationCountAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.GetUnreadConversationCountAsync(userId);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateLastMessageAsync_WhenExceptionOccurs_ShouldThrow()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UpdateLastMessageAsync(conversationId, messageId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var act = async () => await _service.UpdateLastMessageAsync(conversationId, messageId);
            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task CreateConversationAsync_WithUserSearchServiceFailure_ShouldStillCreateConversation()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            var conversation = TestDataFactory.CreateConversation(
                initiatorId: request.InitiatorId,
                receiverId: request.ReceiverId);

            var message = TestDataFactory.CreateMessage(
                conversationId: conversation.Id,
                senderId: request.InitiatorId,
                receiverId: request.ReceiverId);

            _conversationRepositoryMock
                .Setup(x => x.GetConversationBetweenUsersAsync(request.InitiatorId, request.ReceiverId))
                .ReturnsAsync((BackendMessages.Models.Conversation?)null);

            _userSearchServiceMock
                .Setup(x => x.GetUserByIdAsync(request.ReceiverId))
                .ThrowsAsync(new Exception("User service error"));

            _conversationRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<BackendMessages.Models.Conversation>()))
                .ReturnsAsync(conversation);

            _messageRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<BackendMessages.Models.Message>()))
                .ReturnsAsync(message);

            _conversationRepositoryMock
                .Setup(x => x.UpdateLastMessageAsync(conversation.Id, message.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateConversationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue(); // Should succeed despite user service failure
        }

        [Fact]
        public async Task GetConversationByIdAsync_WithConversationNotFound_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            _conversationRepositoryMock
                .Setup(x => x.GetByIdAsync(conversationId))
                .ReturnsAsync((BackendMessages.Models.Conversation?)null);

            // Act
            var result = await _service.GetConversationByIdAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetConversationDetailAsync_WithConversationNotFound_ShouldReturnNull()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _conversationRepositoryMock
                .Setup(x => x.UserCanAccessConversationAsync(conversationId, userId))
                .ReturnsAsync(true);

            _conversationRepositoryMock
                .Setup(x => x.GetByIdWithMessagesAsync(conversationId, It.IsAny<int>()))
                .ReturnsAsync((BackendMessages.Models.Conversation?)null);

            // Act
            var result = await _service.GetConversationDetailAsync(conversationId, userId);

            // Assert
            result.Should().BeNull();
        }
    }
}
