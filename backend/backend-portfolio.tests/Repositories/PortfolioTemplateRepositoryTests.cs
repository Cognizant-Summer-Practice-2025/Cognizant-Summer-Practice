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
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Repositories
{
    public class PortfolioTemplateRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly PortfolioTemplateRepository _repository;
        private readonly Fixture _fixture;

        public PortfolioTemplateRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new PortfolioTemplateRepository(_context);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Configure AutoFixture to handle DateOnly properly - generate valid dates
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-new Random().Next(1, 3650))));
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetAllTemplatesAsync Tests

        [Fact]
        public async Task GetAllTemplatesAsync_WithMultipleTemplates_ShouldReturnAllTemplates()
        {
            // Arrange
            var templates = new List<PortfolioTemplate>
            {
                TestDataFactory.CreatePortfolioTemplate(),
                TestDataFactory.CreatePortfolioTemplate(),
                TestDataFactory.CreatePortfolioTemplate()
            };
            await _context.PortfolioTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllTemplatesAsync();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllTemplatesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllTemplatesAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetTemplateByIdAsync Tests

        [Fact]
        public async Task GetTemplateByIdAsync_WithExistingId_ShouldReturnTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetTemplateByIdAsync(template.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(template.Id);
        }

        [Fact]
        public async Task GetTemplateByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetTemplateByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTemplateByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetTemplateByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateTemplateAsync Tests

        [Fact]
        public async Task CreateTemplateAsync_WithValidRequest_ShouldCreateTemplate()
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Name, "Modern Template")
                .With(r => r.Description, "A modern portfolio template")
                .With(r => r.PreviewImageUrl, "https://example.com/preview.jpg")
                .With(r => r.IsActive, true)
                .Create();

            // Act
            var result = await _repository.CreateTemplateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.Name.Should().Be("Modern Template");
            result.Description.Should().Be("A modern portfolio template");
            result.PreviewImageUrl.Should().Be("https://example.com/preview.jpg");
            result.IsActive.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateTemplateAsync_WithInactiveTemplate_ShouldCreateInactiveTemplate()
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.IsActive, false)
                .Create();

            // Act
            var result = await _repository.CreateTemplateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsActive.Should().BeFalse();
        }

        #endregion

        #region UpdateTemplateAsync Tests

        [Fact]
        public async Task UpdateTemplateAsync_WithValidRequest_ShouldUpdateTemplate()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<PortfolioTemplateUpdateRequest>()
                .With(r => r.Name, "Updated Template Name")
                .With(r => r.Description, "Updated description")
                .With(r => r.PreviewImageUrl, "https://example.com/updated-preview.jpg")
                .With(r => r.IsActive, false)
                .Create();

            // Act
            var result = await _repository.UpdateTemplateAsync(template.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Template Name");
            result.Description.Should().Be("Updated description");
            result.PreviewImageUrl.Should().Be("https://example.com/updated-preview.jpg");
            result.IsActive.Should().BeFalse();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateTemplateAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();

            // Act
            var result = await _repository.UpdateTemplateAsync(nonExistingId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTemplateAsync_WithNullFields_ShouldNotUpdateNullFields()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            var originalName = template.Name;
            var originalDescription = template.Description;
            var originalPreviewImageUrl = template.PreviewImageUrl;
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new PortfolioTemplateUpdateRequest
            {
                Name = null,
                Description = null,
                PreviewImageUrl = null,
                IsActive = null
            };

            // Act
            var result = await _repository.UpdateTemplateAsync(template.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(originalName);
            result.Description.Should().Be(originalDescription);
            result.PreviewImageUrl.Should().Be(originalPreviewImageUrl);
        }

        #endregion

        #region DeleteTemplateAsync Tests

        [Fact]
        public async Task DeleteTemplateAsync_WithExistingId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteTemplateAsync(template.Id);

            // Assert
            result.Should().BeTrue();
            var deletedTemplate = await _context.PortfolioTemplates.FindAsync(template.Id);
            deletedTemplate.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTemplateAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteTemplateAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetActiveTemplatesAsync Tests

        [Fact]
        public async Task GetActiveTemplatesAsync_WithActiveAndInactiveTemplates_ShouldReturnOnlyActive()
        {
            // Arrange
            var activeTemplates = new List<PortfolioTemplate>
            {
                _fixture.Build<PortfolioTemplate>()
                    .With(t => t.IsActive, true)
                    .With(t => t.CreatedAt, DateTime.UtcNow)
                    .With(t => t.UpdatedAt, DateTime.UtcNow)
                    .Without(t => t.Portfolios)
                    .Create(),
                _fixture.Build<PortfolioTemplate>()
                    .With(t => t.IsActive, true)
                    .With(t => t.CreatedAt, DateTime.UtcNow)
                    .With(t => t.UpdatedAt, DateTime.UtcNow)
                    .Without(t => t.Portfolios)
                    .Create()
            };

            var inactiveTemplate = _fixture.Build<PortfolioTemplate>()
                .With(t => t.IsActive, false)
                .With(t => t.CreatedAt, DateTime.UtcNow)
                .With(t => t.UpdatedAt, DateTime.UtcNow)
                .Without(t => t.Portfolios)
                .Create();

            await _context.PortfolioTemplates.AddRangeAsync(activeTemplates);
            await _context.PortfolioTemplates.AddAsync(inactiveTemplate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveTemplatesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            result.All(t => t.IsActive).Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveTemplatesAsync_WithNoActiveTemplates_ShouldReturnEmptyList()
        {
            // Arrange
            var inactiveTemplate = _fixture.Build<PortfolioTemplate>()
                .With(t => t.IsActive, false)
                .With(t => t.CreatedAt, DateTime.UtcNow)
                .With(t => t.UpdatedAt, DateTime.UtcNow)
                .Without(t => t.Portfolios)
                .Create();
            await _context.PortfolioTemplates.AddAsync(inactiveTemplate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveTemplatesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetActiveTemplatesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetActiveTemplatesAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
