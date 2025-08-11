using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Services;
using backend_portfolio.Repositories;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.Models;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Services.Mappers;
using System.Collections.Generic;
using backend_portfolio.tests.Helpers;

namespace backend_portfolio.tests.Services
{
    public class ProjectCommandServiceTests
    {
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IPortfolioRepository> _mockPortfolioRepository;
        private readonly Mock<IValidationService<ProjectCreateRequest>> _mockCreateValidator;
        private readonly Mock<IValidationService<ProjectUpdateRequest>> _mockUpdateValidator;
        private readonly Mock<IProjectMapper> _mockProjectMapper;
        private readonly Mock<ILogger<ProjectCommandService>> _mockLogger;
        private readonly ProjectCommandService _service;
        private readonly Fixture _fixture;

        public ProjectCommandServiceTests()
        {
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockPortfolioRepository = new Mock<IPortfolioRepository>();
            _mockCreateValidator = new Mock<IValidationService<ProjectCreateRequest>>();
            _mockUpdateValidator = new Mock<IValidationService<ProjectUpdateRequest>>();
            _mockProjectMapper = new Mock<IProjectMapper>();
            _mockLogger = new Mock<ILogger<ProjectCommandService>>();
            _fixture = new Fixture();

            _service = new ProjectCommandService(
                _mockProjectRepository.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object,
                _mockProjectMapper.Object,
                _mockLogger.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new ProjectCommandService(
                _mockProjectRepository.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object,
                _mockProjectMapper.Object,
                _mockLogger.Object
            );

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidRequest_ShouldReturnProjectResponse()
        {
            // Arrange
            var request = _fixture.Create<ProjectCreateRequest>();
            var project = TestDataFactory.CreateProject();
            var expectedResponse = _fixture.Create<ProjectResponse>();

            _mockCreateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockProjectRepository.Setup(x => x.CreateProjectAsync(request))
                .ReturnsAsync(project);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(project))
                .Returns(expectedResponse);

            // Act
            var result = await _service.CreateProjectAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockCreateValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockProjectRepository.Verify(x => x.CreateProjectAsync(request), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDto(project), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var request = _fixture.Create<ProjectCreateRequest>();
            var validationErrors = new[] { "Title is required", "Description is too long" };

            _mockCreateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Failure(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProjectAsync(request));
            exception.Message.Should().Contain("Title is required");
            exception.Message.Should().Contain("Description is too long");
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidRequest_ShouldReturnUpdatedProject()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();
            var existingProject = TestDataFactory.CreateProject();
            var updatedProject = TestDataFactory.CreateProject();
            var expectedResponse = _fixture.Create<ProjectResponse>();

            _mockUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync(updatedProject);
            _mockProjectMapper.Setup(x => x.MapToResponseDto(updatedProject))
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProjectAsync(projectId, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedResponse);
            _mockUpdateValidator.Verify(x => x.ValidateAsync(request), Times.Once);
            _mockProjectRepository.Verify(x => x.UpdateProjectAsync(projectId, request), Times.Once);
            _mockProjectMapper.Verify(x => x.MapToResponseDto(updatedProject), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithInvalidRequest_ShouldThrowArgumentException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();
            var validationErrors = new[] { "Title cannot be empty" };

            _mockUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Failure(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProjectAsync(projectId, request));
            exception.Message.Should().Contain("Title cannot be empty");
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentProject_ShouldReturnNull()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();

            _mockUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockProjectRepository.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _service.UpdateProjectAsync(projectId, request);

            // Assert
            result.Should().BeNull();
            _mockProjectRepository.Verify(x => x.UpdateProjectAsync(projectId, request), Times.Once);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithExistingProject_ShouldReturnTrue()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingProject = TestDataFactory.CreateProject();

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(x => x.DeleteProjectAsync(projectId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteProjectAsync(projectId);

            // Assert
            result.Should().BeTrue();
            _mockProjectRepository.Verify(x => x.DeleteProjectAsync(projectId), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistentProject_ShouldReturnFalse()
        {
            // Arrange
            var projectId = Guid.NewGuid();

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act
            var result = await _service.DeleteProjectAsync(projectId);

            // Assert
            result.Should().BeFalse();
            _mockProjectRepository.Verify(x => x.DeleteProjectAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task CreateProjectAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var request = _fixture.Create<ProjectCreateRequest>();
            var expectedException = new InvalidOperationException("Database error");

            _mockCreateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockProjectRepository.Setup(x => x.CreateProjectAsync(request))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateProjectAsync(request));
            thrownException.Should().Be(expectedException);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();
            var existingProject = TestDataFactory.CreateProject();
            var expectedException = new InvalidOperationException("Database error");

            _mockUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Success());
            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(x => x.UpdateProjectAsync(projectId, request))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UpdateProjectAsync(projectId, request));
            thrownException.Should().Be(expectedException);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var existingProject = TestDataFactory.CreateProject();
            var expectedException = new InvalidOperationException("Database error");

            _mockProjectRepository.Setup(x => x.GetProjectByIdAsync(projectId))
                .ReturnsAsync(existingProject);
            _mockProjectRepository.Setup(x => x.DeleteProjectAsync(projectId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteProjectAsync(projectId));
            thrownException.Should().Be(expectedException);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidationFailure_ShouldNotCallRepository()
        {
            // Arrange
            var request = _fixture.Create<ProjectCreateRequest>();
            var validationErrors = new[] { "Validation error" };

            _mockCreateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Failure(validationErrors));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProjectAsync(request));
            _mockProjectRepository.Verify(x => x.CreateProjectAsync(It.IsAny<ProjectCreateRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithValidationFailure_ShouldNotCallRepository()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = _fixture.Create<ProjectUpdateRequest>();
            var validationErrors = new[] { "Validation error" };

            _mockUpdateValidator.Setup(x => x.ValidateAsync(request))
                .ReturnsAsync(ValidationResult.Failure(validationErrors));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProjectAsync(projectId, request));
            _mockProjectRepository.Verify(x => x.UpdateProjectAsync(It.IsAny<Guid>(), It.IsAny<ProjectUpdateRequest>()), Times.Never);
        }

        #endregion
    }
} 