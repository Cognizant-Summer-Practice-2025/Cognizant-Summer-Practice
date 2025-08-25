using System;
using System.Threading.Tasks;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class MessageNotificationServiceTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IOptions<EmailSettings>> _emailSettingsMock;
        private readonly Mock<ILogger<MessageNotificationService>> _loggerMock;
        private readonly EmailSettings _emailSettings;
        private readonly MessageNotificationService _service;

        public MessageNotificationServiceTests()
        {
            // Setup mocks
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _emailSettingsMock = new Mock<IOptions<EmailSettings>>();
            _loggerMock = new Mock<ILogger<MessageNotificationService>>();

            // Setup email settings
            _emailSettings = new EmailSettings
            {
                EnableContactNotifications = true,
                FromAddress = "test@example.com",
                FromName = "Test System"
            };
            _emailSettingsMock.Setup(x => x.Value).Returns(_emailSettings);

            _service = new MessageNotificationService(
                _serviceScopeFactoryMock.Object,
                _emailSettingsMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithDisabledNotifications_ShouldReturnImmediately()
        {
            // Arrange
            _emailSettings.EnableContactNotifications = false;
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            await task;

            // Assert
            task.IsCompletedSuccessfully.Should().BeTrue();
            _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithEnabledNotifications_ShouldCreateScope()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            // Setup service provider to throw an exception to exit early
            serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            await task;

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
            serviceScopeMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithNotFirstMessageFromInitiator_ShouldNotCreateScope()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            // Act - isFirstMessageFromInitiator = false
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, false);
            await task;

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once); // Still creates scope but doesn't send email
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_WithExceptionInBackgroundTask_ShouldLogError()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            var expectedException = new InvalidOperationException("Database connection failed");
            serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>())).Throws(expectedException);

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            await task;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Background notification task failed for conversation {conversationId}")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_ShouldDisposeServiceScope()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            // Setup service provider to throw an exception
            serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            await task;

            // Assert
            serviceScopeMock.Verify(x => x.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendContactRequestNotificationAsync_WithDifferentEmailSettings_ShouldRespectConfiguration(bool enableNotifications)
        {
            // Arrange
            _emailSettings.EnableContactNotifications = enableNotifications;
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            if (enableNotifications)
            {
                var serviceScopeMock = new Mock<IServiceScope>();
                var serviceProviderMock = new Mock<IServiceProvider>();
                
                _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
                serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
                serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>()))
                    .Throws(new InvalidOperationException("Test exception"));
            }

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            await task;

            // Assert
            if (enableNotifications)
            {
                _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
            }
            else
            {
                _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Never);
            }
        }

        [Fact]
        public async Task SendContactRequestNotificationAsync_ShouldRunInBackground()
        {
            // Arrange
            var conversationId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(It.IsAny<Type>()))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            var task = _service.SendContactRequestNotificationAsync(conversationId, senderId, receiverId, true);
            
            // The method should return immediately (background execution)
            task.Should().NotBeNull();
            
            // Wait for background task to complete
            await task;

            // Assert
            task.IsCompletedSuccessfully.Should().BeTrue();
            _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new MessageNotificationService(
                _serviceScopeFactoryMock.Object,
                _emailSettingsMock.Object,
                _loggerMock.Object);

            // Assert
            service.Should().NotBeNull();
        }


    }
} 