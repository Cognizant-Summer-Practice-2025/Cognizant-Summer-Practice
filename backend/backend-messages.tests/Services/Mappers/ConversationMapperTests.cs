using System;
using System.Collections.Generic;
using System.Linq;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.DTO.Conversation.Response;
using BackendMessages.Models;
using BackendMessages.Services.Mappers;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services.Mappers
{
    public class ConversationMapperTests
    {
        [Fact]
        public void ToResponse_WithValidConversation_ShouldMapCorrectly()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;
            var otherUserName = "John Doe";
            var otherUserAvatar = "avatar.jpg";
            var otherUserProfessionalTitle = "Software Engineer";
            var unreadCount = 5;
            var isOnline = true;

            // Act
            var result = ConversationMapper.ToResponse(conversation, currentUserId, otherUserName, otherUserAvatar, otherUserProfessionalTitle, unreadCount, isOnline);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(conversation.Id);
            result.InitiatorId.Should().Be(conversation.InitiatorId);
            result.ReceiverId.Should().Be(conversation.ReceiverId);
            result.OtherUserId.Should().Be(conversation.ReceiverId);
            result.OtherUserName.Should().Be(otherUserName);
            result.OtherUserAvatar.Should().Be(otherUserAvatar);
            result.OtherUserProfessionalTitle.Should().Be(otherUserProfessionalTitle);
            result.UnreadCount.Should().Be(unreadCount);
            result.IsOnline.Should().Be(isOnline);
            result.CreatedAt.Should().Be(conversation.CreatedAt);
            result.UpdatedAt.Should().Be(conversation.UpdatedAt);
        }

        [Fact]
        public void ToResponse_WhenCurrentUserIsReceiver_ShouldMapOtherUserIdCorrectly()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.ReceiverId;

            // Act
            var result = ConversationMapper.ToResponse(conversation, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.OtherUserId.Should().Be(conversation.InitiatorId);
        }

        [Fact]
        public void ToResponse_WithoutOptionalParameters_ShouldUseDefaults()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;

            // Act
            var result = ConversationMapper.ToResponse(conversation, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.OtherUserName.Should().Be("Unknown User");
            result.OtherUserAvatar.Should().BeNull();
            result.OtherUserProfessionalTitle.Should().BeNull();
            result.UnreadCount.Should().Be(0);
            result.IsOnline.Should().BeFalse();
        }

        [Fact]
        public void ToDetailResponse_WithValidConversation_ShouldMapCorrectly()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;
            var messages = TestDataFactory.CreateMessages(3, conversation.Id);
            conversation.Messages = messages;

            // Act
            var result = ConversationMapper.ToDetailResponse(conversation, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(conversation.Id);
            result.RecentMessages.Should().HaveCount(3);
        }

        [Fact]
        public void ToDetailResponse_WithNullMessages_ShouldReturnEmptyList()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;
            conversation.Messages = new List<Message>();

            // Act
            var result = ConversationMapper.ToDetailResponse(conversation, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.RecentMessages.Should().BeEmpty();
        }

        [Fact]
        public void ToEntity_WithValidRequest_ShouldMapCorrectly()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            // Act
            var result = ConversationMapper.ToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.InitiatorId.Should().Be(request.InitiatorId);
            result.ReceiverId.Should().Be(request.ReceiverId);
        }

        [Fact]
        public void ToResponseList_WithValidConversations_ShouldMapAll()
        {
            // Arrange
            var conversations = TestDataFactory.CreateConversations(3);
            var currentUserId = conversations.First().InitiatorId;

            // Act
            var result = ConversationMapper.ToResponseList(conversations, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r != null);
        }

        [Fact]
        public void ToResponseList_WithEmptyList_ShouldReturnEmpty()
        {
            // Arrange
            var conversations = new List<Conversation>();
            var currentUserId = Guid.NewGuid();

            // Act
            var result = ConversationMapper.ToResponseList(conversations, currentUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void ToPagedResponse_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var conversations = TestDataFactory.CreateConversations(5);
            var currentUserId = conversations.First().InitiatorId;
            var totalCount = 15; // Changed to 15 so there will be a next page
            var pageNumber = 2;
            var pageSize = 5;

            // Act
            var result = ConversationMapper.ToPagedResponse(conversations, currentUserId, totalCount, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Conversations.Should().HaveCount(5);
            result.TotalCount.Should().Be(totalCount);
            result.PageNumber.Should().Be(pageNumber);
            result.PageSize.Should().Be(pageSize);
            result.HasNextPage.Should().BeTrue(); // page 2 of 3 pages, so should have next page
            result.HasPreviousPage.Should().BeTrue();
        }

        [Fact]
        public void ToPagedResponse_WithFirstPage_ShouldHaveNoPreviousPage()
        {
            // Arrange
            var conversations = TestDataFactory.CreateConversations(5);
            var currentUserId = conversations.First().InitiatorId;
            var totalCount = 10;
            var pageNumber = 1;
            var pageSize = 5;

            // Act
            var result = ConversationMapper.ToPagedResponse(conversations, currentUserId, totalCount, pageNumber, pageSize);

            // Assert
            result.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public void ToPagedResponse_WithLastPage_ShouldHaveNoNextPage()
        {
            // Arrange
            var conversations = TestDataFactory.CreateConversations(5);
            var currentUserId = conversations.First().InitiatorId;
            var totalCount = 10;
            var pageNumber = 2;
            var pageSize = 5;

            // Act
            var result = ConversationMapper.ToPagedResponse(conversations, currentUserId, totalCount, pageNumber, pageSize);

            // Assert
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public void ToCreateConversationResponse_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;
            var initialMessage = TestDataFactory.CreateMessage();

            // Act
            var result = ConversationMapper.ToCreateConversationResponse(conversation, currentUserId, initialMessage);

            // Assert
            result.Should().NotBeNull();
            result.Conversation.Should().NotBeNull();
            result.InitialMessage.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToCreateConversationResponse_WithError_ShouldMapError()
        {
            // Arrange
            var conversation = TestDataFactory.CreateConversation();
            var currentUserId = conversation.InitiatorId;
            var error = "Something went wrong";

            // Act
            var result = ConversationMapper.ToCreateConversationResponse(conversation, currentUserId, null, false, error);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ToDeleteConversationResponse_WithSuccess_ShouldMapCorrectly()
        {
            // Act
            var result = ConversationMapper.ToDeleteConversationResponse();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToDeleteConversationResponse_WithError_ShouldMapError()
        {
            // Arrange
            var error = "Delete failed";

            // Act
            var result = ConversationMapper.ToDeleteConversationResponse(false, error);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ToStatsResponse_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var totalConversations = 10;
            var unreadConversations = 3;
            var totalMessages = 50;
            var unreadMessages = 15;
            var lastActivity = DateTime.UtcNow;

            // Act
            var result = ConversationMapper.ToStatsResponse(totalConversations, unreadConversations, totalMessages, unreadMessages, lastActivity);

            // Assert
            result.Should().NotBeNull();
            result.TotalConversations.Should().Be(totalConversations);
            result.UnreadConversations.Should().Be(unreadConversations);
            result.TotalMessages.Should().Be(totalMessages);
            result.UnreadMessages.Should().Be(unreadMessages);
            result.LastActivity.Should().Be(lastActivity);
        }

        [Fact]
        public void ToStatsResponse_WithoutLastActivity_ShouldMapCorrectly()
        {
            // Arrange
            var totalConversations = 10;
            var unreadConversations = 3;
            var totalMessages = 50;
            var unreadMessages = 15;

            // Act
            var result = ConversationMapper.ToStatsResponse(totalConversations, unreadConversations, totalMessages, unreadMessages);

            // Assert
            result.Should().NotBeNull();
            result.LastActivity.Should().BeNull();
        }
    }
}
