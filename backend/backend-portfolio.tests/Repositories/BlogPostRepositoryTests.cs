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
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Repositories
{
    public class BlogPostRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly BlogPostRepository _repository;
        private readonly Fixture _fixture;

        public BlogPostRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new BlogPostRepository(_context);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Configure AutoFixture to handle DateOnly properly - generate valid dates
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-new Random().Next(1, 3650))));
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

        #region GetAllBlogPostsAsync Tests

        [Fact]
        public async Task GetAllBlogPostsAsync_WithMultipleBlogPosts_ShouldReturnAllBlogPosts()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPosts = new List<BlogPost>
            {
                TestDataFactory.CreateBlogPost(portfolio.Id),
                TestDataFactory.CreateBlogPost(portfolio.Id),
                TestDataFactory.CreateBlogPost(portfolio.Id)
            };
            await _context.BlogPosts.AddRangeAsync(blogPosts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllBlogPostsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.True(result.All(b => b.Portfolio != null));
        }

        [Fact]
        public async Task GetAllBlogPostsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllBlogPostsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBlogPostByIdAsync Tests

        [Fact]
        public async Task GetBlogPostByIdAsync_WithExistingId_ShouldReturnBlogPost()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBlogPostByIdAsync(blogPost.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(blogPost.Id);
            result.Portfolio.Should().NotBeNull();
        }

        [Fact]
        public async Task GetBlogPostByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBlogPostByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBlogPostByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetBlogPostByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBlogPostsByPortfolioIdAsync Tests

        [Fact]
        public async Task GetBlogPostsByPortfolioIdAsync_WithExistingPortfolio_ShouldReturnBlogPostsOrderedByCreatedAt()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPosts = new List<BlogPost>
            {
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-3))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };
            await _context.BlogPosts.AddRangeAsync(blogPosts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBlogPostsByPortfolioIdAsync(portfolio.Id);

            // Assert
            Assert.Equal(3, result.Count);
            result.Should().BeInDescendingOrder(b => b.CreatedAt);
        }

        [Fact]
        public async Task GetBlogPostsByPortfolioIdAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetBlogPostsByPortfolioIdAsync(nonExistingPortfolioId);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateBlogPostAsync Tests

        [Fact]
        public async Task CreateBlogPostAsync_WithValidRequest_ShouldCreateBlogPost()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<BlogPostCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.Title, "Test Blog Post")
                .With(r => r.IsPublished, true)
                .Create();

            // Act
            var result = await _repository.CreateBlogPostAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.PortfolioId.Should().Be(portfolio.Id);
            result.Title.Should().Be("Test Blog Post");
            result.IsPublished.Should().BeTrue();
            result.PublishedAt.Should().NotBeNull();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateBlogPostAsync_WithUnpublishedPost_ShouldCreateWithNullPublishedAt()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<BlogPostCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.IsPublished, false)
                .Create();

            // Act
            var result = await _repository.CreateBlogPostAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsPublished.Should().BeFalse();
            result.PublishedAt.Should().BeNull();
        }

        #endregion

        #region UpdateBlogPostAsync Tests

        [Fact]
        public async Task UpdateBlogPostAsync_WithValidRequest_ShouldUpdateBlogPost()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);
            blogPost.IsPublished = false;
            blogPost.PublishedAt = null;
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<BlogPostUpdateRequest>()
                .With(r => r.Title, "Updated Title")
                .With(r => r.IsPublished, true)
                .Create();

            // Act
            var result = await _repository.UpdateBlogPostAsync(blogPost.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.IsPublished.Should().BeTrue();
            result.PublishedAt.Should().NotBeNull();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateBlogPostAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var request = _fixture.Create<BlogPostUpdateRequest>();

            // Act
            var result = await _repository.UpdateBlogPostAsync(nonExistingId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateBlogPostAsync_WithPublishingAlreadyPublishedPost_ShouldNotChangePublishedAt()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var originalPublishedAt = DateTime.UtcNow.AddDays(-1);
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);
            blogPost.IsPublished = true;
            blogPost.PublishedAt = originalPublishedAt;
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<BlogPostUpdateRequest>()
                .With(r => r.IsPublished, true)
                .Create();

            // Act
            var result = await _repository.UpdateBlogPostAsync(blogPost.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.PublishedAt.Should().Be(originalPublishedAt);
        }

        #endregion

        #region DeleteBlogPostAsync Tests

        [Fact]
        public async Task DeleteBlogPostAsync_WithExistingId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteBlogPostAsync(blogPost.Id);

            // Assert
            result.Should().BeTrue();
            var deletedBlogPost = await _context.BlogPosts.FindAsync(blogPost.Id);
            deletedBlogPost.Should().BeNull();
        }

        [Fact]
        public async Task DeleteBlogPostAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteBlogPostAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetPublishedBlogPostsAsync Tests

        [Fact]
        public async Task GetPublishedBlogPostsAsync_WithPublishedAndUnpublishedPosts_ShouldReturnOnlyPublished()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var publishedPosts = new List<BlogPost>
            {
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.IsPublished, true)
                    .With(b => b.PublishedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.IsPublished, true)
                    .With(b => b.PublishedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };

            var unpublishedPost = _fixture.Build<BlogPost>()
                .With(b => b.PortfolioId, portfolio.Id)
                .With(b => b.IsPublished, false)
                .With(b => b.PublishedAt, (DateTime?)null)
                .Without(b => b.Portfolio)
                .Create();

            await _context.BlogPosts.AddRangeAsync(publishedPosts);
            await _context.BlogPosts.AddAsync(unpublishedPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPublishedBlogPostsAsync(portfolio.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.All(b => b.IsPublished));
            result.Should().BeInDescendingOrder(b => b.PublishedAt);
        }

        [Fact]
        public async Task GetPublishedBlogPostsAsync_WithNoPublishedPosts_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();

            // Act
            var result = await _repository.GetPublishedBlogPostsAsync(portfolio.Id);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBlogPostsByTagAsync Tests

        [Fact]
        public async Task GetBlogPostsByTagAsync_WithPostsContainingTag_ShouldReturnFilteredPosts()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var targetTag = "test-tag";
            var blogPostsWithTag = new List<BlogPost>
            {
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.Tags, new string[] { targetTag, "other-tag" })
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-1))
                    .Without(b => b.Portfolio)
                    .Create(),
                _fixture.Build<BlogPost>()
                    .With(b => b.PortfolioId, portfolio.Id)
                    .With(b => b.Tags, new string[] { "another-tag", targetTag })
                    .With(b => b.CreatedAt, DateTime.UtcNow.AddDays(-2))
                    .Without(b => b.Portfolio)
                    .Create()
            };

            var blogPostWithoutTag = _fixture.Build<BlogPost>()
                .With(b => b.PortfolioId, portfolio.Id)
                .With(b => b.Tags, new string[] { "different-tag" })
                .Without(b => b.Portfolio)
                .Create();

            await _context.BlogPosts.AddRangeAsync(blogPostsWithTag);
            await _context.BlogPosts.AddAsync(blogPostWithoutTag);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBlogPostsByTagAsync(targetTag);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.All(b => b.Tags != null && b.Tags.Contains(targetTag)));
            result.Should().BeInDescendingOrder(b => b.CreatedAt);
        }

        [Fact]
        public async Task GetBlogPostsByTagAsync_WithNonExistingTag_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPost = TestDataFactory.CreateBlogPost(portfolio.Id);
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBlogPostsByTagAsync("non-existing-tag");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBlogPostsByTagAsync_WithNullTags_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var blogPost = _fixture.Build<BlogPost>()
                .With(b => b.PortfolioId, portfolio.Id)
                .With(b => b.Tags, (string[]?)null)
                .Without(b => b.Portfolio)
                .Create();
            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetBlogPostsByTagAsync("any-tag");

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
