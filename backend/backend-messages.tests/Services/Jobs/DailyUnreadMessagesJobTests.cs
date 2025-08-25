using System;
using System.Threading.Tasks;
using BackendMessages.Services.Abstractions;
using BackendMessages.Services.Jobs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using Xunit;

namespace BackendMessages.Tests.Services.Jobs
{
    public class DailyUnreadMessagesJobTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<DailyUnreadMessagesJob>> _loggerMock;
        private readonly Mock<IJobExecutionContext> _jobExecutionContextMock;
        private readonly DailyUnreadMessagesJob _job;

        public DailyUnreadMessagesJobTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<DailyUnreadMessagesJob>>();
            _jobExecutionContextMock = new Mock<IJobExecutionContext>();

            _job = new DailyUnreadMessagesJob(
                _notificationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Execute_WithSuccessfulNotificationService_ShouldCompleteSuccessfully()
        {
            // Arrange
            _notificationServiceMock.Setup(x => x.SendDailyUnreadMessagesNotificationsAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _job.Execute(_jobExecutionContextMock.Object);

            // Assert
            _notificationServiceMock.Verify(x => x.SendDailyUnreadMessagesNotificationsAsync(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenNotificationServiceThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var exception = new Exception("Notification service failed");
            _notificationServiceMock.Setup(x => x.SendDailyUnreadMessagesNotificationsAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => 
                _job.Execute(_jobExecutionContextMock.Object));

            thrownException.Should().Be(exception);
            _notificationServiceMock.Verify(x => x.SendDailyUnreadMessagesNotificationsAsync(), Times.Once);
        }

        [Fact]
        public async Task Execute_WhenNotificationServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var exception = new Exception("Notification service failed");
            _notificationServiceMock.Setup(x => x.SendDailyUnreadMessagesNotificationsAsync())
                .ThrowsAsync(exception);

            // Act
            try
            {
                await _job.Execute(_jobExecutionContextMock.Object);
            }
            catch
            {
                // Expected exception
            }

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during twice daily unread messages notification")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
} 