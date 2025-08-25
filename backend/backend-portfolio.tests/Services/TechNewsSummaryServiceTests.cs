using System;
using System.Threading.Tasks;
using backend_portfolio.DTO.TechNews;
using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_portfolio.tests.Services
{
    public class TechNewsSummaryServiceTests
    {
        private readonly Mock<ITechNewsSummaryRepository> _mockRepository;
        private readonly Mock<ILogger<TechNewsSummaryService>> _mockLogger;
        private readonly TechNewsSummaryService _service;

        public TechNewsSummaryServiceTests()
        {
            _mockRepository = new Mock<ITechNewsSummaryRepository>();
            _mockLogger = new Mock<ILogger<TechNewsSummaryService>>();
            _service = new TechNewsSummaryService(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync((TechNewsSummary?)null);

            // Act
            var result = await _service.GetLatestAsync();

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldReturnMappedDto_WhenRepositoryReturnsSummary()
        {
            // Arrange
            var summary = new TechNewsSummary
            {
                Id = Guid.NewGuid(),
                Summary = "Test summary",
                WorkflowCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync(summary);

            // Act
            var result = await _service.GetLatestAsync();

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(summary.Id);
            result.Summary.Should().Be(summary.Summary);
            result.WorkflowCompleted.Should().Be(summary.WorkflowCompleted);
            result.CreatedAt.Should().Be(summary.CreatedAt);
            result.UpdatedAt.Should().Be(summary.UpdatedAt);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Database error");
            _mockRepository.Setup(r => r.GetLatestAsync()).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetLatestAsync());
            exception.Should().Be(expectedException);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
        }

        [Fact]
        public async Task UpsertAsync_ShouldCreateNewSummary_WhenNoExistingSummary()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "New summary",
                WorkflowCompleted = false
            };

            var newSummary = new TechNewsSummary
            {
                Id = Guid.NewGuid(),
                Summary = request.Summary,
                WorkflowCompleted = request.WorkflowCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync((TechNewsSummary?)null);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<TechNewsSummary>())).ReturnsAsync(newSummary);

            // Act
            var result = await _service.UpsertAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(newSummary.Id);
            result.Summary.Should().Be(request.Summary);
            result.WorkflowCompleted.Should().Be(request.WorkflowCompleted);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldUpdateExistingSummary_WhenSummaryExists()
        {
            // Arrange
            var existingSummary = new TechNewsSummary
            {
                Id = Guid.NewGuid(),
                Summary = "Old summary",
                WorkflowCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Updated summary",
                WorkflowCompleted = true
            };

            var updatedSummary = new TechNewsSummary
            {
                Id = existingSummary.Id,
                Summary = request.Summary,
                WorkflowCompleted = request.WorkflowCompleted,
                CreatedAt = existingSummary.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync(existingSummary);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TechNewsSummary>())).ReturnsAsync(updatedSummary);

            // Act
            var result = await _service.UpsertAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingSummary.Id);
            result.Summary.Should().Be(request.Summary);
            result.WorkflowCompleted.Should().Be(request.WorkflowCompleted);
            result.CreatedAt.Should().Be(existingSummary.CreatedAt); // Should not change

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowArgumentException_WhenSummaryIsEmpty()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "",
                WorkflowCompleted = false
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpsertAsync(request));
            exception.ParamName.Should().Be("request");

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Never);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowArgumentException_WhenSummaryIsWhitespace()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "   ",
                WorkflowCompleted = false
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpsertAsync(request));
            exception.ParamName.Should().Be("request");

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Never);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowArgumentException_WhenSummaryIsNull()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = null!,
                WorkflowCompleted = false
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpsertAsync(request));
            exception.ParamName.Should().Be("request");

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Never);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowException_WhenRepositoryThrowsOnGetLatest()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Test summary",
                WorkflowCompleted = false
            };

            var expectedException = new InvalidOperationException("Database error");
            _mockRepository.Setup(r => r.GetLatestAsync()).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpsertAsync(request));
            exception.Should().Be(expectedException);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowException_WhenRepositoryThrowsOnCreate()
        {
            // Arrange
            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Test summary",
                WorkflowCompleted = false
            };

            var expectedException = new InvalidOperationException("Database error");
            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync((TechNewsSummary?)null);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<TechNewsSummary>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpsertAsync(request));
            exception.Should().Be(expectedException);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }

        [Fact]
        public async Task UpsertAsync_ShouldThrowException_WhenRepositoryThrowsOnUpdate()
        {
            // Arrange
            var existingSummary = new TechNewsSummary
            {
                Id = Guid.NewGuid(),
                Summary = "Old summary",
                WorkflowCompleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new TechNewsSummaryRequestDto
            {
                Summary = "Updated summary",
                WorkflowCompleted = true
            };

            var expectedException = new InvalidOperationException("Database error");
            _mockRepository.Setup(r => r.GetLatestAsync()).ReturnsAsync(existingSummary);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TechNewsSummary>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpsertAsync(request));
            exception.Should().Be(expectedException);

            _mockRepository.Verify(r => r.GetLatestAsync(), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TechNewsSummary>()), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<TechNewsSummary>()), Times.Never);
        }
    }
}
