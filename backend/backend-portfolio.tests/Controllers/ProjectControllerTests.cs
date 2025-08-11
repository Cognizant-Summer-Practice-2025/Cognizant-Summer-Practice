using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using backend_portfolio.Controllers;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Project.Request;
using AutoFixture;
using AutoFixture.Kernel;

namespace backend_portfolio.tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectQueryService> _mockProjectQueryService;
        private readonly Mock<IProjectCommandService> _mockProjectCommandService;
        private readonly Mock<ILogger<ProjectController>> _mockLogger;
        private readonly ProjectController _controller;
        private readonly Fixture _fixture;

        public ProjectControllerTests()
        {
            _mockProjectQueryService = new Mock<IProjectQueryService>();
            _mockProjectCommandService = new Mock<IProjectCommandService>();
            _mockLogger = new Mock<ILogger<ProjectController>>();

            _controller = new ProjectController(
                _mockProjectQueryService.Object,
                _mockProjectCommandService.Object,
                _mockLogger.Object
            );

            _fixture = CreateFixtureWithDateOnlySupport();
        }

        private static Fixture CreateFixtureWithDateOnlySupport()
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new DateOnlyGenerator());
            return fixture;
        }

        // Custom DateOnly generator for AutoFixture
        private class DateOnlyGenerator : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(DateOnly))
                {
                    var random = new Random();
                    var year = random.Next(1900, 2100);
                    var month = random.Next(1, 13);
                    var day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                    return new DateOnly(year, month, day);
                }
                return new NoSpecimen();
            }
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var controller = new ProjectController(
                _mockProjectQueryService.Object,
                _mockProjectCommandService.Object,
                _mockLogger.Object
            );

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenProjectQueryServiceIsNull()
        {
            // Act
            var controller = new ProjectController(null!, _mockProjectCommandService.Object, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenProjectCommandServiceIsNull()
        {
            // Act
            var controller = new ProjectController(_mockProjectQueryService.Object, null!, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var controller = new ProjectController(_mockProjectQueryService.Object, _mockProjectCommandService.Object, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreNull()
        {
            // Act
            var controller = new ProjectController(null!, null!, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetAllProjects Tests

        [Fact]
        public async Task GetAllProjects_ShouldReturnOkResult_WhenProjectsExist()
        {
            // Arrange
            var projects = new List<ProjectResponse>
            {
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Project 1" },
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Project 2" }
            };

            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(projects);
            _mockProjectQueryService.Verify(x => x.GetAllProjectsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProjects_ShouldReturnOkResult_WhenNoProjectsExist()
        {
            // Arrange
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(emptyProjects);
            _mockProjectQueryService.Verify(x => x.GetAllProjectsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProjects_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task GetAllProjects_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetAllProjects();

            // Assert
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
        public async Task GetAllProjects_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
        {
            // Arrange
            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ThrowsAsync(new NullReferenceException("Null reference error"));

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetProjectById Tests

        [Fact]
        public async Task GetProjectById_ShouldReturnOkResult_WhenProjectExists()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectResponse
            {
                Id = projectId,
                Title = "Test Project",
                Description = "Test Description"
            };

            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(project);
            _mockProjectQueryService.Verify(x => x.GetProjectByIdAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((ProjectResponse?)null);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(new { message = $"Project with ID {projectId} not found." });
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task GetProjectById_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetProjectById(projectId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting project by ID: {projectId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectById_ShouldHandleEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(emptyGuid))
                .ReturnsAsync((ProjectResponse?)null);

            // Act
            var result = await _controller.GetProjectById(emptyGuid);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockProjectQueryService.Verify(x => x.GetProjectByIdAsync(emptyGuid), Times.Once);
        }

        [Fact]
        public async Task GetProjectById_ShouldReturnCompleteProjectData_WhenProjectHasAllFields()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectResponse
            {
                Id = projectId,
                PortfolioId = Guid.NewGuid(),
                Title = "Complete Project",
                Description = "Complete Description",
                ImageUrl = "https://example.com/image.jpg",
                DemoUrl = "https://demo.example.com",
                GithubUrl = "https://github.com/user/repo",
                Technologies = new[] { "React", "TypeScript", "Node.js" },
                Featured = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };

            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var result = await _controller.GetProjectById(projectId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(project);
        }

        #endregion

        #region GetProjectsByPortfolioId Tests

        [Fact]
        public async Task GetProjectsByPortfolioId_ShouldReturnOkResult_WhenProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<ProjectResponse>
            {
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Project 1", PortfolioId = portfolioId },
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Project 2", PortfolioId = portfolioId }
            };

            _mockProjectQueryService.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(projects);
            _mockProjectQueryService.Verify(x => x.GetProjectsByPortfolioIdAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task GetProjectsByPortfolioId_ShouldReturnOkResult_WhenNoProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(emptyProjects);
        }

        [Fact]
        public async Task GetProjectsByPortfolioId_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockProjectQueryService.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task GetProjectsByPortfolioId_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockProjectQueryService.Setup(x => x.GetProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetProjectsByPortfolioId(portfolioId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting projects for portfolio: {portfolioId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetProjectsByPortfolioId_ShouldHandleEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetProjectsByPortfolioIdAsync(emptyGuid))
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetProjectsByPortfolioId(emptyGuid);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockProjectQueryService.Verify(x => x.GetProjectsByPortfolioIdAsync(emptyGuid), Times.Once);
        }

        #endregion

        #region GetFeaturedProjects Tests

        [Fact]
        public async Task GetFeaturedProjects_ShouldReturnOkResult_WhenFeaturedProjectsExist()
        {
            // Arrange
            var featuredProjects = new List<ProjectResponse>
            {
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Featured Project 1", Featured = true },
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Featured Project 2", Featured = true }
            };

            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsAsync())
                .ReturnsAsync(featuredProjects);

            // Act
            var result = await _controller.GetFeaturedProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(featuredProjects);
            _mockProjectQueryService.Verify(x => x.GetFeaturedProjectsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjects_ShouldReturnOkResult_WhenNoFeaturedProjectsExist()
        {
            // Arrange
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsAsync())
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetFeaturedProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(emptyProjects);
        }

        [Fact]
        public async Task GetFeaturedProjects_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetFeaturedProjects();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task GetFeaturedProjects_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetFeaturedProjects();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting featured projects")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjects_ShouldReturnProjectsWithFeaturedFlag()
        {
            // Arrange
            var featuredProjects = new List<ProjectResponse>
            {
                new ProjectResponse 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Featured Project", 
                    Featured = true,
                    Technologies = new[] { "React", "Node.js" },
                    ImageUrl = "https://example.com/featured.jpg"
                }
            };

            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsAsync())
                .ReturnsAsync(featuredProjects);

            // Act
            var result = await _controller.GetFeaturedProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponse>;
            projects.Should().NotBeNull();
            projects!.All(p => p.Featured).Should().BeTrue();
        }

        #endregion

        #region GetFeaturedProjectsByPortfolioId Tests

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldReturnOkResult_WhenFeaturedProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var featuredProjects = new List<ProjectResponse>
            {
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Featured Project 1", PortfolioId = portfolioId, Featured = true },
                new ProjectResponse { Id = Guid.NewGuid(), Title = "Featured Project 2", PortfolioId = portfolioId, Featured = true }
            };

            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(featuredProjects);

            // Act
            var result = await _controller.GetFeaturedProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(featuredProjects);
            _mockProjectQueryService.Verify(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId), Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldReturnOkResult_WhenNoFeaturedProjectsExist()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetFeaturedProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(emptyProjects);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetFeaturedProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetFeaturedProjectsByPortfolioId(portfolioId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting featured projects for portfolio: {portfolioId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldHandleEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            var emptyProjects = new List<ProjectResponse>();
            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(emptyGuid))
                .ReturnsAsync(emptyProjects);

            // Act
            var result = await _controller.GetFeaturedProjectsByPortfolioId(emptyGuid);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockProjectQueryService.Verify(x => x.GetFeaturedProjectsByPortfolioIdAsync(emptyGuid), Times.Once);
        }

        [Fact]
        public async Task GetFeaturedProjectsByPortfolioId_ShouldReturnOnlyFeaturedProjectsForPortfolio()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var featuredProjects = new List<ProjectResponse>
            {
                new ProjectResponse 
                { 
                    Id = Guid.NewGuid(), 
                    Title = "Portfolio Featured Project", 
                    PortfolioId = portfolioId,
                    Featured = true,
                    DemoUrl = "https://demo.example.com",
                    GithubUrl = "https://github.com/user/repo"
                }
            };

            _mockProjectQueryService.Setup(x => x.GetFeaturedProjectsByPortfolioIdAsync(portfolioId))
                .ReturnsAsync(featuredProjects);

            // Act
            var result = await _controller.GetFeaturedProjectsByPortfolioId(portfolioId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var projects = okResult!.Value as List<ProjectResponse>;
            projects.Should().NotBeNull();
            projects!.All(p => p.Featured && p.PortfolioId == portfolioId).Should().BeTrue();
        }

        #endregion

        #region CreateProject Tests

        [Fact]
        public async Task CreateProject_ShouldReturnCreatedResult_WhenValidRequestProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project",
                Description = "Project Description"
            };

            var createdProject = new ProjectResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                PortfolioId = request.PortfolioId
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(createdProject);
            createdResult.ActionName.Should().Be(nameof(_controller.GetProjectById));
            createdResult.RouteValues!["id"].Should().Be(createdProject.Id);
            _mockProjectCommandService.Verify(x => x.CreateProjectAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreateProject_ShouldReturnCreatedResult_WhenCompleteRequestProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Complete Project",
                Description = "Complete project with all fields",
                ImageUrl = "https://example.com/image.jpg",
                DemoUrl = "https://demo.example.com",
                GithubUrl = "https://github.com/user/repo",
                Technologies = new[] { "React", "TypeScript", "Node.js", "PostgreSQL" },
                Featured = true
            };

            var createdProject = new ProjectResponse
            {
                Id = Guid.NewGuid(),
                PortfolioId = request.PortfolioId,
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DemoUrl = request.DemoUrl,
                GithubUrl = request.GithubUrl,
                Technologies = request.Technologies,
                Featured = request.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(createdProject);
        }

        [Fact]
        public async Task CreateProject_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var request = new ProjectCreateRequest { Title = "" }; // Invalid request
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(new ArgumentException("Title is required"));

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Title is required" });
            _mockProjectCommandService.Verify(x => x.CreateProjectAsync(request), Times.Once);
        }

        [Fact]
        public async Task CreateProject_ShouldReturnBadRequest_WhenPortfolioIdIsEmpty()
        {
            // Arrange
            var request = new ProjectCreateRequest 
            { 
                PortfolioId = Guid.Empty,
                Title = "Valid Title" 
            };
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(new ArgumentException("PortfolioId is required"));

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = "PortfolioId is required" });
        }

        [Fact]
        public async Task CreateProject_ShouldReturnBadRequest_WhenTitleIsNull()
        {
            // Arrange
            var request = new ProjectCreateRequest 
            { 
                PortfolioId = Guid.NewGuid(),
                Title = null! 
            };
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(new ArgumentException("Title cannot be null"));

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateProject_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project"
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task CreateProject_ShouldLogWarning_WhenValidationFails()
        {
            // Arrange
            var request = new ProjectCreateRequest { Title = "" };
            var argumentException = new ArgumentException("Title is required");
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(argumentException);

            // Act
            await _controller.CreateProject(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed while creating project")),
                    argumentException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProject_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project"
            };
            var exception = new Exception("Database error");
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(exception);

            // Act
            await _controller.CreateProject(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating project")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateProject_ShouldHandleNullRequest()
        {
            // Arrange
            ProjectCreateRequest? request = null;
            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request!))
                .ThrowsAsync(new ArgumentNullException(nameof(request)));

            // Act
            var result = await _controller.CreateProject(request!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region UpdateProject Tests

        [Fact]
        public async Task UpdateProject_ShouldReturnOkResult_WhenValidRequestProvided()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest
            {
                Title = "Updated Project",
                Description = "Updated Description"
            };

            var updatedProject = new ProjectResponse
            {
                Id = projectId,
                Title = request.Title,
                Description = request.Description
            };

            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync(updatedProject);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(updatedProject);
            _mockProjectCommandService.Verify(x => x.UpdateProjectAsync(projectId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnOkResult_WhenCompleteUpdateRequestProvided()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest
            {
                Title = "Completely Updated Project",
                Description = "Completely updated description",
                ImageUrl = "https://example.com/updated-image.jpg",
                DemoUrl = "https://updated-demo.example.com",
                GithubUrl = "https://github.com/user/updated-repo",
                Technologies = new[] { "Vue.js", "Python", "Django", "MongoDB" },
                Featured = false
            };

            var updatedProject = new ProjectResponse
            {
                Id = projectId,
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DemoUrl = request.DemoUrl,
                GithubUrl = request.GithubUrl,
                Technologies = request.Technologies,
                Featured = request.Featured ?? false,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync(updatedProject);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(updatedProject);
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest { Title = "Updated Project" };

            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync((ProjectResponse?)null);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(new { message = $"Project with ID {projectId} not found." });
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest { Title = "" }; // Invalid request
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ThrowsAsync(new ArgumentException("Title cannot be empty"));

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Title cannot be empty" });
        }

        [Fact]
        public async Task UpdateProject_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest { Title = "Updated Project" };
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task UpdateProject_ShouldLogWarning_WhenValidationFails()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest { Title = "" };
            var argumentException = new ArgumentException("Title cannot be empty");
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ThrowsAsync(argumentException);

            // Act
            await _controller.UpdateProject(projectId, request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Validation failed while updating project: {projectId}")),
                    argumentException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProject_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest { Title = "Updated Project" };
            var exception = new Exception("Database error");
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ThrowsAsync(exception);

            // Act
            await _controller.UpdateProject(projectId, request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while updating project: {projectId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProject_ShouldHandleEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            var request = new ProjectUpdateRequest { Title = "Updated Project" };
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(emptyGuid, request))
                .ReturnsAsync((ProjectResponse?)null);

            // Act
            var result = await _controller.UpdateProject(emptyGuid, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockProjectCommandService.Verify(x => x.UpdateProjectAsync(emptyGuid, request), Times.Once);
        }

        [Fact]
        public async Task UpdateProject_ShouldHandleNullRequest()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            ProjectUpdateRequest? request = null;
            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request!))
                .ThrowsAsync(new ArgumentNullException(nameof(request)));

            // Act
            var result = await _controller.UpdateProject(projectId, request!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateProject_ShouldHandlePartialUpdate()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest
            {
                Title = "Only Title Updated",
                // All other fields are null, testing partial update
            };

            var updatedProject = new ProjectResponse
            {
                Id = projectId,
                Title = request.Title,
                Description = "Old Description", // Unchanged
                ImageUrl = "https://old-image.com", // Unchanged
                Featured = true // Unchanged
            };

            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync(updatedProject);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(updatedProject);
        }

        #endregion

        #region DeleteProject Tests

        [Fact]
        public async Task DeleteProject_ShouldReturnNoContent_WhenProjectIsDeleted()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockProjectCommandService.Verify(x => x.DeleteProjectAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().BeEquivalentTo(new { message = $"Project with ID {projectId} not found." });
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });
        }

        [Fact]
        public async Task DeleteProject_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ThrowsAsync(exception);

            // Act
            await _controller.DeleteProject(projectId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while deleting project: {projectId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteProject_ShouldHandleEmptyGuid()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(emptyGuid))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProject(emptyGuid);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockProjectCommandService.Verify(x => x.DeleteProjectAsync(emptyGuid), Times.Once);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ThrowsAsync(new NullReferenceException("Null reference error"));

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnInternalServerError_WhenInvalidOperationExceptionOccurs()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            _mockProjectCommandService.Setup(x => x.DeleteProjectAsync(projectId))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.DeleteProject(projectId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task CreateProject_ShouldHandleRequestWithEmptyTechnologies()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project with Empty Technologies",
                Technologies = Array.Empty<string>()
            };

            var createdProject = new ProjectResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                PortfolioId = request.PortfolioId,
                Technologies = request.Technologies
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var project = createdResult!.Value as ProjectResponse;
            project!.Technologies.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateProject_ShouldHandleRequestWithNullTechnologies()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new ProjectUpdateRequest
            {
                Title = "Project with Null Technologies",
                Technologies = null
            };

            var updatedProject = new ProjectResponse
            {
                Id = projectId,
                Title = request.Title,
                Technologies = null
            };

            _mockProjectCommandService.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync(updatedProject);

            // Act
            var result = await _controller.UpdateProject(projectId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var project = okResult!.Value as ProjectResponse;
            project!.Technologies.Should().BeNull();
        }

        [Fact]
        public async Task GetAllProjects_ShouldHandleLargeNumberOfProjects()
        {
            // Arrange
            var projects = new List<ProjectResponse>();
            for (int i = 0; i < 1000; i++)
            {
                projects.Add(new ProjectResponse
                {
                    Id = Guid.NewGuid(),
                    Title = $"Project {i}",
                    PortfolioId = Guid.NewGuid()
                });
            }

            _mockProjectQueryService.Setup(x => x.GetAllProjectsAsync())
                .ReturnsAsync(projects);

            // Act
            var result = await _controller.GetAllProjects();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedProjects = okResult!.Value as List<ProjectResponse>;
            returnedProjects!.Count.Should().Be(1000);
        }

        [Fact]
        public async Task CreateProject_ShouldHandleRequestWithSpecialCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project with Special Characters: àáâãäåæçèéêë",
                Description = "Description with emojis 🚀🔥💻 and symbols @#$%^&*()",
                GithubUrl = "https://github.com/user/special-repo-with-дashes",
                DemoUrl = "https://special-demo.example.com/項目"
            };

            var createdProject = new ProjectResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                PortfolioId = request.PortfolioId,
                GithubUrl = request.GithubUrl,
                DemoUrl = request.DemoUrl
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(createdProject);
        }

        [Fact]
        public async Task CreateProject_ShouldHandleRequestWithVeryLongStrings()
        {
            // Arrange
            var longTitle = new string('A', 500);
            var longDescription = new string('B', 2000);
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = longTitle,
                Description = longDescription
            };

            var createdProject = new ProjectResponse
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                PortfolioId = request.PortfolioId
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(createdProject);

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var project = createdResult!.Value as ProjectResponse;
            project!.Title.Length.Should().Be(500);
            project.Description!.Length.Should().Be(2000);
        }

        [Theory]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("")]
        public async Task CreateProject_ShouldHandleWhitespaceTitle(string title)
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = title
            };

            _mockProjectCommandService.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(new ArgumentException("Title cannot be empty or whitespace"));

            // Act
            var result = await _controller.CreateProject(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetProjectById_ShouldHandleMultipleConcurrentRequests()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectResponse
            {
                Id = projectId,
                Title = "Concurrent Test Project"
            };

            _mockProjectQueryService.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(project);

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _controller.GetProjectById(projectId))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().AllBeOfType<OkObjectResult>();
            _mockProjectQueryService.Verify(x => x.GetProjectByIdAsync(projectId), Times.Exactly(10));
        }

        #endregion
    }
} 