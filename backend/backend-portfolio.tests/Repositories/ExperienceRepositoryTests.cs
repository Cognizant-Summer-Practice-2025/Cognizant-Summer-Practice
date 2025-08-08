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
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Repositories
{
    public class ExperienceRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly ExperienceRepository _repository;
        private readonly Fixture _fixture;

        public ExperienceRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new ExperienceRepository(_context);
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

        #region GetAllExperienceAsync Tests

        [Fact]
        public async Task GetAllExperienceAsync_WithMultipleExperiences_ShouldReturnAllExperiences()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experiences = new List<Experience>
            {
                TestDataFactory.CreateExperience(portfolio.Id),
                TestDataFactory.CreateExperience(portfolio.Id),
                TestDataFactory.CreateExperience(portfolio.Id)
            };
            await _context.Experience.AddRangeAsync(experiences);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllExperienceAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(e => e.Portfolio != null);
        }

        [Fact]
        public async Task GetAllExperienceAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllExperienceAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetExperienceByIdAsync Tests

        [Fact]
        public async Task GetExperienceByIdAsync_WithExistingId_ShouldReturnExperience()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experience = TestDataFactory.CreateExperience(portfolio.Id);
            await _context.Experience.AddAsync(experience);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExperienceByIdAsync(experience.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(experience.Id);
            result.Portfolio.Should().NotBeNull();
        }

        [Fact]
        public async Task GetExperienceByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetExperienceByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetExperienceByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetExperienceByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetExperienceByPortfolioIdAsync Tests

        [Fact]
        public async Task GetExperienceByPortfolioIdAsync_WithExistingPortfolio_ShouldReturnExperiencesOrderedByStartDate()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experiences = new List<Experience>
            {
                _fixture.Build<Experience>()
                    .With(e => e.PortfolioId, portfolio.Id)
                    .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)))
                    .Without(e => e.Portfolio)
                    .Create(),
                _fixture.Build<Experience>()
                    .With(e => e.PortfolioId, portfolio.Id)
                    .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
                    .Without(e => e.Portfolio)
                    .Create(),
                _fixture.Build<Experience>()
                    .With(e => e.PortfolioId, portfolio.Id)
                    .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)))
                    .Without(e => e.Portfolio)
                    .Create()
            };
            await _context.Experience.AddRangeAsync(experiences);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetExperienceByPortfolioIdAsync(portfolio.Id);

            // Assert
            result.Should().HaveCount(3);
            result.Should().BeInDescendingOrder(e => e.StartDate);
        }

        [Fact]
        public async Task GetExperienceByPortfolioIdAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetExperienceByPortfolioIdAsync(nonExistingPortfolioId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region CreateExperienceAsync Tests

        [Fact]
        public async Task CreateExperienceAsync_WithValidRequest_ShouldCreateExperience()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<ExperienceCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.JobTitle, "Software Engineer")
                .With(r => r.CompanyName, "Tech Corp")
                .With(r => r.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)))
                .With(r => r.EndDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
                .With(r => r.IsCurrent, false)
                .Create();

            // Act
            var result = await _repository.CreateExperienceAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.PortfolioId.Should().Be(portfolio.Id);
            result.JobTitle.Should().Be("Software Engineer");
            result.CompanyName.Should().Be("Tech Corp");
            result.IsCurrent.Should().BeFalse();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateExperienceAsync_WithCurrentPosition_ShouldCreateCurrentExperience()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<ExperienceCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.IsCurrent, true)
                .Create();

            // Act
            var result = await _repository.CreateExperienceAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsCurrent.Should().BeTrue();
        }

        #endregion

        #region UpdateExperienceAsync Tests

        [Fact]
        public async Task UpdateExperienceAsync_WithValidRequest_ShouldUpdateExperience()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experience = TestDataFactory.CreateExperience(portfolio.Id);
            await _context.Experience.AddAsync(experience);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<ExperienceUpdateRequest>()
                .With(r => r.JobTitle, "Senior Software Engineer")
                .With(r => r.CompanyName, "New Tech Corp")
                .With(r => r.IsCurrent, true)
                .Create();

            // Act
            var result = await _repository.UpdateExperienceAsync(experience.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.JobTitle.Should().Be("Senior Software Engineer");
            result.CompanyName.Should().Be("New Tech Corp");
            result.IsCurrent.Should().BeTrue();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateExperienceAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var request = _fixture.Create<ExperienceUpdateRequest>();

            // Act
            var result = await _repository.UpdateExperienceAsync(nonExistingId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateExperienceAsync_WithNullFields_ShouldNotUpdateNullFields()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experience = TestDataFactory.CreateExperience(portfolio.Id);
            var originalJobTitle = experience.JobTitle;
            var originalCompanyName = experience.CompanyName;
            await _context.Experience.AddAsync(experience);
            await _context.SaveChangesAsync();

            var request = new ExperienceUpdateRequest
            {
                JobTitle = null,
                CompanyName = null,
                StartDate = null,
                EndDate = null,
                IsCurrent = null,
                Description = null,
                SkillsUsed = null
            };

            // Act
            var result = await _repository.UpdateExperienceAsync(experience.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.JobTitle.Should().Be(originalJobTitle);
            result.CompanyName.Should().Be(originalCompanyName);
        }

        #endregion

        #region DeleteExperienceAsync Tests

        [Fact]
        public async Task DeleteExperienceAsync_WithExistingId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var experience = TestDataFactory.CreateExperience(portfolio.Id);
            await _context.Experience.AddAsync(experience);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteExperienceAsync(experience.Id);

            // Assert
            result.Should().BeTrue();
            var deletedExperience = await _context.Experience.FindAsync(experience.Id);
            deletedExperience.Should().BeNull();
        }

        [Fact]
        public async Task DeleteExperienceAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteExperienceAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetCurrentExperienceAsync Tests

        [Fact]
        public async Task GetCurrentExperienceAsync_WithCurrentAndPastExperiences_ShouldReturnOnlyCurrent()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var currentExperiences = new List<Experience>
            {
                _fixture.Build<Experience>()
                    .With(e => e.PortfolioId, portfolio.Id)
                    .With(e => e.IsCurrent, true)
                    .Without(e => e.Portfolio)
                    .Create(),
                _fixture.Build<Experience>()
                    .With(e => e.PortfolioId, portfolio.Id)
                    .With(e => e.IsCurrent, true)
                    .Without(e => e.Portfolio)
                    .Create()
            };

            var pastExperience = _fixture.Build<Experience>()
                .With(e => e.PortfolioId, portfolio.Id)
                .With(e => e.IsCurrent, false)
                .Without(e => e.Portfolio)
                .Create();

            await _context.Experience.AddRangeAsync(currentExperiences);
            await _context.Experience.AddAsync(pastExperience);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCurrentExperienceAsync(portfolio.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.IsCurrent && e.PortfolioId == portfolio.Id);
        }

        [Fact]
        public async Task GetCurrentExperienceAsync_WithNoCurrentExperiences_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var pastExperience = _fixture.Build<Experience>()
                .With(e => e.PortfolioId, portfolio.Id)
                .With(e => e.IsCurrent, false)
                .Without(e => e.Portfolio)
                .Create();
            await _context.Experience.AddAsync(pastExperience);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetCurrentExperienceAsync(portfolio.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCurrentExperienceAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetCurrentExperienceAsync(nonExistingPortfolioId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
