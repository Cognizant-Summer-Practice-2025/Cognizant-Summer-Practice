using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Repositories;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Repositories
{
    public class MessageReportRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<MessagesDbContext> _options;
        private readonly Mock<ILogger<MessageReportRepository>> _loggerMock;
        private readonly MessagesDbContext _context;
        private readonly MessageReportRepository _repository;

        public MessageReportRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"MessageReportRepositoryTests_{Guid.NewGuid()}")
                .Options;

            _context = new MessagesDbContext(_options);
            _loggerMock = new Mock<ILogger<MessageReportRepository>>();
            _repository = new MessageReportRepository(_context, _loggerMock.Object);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task CreateReportAsync_ShouldCreateReport()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var reportedByUserId = Guid.NewGuid();
            var reason = "Inappropriate content";

            // Act
            var result = await _repository.CreateReportAsync(messageId, reportedByUserId, reason);

            // Assert
            result.Should().NotBeNull();
            result.MessageId.Should().Be(messageId);
            result.ReportedByUserId.Should().Be(reportedByUserId);
            result.Reason.Should().Be(reason);
            result.CreatedAt.Should().NotBe(default);

            var savedReport = await _context.MessageReports.FindAsync(result.Id);
            savedReport.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateReportAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.CreateReportAsync(Guid.NewGuid(), Guid.NewGuid(), "reason"));
        }

        [Fact]
        public async Task HasUserReportedMessageAsync_WhenUserHasReported_ShouldReturnTrue()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var report = TestDataFactory.CreateMessageReport(messageId: messageId, reportedByUserId: userId);
            _context.MessageReports.Add(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserReportedMessageAsync(messageId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasUserReportedMessageAsync_WhenUserHasNotReported_ShouldReturnFalse()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.HasUserReportedMessageAsync(messageId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasUserReportedMessageAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.HasUserReportedMessageAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task GetReportsByMessageIdAsync_ShouldReturnReports()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var messageId = message.Id;
            var reports = new List<MessageReport>
            {
                TestDataFactory.CreateMessageReport(messageId: messageId),
                TestDataFactory.CreateMessageReport(messageId: messageId),
                TestDataFactory.CreateMessageReport(messageId: messageId)
            };
            _context.MessageReports.AddRange(reports);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetReportsByMessageIdAsync(messageId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r.MessageId == messageId);
        }

        [Fact]
        public async Task GetReportsByMessageIdAsync_WhenNoReportsExist_ShouldReturnEmpty()
        {
            // Arrange
            var messageId = Guid.NewGuid();

            // Act
            var result = await _repository.GetReportsByMessageIdAsync(messageId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReportsByMessageIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetReportsByMessageIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_ShouldReturnReports()
        {
            // Arrange
            var userId = Guid.NewGuid();
            
            // Create messages first to satisfy foreign key constraints
            var messages = new List<Message>
            {
                TestDataFactory.CreateMessage(),
                TestDataFactory.CreateMessage(),
                TestDataFactory.CreateMessage()
            };
            _context.Messages.AddRange(messages);
            await _context.SaveChangesAsync();

            var reports = new List<MessageReport>
            {
                TestDataFactory.CreateMessageReport(messageId: messages[0].Id, reportedByUserId: userId),
                TestDataFactory.CreateMessageReport(messageId: messages[1].Id, reportedByUserId: userId),
                TestDataFactory.CreateMessageReport(messageId: messages[2].Id, reportedByUserId: userId)
            };
            _context.MessageReports.AddRange(reports);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetReportsByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(r => r.ReportedByUserId == userId);
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_WhenNoReportsExist_ShouldReturnEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _repository.GetReportsByUserIdAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetReportsByUserIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetReportsByUserIdAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetReportByIdAsync_WhenReportExists_ShouldReturnReport()
        {
            // Arrange
            var message = TestDataFactory.CreateMessage();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateMessageReport(messageId: message.Id);
            _context.MessageReports.Add(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetReportByIdAsync(report.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(report.Id);
        }

        [Fact]
        public async Task GetReportByIdAsync_WhenReportDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetReportByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetReportByIdAsync_WhenExceptionOccurs_ShouldThrowException()
        {
            // Arrange
            _context.Dispose(); // This will cause an exception

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => 
                _repository.GetReportByIdAsync(Guid.NewGuid()));
        }
    }
}
