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
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Repositories
{
    public class ProjectRepositoryTests : IDisposable
    {
        private readonly PortfolioDbContext _context;
        private readonly ProjectRepository _repository;
        private readonly Fixture _fixture;

        public ProjectRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(options);
            _repository = new ProjectRepository(_context);
            _fixture = new Fixture();

            // Configure AutoFixture to handle circular references
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            
            // Configure AutoFixture to handle DateOnly properly - generate valid dates
            _fixture.Register(() => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-new Random().Next(1, 3650))));
            
            // Configure AutoFixture to not create navigation properties
            _fixture.Customize<Project>(c => c.Without(p => p.Portfolio));
            _fixture.Customize<Portfolio>(c => c
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Without(p => p.Template));
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

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithMultipleProjects_ShouldReturnAllProjects()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(portfolio.Id),
                TestDataFactory.CreateProject(portfolio.Id),
                TestDataFactory.CreateProject(portfolio.Id)
            };
            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllProjectsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.True(result.All(p => p.Portfolio != null));
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllProjectsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithExistingId_ShouldReturnProject()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var project = TestDataFactory.CreateProject(portfolio.Id);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByIdAsync(project.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(project.Id);
            result.Portfolio.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetProjectByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithEmptyGuid_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetProjectByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetProjectsByPortfolioIdAsync Tests

        [Fact]
        public async Task GetProjectsByPortfolioIdAsync_WithExistingPortfolio_ShouldReturnProjectsOrderedByFeaturedThenTitle()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var projects = new List<Project>
            {
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, false)
                    .With(p => p.Title, "Z Non-Featured Project")
                    .Without(p => p.Portfolio)
                    .Create(),
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, true)
                    .With(p => p.Title, "B Featured Project")
                    .Without(p => p.Portfolio)
                    .Create(),
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, true)
                    .With(p => p.Title, "A Featured Project")
                    .Without(p => p.Portfolio)
                    .Create(),
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, false)
                    .With(p => p.Title, "A Non-Featured Project")
                    .Without(p => p.Portfolio)
                    .Create()
            };
            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectsByPortfolioIdAsync(portfolio.Id);

            // Assert
            Assert.Equal(4, result.Count);
            
            // Featured projects should come first
            var featuredProjects = result.Take(2).ToList();
            Assert.True(featuredProjects.All(p => p.Featured));
            featuredProjects.Should().BeInAscendingOrder(p => p.Title);
            
            // Non-featured projects should come after, ordered by title
            var nonFeaturedProjects = result.Skip(2).ToList();
            Assert.True(nonFeaturedProjects.All(p => !p.Featured));
            nonFeaturedProjects.Should().BeInAscendingOrder(p => p.Title);
        }

        [Fact]
        public async Task GetProjectsByPortfolioIdAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetProjectsByPortfolioIdAsync(nonExistingPortfolioId);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidRequest_ShouldCreateProject()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<ProjectCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.Title, "Test Project")
                .With(r => r.Description, "A test project")
                .With(r => r.Featured, true)
                .Create();

            // Act
            var result = await _repository.CreateProjectAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
            result.PortfolioId.Should().Be(portfolio.Id);
            result.Title.Should().Be("Test Project");
            result.Description.Should().Be("A test project");
            result.Featured.Should().BeTrue();
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateProjectAsync_WithNonFeaturedProject_ShouldCreateNonFeaturedProject()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var request = _fixture.Build<ProjectCreateRequest>()
                .With(r => r.PortfolioId, portfolio.Id)
                .With(r => r.Featured, false)
                .Create();

            // Act
            var result = await _repository.CreateProjectAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Featured.Should().BeFalse();
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidRequest_ShouldUpdateProject()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var project = TestDataFactory.CreateProject(portfolio.Id);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var request = _fixture.Build<ProjectUpdateRequest>()
                .With(r => r.Title, "Updated Project Title")
                .With(r => r.Description, "Updated description")
                .With(r => r.Featured, true)
                .Create();

            // Act
            var result = await _repository.UpdateProjectAsync(project.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Project Title");
            result.Description.Should().Be("Updated description");
            result.Featured.Should().BeTrue();
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();

            // Act
            var result = await _repository.UpdateProjectAsync(nonExistingId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNullFields_ShouldNotUpdateNullFields()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var project = TestDataFactory.CreateProject(portfolio.Id);
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var request = new ProjectUpdateRequest
            {
                Title = null,
                Description = null,
                ImageUrl = null,
                DemoUrl = null,
                GithubUrl = null,
                Technologies = null,
                Featured = null
            };

            // Act
            var result = await _repository.UpdateProjectAsync(project.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be(originalTitle);
            result.Description.Should().Be(originalDescription);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithExistingId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var project = TestDataFactory.CreateProject(portfolio.Id);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteProjectAsync(project.Id);

            // Assert
            result.Should().BeTrue();
            var deletedProject = await _context.Projects.FindAsync(project.Id);
            deletedProject.Should().BeNull();
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistingId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteProjectAsync(nonExistingId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetFeaturedProjectsAsync Tests

        [Fact]
        public async Task GetFeaturedProjectsAsync_WithFeaturedAndNonFeaturedProjects_ShouldReturnOnlyFeatured()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var featuredProjects = new List<Project>
            {
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, true)
                    .Without(p => p.Portfolio)
                    .Create(),
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio.Id)
                    .With(p => p.Featured, true)
                    .Without(p => p.Portfolio)
                    .Create()
            };

            var nonFeaturedProject = _fixture.Build<Project>()
                .With(p => p.PortfolioId, portfolio.Id)
                .With(p => p.Featured, false)
                .Without(p => p.Portfolio)
                .Create();

            await _context.Projects.AddRangeAsync(featuredProjects);
            await _context.Projects.AddAsync(nonFeaturedProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFeaturedProjectsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.All(p => p.Featured));
            Assert.True(result.All(p => p.Portfolio != null));
        }

        [Fact]
        public async Task GetFeaturedProjectsAsync_WithNoFeaturedProjects_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var nonFeaturedProject = _fixture.Build<Project>()
                .With(p => p.PortfolioId, portfolio.Id)
                .With(p => p.Featured, false)
                .Without(p => p.Portfolio)
                .Create();
            await _context.Projects.AddAsync(nonFeaturedProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFeaturedProjectsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetFeaturedProjectsByPortfolioIdAsync Tests

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_WithFeaturedProjectsInPortfolio_ShouldReturnFeaturedProjectsForPortfolio()
        {
            // Arrange
            var portfolio1 = await CreatePortfolioAsync();
            var portfolio2 = await CreatePortfolioAsync();

            var portfolio1FeaturedProjects = new List<Project>
            {
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio1.Id)
                    .With(p => p.Featured, true)
                    .Without(p => p.Portfolio)
                    .Create(),
                _fixture.Build<Project>()
                    .With(p => p.PortfolioId, portfolio1.Id)
                    .With(p => p.Featured, true)
                    .Without(p => p.Portfolio)
                    .Create()
            };

            var portfolio1NonFeaturedProject = _fixture.Build<Project>()
                .With(p => p.PortfolioId, portfolio1.Id)
                .With(p => p.Featured, false)
                .Without(p => p.Portfolio)
                .Create();

            var portfolio2FeaturedProject = _fixture.Build<Project>()
                .With(p => p.PortfolioId, portfolio2.Id)
                .With(p => p.Featured, true)
                .Without(p => p.Portfolio)
                .Create();

            await _context.Projects.AddRangeAsync(portfolio1FeaturedProjects);
            await _context.Projects.AddAsync(portfolio1NonFeaturedProject);
            await _context.Projects.AddAsync(portfolio2FeaturedProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFeaturedProjectsByPortfolioIdAsync(portfolio1.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.All(p => p.PortfolioId == portfolio1.Id));
            Assert.True(result.All(p => p.Featured));
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_WithNoFeaturedProjectsInPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var portfolio = await CreatePortfolioAsync();
            var nonFeaturedProject = _fixture.Build<Project>()
                .With(p => p.PortfolioId, portfolio.Id)
                .With(p => p.Featured, false)
                .Without(p => p.Portfolio)
                .Create();
            await _context.Projects.AddAsync(nonFeaturedProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFeaturedProjectsByPortfolioIdAsync(portfolio.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_WithNonExistingPortfolio_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistingPortfolioId = Guid.NewGuid();

            // Act
            var result = await _repository.GetFeaturedProjectsByPortfolioIdAsync(nonExistingPortfolioId);

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
