using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Controllers;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.Services.Abstractions;
using System.Linq;

namespace backend_portfolio.tests.Controllers
{
    public class PortfolioTemplateControllerTests
    {
        private readonly Mock<IPortfolioTemplateService> _mockPortfolioTemplateService;
        private readonly Mock<ILogger<PortfolioTemplateController>> _mockLogger;
        private readonly PortfolioTemplateController _controller;
        private readonly Fixture _fixture;

        public PortfolioTemplateControllerTests()
        {
            _mockPortfolioTemplateService = new Mock<IPortfolioTemplateService>();
            _mockLogger = new Mock<ILogger<PortfolioTemplateController>>();
            _controller = new PortfolioTemplateController(
                _mockPortfolioTemplateService.Object,
                _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetAllTemplates_WhenCalled_ShouldReturnOkWithTemplates()
        {
            // Arrange
            var templates = _fixture.CreateMany<PortfolioTemplateResponse>(3).ToList();
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ReturnsAsync(templates);

            // Act
            var result = await _controller.GetAllTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(templates);
        }

        [Fact]
        public async Task GetAllTemplates_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetTemplateById_WithValidId_ShouldReturnOkWithTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var template = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .Create();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ReturnsAsync(template);

            // Act
            var result = await _controller.GetTemplateById(templateId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(template);
        }

        [Fact]
        public async Task GetTemplateById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ReturnsAsync((PortfolioTemplateResponse?)null);

            // Act
            var result = await _controller.GetTemplateById(templateId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetTemplateById_WithEmptyGuid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.GetTemplateById(Guid.Empty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateTemplate_WithValidRequest_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, request.Name)
                .With(t => t.Description, request.Description)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(response);
            createdResult.ActionName.Should().Be(nameof(PortfolioTemplateController.GetTemplateById));
            createdResult.RouteValues!["id"].Should().Be(response.Id);
        }

        [Fact]
        public async Task CreateTemplate_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.CreateTemplate(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateTemplate_WhenServiceThrowsArgumentException_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(new ArgumentException("Invalid template data"));

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateTemplate_WithValidRequest_ShouldReturnOkWithUpdatedTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task UpdateTemplate_WithEmptyGuid_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();

            // Act
            var result = await _controller.UpdateTemplate(Guid.Empty, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateTemplate_WithNullRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var templateId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateTemplate(templateId, null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateTemplate_WhenServiceReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ReturnsAsync((PortfolioTemplateResponse?)null);

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteTemplate_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteTemplate(templateId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteTemplate_WithEmptyGuid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.DeleteTemplate(Guid.Empty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteTemplate_WhenServiceReturnsFalse_ShouldReturnNotFound()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteTemplate(templateId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteTemplate_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteTemplate(templateId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateTemplate_WithInvalidName_ShouldReturnBadRequest(string? name)
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Name, name!)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(new ArgumentException("Name is required"));

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateTemplate_WithExistingName_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Name, "Existing Template")
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(new ArgumentException("Template with this name already exists"));

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetAllTemplates_WithNoTemplates_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyList = new List<PortfolioTemplateResponse>();
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetAllTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var templates = okResult!.Value as List<PortfolioTemplateResponse>;
            templates.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateTemplate_WithVeryLongDescription_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Description, new string('A', 2000)) // Very long description
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(new ArgumentException("Description is too long"));

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetTemplateById_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetTemplateById(templateId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task UpdateTemplate_WhenServiceThrowsArgumentException_ShouldReturnBadRequest()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ThrowsAsync(new ArgumentException("Invalid update data"));

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateTemplate_WithValidMinimalData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Minimal Template",
                Description = "A simple template"
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, request.Name)
                .With(t => t.Description, request.Description)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.Value.Should().BeEquivalentTo(response);
        }

        [Theory]
        [InlineData("Template with Unicode 测试")]
        [InlineData("Template with Emoji 🚀")]
        [InlineData("Template-with-dashes")]
        [InlineData("Template_with_underscores")]
        [InlineData("Template123WithNumbers")]
        public async Task CreateTemplate_WithVariousNameFormats_ShouldSucceed(string templateName)
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Name, templateName)
                .Create();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, templateName)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Should().Be(templateName);
        }

        [Fact]
        public async Task GetAllTemplates_WithLargeNumberOfTemplates_ShouldReturnAllTemplates()
        {
            // Arrange
            var templates = _fixture.CreateMany<PortfolioTemplateResponse>(100).ToList();
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ReturnsAsync(templates);

            // Act
            var result = await _controller.GetAllTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedTemplates = okResult!.Value as List<PortfolioTemplateResponse>;
            returnedTemplates.Should().HaveCount(100);
        }
    }
} 