using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend_user.Repositories;
using backend_user.Models;
using backend_user.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class BookmarkRepositoryTests : IDisposable
{
    private readonly UserDbContext _context;
    private readonly BookmarkRepository _repository;

    public BookmarkRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new UserDbContext(options);
        _repository = new BookmarkRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddBookmark_AddsBookmark()
    {
        var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), PortfolioId = "p1", CreatedAt = DateTime.UtcNow };
        var result = await _repository.AddBookmark(bookmark);
        result.Id.Should().Be(bookmark.Id);
        (await _context.Bookmarks.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RemoveBookmark_RemovesBookmarkOrReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1" };
        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync();
        var removed = await _repository.RemoveBookmark(userId, "p1");
        removed.Should().BeTrue();
        (await _context.Bookmarks.FindAsync(bookmark.Id)).Should().BeNull();
        (await _repository.RemoveBookmark(userId, "p1")).Should().BeFalse();
    }

    [Fact]
    public async Task GetUserBookmarks_ReturnsBookmarks()
    {
        var userId = Guid.NewGuid();
        _context.Bookmarks.Add(new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1", CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();
        var list = (await _repository.GetUserBookmarks(userId)).ToList();
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task IsBookmarked_ReturnsTrueOrFalse()
    {
        var userId = Guid.NewGuid();
        _context.Bookmarks.Add(new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1" });
        await _context.SaveChangesAsync();
        (await _repository.IsBookmarked(userId, "p1")).Should().BeTrue();
        (await _repository.IsBookmarked(userId, "p2")).Should().BeFalse();
    }

    [Fact]
    public async Task GetBookmark_ReturnsBookmarkOrNull()
    {
        var userId = Guid.NewGuid();
        var bookmark = new Bookmark { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1" };
        _context.Bookmarks.Add(bookmark);
        await _context.SaveChangesAsync();
        var found = await _repository.GetBookmark(userId, "p1");
        found.Should().NotBeNull();
        (await _repository.GetBookmark(userId, "p2")).Should().BeNull();
    }
}