using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Repositories;
using backend_portfolio.Models;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.tests.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.Services
{
    public class ProjectQueryServiceTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IProjectMapper> _mockProjectMapper;
        private readonly Mock<ILogger<ProjectQueryService>> _mockLogger;
        private readonly ProjectQueryService _service;

        public ProjectQueryServiceTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockProjectMapper = new Mock<IProjectMapper>();
            _mockLogger = new Mock<ILogger<ProjectQueryService>>();

            _service = new ProjectQueryService(
                _mockProjectRepository.Object,
                _mockProjectMapper.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new ProjectQueryService(
                _mockProjectRepository.Object,
                _mockProjectMapper.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenRepositoryIsNull()
        {
            // Act
            var service = new ProjectQueryService(
                null!,
                _mockProjectMapper.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenMapperIsNull()
        {
            // Act
            var service = new ProjectQueryService(
                _mockProjectRepository.Object,
                null!,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var service = new ProjectQueryService(
                _mockProjectRepository.Object,
                _mockProjectMapper.Object,
                null!
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_ShouldReturnProjects_WhenProjectsExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            var expectedResponses = new List<ProjectResponse>
            {
                new ProjectResponse { Id = projects[0].Id, Title = projects[0].Title },
                new ProjectResponse { Id = projects[1].Id, Title = projects[1].Title }
            };

            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);

            _mockProjectRepository.Verify(x => x.GetAllProjectsAsync(), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDtos(projects), Times.Once);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldReturnEmptyList_WhenNoProjectsExist()
        {
            // Arrange
            var projects = new List<Project>();
            var expectedResponses = new List<ProjectResponse>();

            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockProjectRepository.Verify(x => x.GetAllProjectsAsync(), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDtos(projects), Times.Once);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var exception = new InvalidOperationException("Database connection failed");
            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var action = () => _service.GetAllProjectsAsync();
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting all projects")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldHandleNullFromRepository()
        {
            // Arrange
            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync((List<Project>)null!);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(null!))
                .Returns(new List<ProjectResponse>());

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnProject_WhenProjectExists()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = TestDataFactory.CreateProject();
            project.Id = projectId;

            var expectedResponse = new ProjectResponse
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description
            };

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(project))
                .Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectByIdAsync(projectId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);

            _mockProjectRepository.Verify(x => x.GetProjectByIdAsync(projectId), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDto(project), Times.Once);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _service.GetProjectByIdAsync(projectId);

            // Assert
            result.Should().BeNull();

            _mockProjectRepository.Verify(x => x.GetProjectByIdAsync(projectId), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDto(It.IsAny<Project>()), Times.Never);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var exception = new InvalidOperationException("Database connection failed");
            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ThrowsAsync(exception);

            // Act & Assert
            var action = () => _service.GetProjectByIdAsync(projectId);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting project by ID")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldHandleEmptyGuid()
        {
            // Arrange
            var projectId = Guid.Empty;

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _service.GetProjectByIdAsync(projectId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetProjectsByPortfolioIdAsync Tests

        [Fact]
        public async Task GetProjectsByPortfolioIdAsync_ShouldReturnProjects_WhenProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            var expectedResponses = new List<ProjectResponse>
            {
                new ProjectResponse { Id = projects[0].Id, Title = projects[0].Title },
                new ProjectResponse { Id = projects[1].Id, Title = projects[1].Title }
            };

            _mockProjectRepository.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetProjectsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);

            _mockProjectRepository.Verify(x => x.GetProjectsByPortfolioIdAsync(portfolioId), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDtos(projects), Times.Once);
        }

        [Fact]
        public async Task GetProjectsByPortfolioIdAsync_ShouldReturnEmptyList_WhenNoProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<Project>();
            var expectedResponses = new List<ProjectResponse>();

            _mockProjectRepository.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetProjectsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProjectsByPortfolioIdAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var exception = new InvalidOperationException("Database connection failed");
            _mockProjectRepository.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(exception);

            // Act & Assert
            var action = () => _service.GetProjectsByPortfolioIdAsync(portfolioId);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting projects for portfolio")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region GetFeaturedProjectsAsync Tests

        [Fact]
        public async Task GetFeaturedProjectsAsync_ShouldReturnProjects_WhenFeaturedProjectsExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            var expectedResponses = new List<ProjectResponse>
            {
                new ProjectResponse { Id = projects[0].Id, Title = projects[0].Title },
                new ProjectResponse { Id = projects[1].Id, Title = projects[1].Title }
            };

            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetFeaturedProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);

            _mockProjectRepository.Verify(x => x.GetFeaturedProjectsAsync(), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDtos(projects), Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjectsAsync_ShouldReturnEmptyList_WhenNoFeaturedProjectsExist()
        {
            // Arrange
            var projects = new List<Project>();
            var expectedResponses = new List<ProjectResponse>();

            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetFeaturedProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFeaturedProjectsAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var exception = new InvalidOperationException("Database connection failed");
            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsAsync())
                .ThrowsAsync(exception);

            // Act & Assert
            var action = () => _service.GetFeaturedProjectsAsync();
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting featured projects")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region GetFeaturedProjectsByPortfolioIdAsync Tests

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_ShouldReturnProjects_WhenFeaturedProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            var expectedResponses = new List<ProjectResponse>
            {
                new ProjectResponse { Id = projects[0].Id, Title = projects[0].Title },
                new ProjectResponse { Id = projects[1].Id, Title = projects[1].Title }
            };

            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetFeaturedProjectsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedResponses);

            _mockProjectRepository.Verify(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDtos(projects), Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_ShouldReturnEmptyList_WhenNoFeaturedProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<Project>();
            var expectedResponses = new List<ProjectResponse>();

            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetFeaturedProjectsByPortfolioIdAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioIdAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var exception = new InvalidOperationException("Database connection failed");
            _mockProjectRepository.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(exception);

            // Act & Assert
            var action = () => _service.GetFeaturedProjectsByPortfolioIdAsync(portfolioId);
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting featured projects for portfolio")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task GetAllProjectsAsync_ShouldHandleMapperReturningNull()
        {
            // Arrange
            var projects = new List<Project> { TestDataFactory.CreateProject() };
            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns((IEnumerable<ProjectResponse>)null!);

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldHandleMapperReturningNull()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = TestDataFactory.CreateProject();
            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(project))
                .Returns((ProjectResponse)null!);

            // Act
            var result = await _service.GetProjectByIdAsync(projectId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var projects = new List<Project> { TestDataFactory.CreateProject() };
            var expectedResponses = new List<ProjectResponse> { new ProjectResponse() };

            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _service.GetAllProjectsAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(5);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = TestDataFactory.CreateProject();
            var expectedResponse = new ProjectResponse();

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(project))
                .Returns(expectedResponse);

            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _service.GetProjectByIdAsync(projectId))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(5);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetAllProjectsAsync_ShouldHandleRepositoryReturningInvalidData(string _)
        {
            // Arrange
            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync((List<Project>)null!);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(null!))
                .Returns(new List<ProjectResponse>());

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldHandleRepositoryReturningNullProjects()
        {
            // Arrange
            var projects = new List<Project> { null! };
            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(new List<ProjectResponse>());

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Performance and Stress Tests

        [Fact]
        public async Task GetAllProjectsAsync_ShouldHandleLargeDataSet()
        {
            // Arrange
            var projects = Enumerable.Range(0, 1000)
                .Select(_ => TestDataFactory.CreateProject())
                .ToList();

            var expectedResponses = projects.Select(p => new ProjectResponse { Id = p.Id, Title = p.Title }).ToList();

            _mockProjectRepository.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);
            _mockProjectMapper.Setup(x => x.MapToResponseDtos(projects))
                .Returns(expectedResponses);

            // Act
            var result = await _service.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1000);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldHandleRapidRequests()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = TestDataFactory.CreateProject();
            var expectedResponse = new ProjectResponse { Id = project.Id, Title = project.Title };

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(project))
                .Returns(expectedResponse);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var tasks = Enumerable.Range(0, 100)
                .Select(_ => _service.GetProjectByIdAsync(projectId))
                .ToArray();

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
        }

        #endregion
    }
} 