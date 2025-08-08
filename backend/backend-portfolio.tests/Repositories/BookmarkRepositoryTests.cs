using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Repositories
{
    public class BookmarkRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly BookmarkRepository _repository;
        private readonly Fixture _fixture;

        public BookmarkRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new BookmarkRepository(_context);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private async Task<Portfolio> CreatePortfolioAsync()
        {
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var portfolio = TestDataFactory.CreatePortfolio();
            portfolio.TemplateId = template.Id;
            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();
            return portfolio;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetAllBookmarksAsync Tests

        [Fact]
        public async Task GetAllBookmarksAsync_WithMultipleBookmarks_ShouldReturnAllBookmarks()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var bookmarks = new List<Bookmark>
            {
                TestDataFactory.CreateBookmark(portfolioId: portfolio.Id),
                TestDataFactory.CreateBookmark(portfolioId: portfolio.Id),
                TestDataFactory.CreateBookmark(portfolioId: portfolio.Id)
            };
            await _context.Bookmarks.AddRangeAsync(bookmarks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllBookmarksAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(b => b.Portfolio != null);
        }

        [Fact]
        public async Task GetAllBookmarksAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllBookmarksAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetBookmarkByIdAsync Tests

        [Fact]
        public async Task GetBookmarkByIdAsync_WithExistingId_ShouldReturnBookmark()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var bookmark = TestDataFactory.CreateBookmark(portfolioId: portfolio.Id);
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookmarkByIdAsync(bookmark.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(bookmark.Id);
            result.Portfolio.Should().NotBeNull();
        }

        [Fact]
        public async Task GetBookmarkByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBookmarkByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBookmarkByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetBookmarkByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBookmarksByUserIdAsync Tests

        [Fact]
        public async Task GetBookmarksByUserIdAsync_WithExistingUser_ShouldReturnUserBookmarks()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var userBookmarks = new List<Bookmark>
            {
                _fixture.Build<Bookmark>()
                    .With(b => b.UserId, userId)
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<Bookmark>()
                    .With(b => b.UserId, userId)
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };

            var otherUserBookmark = _fixture.Build<Bookmark>()
                .With(b => b.UserId, otherUserId)
                .With(b => b.PortfolioId, portfolio.Id)
                .Without(b => b.Portfolio)
                .Create();

            await _context.Bookmarks.AddRangeAsync(userBookmarks);
            await _context.Bookmarks.AddAsync(otherUserBookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookmarksByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.UserId == userId);
            result.Should().BeInDescendingOrder(b => b.CreatedAt);
            result.Should().OnlyContain(b => b.Portfolio != null);
        }

        [Fact]
        public async Task GetBookmarksByUserIdAsync_WithNonExistingUser_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingUserId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBookmarksByUserIdAsync(nonExistingUserId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetBookmarksByPortfolioIdAsync Tests

        [Fact]
        public async Task GetBookmarksByPortfolioIdAsync_WithExistingPortfolio_ShouldReturnPortfolioBookmarks()
        {
            // Arrange
            var portfolio1 = await CreatePortfolioAsync();
            var portfolio2 = await CreatePortfolioAsync();

            var portfolio1Bookmarks = new List<Bookmark>
            {
                _fixture.Build<Bookmark>()
                    .With(b => b.PortfolioId, portfolio1.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<Bookmark>()
                    .With(b => b.PortfolioId, portfolio1.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };

            var portfolio2Bookmark = _fixture.Build<Bookmark>()
                .With(b => b.PortfolioId, portfolio2.Id)
                .Without(b => b.Portfolio)
                .Create();

            await _context.Bookmarks.AddRangeAsync(portfolio1Bookmarks);
            await _context.Bookmarks.AddAsync(portfolio2Bookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookmarksByPortfolioIdAsync(portfolio1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.PortfolioId == portfolio1.Id);
            result.Should().BeInDescendingOrder(b => b.CreatedAt);
            result.Should().OnlyContain(b => b.Portfolio != null);
        }

        [Fact]
        public async Task GetBookmarksByPortfolioIdAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBookmarksByPortfolioIdAsync(nonExistingPortfolioId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region CreateBookmarkAsync Tests

        [Fact]
        public async Task CreateBookmarkAsync_WithValidRequest_ShouldCreateBookmark()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<BookmarkCreateRequest>()
                .With(r => r.UserId, Guid.NewGuid())
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.CollectionName, "Favorites")
                .With(r => r.Notes, "Great portfolio!")
                .Create();

            // Act
            var result = await _repository.CreateBookmarkAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.UserId.Should().Be(request.UserId);
            result.PortfolioId.Should().Be(request.PortfolioId);
            result.CollectionName.Should().Be("Favorites");
            result.Notes.Should().Be("Great portfolio!");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        #endregion

        #region UpdateBookmarkAsync Tests

        [Fact]
        public async Task UpdateBookmarkAsync_WithValidRequest_ShouldUpdateBookmark()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var bookmark = TestDataFactory.CreateBookmark(portfolioId: portfolio.Id);
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<BookmarkUpdateRequest>()
                .With(r => r.CollectionName, "Updated Collection")
                .With(r => r.Notes, "Updated notes")
                .Create();

            // Act
            var result = await _repository.UpdateBookmarkAsync(bookmark.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.CollectionName.Should().Be("Updated Collection");
            result.Notes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task UpdateBookmarkAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var request = _fixture.Create<BookmarkUpdateRequest>();

            // Act
            var result = await _repository.UpdateBookmarkAsync(nonExistingId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateBookmarkAsync_WithNullFields_ShouldNotUpdateNullFields()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var bookmark = TestDataFactory.CreateBookmark(portfolioId: portfolio.Id);
            var originalCollectionName = bookmark.CollectionName;
            var originalNotes = bookmark.Notes;
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            var request = new BookmarkUpdateRequest
            {
                CollectionName = null,
                Notes = null
            };

            // Act
            var result = await _repository.UpdateBookmarkAsync(bookmark.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.CollectionName.Should().Be(originalCollectionName);
            result.Notes.Should().Be(originalNotes);
        }

        #endregion

        #region DeleteBookmarkAsync Tests

        [Fact]
        public async Task DeleteBookmarkAsync_WithExistingId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var bookmark = TestDataFactory.CreateBookmark(portfolioId: portfolio.Id);
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteBookmarkAsync(bookmark.Id);

            // Assert
            result.Should().BeTrue();
            var deletedBookmark = await _context.Bookmarks.FindAsync(bookmark.Id);
            deletedBookmark.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBookmarkAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteBookmarkAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteBookmarkByUserAndPortfolioAsync Tests

        [Fact]
        public async Task DeleteBookmarkByUserAndPortfolioAsync_WithExistingBookmark_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var userId = Guid.NewGuid();
            var bookmark = TestDataFactory.CreateBookmark(userId, portfolio.Id);
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteBookmarkByUserAndPortfolioAsync(userId, portfolio.Id);

            // Assert
            result.Should().BeTrue();
            var deletedBookmark = await _context.Bookmarks.FindAsync(bookmark.Id);
            deletedBookmark.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBookmarkByUserAndPortfolioAsync_WithNonExistingBookmark_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteBookmarkByUserAndPortfolioAsync(userId, portfolioId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region BookmarkExistsAsync Tests

        [Fact]
        public async Task BookmarkExistsAsync_WithExistingBookmark_ShouldReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var userId = Guid.NewGuid();
            var bookmark = TestDataFactory.CreateBookmark(userId, portfolio.Id);
            await _context.Bookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.BookmarkExistsAsync(userId, portfolio.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task BookmarkExistsAsync_WithNonExistingBookmark_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.BookmarkExistsAsync(userId, portfolioId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetBookmarksByCollectionAsync Tests

        [Fact]
        public async Task GetBookmarksByCollectionAsync_WithExistingCollection_ShouldReturnCollectionBookmarks()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var userId = Guid.NewGuid();
            var collectionName = "Favorites";

            var collectionBookmarks = new List<Bookmark>
            {
                _fixture.Build<Bookmark>()
                    .With(b => b.UserId, userId)
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CollectionName, collectionName)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<Bookmark>()
                    .With(b => b.UserId, userId)
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CollectionName, collectionName)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };

            var differentCollectionBookmark = _fixture.Build<Bookmark>()
                .With(b => b.UserId, userId)
                .With(b => b.PortfolioId, portfolio.Id)
                .With(b => b.CollectionName, "Different Collection")
                .Without(b => b.Portfolio)
                .Create();

            await _context.Bookmarks.AddRangeAsync(collectionBookmarks);
            await _context.Bookmarks.AddAsync(differentCollectionBookmark);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBookmarksByCollectionAsync(userId, collectionName);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.UserId == userId && b.CollectionName == collectionName);
            result.Should().BeInDescendingOrder(b => b.CreatedAt);
            result.Should().OnlyContain(b => b.Portfolio != null);
        }

        [Fact]
        public async Task GetBookmarksByCollectionAsync_WithNonExistingCollection_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nonExistingCollection = "Non-existing Collection";

            // Act
            var result = await _repository.GetBookmarksByCollectionAsync(userId, nonExistingCollection);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
