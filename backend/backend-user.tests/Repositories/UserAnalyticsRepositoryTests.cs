using Xunit;
using FluentAssertions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using backend_user.tests.Helpers;

namespace backend_user.tests.Repositories
{
    public class UserAnalyticsRepositoryTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly IUserAnalyticsRepository _repository;

        public UserAnalyticsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(options);
            _repository = new UserAnalyticsRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region CreateUserAnalyticsAsync Tests

        [Fact]
        public async Task CreateUserAnalyticsAsync_WithValidData_ShouldCreateAndReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics = new UserAnalytics
            {
                UserId = user.Id,
                SessionId = "session-123",
                EventType = "page_view",
                EventData = "{\"page\":\"/home\"}",
                IpAddress = IPAddress.Parse("192.168.1.1"),
                UserAgent = "Mozilla/5.0",
                ReferrerUrl = "https://google.com"
            };

            // Act
            var result = await _repository.CreateUserAnalyticsAsync(analytics);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.UserId.Should().Be(user.Id);
            result.SessionId.Should().Be("session-123");
            result.EventType.Should().Be("page_view");
            result.EventData.Should().Be("{\"page\":\"/home\"}");
            result.IpAddress.Should().Be(IPAddress.Parse("192.168.1.1"));
            result.UserAgent.Should().Be("Mozilla/5.0");
            result.ReferrerUrl.Should().Be("https://google.com");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        #endregion

        #region GetUserAnalyticsByIdAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByIdAsync_WithExistingId_ShouldReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics = TestDataFactory.CreateValidUserAnalytics(user.Id);
            await _context.UserAnalytics.AddAsync(analytics);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsByIdAsync(analytics.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(analytics.Id);
            result.UserId.Should().Be(user.Id);
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetUserAnalyticsByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserAnalyticsByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserAnalyticsAsync Tests

        [Fact]
        public async Task GetUserAnalyticsAsync_WithExistingUser_ShouldReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(a => a.UserId == user.Id);
        }

        [Fact]
        public async Task GetUserAnalyticsAsync_WithNonExistentUser_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetUserAnalyticsAsync(Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetUserAnalyticsBySessionAsync Tests

        [Fact]
        public async Task GetUserAnalyticsBySessionAsync_WithExistingSession_ShouldReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics1.SessionId = "session-123";
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics2.SessionId = "session-123";
            var analytics3 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics3.SessionId = "session-456";

            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2, analytics3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsBySessionAsync("session-123");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(a => a.SessionId == "session-123");
        }

        #endregion

        #region GetUserAnalyticsByEventTypeAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByEventTypeAsync_WithExistingEventType_ShouldReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics1.EventType = "page_view";
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics2.EventType = "page_view";
            var analytics3 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics3.EventType = "button_click";

            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2, analytics3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsByEventTypeAsync("page_view");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(a => a.EventType == "page_view");
        }

        #endregion

        #region GetAllUserAnalyticsAsync Tests

        [Fact]
        public async Task GetAllUserAnalyticsAsync_WithExistingAnalytics_ShouldReturnAllAnalytics()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user1.Id);
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user2.Id);
            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUserAnalyticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
        }

        [Fact]
        public async Task GetAllUserAnalyticsAsync_WithNoAnalytics_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllUserAnalyticsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetUserAnalyticsByDateRangeAsync Tests

        [Fact]
        public async Task GetUserAnalyticsByDateRangeAsync_WithAnalyticsInRange_ShouldReturnAnalytics()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var startDate = DateTime.UtcNow.AddDays(-2);
            var endDate = DateTime.UtcNow.AddDays(1);

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics1.CreatedAt = DateTime.UtcNow.AddDays(-1);
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics2.CreatedAt = DateTime.UtcNow;
            var analytics3 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            analytics3.CreatedAt = DateTime.UtcNow.AddDays(2); // Outside range

            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2, analytics3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsByDateRangeAsync(user.Id, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(a => a.UserId == user.Id);
            result.Should().OnlyContain(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);
        }

        #endregion

        #region GetUserAnalyticsCountAsync Tests

        [Fact]
        public async Task GetUserAnalyticsCountAsync_WithExistingAnalytics_ShouldReturnCorrectCount()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var analytics1 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            var analytics2 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            var analytics3 = TestDataFactory.CreateValidUserAnalytics(user.Id);
            await _context.UserAnalytics.AddRangeAsync(analytics1, analytics2, analytics3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserAnalyticsCountAsync(user.Id);

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public async Task GetUserAnalyticsCountAsync_WithNoAnalytics_ShouldReturnZero()
        {
            // Act
            var result = await _repository.GetUserAnalyticsCountAsync(Guid.NewGuid());

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region DeleteUserAnalyticsAsync Tests

        [Fact]
        public async Task DeleteUserAnalyticsAsync_WithExistingAnalytics_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var analytics = TestDataFactory.CreateValidUserAnalytics();
            await _context.UserAnalytics.AddAsync(analytics);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteUserAnalyticsAsync(analytics.Id);

            // Assert
            result.Should().BeTrue();
            var deletedAnalytics = await _context.UserAnalytics.FindAsync(analytics.Id);
            deletedAnalytics.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUserAnalyticsAsync_WithNonExistentAnalytics_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteUserAnalyticsAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
