using System;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.Services.Validators;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services.Validators
{
    public class ConversationValidatorTests
    {
        [Fact]
        public void CreateConversationValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void CreateConversationValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new CreateConversationValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void CreateConversationValidator_WithEmptyInitiatorId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.Empty,
                ReceiverId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("InitiatorId is required");
        }

        [Fact]
        public void CreateConversationValidator_WithEmptyReceiverId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.Empty
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ReceiverId is required");
        }

        [Fact]
        public void CreateConversationValidator_WithSameInitiatorAndReceiver_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var userId = Guid.NewGuid();
            var request = new CreateConversationRequest
            {
                InitiatorId = userId,
                ReceiverId = userId
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Cannot create conversation with yourself");
        }

        [Fact]
        public void CreateConversationValidator_WithLongInitialMessage_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = new string('a', 5001)
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Initial message cannot exceed 5000 characters");
        }

        [Fact]
        public void CreateConversationValidator_WithEmptyInitialMessage_ShouldReturnValid()
        {
            // Arrange
            var validator = new CreateConversationValidator();
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                InitialMessage = ""
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void DeleteConversationValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new DeleteConversationValidator();
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void DeleteConversationValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteConversationValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void DeleteConversationValidator_WithEmptyConversationId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteConversationValidator();
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ConversationId is required");
        }

        [Fact]
        public void DeleteConversationValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteConversationValidator();
            var request = new DeleteConversationRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("UserId is required");
        }

        [Fact]
        public void GetConversationsValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void GetConversationsValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void GetConversationsValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.Empty,
                Page = 1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("UserId is required");
        }

        [Fact]
        public void GetConversationsValidator_WithZeroPage_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 0,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Page must be greater than 0");
        }

        [Fact]
        public void GetConversationsValidator_WithNegativePage_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = -1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Page must be greater than 0");
        }

        [Fact]
        public void GetConversationsValidator_WithZeroPageSize_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 0
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("PageSize must be between 1 and 100");
        }

        [Fact]
        public void GetConversationsValidator_WithLargePageSize_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationsValidator();
            var request = new GetConversationsRequest
            {
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 101
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("PageSize must be between 1 and 100");
        }

        [Fact]
        public void GetConversationMessagesValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void GetConversationMessagesValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void GetConversationMessagesValidator_WithEmptyConversationId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.Empty,
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ConversationId is required");
        }

        [Fact]
        public void GetConversationMessagesValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.Empty,
                Page = 1,
                PageSize = 20
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("UserId is required");
        }

        [Fact]
        public void GetConversationMessagesValidator_WithInvalidDateRange_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20,
                Before = DateTime.UtcNow.AddDays(-1),
                After = DateTime.UtcNow
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Before date must be after the After date");
        }

        [Fact]
        public void GetConversationMessagesValidator_WithValidDateRange_ShouldReturnValid()
        {
            // Arrange
            var validator = new GetConversationMessagesValidator();
            var request = new GetConversationMessagesRequest
            {
                ConversationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Page = 1,
                PageSize = 20,
                Before = DateTime.UtcNow,
                After = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }
    }
}
