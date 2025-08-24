using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.DTO.Notification;
using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Services
{
    public class NotificationServiceTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<IUserSearchService> _userSearchServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly NotificationService _service;
        private readonly Guid _testUserId1 = Guid.NewGuid();
        private readonly Guid _testUserId2 = Guid.NewGuid();
        private readonly Guid _testSenderId1 = Guid.NewGuid();
        private readonly Guid _testSenderId2 = Guid.NewGuid();

        public NotificationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"NotificationServiceTests_{Guid.NewGuid()}")
                .Options;
            _context = new MessagesDbContext(options);

            // Setup mocks
            _userSearchServiceMock = new Mock<IUserSearchService>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<NotificationService>>();

            _service = new NotificationService(
                _context,
                _userSearchServiceMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WithUnreadMessages_ShouldReturnSummaries()
        {
            // Arrange
            await SetupTestData();

            var user1 = new SearchUser
            {
                Id = _testUserId1,
                Username = "user1",
                Email = "user1@test.com",
                FullName = "User One"
            };

            var user2 = new SearchUser
            {
                Id = _testUserId2,
                Username = "user2",
                Email = "user2@test.com",
                FullName = "User Two"
            };

            var sender1 = new SearchUser
            {
                Id = _testSenderId1,
                Username = "sender1",
                Email = "sender1@test.com"
            };

            var sender2 = new SearchUser
            {
                Id = _testSenderId2,
                Username = "sender2",
                Email = "sender2@test.com"
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync(user1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId2)).ReturnsAsync(user2);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId1)).ReturnsAsync(sender1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId2)).ReturnsAsync(sender2);

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().HaveCount(2);
            
            var user1Summary = result.FirstOrDefault(s => s.UserId == _testUserId1);
            user1Summary.Should().NotBeNull();
            user1Summary!.UserName.Should().Be("User One");
            user1Summary.UserEmail.Should().Be("user1@test.com");
            user1Summary.UnreadCount.Should().Be(2);
            user1Summary.SenderNames.Should().Contain("sender1");

            var user2Summary = result.FirstOrDefault(s => s.UserId == _testUserId2);
            user2Summary.Should().NotBeNull();
            user2Summary!.UserName.Should().Be("User Two");
            user2Summary.UserEmail.Should().Be("user2@test.com");
            user2Summary.UnreadCount.Should().Be(1);
            user2Summary.SenderNames.Should().Contain("sender2");
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WithNoUnreadMessages_ShouldReturnEmptyList()
        {
            // Arrange - no unread messages in database

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WithDeletedMessages_ShouldIgnoreDeletedMessages()
        {
            // Arrange
            var deletedMessage = TestDataFactory.CreateMessage(
                conversationId: Guid.NewGuid(),
                senderId: _testSenderId1,
                receiverId: _testUserId1,
                content: "Deleted message");
            deletedMessage.IsRead = false;
            deletedMessage.DeletedAt = DateTime.UtcNow;

            _context.Messages.Add(deletedMessage);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WhenUserNotFound_ShouldSkipUser()
        {
            // Arrange
            await SetupTestData();

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync((SearchUser?)null);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId2)).ReturnsAsync(new SearchUser
            {
                Id = _testUserId2,
                Username = "user2",
                Email = "user2@test.com",
                FullName = "User Two"
            });
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId2)).ReturnsAsync(new SearchUser
            {
                Id = _testSenderId2,
                Username = "sender2"
            });

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].UserId.Should().Be(_testUserId2);
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WhenSenderNotFound_ShouldContinueWithoutSender()
        {
            // Arrange
            await SetupTestData();

            var user1 = new SearchUser
            {
                Id = _testUserId1,
                Username = "user1",
                Email = "user1@test.com",
                FullName = "User One"
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync(user1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId1)).ReturnsAsync((SearchUser?)null);

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].SenderNames.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUsersWithUnreadMessagesAsync_WhenExceptionThrown_ShouldReturnEmptyList()
        {
            // Arrange
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Service unavailable"));

            await SetupTestData();

            // Act
            var result = await _service.GetUsersWithUnreadMessagesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SendDailyUnreadMessagesNotificationsAsync_WithUsersToNotify_ShouldSendNotifications()
        {
            // Arrange
            await SetupTestData();

            var user1 = new SearchUser
            {
                Id = _testUserId1,
                Username = "user1",
                Email = "user1@test.com",
                FullName = "User One"
            };

            var sender1 = new SearchUser
            {
                Id = _testSenderId1,
                Username = "sender1"
            };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync(user1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId1)).ReturnsAsync(sender1);
            _emailServiceMock.Setup(x => x.SendUnreadMessagesNotificationAsync(It.IsAny<UnreadMessagesNotification>()))
                .ReturnsAsync(true);

            // Act
            await _service.SendDailyUnreadMessagesNotificationsAsync();

            // Assert
            _emailServiceMock.Verify(x => x.SendUnreadMessagesNotificationAsync(It.Is<UnreadMessagesNotification>(n =>
                n.RecipientEmail == "user1@test.com" &&
                n.RecipientName == "User One" &&
                n.UnreadCount == 2)), Times.Once);
        }

        [Fact]
        public async Task SendDailyUnreadMessagesNotificationsAsync_WithNoUsersToNotify_ShouldNotSendNotifications()
        {
            // Arrange - no unread messages

            // Act
            await _service.SendDailyUnreadMessagesNotificationsAsync();

            // Assert
            _emailServiceMock.Verify(x => x.SendUnreadMessagesNotificationAsync(It.IsAny<UnreadMessagesNotification>()), Times.Never);
        }

        [Fact]
        public async Task SendDailyUnreadMessagesNotificationsAsync_WhenEmailServiceFails_ShouldContinueWithOtherUsers()
        {
            // Arrange
            await SetupTestData();

            var user1 = new SearchUser
            {
                Id = _testUserId1,
                Username = "user1",
                Email = "user1@test.com",
                FullName = "User One"
            };

            var user2 = new SearchUser
            {
                Id = _testUserId2,
                Username = "user2",
                Email = "user2@test.com",
                FullName = "User Two"
            };

            var sender1 = new SearchUser { Id = _testSenderId1, Username = "sender1" };
            var sender2 = new SearchUser { Id = _testSenderId2, Username = "sender2" };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync(user1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId2)).ReturnsAsync(user2);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId1)).ReturnsAsync(sender1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId2)).ReturnsAsync(sender2);

            _emailServiceMock.Setup(x => x.SendUnreadMessagesNotificationAsync(It.Is<UnreadMessagesNotification>(n => 
                n.RecipientEmail == "user1@test.com"))).ReturnsAsync(false);
            _emailServiceMock.Setup(x => x.SendUnreadMessagesNotificationAsync(It.Is<UnreadMessagesNotification>(n => 
                n.RecipientEmail == "user2@test.com"))).ReturnsAsync(true);

            // Act
            await _service.SendDailyUnreadMessagesNotificationsAsync();

            // Assert
            _emailServiceMock.Verify(x => x.SendUnreadMessagesNotificationAsync(It.IsAny<UnreadMessagesNotification>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SendDailyUnreadMessagesNotificationsAsync_WhenExceptionThrown_ShouldHandleGracefully()
        {
            // Arrange
            await SetupTestData();

            var user1 = new SearchUser
            {
                Id = _testUserId1,
                Username = "user1",
                Email = "user1@test.com",
                FullName = "User One"
            };

            var sender1 = new SearchUser { Id = _testSenderId1, Username = "sender1" };

            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testUserId1)).ReturnsAsync(user1);
            _userSearchServiceMock.Setup(x => x.GetUserByIdAsync(_testSenderId1)).ReturnsAsync(sender1);
            _emailServiceMock.Setup(x => x.SendUnreadMessagesNotificationAsync(It.IsAny<UnreadMessagesNotification>()))
                .ThrowsAsync(new Exception("Email service error"));

            // Act & Assert - should not throw
            await _service.SendDailyUnreadMessagesNotificationsAsync();

            _emailServiceMock.Verify(x => x.SendUnreadMessagesNotificationAsync(It.IsAny<UnreadMessagesNotification>()), Times.Once);
        }

        private async Task SetupTestData()
        {
            // Create unread messages for user1 (2 messages from sender1)
            var message1 = TestDataFactory.CreateMessage(
                conversationId: Guid.NewGuid(),
                senderId: _testSenderId1,
                receiverId: _testUserId1,
                content: "Message 1");
            message1.IsRead = false;

            var message2 = TestDataFactory.CreateMessage(
                conversationId: Guid.NewGuid(),
                senderId: _testSenderId1,
                receiverId: _testUserId1,
                content: "Message 2");
            message2.IsRead = false;

            // Create unread message for user2 (1 message from sender2)
            var message3 = TestDataFactory.CreateMessage(
                conversationId: Guid.NewGuid(),
                senderId: _testSenderId2,
                receiverId: _testUserId2,
                content: "Message 3");
            message3.IsRead = false;

            // Create read message (should be ignored)
            var readMessage = TestDataFactory.CreateMessage(
                conversationId: Guid.NewGuid(),
                senderId: _testSenderId1,
                receiverId: _testUserId1,
                content: "Read message");
            readMessage.IsRead = true;

            _context.Messages.AddRange(message1, message2, message3, readMessage);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 