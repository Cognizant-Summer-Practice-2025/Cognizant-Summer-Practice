using Xunit;
using Moq;
using backend_user.Services;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BookmarkServiceTests
{
    private readonly Mock<IBookmarkRepository> _mockBookmarkRepo = new();
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly BookmarkService _service;

    public BookmarkServiceTests()
    {
        _service = new BookmarkService(_mockBookmarkRepo.Object, _mockUserRepo.Object);
    }

    [Fact]
    public async Task AddBookmarkAsync_UserNotFound_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var request = new AddBookmarkRequest { PortfolioId = "p1" };
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync((User)null);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddBookmarkAsync(userId, request));
    }

    [Fact]
    public async Task AddBookmarkAsync_AlreadyBookmarked_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var request = new AddBookmarkRequest { PortfolioId = "p1" };
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync(new User { Id = userId });
        _mockBookmarkRepo.Setup(x => x.IsBookmarked(userId, request.PortfolioId)).ReturnsAsync(true);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddBookmarkAsync(userId, request));
    }

    [Fact]
    public async Task AddBookmarkAsync_Valid_AddsAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var request = new AddBookmarkRequest { PortfolioId = "p1", PortfolioTitle = "t", PortfolioOwnerName = "o" };
        var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1", PortfolioTitle = "t", PortfolioOwnerName = "o", CreatedAt = DateTime.UtcNow };
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync(new User { Id = userId });
        _mockBookmarkRepo.Setup(x => x.IsBookmarked(userId, request.PortfolioId)).ReturnsAsync(false);
        _mockBookmarkRepo.Setup(x => x.AddBookmark(It.IsAny<Bookmark>())).ReturnsAsync(bookmark);
        var result = await _service.AddBookmarkAsync(userId, request);
        Assert.Equal(bookmark.Id, result.Id);
        Assert.Equal(bookmark.UserId, result.UserId);
        Assert.Equal(bookmark.PortfolioId, result.PortfolioId);
        Assert.Equal(bookmark.PortfolioTitle, result.PortfolioTitle);
        Assert.Equal(bookmark.PortfolioOwnerName, result.PortfolioOwnerName);
        Assert.Equal(bookmark.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task RemoveBookmarkAsync_UserNotFound_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync((User)null);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveBookmarkAsync(userId, "p1"));
    }

    [Fact]
    public async Task RemoveBookmarkAsync_NotFound_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync(new User { Id = userId });
        _mockBookmarkRepo.Setup(x => x.RemoveBookmark(userId, "p1")).ReturnsAsync(false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveBookmarkAsync(userId, "p1"));
    }

    [Fact]
    public async Task RemoveBookmarkAsync_Valid_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync(new User { Id = userId });
        _mockBookmarkRepo.Setup(x => x.RemoveBookmark(userId, "p1")).ReturnsAsync(true);
        var result = await _service.RemoveBookmarkAsync(userId, "p1");
        Assert.True(result);
    }

    [Fact]
    public async Task GetUserBookmarksAsync_UserNotFound_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync((User)null);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetUserBookmarksAsync(userId));
    }

    [Fact]
    public async Task GetUserBookmarksAsync_Valid_ReturnsResponses()
    {
        var userId = Guid.NewGuid();
        _mockUserRepo.Setup(x => x.GetUserById(userId)).ReturnsAsync(new User { Id = userId });
        var bookmarks = new List<Bookmark> { new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1", CreatedAt = DateTime.UtcNow } };
        _mockBookmarkRepo.Setup(x => x.GetUserBookmarks(userId)).ReturnsAsync(bookmarks);
        var result = await _service.GetUserBookmarksAsync(userId);
        var responses = new List<BookmarkResponse>(result);
        Assert.Single(responses);
        Assert.Equal(bookmarks[0].Id, responses[0].Id);
    }

    [Fact]
    public async Task GetBookmarkStatusAsync_Valid_ReturnsStatus()
    {
        var userId = Guid.NewGuid();
        _mockBookmarkRepo.Setup(x => x.IsBookmarked(userId, "p1")).ReturnsAsync(true);
        var result = await _service.GetBookmarkStatusAsync(userId, "p1");
        Assert.True(result.IsBookmarked);
    }
}