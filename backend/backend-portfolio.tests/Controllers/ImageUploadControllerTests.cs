using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Kernel;
using backend_portfolio.Controllers;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.DTO.ImageUpload.Response;

namespace backend_portfolio.tests.Controllers
{
    public class ImageUploadControllerTests
    {
        private readonly Mock<IImageUploadUtility> _mockImageUploadUtility;
        private readonly Mock<ILogger<ImageUploadController>> _mockLogger;
        private readonly ImageUploadController _controller;
        private readonly Fixture _fixture;

        public ImageUploadControllerTests()
        {
            _mockImageUploadUtility = new Mock<IImageUploadUtility>();
            _mockLogger = new Mock<ILogger<ImageUploadController>>();
            _controller = new ImageUploadController(
                _mockImageUploadUtility.Object,
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
        public void Constructor_ShouldCreateInstance_WhenImageUploadUtilityIsNull()
        {
            // Act
            var controller = new ImageUploadController(null!, _mockLogger.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var controller = new ImageUploadController(_mockImageUploadUtility.Object, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var controller = new ImageUploadController(
                _mockImageUploadUtility.Object,
                _mockLogger.Object
            );

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region UploadImage Tests

        [Fact]
        public async Task UploadImage_ShouldReturnOkResult_WhenImageUploadedSuccessfully()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "profile_images";
            var imagePath = "/uploads/profile_images/test.jpg";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.IsValidSubfolder(subfolder))
                .Returns(true);
            _mockImageUploadUtility.Setup(x => x.SaveImageAsync(imageFile, subfolder))
                .ReturnsAsync(imagePath);
            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as ImageUploadResponse;
            response.Should().NotBeNull();
            response!.ImagePath.Should().Be(imagePath);
            response.FileName.Should().Be("test.jpg");
            response.Subfolder.Should().Be(subfolder);
            response.FileSize.Should().Be(1024);
            response.Message.Should().Be("Image uploaded successfully");
            _mockImageUploadUtility.Verify(x => x.SaveImageAsync(imageFile, subfolder), Times.Once);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenImageFileIsNull()
        {
            // Arrange
            IFormFile? imageFile = null;
            var subfolder = "profile_images";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile!, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InvalidFile");
            errorResponse.Message.Should().Be("No file was uploaded or file is empty");
            errorResponse.SupportedFormats.Should().Contain(".jpg");
            errorResponse.SupportedSubfolders.Should().BeEquivalentTo(supportedSubfolders);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenImageFileIsEmpty()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 0);
            var subfolder = "profile_images";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InvalidFile");
            errorResponse.Message.Should().Be("No file was uploaded or file is empty");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenSubfolderIsNull()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            string? subfolder = null;
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InvalidSubfolder");
            errorResponse.Message.Should().Be("Subfolder is required");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenSubfolderIsEmpty()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InvalidSubfolder");
            errorResponse.Message.Should().Be("Subfolder is required");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenSubfolderIsWhitespace()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "   ";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InvalidSubfolder");
            errorResponse.Message.Should().Be("Subfolder is required");
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenSubfolderIsNotSupported()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "invalid_folder";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.IsValidSubfolder(subfolder))
                .Returns(false);
            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("UnsupportedSubfolder");
            errorResponse.Message.Should().Be($"Subfolder '{subfolder}' is not supported");
            errorResponse.SupportedSubfolders.Should().BeEquivalentTo(supportedSubfolders);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "profile_images";
            var errorMessage = "Invalid file format";
            var supportedSubfolders = new List<string> { "profile_images", "project_images" };

            _mockImageUploadUtility.Setup(x => x.IsValidSubfolder(subfolder))
                .Returns(true);
            _mockImageUploadUtility.Setup(x => x.SaveImageAsync(imageFile, subfolder))
                .ThrowsAsync(new ArgumentException(errorMessage));
            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var errorResponse = badRequestResult!.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("ValidationFailed");
            errorResponse.Message.Should().Be(errorMessage);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var imageFile = CreateMockFormFile("test.jpg", 1024);
            var subfolder = "profile_images";

            _mockImageUploadUtility.Setup(x => x.IsValidSubfolder(subfolder))
                .Returns(true);
            _mockImageUploadUtility.Setup(x => x.SaveImageAsync(imageFile, subfolder))
                .ThrowsAsync(new Exception("File system error"));

            // Act
            var result = await _controller.UploadImage(imageFile, subfolder);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            var errorResponse = objectResult.Value as ImageUploadErrorResponse;
            errorResponse.Should().NotBeNull();
            errorResponse!.Error.Should().Be("InternalServerError");
            errorResponse.Message.Should().Be("An error occurred while uploading the image");
        }

        #endregion

        #region DeleteImage Tests

        [Fact]
        public void DeleteImage_ShouldReturnOkResult_WhenImageDeletedSuccessfully()
        {
            // Arrange
            var imagePath = "/uploads/profile_images/test.jpg";

            _mockImageUploadUtility.Setup(x => x.DeleteImage(imagePath))
                .Returns(true);

            // Act
            var result = _controller.DeleteImage(imagePath);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image deleted successfully");
            _mockImageUploadUtility.Verify(x => x.DeleteImage(imagePath), Times.Once);
        }

        [Fact]
        public void DeleteImage_ShouldReturnNotFound_WhenImageDoesNotExist()
        {
            // Arrange
            var imagePath = "/uploads/profile_images/nonexistent.jpg";

            _mockImageUploadUtility.Setup(x => x.DeleteImage(imagePath))
                .Returns(false);

            // Act
            var result = _controller.DeleteImage(imagePath);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image not found");
        }

        [Fact]
        public void DeleteImage_ShouldReturnBadRequest_WhenImagePathIsNull()
        {
            // Arrange
            string? imagePath = null;

            // Act
            var result = _controller.DeleteImage(imagePath!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image path is required");
        }

        [Fact]
        public void DeleteImage_ShouldReturnBadRequest_WhenImagePathIsEmpty()
        {
            // Arrange
            var imagePath = "";

            // Act
            var result = _controller.DeleteImage(imagePath);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image path is required");
        }

        [Fact]
        public void DeleteImage_ShouldReturnBadRequest_WhenImagePathIsWhitespace()
        {
            // Arrange
            var imagePath = "   ";

            // Act
            var result = _controller.DeleteImage(imagePath);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image path is required");
        }

        [Fact]
        public void DeleteImage_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var imagePath = "/uploads/profile_images/test.jpg";

            _mockImageUploadUtility.Setup(x => x.DeleteImage(imagePath))
                .Throws(new Exception("File system error"));

            // Act
            var result = _controller.DeleteImage(imagePath);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            var response = objectResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("An error occurred while deleting the image");
        }

        #endregion

        #region GetSupportedSubfolders Tests

        [Fact]
        public void GetSupportedSubfolders_ShouldReturnOkResult_WhenSubfoldersExist()
        {
            // Arrange
            var supportedSubfolders = new List<string> { "profile_images", "project_images", "blog_images" };

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = _controller.GetSupportedSubfolders();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var subfoldersProperty = response.GetType().GetProperty("subfolders");
            subfoldersProperty.Should().NotBeNull();
            var subfolders = subfoldersProperty!.GetValue(response);
            subfolders.Should().BeEquivalentTo(supportedSubfolders);
            _mockImageUploadUtility.Verify(x => x.GetSupportedSubfolders(), Times.Once);
        }

        [Fact]
        public void GetSupportedSubfolders_ShouldReturnOkResult_WhenNoSubfoldersExist()
        {
            // Arrange
            var supportedSubfolders = new List<string>();

            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Returns(supportedSubfolders);

            // Act
            var result = _controller.GetSupportedSubfolders();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var subfoldersProperty = response.GetType().GetProperty("subfolders");
            subfoldersProperty.Should().NotBeNull();
            var subfolders = subfoldersProperty!.GetValue(response) as IEnumerable<string>;
            subfolders.Should().NotBeNull();
            subfolders!.Should().BeEmpty();
        }

        [Fact]
        public void GetSupportedSubfolders_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockImageUploadUtility.Setup(x => x.GetSupportedSubfolders())
                .Throws(new Exception("Configuration error"));

            // Act
            var result = _controller.GetSupportedSubfolders();

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            var response = objectResult.Value;
            var messageProperty = response.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("An error occurred while getting supported subfolders");
        }

        #endregion

        #region Helper Methods

        private static IFormFile CreateMockFormFile(string fileName, long length)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            return mockFile.Object;
        }

        #endregion
    }
} 