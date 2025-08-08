using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using backend_portfolio.Controllers;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Project.Request;

namespace backend_portfolio.tests.Controllers
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectQueryService> _mockProjectQueryService;
        private readonly Mock<IProjectCommandService> _mockProjectCommandService;
        private readonly Mock<ILogger<ProjectController>> _mockLogger;
        private readonly ProjectController _controller;

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

        #endregion
    }
} 