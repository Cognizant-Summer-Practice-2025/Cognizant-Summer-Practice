using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class EmailValidatorTests
    {
        private readonly EmailValidator _validator;

        public EmailValidatorTests()
        {
            _validator = new EmailValidator();
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name@domain.co.uk", true)]
        [InlineData("user+tag@example.org", true)]
        [InlineData("123@456.789", true)]
        [InlineData("test@sub.domain.com", true)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData(null, false)]
        [InlineData("invalid-email", false)]
        [InlineData("@example.com", false)]
        [InlineData("test@", false)]
        [InlineData("test.example.com", false)]
        [InlineData("test @example.com", false)]
        [InlineData("test@ example.com", false)]
        [InlineData("test@example .com", false)]
        public void IsValidEmail_WithVariousInputs_ShouldReturnExpectedResult(string email, bool expected)
        {
            // Act
            var result = _validator.IsValidEmail(email);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValidUnreadMessagesNotification_WithValidNotification_ShouldReturnTrue()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 5
            };

            // Act
            var result = _validator.IsValidUnreadMessagesNotification(notification);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidUnreadMessagesNotification_WithNullNotification_ShouldReturnFalse()
        {
            // Act
            var result = _validator.IsValidUnreadMessagesNotification(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("", "Test User", 5)]
        [InlineData(" ", "Test User", 5)]
        [InlineData(null, "Test User", 5)]
        [InlineData("invalid-email", "Test User", 5)]
        [InlineData("test@example.com", "Test User", 0)]
        [InlineData("test@example.com", "Test User", -1)]
        public void IsValidUnreadMessagesNotification_WithInvalidData_ShouldReturnFalse(
            string email, string name, int unreadCount)
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = email,
                RecipientName = name,
                UnreadCount = unreadCount
            };

            // Act
            var result = _validator.IsValidUnreadMessagesNotification(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidContactRequestNotification_WithValidNotification_ShouldReturnTrue()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = "sender", Email = "sender@test.com" },
                Recipient = new SearchUser { Username = "recipient", Email = "recipient@test.com" }
            };

            // Act
            var result = _validator.IsValidContactRequestNotification(notification);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidContactRequestNotification_WithNullNotification_ShouldReturnFalse()
        {
            // Act
            var result = _validator.IsValidContactRequestNotification(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidContactRequestNotification_WithNullRecipient_ShouldReturnFalse()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = "sender", Email = "sender@test.com" },
                Recipient = null!
            };

            // Act
            var result = _validator.IsValidContactRequestNotification(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidContactRequestNotification_WithNullSender_ShouldReturnFalse()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = null!,
                Recipient = new SearchUser { Username = "recipient", Email = "recipient@test.com" }
            };

            // Act
            var result = _validator.IsValidContactRequestNotification(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("", "sender")]
        [InlineData(" ", "sender")]
        [InlineData(null, "sender")]
        [InlineData("invalid-email", "sender")]
        [InlineData("recipient@test.com", "")]
        [InlineData("recipient@test.com", " ")]
        [InlineData("recipient@test.com", null)]
        public void IsValidContactRequestNotification_WithInvalidData_ShouldReturnFalse(
            string recipientEmail, string senderUsername)
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = senderUsername, Email = "sender@test.com" },
                Recipient = new SearchUser { Username = "recipient", Email = recipientEmail }
            };

            // Act
            var result = _validator.IsValidContactRequestNotification(notification);

            // Assert
            result.Should().BeFalse();
        }
    }
} 