using System;
using System.Linq;
using System.Threading.Tasks;
using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_portfolio.tests.Repositories
{
    public class TechNewsSummaryRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly TechNewsSummaryRepository _repository;
        private readonly Mock<ILogger<TechNewsSummaryRepository>> _mockLogger;

        public TechNewsSummaryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _mockLogger = new Mock<ILogger<TechNewsSummaryRepository>>();
            _repository = new TechNewsSummaryRepository(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnNull_WhenNoSummariesExist()
        {
            // Act
            var result = await _repository.GetLatestAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnLatestSummary_WhenMultipleSummariesExist()
        {
            // Arrange
            var summary1 = new TechNewsSummary
            {
                Summary = "Old summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var summary2 = new TechNewsSummary
            {
                Summary = "New summary",
                WorkflowCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var summary3 = new TechNewsSummary
            {
                Summary = "Latest summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TechNewsSummaries.AddRange(summary1, summary2, summary3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(summary3.Id, result.Id);
            Assert.Equal("Latest summary", result.Summary);
            Assert.Equal(summary3.CreatedAt, result.CreatedAt);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnLatestByCreatedAt_WhenTieBreakerNeeded()
        {
            // Arrange
            var sameTime = DateTime.UtcNow;
            var summary1 = new TechNewsSummary
            {
                Summary = "First summary",
                WorkflowCompleted = true,
                CreatedAt = sameTime,
                UpdatedAt = sameTime.AddHours(1)
            };

            var summary2 = new TechNewsSummary
            {
                Summary = "Second summary",
                WorkflowCompleted = false,
                CreatedAt = sameTime,
                UpdatedAt = sameTime.AddHours(2)
            };

            _context.TechNewsSummaries.AddRange(summary1, summary2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(summary2.Id, result.Id); // Should get the one with later updated_at
        }

        [Fact]
        public async Task GetLatestWithMetadataAsync_ShouldReturnLatestSummary_WithLogging()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Summary = "Test summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TechNewsSummaries.Add(summary);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestWithMetadataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(summary.Id, result.Id);
            Assert.Equal("Test summary", result.Summary);
        }

        [Fact]
        public async Task GetLatestWithMetadataAsync_ShouldReturnNull_WhenNoSummariesExist()
        {
            // Act
            var result = await _repository.GetLatestWithMetadataAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateNewSummary_WithCorrectTimestamps()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Summary = "New summary",
                WorkflowCompleted = false
            };

            var beforeCreation = DateTime.UtcNow;

            // Act
            var result = await _repository.CreateAsync(summary);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("New summary", result.Summary);
            Assert.False(result.WorkflowCompleted);
            Assert.True(result.CreatedAt >= beforeCreation);
            Assert.True(result.UpdatedAt >= beforeCreation);
            Assert.True((result.UpdatedAt - result.CreatedAt).TotalMilliseconds < 10); // Allow small time difference

            // Verify it was saved to database
            var savedSummary = await _context.TechNewsSummaries.FindAsync(result.Id);
            Assert.NotNull(savedSummary);
            Assert.Equal("New summary", savedSummary.Summary);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateSummary_WithNewUpdatedAt()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Summary = "Original summary",
                WorkflowCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.TechNewsSummaries.Add(summary);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = summary.UpdatedAt;
            summary.Summary = "Updated summary";
            summary.WorkflowCompleted = true;

            var beforeUpdate = DateTime.UtcNow;

            // Act
            var result = await _repository.UpdateAsync(summary);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated summary", result.Summary);
            Assert.True(result.WorkflowCompleted);
            Assert.True(result.UpdatedAt > originalUpdatedAt);
            Assert.True(result.UpdatedAt >= beforeUpdate);
            Assert.Equal(summary.CreatedAt, result.CreatedAt); // CreatedAt should not change
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnSummary_WhenIdExists()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Summary = "Test summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TechNewsSummaries.Add(summary);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(summary.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(summary.Id, result.Id);
            Assert.Equal("Test summary", result.Summary);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            // Arrange - Create a summary with invalid data that would cause a database error
            var summary = new TechNewsSummary
            {
                Summary = "Test summary",
                WorkflowCompleted = false
            };

            // Dispose context to simulate database error
            _context.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.CreateAsync(summary));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Id = Guid.NewGuid(),
                Summary = "Test summary",
                WorkflowCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Dispose context to simulate database error
            _context.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.UpdateAsync(summary));
        }

        [Fact]
        public async Task GetLatestAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            // Arrange - Dispose context to simulate database error
            _context.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.GetLatestAsync());
        }

        [Fact]
        public async Task GetLatestWithMetadataAsync_ShouldThrowException_WhenDatabaseErrorOccurs()
        {
            // Arrange - Dispose context to simulate database error
            _context.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.GetLatestWithMetadataAsync());
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
