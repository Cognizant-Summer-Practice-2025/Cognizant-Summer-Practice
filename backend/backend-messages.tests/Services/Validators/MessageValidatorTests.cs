using System;
using BackendMessages.DTO.Message.Request;
using BackendMessages.Services.Validators;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services.Validators
{
    public class MessageValidatorTests
    {
        [Fact]
        public void SendMessageValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void SendMessageValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void SendMessageValidator_WithEmptyConversationId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.Empty,
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ConversationId is required");
        }

        [Fact]
        public void SendMessageValidator_WithEmptySenderId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.Empty,
                ReceiverId = Guid.NewGuid(),
                Content = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("SenderId is required");
        }

        [Fact]
        public void SendMessageValidator_WithEmptyReceiverId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.Empty,
                Content = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ReceiverId is required");
        }

        [Fact]
        public void SendMessageValidator_WithEmptyContent_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = ""
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Message content cannot be empty");
        }

        [Fact]
        public void SendMessageValidator_WithWhitespaceContent_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = "   "
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Message content cannot be empty");
        }

        [Fact]
        public void SendMessageValidator_WithLongContent_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                Content = new string('a', 5001)
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Message content cannot exceed 5000 characters");
        }

        [Fact]
        public void SendMessageValidator_WithSameSenderAndReceiver_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new SendMessageValidator();
            var userId = Guid.NewGuid();
            var request = new SendMessageRequest
            {
                ConversationId = Guid.NewGuid(),
                SenderId = userId,
                ReceiverId = userId,
                Content = "Hello!"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Cannot send message to yourself");
        }

        [Fact]
        public void MarkMessageAsReadValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new MarkMessageAsReadValidator();
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void MarkMessageAsReadValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessageAsReadValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void MarkMessageAsReadValidator_WithEmptyMessageId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessageAsReadValidator();
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("MessageId is required");
        }

        [Fact]
        public void MarkMessageAsReadValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessageAsReadValidator();
            var request = new MarkMessageAsReadRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("UserId is required");
        }

        [Fact]
        public void MarkMessagesAsReadValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new MarkMessagesAsReadValidator();
            var request = new MarkMessagesAsReadRequest
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
        public void MarkMessagesAsReadValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessagesAsReadValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void MarkMessagesAsReadValidator_WithEmptyConversationId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessagesAsReadValidator();
            var request = new MarkMessagesAsReadRequest
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
        public void MarkMessagesAsReadValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new MarkMessagesAsReadValidator();
            var request = new MarkMessagesAsReadRequest
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
        public void DeleteMessageValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new DeleteMessageValidator();
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void DeleteMessageValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteMessageValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void DeleteMessageValidator_WithEmptyMessageId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteMessageValidator();
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.Empty,
                UserId = Guid.NewGuid()
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("MessageId is required");
        }

        [Fact]
        public void DeleteMessageValidator_WithEmptyUserId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new DeleteMessageValidator();
            var request = new DeleteMessageRequest
            {
                MessageId = Guid.NewGuid(),
                UserId = Guid.Empty
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("UserId is required");
        }

        [Fact]
        public void ReportMessageValidator_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void ReportMessageValidator_WithNullRequest_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();

            // Act
            var result = validator.Validate(null!);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Request cannot be null");
        }

        [Fact]
        public void ReportMessageValidator_WithEmptyMessageId_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.Empty,
                ReportedById = Guid.NewGuid(),
                Reason = "Inappropriate content"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("MessageId is required");
        }

        [Fact]
        public void ReportMessageValidator_WithEmptyReportedById_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.Empty,
                Reason = "Inappropriate content"
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("ReportedById is required");
        }

        [Fact]
        public void ReportMessageValidator_WithEmptyReason_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = ""
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Reason is required");
        }

        [Fact]
        public void ReportMessageValidator_WithWhitespaceReason_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = "   "
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Reason is required");
        }

        [Fact]
        public void ReportMessageValidator_WithLongReason_ShouldReturnInvalid()
        {
            // Arrange
            var validator = new ReportMessageValidator();
            var request = new ReportMessageRequest
            {
                MessageId = Guid.NewGuid(),
                ReportedById = Guid.NewGuid(),
                Reason = new string('a', 501)
            };

            // Act
            var result = validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Be("Reason cannot exceed 500 characters");
        }
    }
}
