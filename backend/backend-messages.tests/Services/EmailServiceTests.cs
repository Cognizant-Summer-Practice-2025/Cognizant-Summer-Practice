using System;
using System.Threading.Tasks;
using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IEmailValidator> _emailValidatorMock;
        private readonly Mock<IEmailTemplateEngine> _templateEngineMock;
        private readonly Mock<ISmtpClientService> _smtpClientServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly EmailService _service;

        public EmailServiceTests()
        {
            _loggerMock = new Mock<ILogger<EmailService>>();
            _emailValidatorMock = new Mock<IEmailValidator>();
            _templateEngineMock = new Mock<IEmailTemplateEngine>();
            _smtpClientServiceMock = new Mock<ISmtpClientService>();
            _configurationMock = new Mock<IConfiguration>();

            _service = new EmailService(
                _loggerMock.Object,
                _emailValidatorMock.Object,
                _templateEngineMock.Object,
                _smtpClientServiceMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WithValidNotification_ShouldReturnTrue()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 5
            };

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateUnreadMessagesTemplate(notification))
                .Returns(("<html>Test HTML</html>", "Test Text"));
            
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns("noreply@test.com");
            _configurationMock.Setup(x => x["Email:FromName"]).Returns("Test Service");
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeTrue();
            _emailValidatorMock.Verify(x => x.IsValidUnreadMessagesNotification(notification), Times.Once);
            _templateEngineMock.Verify(x => x.GenerateUnreadMessagesTemplate(notification), Times.Once);
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(em => 
                em.RecipientEmail == notification.RecipientEmail &&
                em.Subject == "You have 5 unread messages" &&
                em.FromEmail == "noreply@test.com")), Times.Once);
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WithSingleMessage_ShouldUseSingularSubject()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 1
            };

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateUnreadMessagesTemplate(notification))
                .Returns(("<html>Test HTML</html>", "Test Text"));
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeTrue();
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(em => 
                em.Subject == "You have 1 unread message")), Times.Once);
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WithInvalidNotification_ShouldReturnFalse()
        {
            // Arrange
            var notification = new UnreadMessagesNotification();

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(false);

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeFalse();
            _templateEngineMock.Verify(x => x.GenerateUnreadMessagesTemplate(It.IsAny<UnreadMessagesNotification>()), Times.Never);
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Never);
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WhenSmtpFails_ShouldReturnFalse()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 3
            };

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateUnreadMessagesTemplate(notification))
                .Returns(("<html>Test HTML</html>", "Test Text"));
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WhenExceptionThrown_ShouldReturnFalse()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 2
            };

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateUnreadMessagesTemplate(notification))
                .Throws(new Exception("Template generation failed"));

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithValidNotification_ShouldReturnTrue()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = "sender", Email = "sender@test.com" },
                Recipient = new SearchUser { Username = "recipient", Email = "recipient@test.com", FullName = "Recipient User" }
            };

            _emailValidatorMock.Setup(x => x.IsValidContactRequestNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateContactRequestTemplate(notification))
                .Returns(("<html>Contact Request</html>", "Contact Request Text"));
            
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns("noreply@test.com");
            _configurationMock.Setup(x => x["Email:FromName"]).Returns("Test Service");
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendContactRequestNotificationAsync(notification);

            // Assert
            result.Should().BeTrue();
            _emailValidatorMock.Verify(x => x.IsValidContactRequestNotification(notification), Times.Once);
            _templateEngineMock.Verify(x => x.GenerateContactRequestTemplate(notification), Times.Once);
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(em => 
                em.RecipientEmail == notification.Recipient.Email &&
                em.RecipientName == "Recipient User" &&
                em.Subject == "sender wants to contact you")), Times.Once);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithNullFullName_ShouldUseUsername()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = "sender", Email = "sender@test.com" },
                Recipient = new SearchUser { Username = "recipient", Email = "recipient@test.com", FullName = null! }
            };

            _emailValidatorMock.Setup(x => x.IsValidContactRequestNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateContactRequestTemplate(notification))
                .Returns(("<html>Contact Request</html>", "Contact Request Text"));
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendContactRequestNotificationAsync(notification);

            // Assert
            result.Should().BeTrue();
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(em => 
                em.RecipientName == "recipient")), Times.Once);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithInvalidNotification_ShouldReturnFalse()
        {
            // Arrange
            var notification = new ContactRequestNotification();

            _emailValidatorMock.Setup(x => x.IsValidContactRequestNotification(notification))
                .Returns(false);

            // Act
            var result = await _service.SendContactRequestNotificationAsync(notification);

            // Assert
            result.Should().BeFalse();
            _templateEngineMock.Verify(x => x.GenerateContactRequestTemplate(It.IsAny<ContactRequestNotification>()), Times.Never);
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailMessage>()), Times.Never);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WhenExceptionThrown_ShouldReturnFalse()
        {
            // Arrange
            var notification = new ContactRequestNotification
            {
                Sender = new SearchUser { Username = "sender", Email = "sender@test.com" },
                Recipient = new SearchUser { Username = "recipient", Email = "recipient@test.com" }
            };

            _emailValidatorMock.Setup(x => x.IsValidContactRequestNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateContactRequestTemplate(notification))
                .Throws(new Exception("Template generation failed"));

            // Act
            var result = await _service.SendContactRequestNotificationAsync(notification);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendUnreadMessagesNotificationAsync_WithDefaultConfiguration_ShouldUseDefaultValues()
        {
            // Arrange
            var notification = new UnreadMessagesNotification
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                UnreadCount = 1
            };

            _emailValidatorMock.Setup(x => x.IsValidUnreadMessagesNotification(notification))
                .Returns(true);
            
            _templateEngineMock.Setup(x => x.GenerateUnreadMessagesTemplate(notification))
                .Returns(("<html>Test HTML</html>", "Test Text"));
            
            // Configuration returns null, should use defaults
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns((string?)null);
            _configurationMock.Setup(x => x["Email:FromName"]).Returns((string?)null);
            
            _smtpClientServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SendUnreadMessagesNotificationAsync(notification);

            // Assert
            result.Should().BeTrue();
            _smtpClientServiceMock.Verify(x => x.SendEmailAsync(It.Is<EmailMessage>(em => 
                em.FromEmail == "noreply@goalkeeper.com" &&
                em.FromName == "GoalKeeper Messages")), Times.Once);
        }
    }
} 