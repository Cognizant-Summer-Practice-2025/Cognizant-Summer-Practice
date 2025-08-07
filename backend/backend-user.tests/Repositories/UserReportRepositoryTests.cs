using Xunit;
using FluentAssertions;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using Microsoft.EntityFrameworkCore;
using backend_user.tests.Helpers;

namespace backend_user.tests.Repositories
{
    public class UserReportRepositoryTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly IUserReportRepository _repository;

        public UserReportRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(options);
            _repository = new UserReportRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region CreateUserReportAsync Tests

        [Fact]
        public async Task CreateUserReportAsync_WithValidData_ShouldCreateAndReturnReport()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report = new UserReport
            {
                UserId = user.Id,
                ReportedByUserId = Guid.NewGuid(),
                Reason = "Inappropriate content",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.CreateUserReportAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.UserId.Should().Be(user.Id);
            result.ReportedByUserId.Should().Be(report.ReportedByUserId);
            result.Reason.Should().Be("Inappropriate content");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateUserReportAsync_WithEmptyReason_ShouldCreateAndReturnReport()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report = new UserReport
            {
                UserId = user.Id,
                ReportedByUserId = Guid.NewGuid(),
                Reason = "",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.CreateUserReportAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Reason.Should().Be("");
        }

        #endregion

        #region GetUserReportByIdAsync Tests

        [Fact]
        public async Task GetUserReportByIdAsync_WithExistingId_ShouldReturnReport()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateValidUserReport(user.Id);
            await _context.UserReports.AddAsync(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserReportByIdAsync(report.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(report.Id);
            result.UserId.Should().Be(user.Id);
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetUserReportByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetUserReportByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserReportsAsync Tests

        [Fact]
        public async Task GetUserReportsAsync_WithExistingUser_ShouldReturnReports()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user.Id);
            var report2 = TestDataFactory.CreateValidUserReport(user.Id);
            await _context.UserReports.AddRangeAsync(report1, report2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserReportsAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(r => r.UserId == user.Id);
        }

        [Fact]
        public async Task GetUserReportsAsync_WithNonExistentUser_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetUserReportsAsync(Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserReportsAsync_WithMultipleUsers_ShouldReturnOnlyTargetUserReports()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user1.Id);
            var report2 = TestDataFactory.CreateValidUserReport(user2.Id);
            var report3 = TestDataFactory.CreateValidUserReport(user1.Id);
            await _context.UserReports.AddRangeAsync(report1, report2, report3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserReportsAsync(user1.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(r => r.UserId == user1.Id);
        }

        #endregion

        #region GetReportsByReporterAsync Tests

        [Fact]
        public async Task GetReportsByReporterAsync_WithExistingReporter_ShouldReturnReports()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var reporter = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user, reporter);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user.Id, reporter.Id);
            var report2 = TestDataFactory.CreateValidUserReport(user.Id, reporter.Id);
            var report3 = TestDataFactory.CreateValidUserReport(user.Id, Guid.NewGuid()); // Different reporter
            await _context.UserReports.AddRangeAsync(report1, report2, report3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetReportsByReporterAsync(reporter.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result.Should().OnlyContain(r => r.ReportedByUserId == reporter.Id);
        }

        [Fact]
        public async Task GetReportsByReporterAsync_WithNonExistentReporter_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetReportsByReporterAsync(Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region HasUserReportedUserAsync Tests

        [Fact]
        public async Task HasUserReportedUserAsync_WithExistingReport_ShouldReturnTrue()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var reporter = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user, reporter);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateValidUserReport(user.Id, reporter.Id);
            await _context.UserReports.AddAsync(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserReportedUserAsync(user.Id, reporter.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasUserReportedUserAsync_WithNonExistentReport_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.HasUserReportedUserAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasUserReportedUserAsync_WithDifferentReporter_ShouldReturnFalse()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var reporter1 = TestDataFactory.CreateValidUser();
            var reporter2 = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user, reporter1, reporter2);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateValidUserReport(user.Id, reporter1.Id);
            await _context.UserReports.AddAsync(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserReportedUserAsync(user.Id, reporter2.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasUserReportedUserAsync_WithDifferentUser_ShouldReturnFalse()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();
            var reporter = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user1, user2, reporter);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateValidUserReport(user1.Id, reporter.Id);
            await _context.UserReports.AddAsync(report);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.HasUserReportedUserAsync(user2.Id, reporter.Id);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetAllUserReportsAsync Tests

        [Fact]
        public async Task GetAllUserReportsAsync_WithExistingReports_ShouldReturnAllReports()
        {
            // Arrange
            var user1 = TestDataFactory.CreateValidUser();
            var user2 = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user1.Id);
            var report2 = TestDataFactory.CreateValidUserReport(user2.Id);
            await _context.UserReports.AddRangeAsync(report1, report2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUserReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
        }

        [Fact]
        public async Task GetAllUserReportsAsync_WithNoReports_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllUserReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUserReportsAsync_WithMultipleReports_ShouldReturnOrderedList()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user.Id);
            report1.CreatedAt = DateTime.UtcNow.AddDays(-2);
            var report2 = TestDataFactory.CreateValidUserReport(user.Id);
            report2.CreatedAt = DateTime.UtcNow.AddDays(-1);
            var report3 = TestDataFactory.CreateValidUserReport(user.Id);
            report3.CreatedAt = DateTime.UtcNow;

            await _context.UserReports.AddRangeAsync(report1, report2, report3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllUserReportsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeInDescendingOrder(x => x.CreatedAt);
            result[0].CreatedAt.Should().Be(report3.CreatedAt);
            result[1].CreatedAt.Should().Be(report2.CreatedAt);
            result[2].CreatedAt.Should().Be(report1.CreatedAt);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateAndRetrieve_ShouldWorkCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var report = TestDataFactory.CreateValidUserReport(user.Id);

            // Act
            var createdReport = await _repository.CreateUserReportAsync(report);
            var retrievedReport = await _repository.GetUserReportByIdAsync(createdReport.Id);

            // Assert
            retrievedReport.Should().NotBeNull();
            retrievedReport!.Id.Should().Be(createdReport.Id);
            retrievedReport.UserId.Should().Be(createdReport.UserId);
            retrievedReport.ReportedByUserId.Should().Be(createdReport.ReportedByUserId);
            retrievedReport.Reason.Should().Be(createdReport.Reason);
        }

        [Fact]
        public async Task MultipleOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var reporter = TestDataFactory.CreateValidUser();
            await _context.Users.AddRangeAsync(user, reporter);
            await _context.SaveChangesAsync();

            var report1 = TestDataFactory.CreateValidUserReport(user.Id, reporter.Id);
            var report2 = TestDataFactory.CreateValidUserReport(user.Id, reporter.Id);

            // Act
            await _repository.CreateUserReportAsync(report1);
            await _repository.CreateUserReportAsync(report2);

            var hasReported = await _repository.HasUserReportedUserAsync(user.Id, reporter.Id);
            var userReports = await _repository.GetUserReportsAsync(user.Id);
            var reporterReports = await _repository.GetReportsByReporterAsync(reporter.Id);
            var allReports = await _repository.GetAllUserReportsAsync();

            // Assert
            hasReported.Should().BeTrue();
            userReports.Should().HaveCount(2);
            reporterReports.Should().HaveCount(2);
            allReports.Should().HaveCount(2);
        }

        #endregion
    }
}
