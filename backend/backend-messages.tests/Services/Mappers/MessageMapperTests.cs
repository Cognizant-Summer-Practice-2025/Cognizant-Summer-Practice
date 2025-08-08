using System;
using System.Collections.Generic;
using System.Linq;
using BackendMessages.DTO.Message.Request;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Models;
using BackendMessages.Services.Mappers;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services.Mappers
{
    public class MessageMapperTests
    {
        [Fact]
        public void ToResponse_WithValidMessage_ShouldMapCorrectly()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();

            // Act
            var result = MessageMapper.ToResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(message.Id);
            result.ConversationId.Should().Be(message.ConversationId);
            result.SenderId.Should().Be(message.SenderId);
            result.ReceiverId.Should().Be(message.ReceiverId);
            result.Content.Should().Be(message.Content ?? string.Empty);
            result.MessageType.Should().Be(message.MessageType);
            result.IsRead.Should().Be(message.IsRead);
            result.CreatedAt.Should().Be(message.CreatedAt);
            result.UpdatedAt.Should().Be(message.UpdatedAt);
            result.ReplyToMessageId.Should().Be(message.ReplyToMessageId);
        }

        [Fact]
        public void ToResponse_WithNullContent_ShouldMapEmptyString()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            message.Content = null;

            // Act
            var result = MessageMapper.ToResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(string.Empty);
        }

        [Fact]
        public void ToResponse_WithReplyToMessage_ShouldMapReplyToMessage()
        {
            // Arrange
            var replyToMessage = TestDataFactory.CreateMessage();
            var message = TestDataFactory.CreateMessage();
            message.ReplyToMessageId = replyToMessage.Id;
            message.ReplyToMessage = replyToMessage;

            // Act
            var result = MessageMapper.ToResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.ReplyToMessage.Should().NotBeNull();
            result.ReplyToMessage!.Id.Should().Be(replyToMessage.Id);
        }

        [Fact]
        public void ToResponse_WithoutReplyToMessage_ShouldMapNull()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();

            // Act
            var result = MessageMapper.ToResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.ReplyToMessage.Should().BeNull();
        }

        [Fact]
        public void ToSummaryResponse_WithValidMessage_ShouldMapCorrectly()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();

            // Act
            var result = MessageMapper.ToSummaryResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(message.Id);
            result.ConversationId.Should().Be(message.ConversationId);
            result.SenderId.Should().Be(message.SenderId);
            result.Content.Should().Be(message.Content ?? string.Empty);
            result.IsRead.Should().Be(message.IsRead);
            result.CreatedAt.Should().Be(message.CreatedAt);
        }

        [Fact]
        public void ToSummaryResponse_WithNullContent_ShouldMapEmptyString()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            message.Content = null;

            // Act
            var result = MessageMapper.ToSummaryResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(string.Empty);
        }

        [Fact]
        public void ToEntity_WithValidRequest_ShouldMapCorrectly()
        {
            // Arrange
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!",
                ReplyToMessageId = Guid.NewGuid()
            };

            // Act
            var result = MessageMapper.ToEntity(request);

            // Assert
            result.Should().NotBeNull();
            result.ConversationId.Should().Be(request.ConversationId);
            result.SenderId.Should().Be(request.SenderId);
            result.ReceiverId.Should().Be(request.ReceiverId);
            result.Content.Should().Be(request.Content);
            result.MessageType.Should().Be(MessageType.Text);
            result.IsRead.Should().BeFalse();
            result.ReplyToMessageId.Should().Be(request.ReplyToMessageId);
        }

        [Fact]
        public void ToResponseList_WithValidMessages_ShouldMapAll()
        {
            // Arrange
            var messages = TestDataFactory.CreateMessages(3, Guid.NewGuid());

            // Act
            var result = MessageMapper.ToResponseList(messages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r != null);
        }

        [Fact]
        public void ToResponseList_WithEmptyList_ShouldReturnEmpty()
        {
            // Arrange
            var messages = new List<Message>();

            // Act
            var result = MessageMapper.ToResponseList(messages);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void ToSummaryResponseList_WithValidMessages_ShouldMapAll()
        {
            // Arrange
            var messages = TestDataFactory.CreateMessages(3, Guid.NewGuid());

            // Act
            var result = MessageMapper.ToSummaryResponseList(messages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r != null);
        }

        [Fact]
        public void ToSummaryResponseList_WithEmptyList_ShouldReturnEmpty()
        {
            // Arrange
            var messages = new List<Message>();

            // Act
            var result = MessageMapper.ToSummaryResponseList(messages);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void ToPagedResponse_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var messages = TestDataFactory.CreateMessages(5, Guid.NewGuid());
            var totalCount = 15; // Changed to 15 so there will be a next page
            var pageNumber = 2;
            var pageSize = 5;

            // Act
            var result = MessageMapper.ToPagedResponse(messages, totalCount, pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Messages.Should().HaveCount(5);
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
            var messages = TestDataFactory.CreateMessages(5, Guid.NewGuid());
            var totalCount = 10;
            var pageNumber = 1;
            var pageSize = 5;

            // Act
            var result = MessageMapper.ToPagedResponse(messages, totalCount, pageNumber, pageSize);

            // Assert
            result.HasPreviousPage.Should().BeFalse();
        }

        [Fact]
        public void ToPagedResponse_WithLastPage_ShouldHaveNoNextPage()
        {
            // Arrange
            var messages = TestDataFactory.CreateMessages(5, Guid.NewGuid());
            var totalCount = 10;
            var pageNumber = 2;
            var pageSize = 5;

            // Act
            var result = MessageMapper.ToPagedResponse(messages, totalCount, pageNumber, pageSize);

            // Assert
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public void ToSendMessageResponse_WithValidMessage_ShouldMapCorrectly()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();

            // Act
            var result = MessageMapper.ToSendMessageResponse(message);

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToSendMessageResponse_WithError_ShouldMapError()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            var error = "Send failed";

            // Act
            var result = MessageMapper.ToSendMessageResponse(message, false, error);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ToMarkAsReadResponse_WithValidData_ShouldMapCorrectly()
        {
            // Arrange
            var messagesMarked = 5;

            // Act
            var result = MessageMapper.ToMarkAsReadResponse(messagesMarked);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.MessagesMarked.Should().Be(messagesMarked);
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToMarkAsReadResponse_WithError_ShouldMapError()
        {
            // Arrange
            var messagesMarked = 0;
            var error = "Mark as read failed";

            // Act
            var result = MessageMapper.ToMarkAsReadResponse(messagesMarked, false, error);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.MessagesMarked.Should().Be(messagesMarked);
            result.Error.Should().Be(error);
        }

        [Fact]
        public void ToDeleteMessageResponse_WithValidData_ShouldMapCorrectly()
        {
            // Act
            var result = MessageMapper.ToDeleteMessageResponse();

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ToDeleteMessageResponse_WithError_ShouldMapError()
        {
            // Arrange
            var error = "Delete failed";

            // Act
            var result = MessageMapper.ToDeleteMessageResponse(false, error);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Error.Should().Be(error);
        }
    }
}
