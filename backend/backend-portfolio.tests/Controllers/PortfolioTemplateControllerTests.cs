using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Kernel;
using backend_portfolio.Controllers;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.Services.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var controller = new PortfolioTemplateController(
                _mockPortfolioTemplateService.Object,
                _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPortfolioTemplateServiceIsNull()
        {
            // Act
            var controller = new PortfolioTemplateController(null!, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var controller = new PortfolioTemplateController(_mockPortfolioTemplateService.Object, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreNull()
        {
            // Act
            var controller = new PortfolioTemplateController(null!, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetAllTemplates Tests

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
            _mockPortfolioTemplateService.Verify(x => x.GetAllTemplatesAsync(), Times.Once);
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
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task GetAllTemplates_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetAllTemplates();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting all templates")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllTemplates_WhenServiceThrowsNullReferenceException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.GetAllTemplatesAsync())
                .ThrowsAsync(new NullReferenceException("Null reference error"));

            // Act
            var result = await _controller.GetAllTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region GetActiveTemplates Tests

        [Fact]
        public async Task GetActiveTemplates_WhenCalled_ShouldReturnOkWithActiveTemplates()
        {
            // Arrange
            var activeTemplates = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.IsActive, true)
                .CreateMany(3).ToList();
            _mockPortfolioTemplateService.Setup(x => x.GetActiveTemplatesAsync())
                .ReturnsAsync(activeTemplates);

            // Act
            var result = await _controller.GetActiveTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var templates = okResult!.Value as List<PortfolioTemplateResponse>;
            templates.Should().NotBeNull();
            templates!.All(t => t.IsActive).Should().BeTrue();
            _mockPortfolioTemplateService.Verify(x => x.GetActiveTemplatesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActiveTemplates_WithNoActiveTemplates_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyList = new List<PortfolioTemplateResponse>();
            _mockPortfolioTemplateService.Setup(x => x.GetActiveTemplatesAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetActiveTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var templates = okResult!.Value as List<PortfolioTemplateResponse>;
            templates.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveTemplates_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.GetActiveTemplatesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetActiveTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task GetActiveTemplates_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.GetActiveTemplatesAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.GetActiveTemplates();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while getting active templates")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetActiveTemplates_ShouldReturnMixedTemplates()
        {
            // Arrange
            var templates = new List<PortfolioTemplateResponse>
            {
                _fixture.Build<PortfolioTemplateResponse>().With(t => t.IsActive, true).Create(),
                _fixture.Build<PortfolioTemplateResponse>().With(t => t.IsActive, true).Create(),
                _fixture.Build<PortfolioTemplateResponse>().With(t => t.IsActive, true).Create()
            };
            _mockPortfolioTemplateService.Setup(x => x.GetActiveTemplatesAsync())
                .ReturnsAsync(templates);

            // Act
            var result = await _controller.GetActiveTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var returnedTemplates = okResult!.Value as List<PortfolioTemplateResponse>;
            returnedTemplates.Should().HaveCount(3);
            returnedTemplates!.All(t => t.IsActive).Should().BeTrue();
        }

        #endregion

        #region GetTemplateById Tests

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
            _mockPortfolioTemplateService.Verify(x => x.GetTemplateByIdAsync(templateId), Times.Once);
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
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Template with ID {templateId} not found.");
        }

        [Fact]
        public async Task GetTemplateById_WithEmptyGuid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.GetTemplateById(Guid.Empty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Template ID cannot be empty.");
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
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task GetTemplateById_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetTemplateById(templateId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting template: {templateId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTemplateById_WithCompleteTemplate_ShouldReturnAllFields()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var template = new PortfolioTemplateResponse
            {
                Id = templateId,
                Name = "Complete Template",
                Description = "A complete template with all fields",
                PreviewImageUrl = "https://example.com/preview.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ReturnsAsync(template);

            // Act
            var result = await _controller.GetTemplateById(templateId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(template);
        }

        #endregion

        #region GetTemplateByName Tests

        [Fact]
        public async Task GetTemplateByName_WithValidName_ShouldReturnOkWithTemplate()
        {
            // Arrange
            var templateName = "Modern Template";
            var template = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, templateName)
                .Create();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ReturnsAsync(template);

            // Act
            var result = await _controller.GetTemplateByName(templateName);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(template);
            _mockPortfolioTemplateService.Verify(x => x.GetTemplateByNameAsync(templateName), Times.Once);
        }

        [Fact]
        public async Task GetTemplateByName_WithNonExistentName_ShouldReturnNotFound()
        {
            // Arrange
            var templateName = "Non-existent Template";
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ReturnsAsync((PortfolioTemplateResponse?)null);

            // Act
            var result = await _controller.GetTemplateByName(templateName);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Template with name '{templateName}' not found.");
        }

        [Fact]
        public async Task GetTemplateByName_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateName = "Error Template";
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetTemplateByName(templateName);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task GetTemplateByName_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var templateName = "Error Template";
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ThrowsAsync(exception);

            // Act
            await _controller.GetTemplateByName(templateName);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while getting template by name: {templateName}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetTemplateByName_WithInvalidName_ShouldStillCallService(string? templateName)
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName!))
                .ReturnsAsync((PortfolioTemplateResponse?)null);

            // Act
            var result = await _controller.GetTemplateByName(templateName!);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockPortfolioTemplateService.Verify(x => x.GetTemplateByNameAsync(templateName!), Times.Once);
        }

        [Theory]
        [InlineData("Template with Unicode 测试")]
        [InlineData("Template with Emoji 🚀")]
        [InlineData("Template-with-dashes")]
        [InlineData("Template_with_underscores")]
        [InlineData("Template123WithNumbers")]
        public async Task GetTemplateByName_WithVariousNameFormats_ShouldSucceed(string templateName)
        {
            // Arrange
            var template = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, templateName)
                .Create();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ReturnsAsync(template);

            // Act
            var result = await _controller.GetTemplateByName(templateName);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Should().Be(templateName);
        }

        [Fact]
        public async Task GetTemplateByName_WithVeryLongName_ShouldSucceed()
        {
            // Arrange
            var longName = new string('A', 500);
            var template = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, longName)
                .Create();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(longName))
                .ReturnsAsync(template);

            // Act
            var result = await _controller.GetTemplateByName(longName);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Length.Should().Be(500);
        }

        #endregion

        #region CreateTemplate Tests

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
            _mockPortfolioTemplateService.Verify(x => x.CreateTemplateAsync(request), Times.Once);
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

        [Fact]
        public async Task CreateTemplate_WithCompleteRequest_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Complete Template",
                Description = "A complete template with all fields",
                PreviewImageUrl = "https://example.com/preview.jpg",
                IsActive = true
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, request.Name)
                .With(t => t.Description, request.Description)
                .With(t => t.PreviewImageUrl, request.PreviewImageUrl)
                .With(t => t.IsActive, request.IsActive)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Should().Be(request.Name);
            responseData.Description.Should().Be(request.Description);
            responseData.PreviewImageUrl.Should().Be(request.PreviewImageUrl);
            responseData.IsActive.Should().Be(request.IsActive);
        }

        [Fact]
        public async Task CreateTemplate_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.CreateTemplate(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Request cannot be null.");
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
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid template data");
        }

        [Fact]
        public async Task CreateTemplate_WhenServiceThrowsArgumentException_ShouldLogWarning()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            var argumentException = new ArgumentException("Invalid template data");
            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(argumentException);

            // Act
            await _controller.CreateTemplate(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed while creating template")),
                    argumentException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateTemplate_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task CreateTemplate_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ThrowsAsync(exception);

            // Act
            await _controller.CreateTemplate(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating template")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateTemplate_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var request = _fixture.Create<PortfolioTemplateCreateRequest>();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateTemplate_WithSpecialCharacters_ShouldSucceed()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Template with Special Characters: àáâãäåæçèéêë",
                Description = "Description with emojis 🚀🔥💻 and symbols @#$%^&*()",
                PreviewImageUrl = "https://example.com/special-preview-with-дashes.jpg"
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

        #endregion

        #region UpdateTemplate Tests

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
            _mockPortfolioTemplateService.Verify(x => x.UpdateTemplateAsync(templateId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateTemplate_WithCompleteRequest_ShouldReturnOkWithUpdatedTemplate()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Updated Template",
                Description = "Updated description",
                PreviewImageUrl = "https://example.com/updated-preview.jpg",
                IsActive = false
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .With(t => t.Name, request.Name)
                .With(t => t.Description, request.Description)
                .With(t => t.PreviewImageUrl, request.PreviewImageUrl)
                .With(t => t.IsActive, request.IsActive ?? true)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Should().Be(request.Name);
            responseData.Description.Should().Be(request.Description);
            responseData.PreviewImageUrl.Should().Be(request.PreviewImageUrl);
            responseData.IsActive.Should().Be(request.IsActive ?? true);
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
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Template ID cannot be empty.");
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
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Request cannot be null.");
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
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Template with ID {templateId} not found.");
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
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Invalid update data");
        }

        [Fact]
        public async Task UpdateTemplate_WhenServiceThrowsArgumentException_ShouldLogWarning()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            var argumentException = new ArgumentException("Invalid update data");
            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ThrowsAsync(argumentException);

            // Act
            await _controller.UpdateTemplate(templateId, request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Validation failed while updating template: {templateId}")),
                    argumentException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTemplate_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task UpdateTemplate_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ThrowsAsync(exception);

            // Act
            await _controller.UpdateTemplate(templateId, request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while updating template: {templateId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTemplate_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = _fixture.Create<PortfolioTemplateUpdateRequest>();
            _controller.ModelState.AddModelError("Name", "Name is invalid");

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateTemplate_WithPartialUpdate_ShouldSucceed()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Only Name Updated",
                // All other fields are null, testing partial update
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .With(t => t.Name, request.Name)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var responseData = okResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Should().Be(request.Name);
        }

        #endregion

        #region DeleteTemplate Tests

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
            _mockPortfolioTemplateService.Verify(x => x.DeleteTemplateAsync(templateId), Times.Once);
        }

        [Fact]
        public async Task DeleteTemplate_WithEmptyGuid_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.DeleteTemplate(Guid.Empty);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Template ID cannot be empty.");
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
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be($"Template with ID {templateId} not found.");
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
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task DeleteTemplate_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var exception = new Exception("Database error");
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ThrowsAsync(exception);

            // Act
            await _controller.DeleteTemplate(templateId);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Error occurred while deleting template: {templateId}")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTemplate_WhenServiceThrowsNullReferenceException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ThrowsAsync(new NullReferenceException("Null reference error"));

            // Act
            var result = await _controller.DeleteTemplate(templateId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task DeleteTemplate_WhenServiceThrowsInvalidOperationException_ShouldReturnInternalServerError()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            _mockPortfolioTemplateService.Setup(x => x.DeleteTemplateAsync(templateId))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.DeleteTemplate(templateId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region SeedDefaultTemplates Tests

        [Fact]
        public async Task SeedDefaultTemplates_WhenCalled_ShouldReturnOkWithSuccessMessage()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.SeedDefaultTemplatesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SeedDefaultTemplates();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { message = "Default templates seeded successfully" });
            _mockPortfolioTemplateService.Verify(x => x.SeedDefaultTemplatesAsync(), Times.Once);
        }

        [Fact]
        public async Task SeedDefaultTemplates_WhenServiceThrowsException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.SeedDefaultTemplatesAsync())
                .ThrowsAsync(new Exception("Seeding error"));

            // Act
            var result = await _controller.SeedDefaultTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be("Internal server error");
        }

        [Fact]
        public async Task SeedDefaultTemplates_WhenServiceThrowsException_ShouldLogError()
        {
            // Arrange
            var exception = new Exception("Seeding error");
            _mockPortfolioTemplateService.Setup(x => x.SeedDefaultTemplatesAsync())
                .ThrowsAsync(exception);

            // Act
            await _controller.SeedDefaultTemplates();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while seeding default templates")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SeedDefaultTemplates_WhenServiceThrowsArgumentException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.SeedDefaultTemplatesAsync())
                .ThrowsAsync(new ArgumentException("Invalid seeding data"));

            // Act
            var result = await _controller.SeedDefaultTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task SeedDefaultTemplates_WhenServiceThrowsInvalidOperationException_ShouldReturnInternalServerError()
        {
            // Arrange
            _mockPortfolioTemplateService.Setup(x => x.SeedDefaultTemplatesAsync())
                .ThrowsAsync(new InvalidOperationException("Templates already seeded"));

            // Act
            var result = await _controller.SeedDefaultTemplates();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetTemplateByName_WithCaseSensitivity_ShouldCallServiceWithExactCase()
        {
            // Arrange
            var templateName = "CaseSensitiveTemplate";
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByNameAsync(templateName))
                .ReturnsAsync((PortfolioTemplateResponse?)null);

            // Act
            var result = await _controller.GetTemplateByName(templateName);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _mockPortfolioTemplateService.Verify(x => x.GetTemplateByNameAsync(templateName), Times.Once);
        }

        [Fact]
        public async Task CreateTemplate_WithNullOptionalFields_ShouldSucceed()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Template with Nulls",
                Description = null,
                PreviewImageUrl = null,
                IsActive = false
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, request.Name)
                .With(t => t.Description, request.Description)
                .With(t => t.PreviewImageUrl, request.PreviewImageUrl)
                .With(t => t.IsActive, request.IsActive)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.Description.Should().BeNull();
            responseData.PreviewImageUrl.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTemplate_WithNullOptionalFields_ShouldSucceed()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var request = new PortfolioTemplateUpdateRequest
            {
                Name = null,
                Description = null,
                PreviewImageUrl = null,
                IsActive = null
            };
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.UpdateTemplateAsync(templateId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateTemplate(templateId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetTemplateById_WithMultipleConcurrentRequests_ShouldHandleCorrectly()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var template = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Id, templateId)
                .Create();
            _mockPortfolioTemplateService.Setup(x => x.GetTemplateByIdAsync(templateId))
                .ReturnsAsync(template);

            // Act
            var tasks = Enumerable.Range(0, 10)
                .Select(_ => _controller.GetTemplateById(templateId))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().AllBeOfType<OkObjectResult>();
            _mockPortfolioTemplateService.Verify(x => x.GetTemplateByIdAsync(templateId), Times.Exactly(10));
        }

        [Fact]
        public async Task CreateTemplate_WithVeryLongValidName_ShouldSucceed()
        {
            // Arrange
            var longName = new string('A', 255); // Maximum reasonable length
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.Name, longName)
                .Create();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.Name, longName)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.Name.Length.Should().Be(255);
        }

        [Theory]
        [InlineData("http://example.com/preview.jpg")]
        [InlineData("https://example.com/preview.png")]
        [InlineData("https://cdn.example.com/templates/preview.webp")]
        [InlineData("")]
        [InlineData(null)]
        public async Task CreateTemplate_WithVariousPreviewImageUrls_ShouldSucceed(string? previewImageUrl)
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.PreviewImageUrl, previewImageUrl)
                .Create();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.PreviewImageUrl, previewImageUrl)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.PreviewImageUrl.Should().Be(previewImageUrl);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateTemplate_WithDifferentIsActiveValues_ShouldSucceed(bool isActive)
        {
            // Arrange
            var request = _fixture.Build<PortfolioTemplateCreateRequest>()
                .With(r => r.IsActive, isActive)
                .Create();
            var response = _fixture.Build<PortfolioTemplateResponse>()
                .With(t => t.IsActive, isActive)
                .Create();

            _mockPortfolioTemplateService.Setup(x => x.CreateTemplateAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateTemplate(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            var responseData = createdResult!.Value as PortfolioTemplateResponse;
            responseData!.IsActive.Should().Be(isActive);
        }

        #endregion
    }
} 