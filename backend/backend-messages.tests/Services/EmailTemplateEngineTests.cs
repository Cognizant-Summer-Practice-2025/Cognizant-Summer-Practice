using System.Collections.Generic;
using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using FluentAssertions;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class EmailTemplateEngineTests
    {
        private readonly EmailTemplateEngine _templateEngine;

        public EmailTemplateEngineTests()
        {
            _templateEngine = new EmailTemplateEngine();
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_WithSenderNames_ShouldIncludeSenderNames()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 3,
                SenderNames = new List<string> { "Alice", "Bob", "Charlie" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("Alice, Bob, Charlie");
            textBody.Should().Contain("Alice, Bob, Charlie");
            
            htmlBody.Should().Contain("3");
            textBody.Should().Contain("3");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_WithEmptySenderNames_ShouldUseDefaultText()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 2,
                SenderNames = new List<string>()
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("various users");
            textBody.Should().Contain("various users");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_WithNullSenderNames_ShouldUseDefaultText()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 1,
                SenderNames = new List<string>()
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("various users");
            textBody.Should().Contain("various users");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_WithSingleMessage_ShouldGenerateCorrectContent()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 1,
                SenderNames = new List<string> { "SingleSender" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("SingleSender");
            textBody.Should().Contain("SingleSender");
            
            htmlBody.Should().Contain("1");
            textBody.Should().Contain("1");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_WithMultipleMessages_ShouldGenerateCorrectContent()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 10,
                SenderNames = new List<string> { "User1", "User2", "User3", "User4", "User5" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("User1, User2, User3, User4, User5");
            textBody.Should().Contain("User1, User2, User3, User4, User5");
            
            htmlBody.Should().Contain("10");
            textBody.Should().Contain("10");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_ShouldContainExpectedHtmlStructure()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 5,
                SenderNames = new List<string> { "TestSender" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().Contain("<!DOCTYPE html>");
            htmlBody.Should().Contain("<html>");
            htmlBody.Should().Contain("</html>");
            htmlBody.Should().Contain("<body");
            htmlBody.Should().Contain("</body>");
            htmlBody.Should().Contain("📬 Unread Messages");
        }

        [Fact]
        public void GenerateUnreadMessagesTemplate_ShouldContainExpectedTextStructure()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 3,
                SenderNames = new List<string> { "TestSender" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            textBody.Should().Contain("UNREAD MESSAGES");
            textBody.Should().Contain("TestSender");
            textBody.Should().Contain("3");
            textBody.Should().NotContain("<html>");
            textBody.Should().NotContain("<body>");
        }

        [Fact]
        public void GenerateContactRequestTemplate_WithValidNotification_ShouldGenerateContent()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser
                {
                    Username = "johndoe",
                    Email = "john@example.com",
                    FullName = "John Doe",
                    ProfessionalTitle = "Software Engineer"
                },
                Recipient = new SearchUser
                {
                    Username = "janedoe",
                    Email = "jane@example.com",
                    FullName = "Jane Doe"
                }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateContactRequestTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("johndoe");
            htmlBody.Should().Contain("John Doe");
            
            textBody.Should().Contain("johndoe");
            textBody.Should().Contain("John Doe");
        }

        [Fact]
        public void GenerateContactRequestTemplate_WithMinimalSenderInfo_ShouldGenerateContent()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser
                {
                    Username = "testuser",
                    Email = "test@example.com"
                    // No FullName or ProfessionalTitle
                },
                Recipient = new SearchUser
                {
                    Username = "recipient",
                    Email = "recipient@example.com"
                }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateContactRequestTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain("testuser");
            textBody.Should().Contain("testuser");
        }

        [Fact]
        public void GenerateContactRequestTemplate_ShouldContainExpectedHtmlStructure()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser
                {
                    Username = "sender",
                    Email = "sender@example.com",
                    FullName = "Sender Name"
                },
                Recipient = new SearchUser
                {
                    Username = "recipient",
                    Email = "recipient@example.com"
                }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateContactRequestTemplate(notification);

            // Assert
            htmlBody.Should().Contain("<!DOCTYPE html>");
            htmlBody.Should().Contain("<html>");
            htmlBody.Should().Contain("</html>");
            htmlBody.Should().Contain("<body");
            htmlBody.Should().Contain("</body>");
            htmlBody.Should().Contain("Contact Request");
        }

        [Fact]
        public void GenerateContactRequestTemplate_ShouldContainExpectedTextStructure()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser
                {
                    Username = "sender",
                    Email = "sender@example.com",
                    FullName = "Sender Name"
                },
                Recipient = new SearchUser
                {
                    Username = "recipient",
                    Email = "recipient@example.com"
                }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateContactRequestTemplate(notification);

            // Assert
            textBody.Should().Contain("CONTACT REQUEST");
            textBody.Should().Contain("sender");
            textBody.Should().Contain("Sender Name");
            textBody.Should().NotContain("<html>");
            textBody.Should().NotContain("<body>");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public void GenerateUnreadMessagesTemplate_WithVariousUnreadCounts_ShouldHandleCorrectly(int unreadCount)
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = unreadCount,
                SenderNames = new List<string> { "TestSender" }
            };

            // Act
            var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

            // Assert
            htmlBody.Should().NotBeNullOrEmpty();
            textBody.Should().NotBeNullOrEmpty();
            
            htmlBody.Should().Contain(unreadCount.ToString());
            textBody.Should().Contain(unreadCount.ToString());
        }
    }
} 