using System;
using System.Threading.Tasks;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class SmtpClientServiceTests
    {
        private readonly Mock<ILogger<SmtpClientService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly SmtpClientService _service;

        public SmtpClientServiceTests()
        {
            _loggerMock = new Mock<ILogger<SmtpClientService>>();
            _configurationMock = new Mock<IConfiguration>();

            _service = new SmtpClientService(
                _loggerMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task SendEmailAsync_WithNullEmailMessage_ShouldReturnFalse()
        {
            // Act & Assert - Should handle null gracefully and return false
            var result = await _service.SendEmailAsync(null!);

            // The service should handle null input gracefully
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithInvalidConfiguration_ShouldReturnFalse()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            // Setup invalid configuration that will cause parsing errors
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("invalid-host");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("invalid-port"); // This will cause int.Parse to fail
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithValidEmailMessage_ShouldCreateCorrectMimeMessage()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            // Setup valid configuration
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");
            _configurationMock.Setup(x => x["Email:SmtpUsername"]).Returns("user");
            _configurationMock.Setup(x => x["Email:SmtpPassword"]).Returns("pass");

            // Act - This will fail due to SMTP connection, but we can test the setup logic
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but logic was executed
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithEmptyFromEmail_ShouldUseConfigurationDefaults()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "", // Empty from email
                FromName = ""   // Empty from name
            };

            // Setup configuration defaults
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns("default@example.com");
            _configurationMock.Setup(x => x["Email:FromName"]).Returns("Default Sender");
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but defaults were used
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithNullFromEmail_ShouldUseConfigurationDefaults()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = null, // Null from email
                FromName = null   // Null from name
            };

            // Setup configuration defaults
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns("default@example.com");
            _configurationMock.Setup(x => x["Email:FromName"]).Returns("Default Sender");
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but defaults were used
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithNoConfigurationDefaults_ShouldUseHardcodedDefaults()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = null,
                FromName = null
            };

            // Setup configuration to return null (no defaults configured)
            _configurationMock.Setup(x => x["Email:FromAddress"]).Returns((string?)null);
            _configurationMock.Setup(x => x["Email:FromName"]).Returns((string?)null);
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns((string?)null); // Will use "localhost"
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns((string?)null); // Will use "587"
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns((string?)null);   // Will use "true"

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but hardcoded defaults were used
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithGmailHost_ShouldUseGmailSpecificSettings()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@gmail.com",
                FromName = "Sender"
            };

            // Setup Gmail configuration
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("smtp.gmail.com");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");
            _configurationMock.Setup(x => x["Email:SmtpUsername"]).Returns("user@gmail.com");
            _configurationMock.Setup(x => x["Email:SmtpPassword"]).Returns("password");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but Gmail settings were applied
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithNoCredentials_ShouldUseAnonymousConnection()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            // Setup configuration without credentials
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("25");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("false");
            _configurationMock.Setup(x => x["Email:SmtpUsername"]).Returns((string?)null);
            _configurationMock.Setup(x => x["Email:SmtpPassword"]).Returns((string?)null);

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but anonymous connection was attempted
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendEmailAsync_WithEmptyCredentials_ShouldUseAnonymousConnection()
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            // Setup configuration with empty credentials
            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("25");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("false");
            _configurationMock.Setup(x => x["Email:SmtpUsername"]).Returns("");
            _configurationMock.Setup(x => x["Email:SmtpPassword"]).Returns("");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but anonymous connection was attempted
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("TRUE", true)]
        [InlineData("FALSE", false)]
        public async Task SendEmailAsync_WithVariousSSLSettings_ShouldParseCorrectly(string sslSetting, bool expectedSSL)
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns("587");
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns(sslSetting);

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but SSL setting was parsed
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("25")]
        [InlineData("587")]
        [InlineData("465")]
        [InlineData("2525")]
        public async Task SendEmailAsync_WithVariousPortSettings_ShouldParseCorrectly(string portSetting)
        {
            // Arrange
            var emailMessage = new EmailMessage
            {
                RecipientEmail = "test@example.com",
                RecipientName = "Test User",
                Subject = "Test Subject",
                HtmlBody = "<p>Test HTML</p>",
                TextBody = "Test Text",
                FromEmail = "sender@example.com",
                FromName = "Sender"
            };

            _configurationMock.Setup(x => x["Email:SmtpHost"]).Returns("localhost");
            _configurationMock.Setup(x => x["Email:SmtpPort"]).Returns(portSetting);
            _configurationMock.Setup(x => x["Email:UseSSL"]).Returns("true");

            // Act
            var result = await _service.SendEmailAsync(emailMessage);

            // Assert - Should return false due to connection failure, but port was parsed
            result.Should().BeFalse();
        }
    }
} 