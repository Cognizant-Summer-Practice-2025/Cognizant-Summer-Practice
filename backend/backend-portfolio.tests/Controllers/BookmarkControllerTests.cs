using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Controllers;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.Repositories;
using backend_portfolio.Models;

namespace backend_portfolio.tests.Controllers
{
    public class BookmarkControllerTests
    {
        private readonly Mock<IBookmarkRepository> _mockBookmarkRepository;
        private readonly Mock<ILogger<BookmarkController>> _mockLogger;
        private readonly BookmarkController _controller;
        private readonly Fixture _fixture;

        public BookmarkControllerTests()
        {
            _mockBookmarkRepository = new Mock<IBookmarkRepository>();
            _mockLogger = new Mock<ILogger<BookmarkController>>();
            _controller = new BookmarkController(
                _mockBookmarkRepository.Object,
                _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetUserBookmarks_WithValidUserId_ShouldReturnOkWithBookmarks()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookmarks = _fixture.CreateMany<Bookmark>(3).ToList();
            _mockBookmarkRepository.Setup(x => x.GetBookmarksByUserIdAsync(userId))
                .ReturnsAsync(bookmarks);

            // Act
            var result = await _controller.GetUserBookmarks(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responses = okResult!.Value as List<BookmarkResponse>;
            responses.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetUserBookmarks_WhenRepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.GetBookmarksByUserIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetUserBookmarks(userId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task ToggleBookmark_WhenBookmarkExists_ShouldRemoveBookmark()
        {
            // Arrange
            var request = _fixture.Create<BookmarkToggleRequest>();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(request.UserId, request.PortfolioId))
                .ReturnsAsync(true);
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkByUserAndPortfolioAsync(request.UserId, request.PortfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ToggleBookmark(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value;
            responseData.Should().NotBeNull();
        }

        [Fact]
        public async Task ToggleBookmark_WhenBookmarkDoesNotExist_ShouldCreateBookmark()
        {
            // Arrange
            var request = _fixture.Create<BookmarkToggleRequest>();
            var createdBookmark = _fixture.Create<Bookmark>();
            
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(request.UserId, request.PortfolioId))
                .ReturnsAsync(false);
            _mockBookmarkRepository.Setup(x => x.CreateBookmarkAsync(It.IsAny<BookmarkCreateRequest>()))
                .ReturnsAsync(createdBookmark);

            // Act
            var result = await _controller.ToggleBookmark(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value;
            responseData.Should().NotBeNull();
        }

        [Fact]
        public async Task ToggleBookmark_WhenDeleteFails_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Create<BookmarkToggleRequest>();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(request.UserId, request.PortfolioId))
                .ReturnsAsync(true);
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkByUserAndPortfolioAsync(request.UserId, request.PortfolioId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ToggleBookmark(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ToggleBookmark_WhenRepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = _fixture.Create<BookmarkToggleRequest>();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(request.UserId, request.PortfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.ToggleBookmark(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task CheckBookmark_WithValidIds_ShouldReturnBookmarkStatus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(userId, portfolioId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CheckBookmark(userId, portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value;
            responseData.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckBookmark_WhenRepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(userId, portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CheckBookmark(userId, portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteBookmark_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var bookmarkId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkAsync(bookmarkId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBookmark(bookmarkId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteBookmark_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var bookmarkId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkAsync(bookmarkId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBookmark(bookmarkId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteBookmark_WhenRepositoryThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var bookmarkId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkAsync(bookmarkId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteBookmark(bookmarkId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetUserBookmarks_WithEmptyGuid_ShouldStillCallRepository()
        {
            // Arrange
            var userId = Guid.Empty;
            var emptyBookmarks = new List<Bookmark>();
            _mockBookmarkRepository.Setup(x => x.GetBookmarksByUserIdAsync(userId))
                .ReturnsAsync(emptyBookmarks);

            // Act
            var result = await _controller.GetUserBookmarks(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockBookmarkRepository.Verify(x => x.GetBookmarksByUserIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ToggleBookmark_ShouldLogErrorWhenExceptionOccurs()
        {
            // Arrange
            var request = _fixture.Create<BookmarkToggleRequest>();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(request.UserId, request.PortfolioId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _controller.ToggleBookmark(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while toggling bookmark")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CheckBookmark_ShouldLogErrorWhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.BookmarkExistsAsync(userId, portfolioId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _controller.CheckBookmark(userId, portfolioId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while checking bookmark")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteBookmark_ShouldLogErrorWhenExceptionOccurs()
        {
            // Arrange
            var bookmarkId = Guid.NewGuid();
            _mockBookmarkRepository.Setup(x => x.DeleteBookmarkAsync(bookmarkId))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _controller.DeleteBookmark(bookmarkId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while deleting bookmark")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
} 