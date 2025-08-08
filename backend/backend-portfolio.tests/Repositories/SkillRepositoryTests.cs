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
using backend_portfolio.DTO.Skill.Request;

namespace backend_portfolio.tests.Repositories
{
    public class SkillRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly SkillRepository _repository;
        private readonly Fixture _fixture;

        public SkillRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new SkillRepository(_context);
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CreateSkillAsync_WithValidSkill_ShouldCreateSuccessfully()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skillRequest = _fixture.Build<SkillCreateRequest>()
                .With(s => s.PortfolioId, portfolioId)
                .With(s => s.Name, "Test Skill")
                .Create();

            // Act
            var result = await _repository.CreateSkillAsync(skillRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Test Skill");
            result.PortfolioId.Should().Be(portfolioId);
        }

        [Fact]
        public async Task CreateSkillAsync_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateSkillAsync(null!));
        }

        [Fact]
        public async Task GetSkillByIdAsync_WithExistingId_ShouldReturnSkill()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skill = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, portfolioId)
                .Without(s => s.Portfolio)
                .Create();
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSkillByIdAsync(skill.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(skill.Id);
            result.Name.Should().Be(skill.Name);
        }

        [Fact]
        public async Task GetSkillByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetSkillByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllSkillsAsync_WithMultipleSkills_ShouldReturnAllSkills()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skills = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, portfolioId)
                .Without(s => s.Portfolio)
                .CreateMany(3)
                .ToList();

            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllSkillsAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(s => s.Id).Should().BeEquivalentTo(skills.Select(s => s.Id));
        }

        [Fact]
        public async Task GetSkillsByPortfolioIdAsync_WithExistingPortfolio_ShouldReturnSkills()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var otherPortfolioId = await CreatePortfolioAsync();

            var skills = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, portfolioId)
                .Without(s => s.Portfolio)
                .CreateMany(2)
                .ToList();

            var otherSkills = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, otherPortfolioId)
                .Without(s => s.Portfolio)
                .CreateMany(3)
                .ToList();

            await _context.Skills.AddRangeAsync(skills);
            await _context.Skills.AddRangeAsync(otherSkills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSkillsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().HaveCount(2);
            result.All(s => s.PortfolioId == portfolioId).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateSkillAsync_WithValidSkill_ShouldUpdateSuccessfully()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skill = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, portfolioId)
                .Without(s => s.Portfolio)
                .Create();
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();

            var updateRequest = new SkillUpdateRequest 
            { 
                Name = "Updated Skill", 
                ProficiencyLevel = 5 
            };

            // Act
            var result = await _repository.UpdateSkillAsync(skill.Id, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Skill");
            result.ProficiencyLevel.Should().Be(5);
        }

        [Fact]
        public async Task DeleteSkillAsync_WithExistingId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skill = _fixture.Build<Skill>()
                .With(s => s.PortfolioId, portfolioId)
                .Without(s => s.Portfolio)
                .Create();
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteSkillAsync(skill.Id);

            // Assert
            result.Should().BeTrue();
            
            var deletedSkill = await _context.Skills.FindAsync(skill.Id);
            deletedSkill.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSkillAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.DeleteSkillAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(null)]
        public async Task CreateSkillAsync_WithDifferentProficiencyLevels_ShouldCreateSuccessfully(int? proficiencyLevel)
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skillRequest = _fixture.Build<SkillCreateRequest>()
                .With(s => s.PortfolioId, portfolioId)
                .With(s => s.ProficiencyLevel, proficiencyLevel)
                .Create();

            // Act
            var result = await _repository.CreateSkillAsync(skillRequest);

            // Assert
            result.Should().NotBeNull();
            result.ProficiencyLevel.Should().Be(proficiencyLevel);
        }

        [Fact]
        public async Task CreateSkillAsync_WithMaxLengthFields_ShouldCreateSuccessfully()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skillRequest = _fixture.Build<SkillCreateRequest>()
                .With(s => s.PortfolioId, portfolioId)
                .With(s => s.Name, new string('A', 100))
                .With(s => s.CategoryType, new string('B', 50))
                .With(s => s.Subcategory, new string('C', 100))
                .With(s => s.Category, new string('D', 255))
                .Create();

            // Act
            var result = await _repository.CreateSkillAsync(skillRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().HaveLength(100);
        }

        [Fact]
        public async Task CreateSkillAsync_WithMinimalRequiredFields_ShouldCreateSuccessfully()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skillRequest = _fixture.Build<SkillCreateRequest>()
                .With(s => s.PortfolioId, portfolioId)
                .With(s => s.Name, "Minimal Skill")
                .With(s => s.CategoryType, (string?)null)
                .With(s => s.Subcategory, (string?)null)
                .With(s => s.Category, (string?)null)
                .With(s => s.ProficiencyLevel, (int?)null)
                .With(s => s.DisplayOrder, (int?)null)
                .Create();

            // Act
            var result = await _repository.CreateSkillAsync(skillRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Minimal Skill");
        }

        [Fact]
        public async Task GetSkillsByPortfolioIdAsync_OrderedByDisplayOrder_ShouldReturnOrderedSkills()
        {
            // Arrange
            var portfolioId = await CreatePortfolioAsync();
            var skills = new[]
            {
                _fixture.Build<Skill>()
                    .With(s => s.PortfolioId, portfolioId)
                    .With(s => s.DisplayOrder, 3)
                    .With(s => s.Name, "Skill C")
                    .Without(s => s.Portfolio)
                    .Create(),
                _fixture.Build<Skill>()
                    .With(s => s.PortfolioId, portfolioId)
                    .With(s => s.DisplayOrder, 1)
                    .With(s => s.Name, "Skill A")
                    .Without(s => s.Portfolio)
                    .Create(),
                _fixture.Build<Skill>()
                    .With(s => s.PortfolioId, portfolioId)
                    .With(s => s.DisplayOrder, 2)
                    .With(s => s.Name, "Skill B")
                    .Without(s => s.Portfolio)
                    .Create()
            };

            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSkillsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().HaveCount(3);
            result.Select(s => s.Name).Should().Equal(new[] { "Skill A", "Skill B", "Skill C" });
        }

        private async Task<Guid> CreatePortfolioAsync()
        {
            var template = _fixture.Build<PortfolioTemplate>()
                .With(t => t.Name, "Test Template")
                .Without(t => t.Portfolios)
                .Create();

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

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();

            return portfolio.Id;
        }
    }
} 