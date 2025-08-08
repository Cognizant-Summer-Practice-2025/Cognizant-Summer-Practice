using System;
using System.Collections.Generic;
using BackendMessages.DTO.Conversation.Response;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.DTO.Conversation.Response
{
    public class ConversationResponseDTOTests
    {
        [Fact]
        public void ConversationResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new ConversationResponse();

            // Assert
            response.Id.Should().BeEmpty();
            response.InitiatorId.Should().BeEmpty();
            response.ReceiverId.Should().BeEmpty();
            response.OtherUserId.Should().BeEmpty();
            response.OtherUserName.Should().BeEmpty();
            response.OtherUserAvatar.Should().BeNull();
            response.OtherUserProfessionalTitle.Should().BeNull();
            response.LastMessageTimestamp.Should().Be(default(DateTime));
            response.LastMessage.Should().BeNull();
            response.UnreadCount.Should().Be(0);
            response.CreatedAt.Should().Be(default(DateTime));
            response.UpdatedAt.Should().Be(default(DateTime));
            response.IsOnline.Should().BeFalse();
        }

        [Fact]
        public void ConversationResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var otherUserName = "John Doe";
            var otherUserAvatar = "https://example.com/avatar.jpg";
            var otherUserProfessionalTitle = "Software Engineer";
            var lastMessageTimestamp = DateTime.UtcNow.AddDays(-1);
            var lastMessage = new MessageSummaryResponse { Id = Guid.NewGuid() };
            var unreadCount = 5;
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow.AddDays(-1);
            var isOnline = true;

            // Act
            var response = new ConversationResponse
            {
                Id = id,
                InitiatorId = initiatorId,
                ReceiverId = receiverId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                OtherUserAvatar = otherUserAvatar,
                OtherUserProfessionalTitle = otherUserProfessionalTitle,
                LastMessageTimestamp = lastMessageTimestamp,
                LastMessage = lastMessage,
                UnreadCount = unreadCount,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsOnline = isOnline
            };

            // Assert
            response.Id.Should().Be(id);
            response.InitiatorId.Should().Be(initiatorId);
            response.ReceiverId.Should().Be(receiverId);
            response.OtherUserId.Should().Be(otherUserId);
            response.OtherUserName.Should().Be(otherUserName);
            response.OtherUserAvatar.Should().Be(otherUserAvatar);
            response.OtherUserProfessionalTitle.Should().Be(otherUserProfessionalTitle);
            response.LastMessageTimestamp.Should().Be(lastMessageTimestamp);
            response.LastMessage.Should().Be(lastMessage);
            response.UnreadCount.Should().Be(unreadCount);
            response.CreatedAt.Should().Be(createdAt);
            response.UpdatedAt.Should().Be(updatedAt);
            response.IsOnline.Should().Be(isOnline);
        }

        [Fact]
        public void ConversationResponse_WithNullOptionalProperties_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User",
                OtherUserAvatar = null,
                OtherUserProfessionalTitle = null,
                LastMessage = null
            };

            // Act & Assert
            response.OtherUserAvatar.Should().BeNull();
            response.OtherUserProfessionalTitle.Should().BeNull();
            response.LastMessage.Should().BeNull();
        }

        [Fact]
        public void ConversationDetailResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new ConversationDetailResponse();

            // Assert
            response.Id.Should().BeEmpty();
            response.InitiatorId.Should().BeEmpty();
            response.ReceiverId.Should().BeEmpty();
            response.OtherUserId.Should().BeEmpty();
            response.OtherUserName.Should().BeEmpty();
            response.OtherUserAvatar.Should().BeNull();
            response.OtherUserProfessionalTitle.Should().BeNull();
            response.LastMessageTimestamp.Should().Be(default(DateTime));
            response.LastMessage.Should().BeNull();
            response.UnreadCount.Should().Be(0);
            response.CreatedAt.Should().Be(default(DateTime));
            response.UpdatedAt.Should().Be(default(DateTime));
            response.IsOnline.Should().BeFalse();
            response.RecentMessages.Should().NotBeNull();
            response.RecentMessages.Should().BeEmpty();
        }

        [Fact]
        public void ConversationDetailResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var otherUserName = "Jane Smith";
            var otherUserAvatar = "https://example.com/avatar.png";
            var otherUserProfessionalTitle = "Product Manager";
            var lastMessageTimestamp = DateTime.UtcNow.AddDays(-2);
            var lastMessage = new MessageResponse { Id = Guid.NewGuid() };
            var unreadCount = 3;
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var updatedAt = DateTime.UtcNow.AddDays(-2);
            var isOnline = false;
            var recentMessages = new List<MessageResponse>
            {
                new MessageResponse { Id = Guid.NewGuid() },
                new MessageResponse { Id = Guid.NewGuid() }
            };

            // Act
            var response = new ConversationDetailResponse
            {
                Id = id,
                InitiatorId = initiatorId,
                ReceiverId = receiverId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName,
                OtherUserAvatar = otherUserAvatar,
                OtherUserProfessionalTitle = otherUserProfessionalTitle,
                LastMessageTimestamp = lastMessageTimestamp,
                LastMessage = lastMessage,
                UnreadCount = unreadCount,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsOnline = isOnline,
                RecentMessages = recentMessages
            };

            // Assert
            response.Id.Should().Be(id);
            response.InitiatorId.Should().Be(initiatorId);
            response.ReceiverId.Should().Be(receiverId);
            response.OtherUserId.Should().Be(otherUserId);
            response.OtherUserName.Should().Be(otherUserName);
            response.OtherUserAvatar.Should().Be(otherUserAvatar);
            response.OtherUserProfessionalTitle.Should().Be(otherUserProfessionalTitle);
            response.LastMessageTimestamp.Should().Be(lastMessageTimestamp);
            response.LastMessage.Should().Be(lastMessage);
            response.UnreadCount.Should().Be(unreadCount);
            response.CreatedAt.Should().Be(createdAt);
            response.UpdatedAt.Should().Be(updatedAt);
            response.IsOnline.Should().Be(isOnline);
            response.RecentMessages.Should().BeEquivalentTo(recentMessages);
        }

        [Fact]
        public void ConversationsPagedResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new ConversationsPagedResponse();

            // Assert
            response.Conversations.Should().NotBeNull();
            response.Conversations.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
            response.PageNumber.Should().Be(0);
            response.PageSize.Should().Be(0);
            response.HasNextPage.Should().BeFalse();
            response.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public void ConversationsPagedResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversations = new List<ConversationResponse>
            {
                new ConversationResponse { Id = Guid.NewGuid() },
                new ConversationResponse { Id = Guid.NewGuid() }
            };
            var totalCount = 15;
            var pageNumber = 2;
            var pageSize = 10;
            var hasNextPage = true;
            var hasPreviousPage = true;

            // Act
            var response = new ConversationsPagedResponse
            {
                Conversations = conversations,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = hasNextPage,
                HasPreviousPage = hasPreviousPage
            };

            // Assert
            response.Conversations.Should().BeEquivalentTo(conversations);
            response.TotalCount.Should().Be(totalCount);
            response.PageNumber.Should().Be(pageNumber);
            response.PageSize.Should().Be(pageSize);
            response.HasNextPage.Should().Be(hasNextPage);
            response.HasPreviousPage.Should().Be(hasPreviousPage);
        }

        [Fact]
        public void CreateConversationResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new CreateConversationResponse();

            // Assert
            response.Conversation.Should().NotBeNull();
            response.InitialMessage.Should().BeNull();
            response.Success.Should().BeFalse();
            response.Error.Should().BeNull();
        }

        [Fact]
        public void CreateConversationResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversation = new ConversationResponse { Id = Guid.NewGuid() };
            var initialMessage = new MessageResponse { Id = Guid.NewGuid() };
            var success = true;
            var error = "Test error";

            // Act
            var response = new CreateConversationResponse
            {
                Conversation = conversation,
                InitialMessage = initialMessage,
                Success = success,
                Error = error
            };

            // Assert
            response.Conversation.Should().Be(conversation);
            response.InitialMessage.Should().Be(initialMessage);
            response.Success.Should().Be(success);
            response.Error.Should().Be(error);
        }

        [Fact]
        public void CreateConversationResponse_WithNullInitialMessage_ShouldBeValid()
        {
            // Arrange
            var response = new CreateConversationResponse
            {
                Conversation = new ConversationResponse(),
                InitialMessage = null,
                Success = true,
                Error = null
            };

            // Act & Assert
            response.InitialMessage.Should().BeNull();
        }

        [Fact]
        public void DeleteConversationResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new DeleteConversationResponse();

            // Assert
            response.Success.Should().BeFalse();
            response.Error.Should().BeNull();
        }

        [Fact]
        public void DeleteConversationResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var success = true;
            var error = "Test error";

            // Act
            var response = new DeleteConversationResponse
            {
                Success = success,
                Error = error
            };

            // Assert
            response.Success.Should().Be(success);
            response.Error.Should().Be(error);
        }

        [Fact]
        public void DeleteConversationResponse_WithNullError_ShouldBeValid()
        {
            // Arrange
            var response = new DeleteConversationResponse
            {
                Success = true,
                Error = null
            };

            // Act & Assert
            response.Error.Should().BeNull();
        }

        [Fact]
        public void ConversationStatsResponse_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var response = new ConversationStatsResponse();

            // Assert
            response.TotalConversations.Should().Be(0);
            response.UnreadConversations.Should().Be(0);
            response.TotalMessages.Should().Be(0);
            response.UnreadMessages.Should().Be(0);
            response.LastActivity.Should().BeNull();
        }

        [Fact]
        public void ConversationStatsResponse_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var totalConversations = 25;
            var unreadConversations = 5;
            var totalMessages = 150;
            var unreadMessages = 12;
            var lastActivity = DateTime.UtcNow.AddHours(-2);

            // Act
            var response = new ConversationStatsResponse
            {
                TotalConversations = totalConversations,
                UnreadConversations = unreadConversations,
                TotalMessages = totalMessages,
                UnreadMessages = unreadMessages,
                LastActivity = lastActivity
            };

            // Assert
            response.TotalConversations.Should().Be(totalConversations);
            response.UnreadConversations.Should().Be(unreadConversations);
            response.TotalMessages.Should().Be(totalMessages);
            response.UnreadMessages.Should().Be(unreadMessages);
            response.LastActivity.Should().Be(lastActivity);
        }

        [Fact]
        public void ConversationStatsResponse_WithNullLastActivity_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationStatsResponse
            {
                TotalConversations = 10,
                UnreadConversations = 2,
                TotalMessages = 50,
                UnreadMessages = 5,
                LastActivity = null
            };

            // Act & Assert
            response.LastActivity.Should().BeNull();
        }

        [Fact]
        public void ConversationResponse_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationResponse
            {
                Id = Guid.Empty,
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.Empty,
                OtherUserId = Guid.Empty,
                OtherUserName = "Test User"
            };

            // Act & Assert
            response.Id.Should().Be(Guid.Empty);
            response.InitiatorId.Should().Be(Guid.Empty);
            response.ReceiverId.Should().Be(Guid.Empty);
            response.OtherUserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void ConversationDetailResponse_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationDetailResponse
            {
                Id = Guid.Empty,
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.Empty,
                OtherUserId = Guid.Empty,
                OtherUserName = "Test User"
            };

            // Act & Assert
            response.Id.Should().Be(Guid.Empty);
            response.InitiatorId.Should().Be(Guid.Empty);
            response.ReceiverId.Should().Be(Guid.Empty);
            response.OtherUserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void ConversationResponse_WithSameInitiatorAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = userId,
                ReceiverId = userId,
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User"
            };

            // Act & Assert
            response.InitiatorId.Should().Be(userId);
            response.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void ConversationDetailResponse_WithSameInitiatorAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = userId,
                ReceiverId = userId,
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User"
            };

            // Act & Assert
            response.InitiatorId.Should().Be(userId);
            response.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void ConversationResponse_WithLongOtherUserName_ShouldBeValid()
        {
            // Arrange
            var longName = new string('a', 100);
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = longName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(longName);
        }

        [Fact]
        public void ConversationDetailResponse_WithLongOtherUserName_ShouldBeValid()
        {
            // Arrange
            var longName = new string('b', 100);
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = longName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(longName);
        }

        [Fact]
        public void ConversationResponse_WithSpecialCharactersInOtherUserName_ShouldBeValid()
        {
            // Arrange
            var specialName = "Jean-Pierre O'Connor";
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = specialName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(specialName);
        }

        [Fact]
        public void ConversationDetailResponse_WithSpecialCharactersInOtherUserName_ShouldBeValid()
        {
            // Arrange
            var specialName = "José María García";
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = specialName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(specialName);
        }

        [Fact]
        public void ConversationResponse_WithUnicodeCharactersInOtherUserName_ShouldBeValid()
        {
            // Arrange
            var unicodeName = "李小明";
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = unicodeName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(unicodeName);
        }

        [Fact]
        public void ConversationDetailResponse_WithUnicodeCharactersInOtherUserName_ShouldBeValid()
        {
            // Arrange
            var unicodeName = "王小明";
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = unicodeName
            };

            // Act & Assert
            response.OtherUserName.Should().Be(unicodeName);
        }

        [Fact]
        public void ConversationResponse_WithEmptyOtherUserName_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = ""
            };

            // Act & Assert
            response.OtherUserName.Should().BeEmpty();
        }

        [Fact]
        public void ConversationDetailResponse_WithEmptyOtherUserName_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = ""
            };

            // Act & Assert
            response.OtherUserName.Should().BeEmpty();
        }

        [Fact]
        public void ConversationResponse_WithNegativeUnreadCount_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User",
                UnreadCount = -1
            };

            // Act & Assert
            response.UnreadCount.Should().Be(-1);
        }

        [Fact]
        public void ConversationDetailResponse_WithNegativeUnreadCount_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User",
                UnreadCount = -5
            };

            // Act & Assert
            response.UnreadCount.Should().Be(-5);
        }

        [Fact]
        public void ConversationsPagedResponse_WithNullConversations_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationsPagedResponse
            {
                Conversations = null!,
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            // Act & Assert
            response.Conversations.Should().BeNull();
        }

        [Fact]
        public void ConversationsPagedResponse_WithNegativeValues_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationsPagedResponse
            {
                Conversations = new List<ConversationResponse>(),
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

        [Fact]
        public void ConversationDetailResponse_WithNullRecentMessages_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationDetailResponse
            {
                Id = Guid.NewGuid(),
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OtherUserId = Guid.NewGuid(),
                OtherUserName = "Test User",
                RecentMessages = null!
            };

            // Act & Assert
            response.RecentMessages.Should().BeNull();
        }

        [Fact]
        public void ConversationStatsResponse_WithNegativeValues_ShouldBeValid()
        {
            // Arrange
            var response = new ConversationStatsResponse
            {
                TotalConversations = -1,
                UnreadConversations = -2,
                TotalMessages = -5,
                UnreadMessages = -3,
                LastActivity = DateTime.UtcNow
            };

            // Act & Assert
            response.TotalConversations.Should().Be(-1);
            response.UnreadConversations.Should().Be(-2);
            response.TotalMessages.Should().Be(-5);
            response.UnreadMessages.Should().Be(-3);
        }
    }
}
