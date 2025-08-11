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
using backend_portfolio.DTO.Portfolio.Request;

namespace backend_portfolio.tests.Repositories
{
    public class PortfolioRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly PortfolioRepository _repository;
        private readonly Fixture _fixture;

        public PortfolioRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new PortfolioRepository(_context);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Configure AutoFixture to handle DateOnly properly - generate valid dates
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-new Random().Next(1, 3650))));
        }

        private async Task<PortfolioTemplate> CreateTemplateAsync()
        {
            var template = _fixture.Build<PortfolioTemplate>()
                .With(t => t.Name, "Test Template")
                .Without(t => t.Portfolios)
                .Create();

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreatePortfolioAsync_WithValidRequest_ShouldCreateSuccessfully()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var request = _fixture.Build<PortfolioCreateRequest>()
                .With(r => r.TemplateName, template.Name)
                .With(r => r.Title, "Test Portfolio")
                .Create();

            // Act
            var result = await _repository.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Portfolio");
            result.TemplateId.Should().Be(template.Id);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WithExistingId_ShouldReturnPortfolio()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var portfolio = _fixture.Build<Portfolio>()
                .With(p => p.TemplateId, template.Id)
                .With(p => p.Title, "Test Portfolio")
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Create();

            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPortfolioByIdAsync(portfolio.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(portfolio.Id);
            result.Title.Should().Be(portfolio.Title);
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetPortfolioByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPortfolioByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetPortfolioByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllPortfoliosAsync_WithMultiplePortfolios_ShouldReturnAllPortfolios()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var portfolios = _fixture.Build<Portfolio>()
                .With(p => p.TemplateId, template.Id)
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .CreateMany(3)
                .ToList();

            await _context.Portfolios.AddRangeAsync(portfolios);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllPortfoliosAsync();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllPortfoliosAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllPortfoliosAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdatePortfolioAsync_WithValidRequest_ShouldUpdateSuccessfully()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var portfolio = _fixture.Build<Portfolio>()
                .With(p => p.TemplateId, template.Id)
                .With(p => p.Title, "Original Title")
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Create();

            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();

            var updateRequest = new PortfolioUpdateRequest
            {
                Title = "Updated Title",
                Bio = "Updated Bio"
            };

            // Act
            var result = await _repository.UpdatePortfolioAsync(portfolio.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.Bio.Should().Be("Updated Bio");
        }

        [Fact]
        public async Task DeletePortfolioAsync_WithExistingId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var portfolio = _fixture.Build<Portfolio>()
                .With(p => p.TemplateId, template.Id)
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Create();

            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeletePortfolioAsync(portfolio.Id);

            // Assert
            result.Should().BeTrue();

            var deletedPortfolio = await _context.Portfolios.FindAsync(portfolio.Id);
            deletedPortfolio.Should().BeNull();
        }

        [Fact]
        public async Task DeletePortfolioAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeletePortfolioAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeletePortfolioAsync_WithEmptyGuid_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeletePortfolioAsync(Guid.Empty);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_WithExistingUserId_ShouldReturnUserPortfolios()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var userPortfolios = _fixture.Build<Portfolio>()
                .With(p => p.UserId, userId)
                .With(p => p.TemplateId, template.Id)
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .CreateMany(2)
                .ToList();

            var otherUserPortfolios = _fixture.Build<Portfolio>()
                .With(p => p.UserId, otherUserId)
                .With(p => p.TemplateId, template.Id)
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .CreateMany(1)
                .ToList();

            await _context.Portfolios.AddRangeAsync(userPortfolios);
            await _context.Portfolios.AddRangeAsync(otherUserPortfolios);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetPortfoliosByUserIdAsync(userId);

            // Assert
            Assert.Equal(2, result.Count);
            result.All(p => p.UserId == userId).Should().BeTrue();
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_WithNonExistingUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingUserId = Guid.NewGuid();

            // Act
            var result = await _repository.GetPortfoliosByUserIdAsync(nonExistingUserId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPortfoliosByUserIdAsync_WithEmptyGuid_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetPortfoliosByUserIdAsync(Guid.Empty);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        [InlineData(Visibility.Unlisted)]
        public async Task CreatePortfolioAsync_WithDifferentVisibilityLevels_ShouldCreateSuccessfully(Visibility visibility)
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var request = _fixture.Build<PortfolioCreateRequest>()
                .With(r => r.TemplateName, template.Name)
                .With(r => r.Visibility, visibility)
                .Create();

            // Act
            var result = await _repository.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Visibility.Should().Be(visibility);
        }

        [Fact]
        public async Task CreatePortfolioAsync_WithMaxLengthFields_ShouldCreateSuccessfully()
        {
            // Arrange
            var template = await CreateTemplateAsync();
            var request = _fixture.Build<PortfolioCreateRequest>()
                .With(r => r.TemplateName, template.Name)
                .With(r => r.Title, new string('A', 255)) // Max length
                .With(r => r.Bio, new string('B', 2000)) // Long bio
                .Create();

            // Act
            var result = await _repository.CreatePortfolioAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().HaveLength(255);
            result.Bio.Should().HaveLength(2000);
        }
    }
} 