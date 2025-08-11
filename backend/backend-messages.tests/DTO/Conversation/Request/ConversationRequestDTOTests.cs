using System;
using System.ComponentModel.DataAnnotations;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.DTO.Conversation.Request
{
    public class ConversationRequestDTOTests
    {
        [Fact]
        public void CreateConversationRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new CreateConversationRequest();

            // Assert
            request.InitiatorId.Should().BeEmpty();
            request.ReceiverId.Should().BeEmpty();
            request.InitialMessage.Should().BeNull();
        }

        [Fact]
        public void CreateConversationRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var initiatorId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var initialMessage = "Hello, let's start a conversation!";

            // Act
            var request = new CreateConversationRequest
            {
                InitiatorId = initiatorId,
                ReceiverId = receiverId,
                InitialMessage = initialMessage
            };

            // Assert
            request.InitiatorId.Should().Be(initiatorId);
            request.ReceiverId.Should().Be(receiverId);
            request.InitialMessage.Should().Be(initialMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithNullInitialMessage_ShouldBeValid()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = null
            };

            // Act & Assert
            request.InitialMessage.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("This is a longer initial message with more content")]
        [InlineData("Message with special chars: !@#$%^&*()")]
        [InlineData("Message with unicode: 🚀🌟💻")]
        public void CreateConversationRequest_WithDifferentInitialMessages_ShouldSetCorrectly(string initialMessage)
        {
            // Arrange
            var request = new CreateConversationRequest();

            // Act
            request.InitialMessage = initialMessage;

            // Assert
            request.InitialMessage.Should().Be(initialMessage);
        }

        [Fact]
        public void DeleteConversationRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new DeleteConversationRequest();

            // Assert
            request.ConversationId.Should().BeEmpty();
            request.UserId.Should().BeEmpty();
        }

        [Fact]
        public void DeleteConversationRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var request = new DeleteConversationRequest
            {
                ConversationId = conversationId,
                UserId = userId
            };

            // Assert
            request.ConversationId.Should().Be(conversationId);
            request.UserId.Should().Be(userId);
        }

        [Fact]
        public void GetConversationsRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new GetConversationsRequest();

            // Assert
            request.UserId.Should().BeEmpty();
            request.Page.Should().Be(1);
            request.PageSize.Should().Be(20);
            request.IncludeDeleted.Should().BeFalse();
        }

        [Fact]
        public void GetConversationsRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 3;
            var pageSize = 50;
            var includeDeleted = true;

            // Act
            var request = new GetConversationsRequest
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                IncludeDeleted = includeDeleted
            };

            // Assert
            request.UserId.Should().Be(userId);
            request.Page.Should().Be(page);
            request.PageSize.Should().Be(pageSize);
            request.IncludeDeleted.Should().Be(includeDeleted);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void GetConversationsRequest_WithDifferentPages_ShouldSetCorrectly(int page)
        {
            // Arrange
            var request = new GetConversationsRequest();

            // Act
            request.Page = page;

            // Assert
            request.Page.Should().Be(page);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void GetConversationsRequest_WithDifferentPageSizes_ShouldSetCorrectly(int pageSize)
        {
            // Arrange
            var request = new GetConversationsRequest();

            // Act
            request.PageSize = pageSize;

            // Assert
            request.PageSize.Should().Be(pageSize);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetConversationsRequest_WithDifferentIncludeDeletedValues_ShouldSetCorrectly(bool includeDeleted)
        {
            // Arrange
            var request = new GetConversationsRequest();

            // Act
            request.IncludeDeleted = includeDeleted;

            // Assert
            request.IncludeDeleted.Should().Be(includeDeleted);
        }

        [Fact]
        public void GetConversationMessagesRequest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var request = new GetConversationMessagesRequest();

            // Assert
            request.ConversationId.Should().BeEmpty();
            request.UserId.Should().BeEmpty();
            request.Page.Should().Be(1);
            request.PageSize.Should().Be(50);
            request.Before.Should().BeNull();
            request.After.Should().BeNull();
        }

        [Fact]
        public void GetConversationMessagesRequest_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var page = 2;
            var pageSize = 25;
            var before = DateTime.UtcNow;
            var after = DateTime.UtcNow.AddDays(-7);

            // Act
            var request = new GetConversationMessagesRequest
            {
                ConversationId = conversationId,
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                Before = before,
                After = after
            };

            // Assert
            request.ConversationId.Should().Be(conversationId);
            request.UserId.Should().Be(userId);
            request.Page.Should().Be(page);
            request.PageSize.Should().Be(pageSize);
            request.Before.Should().Be(before);
            request.After.Should().Be(after);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(100)]
        public void GetConversationMessagesRequest_WithDifferentPages_ShouldSetCorrectly(int page)
        {
            // Arrange
            var request = new GetConversationMessagesRequest();

            // Act
            request.Page = page;

            // Assert
            request.Page.Should().Be(page);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void GetConversationMessagesRequest_WithDifferentPageSizes_ShouldSetCorrectly(int pageSize)
        {
            // Arrange
            var request = new GetConversationMessagesRequest();

            // Act
            request.PageSize = pageSize;

            // Assert
            request.PageSize.Should().Be(pageSize);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithNullBefore_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest();

            // Act
            request.Before = null;

            // Assert
            request.Before.Should().BeNull();
        }

        [Fact]
        public void GetConversationMessagesRequest_WithNullAfter_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest();

            // Act
            request.After = null;

            // Assert
            request.After.Should().BeNull();
        }

        [Fact]
        public void GetConversationMessagesRequest_WithDateTimeValues_ShouldBeValid()
        {
            // Arrange
            var before = DateTime.UtcNow.AddDays(-1);
            var after = DateTime.UtcNow.AddDays(-30);
            var request = new GetConversationMessagesRequest();

            // Act
            request.Before = before;
            request.After = after;

            // Assert
            request.Before.Should().Be(before);
            request.After.Should().Be(after);
        }

        [Fact]
        public void CreateConversationRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.Empty,
                InitialMessage = "Test message"
            };

            // Act & Assert
            request.InitiatorId.Should().Be(Guid.Empty);
            request.ReceiverId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void DeleteConversationRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.Empty,
                UserId = Guid.Empty
            };

            // Act & Assert
            request.ConversationId.Should().Be(Guid.Empty);
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void GetConversationsRequest_WithEmptyGuid_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.Empty,
                Page = 1,
                PageSize = 20,
                IncludeDeleted = false
            };

            // Act & Assert
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithEmptyGuids_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.Empty,
                UserId = Guid.Empty,
                Page = 1,
                PageSize = 50
            };

            // Act & Assert
            request.ConversationId.Should().Be(Guid.Empty);
            request.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void CreateConversationRequest_WithSameInitiatorAndReceiver_ShouldBeValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateConversationRequest
            {
                InitiatorId = userId,
                ReceiverId = userId,
                InitialMessage = "Test message"
            };

            // Act & Assert
            request.InitiatorId.Should().Be(userId);
            request.ReceiverId.Should().Be(userId);
        }

        [Fact]
        public void CreateConversationRequest_WithLongInitialMessage_ShouldBeValid()
        {
            // Arrange
            var longMessage = new string('a', 5000);
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = longMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(longMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithSpecialCharactersInInitialMessage_ShouldBeValid()
        {
            // Arrange
            var specialMessage = "Message with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = specialMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(specialMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithUnicodeCharactersInInitialMessage_ShouldBeValid()
        {
            // Arrange
            var unicodeMessage = "Message with unicode: 🚀🌟💻📱🎉";
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = unicodeMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(unicodeMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithNewLinesInInitialMessage_ShouldBeValid()
        {
            // Arrange
            var multilineMessage = "Line 1\nLine 2\nLine 3";
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = multilineMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(multilineMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithTabCharactersInInitialMessage_ShouldBeValid()
        {
            // Arrange
            var tabMessage = "Message\twith\ttabs";
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = tabMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(tabMessage);
        }

        [Fact]
        public void CreateConversationRequest_WithWhitespaceOnlyInitialMessage_ShouldBeValid()
        {
            // Arrange
            var whitespaceMessage = "   ";
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = whitespaceMessage
            };

            // Act & Assert
            request.InitialMessage.Should().Be(whitespaceMessage);
        }

        [Fact]
        public void GetConversationsRequest_WithNegativePage_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = -1,
                PageSize = 20,
                IncludeDeleted = false
            };

            // Act & Assert
            request.Page.Should().Be(-1);
        }

        [Fact]
        public void GetConversationsRequest_WithZeroPage_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 0,
                PageSize = 20,
                IncludeDeleted = false
            };

            // Act & Assert
            request.Page.Should().Be(0);
        }

        [Fact]
        public void GetConversationsRequest_WithNegativePageSize_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = -1,
                IncludeDeleted = false
            };

            // Act & Assert
            request.PageSize.Should().Be(-1);
        }

        [Fact]
        public void GetConversationsRequest_WithZeroPageSize_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 0,
                IncludeDeleted = false
            };

            // Act & Assert
            request.PageSize.Should().Be(0);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithNegativePage_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = -1,
                PageSize = 50
            };

            // Act & Assert
            request.Page.Should().Be(-1);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithZeroPage_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 0,
                PageSize = 50
            };

            // Act & Assert
            request.Page.Should().Be(0);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithNegativePageSize_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = -1
            };

            // Act & Assert
            request.PageSize.Should().Be(-1);
        }

        [Fact]
        public void GetConversationMessagesRequest_WithZeroPageSize_ShouldBeValid()
        {
            // Arrange
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 0
            };

            // Act & Assert
            request.PageSize.Should().Be(0);
        }
    }
}
