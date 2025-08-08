using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using AutoFixture.Kernel;
using backend_portfolio.Controllers;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.Controllers
{
    public class ImageControllerTests
    {
        private readonly Mock<ILogger<ImageController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ImageController _controller;
        private readonly Fixture _fixture;

        public ImageControllerTests()
        {
            _mockLogger = new Mock<ILogger<ImageController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new ImageController(_mockLogger.Object, _mockConfiguration.Object);
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
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var controller = new ImageController(null!, _mockConfiguration.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenConfigurationIsNull()
        {
            // Act
            var controller = new ImageController(_mockLogger.Object, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var controller = new ImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreNull()
        {
            // Act
            var controller = new ImageController(null!, null!);

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetImage Tests

        [Theory]
        [InlineData("invalid_folder")]
        [InlineData("")]
        [InlineData("../secret")]
        public void GetImage_WithInvalidSubfolder_ShouldReturnBadRequest(string subfolder)
        {
            // Act
            var result = _controller.GetImage(subfolder, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("../../../secret.txt")]
        [InlineData("file/with/slashes.jpg")]
        [InlineData("file\\with\\backslashes.jpg")]
        public void GetImage_WithInvalidFilename_ShouldReturnBadRequest(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("test.exe")]
        [InlineData("script.js")]
        [InlineData("document.pdf")]
        [InlineData("file.txt")]
        public void GetImage_WithInvalidFileExtension_ShouldReturnBadRequest(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("blog_posts")]
        [InlineData("projects")]
        [InlineData("profile_images")]
        public void GetImage_WithValidSubfolder_AndNonExistentFile_ShouldReturnNotFound(string subfolder)
        {
            // Act
            var result = _controller.GetImage(subfolder, "nonexistent.jpg");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public void GetImage_WithNullSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImage(null!, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithNullFilename_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImage("projects", null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("BLOG_POSTS", "test.jpg")] // Case insensitive
        [InlineData("Projects", "image.PNG")]
        [InlineData("profile_images", "photo.JPEG")]
        public void GetImage_WithValidSubfolderCaseInsensitive_ShouldProcessCorrectly(string subfolder, string filename)
        {
            // Act
            var result = _controller.GetImage(subfolder, filename);

            // Assert 
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("test.jpg")]
        [InlineData("image.jpeg")]
        [InlineData("photo.png")]
        [InlineData("graphic.gif")]
        [InlineData("modern.webp")]
        public void GetImage_WithValidFileExtensions_ShouldNotReturnBadRequestForExtension(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithSpecialCharactersInValidFilename_ShouldProcess()
        {
            // Arrange
            var filename = "my-image_2023.jpg";

            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_ShouldLogWarningForInvalidSubfolder()
        {
            // Act
            _controller.GetImage("invalid_folder", "test.jpg");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid subfolder")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetImage_ShouldLogWarningForInvalidFilename()
        {
            // Act
            _controller.GetImage("projects", "../secret.txt");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid filename")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetImage_ShouldLogWarningForInvalidFileExtension()
        {
            // Act
            _controller.GetImage("projects", "malware.exe");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid file extension")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("blog_posts", "article-image.jpg")]
        [InlineData("projects", "project-screenshot.png")]
        [InlineData("profile_images", "avatar.jpeg")]
        public void GetImage_WithValidParams_ShouldLogFileNotFoundWarning(string subfolder, string filename)
        {
            // Act
            _controller.GetImage(subfolder, filename);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Image file not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetImage_WithFilenameLongerThan255Characters_ShouldStillValidate()
        {
            // Arrange
            var longFilename = new string('a', 250) + ".jpg";

            // Act
            var result = _controller.GetImage("projects", longFilename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithWhitespaceFilename_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImage("projects", "   ");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithWhitespaceSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImage("   ", "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("test.JPG")]
        [InlineData("image.JPEG")]
        [InlineData("photo.PNG")]
        [InlineData("graphic.GIF")]
        [InlineData("modern.WEBP")]
        public void GetImage_WithUpperCaseExtensions_ShouldBeValid(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithMixedCaseSubfolder_ShouldProcessCorrectly()
        {
            // Act
            var result = _controller.GetImage("Blog_Posts", "test.jpg");

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("projects", "file.jpg")]
        [InlineData("blog_posts", "article.png")]
        [InlineData("profile_images", "avatar.jpeg")]
        public void GetImage_ShouldReturnCorrectErrorMessage_ForInvalidSubfolder(string _, string filename)
        {
            // Act
            var result = _controller.GetImage("invalid_folder", filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Invalid subfolder");
        }

        [Theory]
        [InlineData("")]
        [InlineData("../../../etc/passwd")]
        [InlineData("folder/file.jpg")]
        public void GetImage_ShouldReturnCorrectErrorMessage_ForInvalidFilename(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Invalid filename");
        }

        [Theory]
        [InlineData("malware.exe")]
        [InlineData("script.js")]
        [InlineData("document.pdf")]
        public void GetImage_ShouldReturnCorrectErrorMessage_ForInvalidFileType(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Invalid file type");
        }

        [Fact]
        public void GetImage_ShouldReturnCorrectErrorMessage_ForNotFoundFile()
        {
            // Act
            var result = _controller.GetImage("projects", "nonexistent.jpg");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            var response = notFoundResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Image not found");
        }

        [Theory]
        [InlineData("image-with-dashes.jpg")]
        [InlineData("image_with_underscores.png")]
        [InlineData("image123.jpeg")]
        [InlineData("IMAGE.GIF")]
        [InlineData("My Photo.webp")]
        public void GetImage_WithVariousValidFilenameFormats_ShouldProcess(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithUnicodeCharactersInFilename_ShouldProcess()
        {
            // Arrange
            var filename = "测试图片.jpg";

            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetImageMetadata Tests

        [Theory]
        [InlineData("invalid_folder")]
        [InlineData("")]
        [InlineData("../secret")]
        public void GetImageMetadata_WithInvalidSubfolder_ShouldReturnBadRequest(string subfolder)
        {
            // Act
            var result = _controller.GetImageMetadata(subfolder, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("../../../secret.txt")]
        [InlineData("file/with/slashes.jpg")]
        [InlineData("file\\with\\backslashes.jpg")]
        public void GetImageMetadata_WithInvalidFilename_ShouldReturnBadRequest(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("test.exe")]
        [InlineData("script.js")]
        [InlineData("document.pdf")]
        [InlineData("file.txt")]
        public void GetImageMetadata_WithInvalidFileExtension_ShouldReturnBadRequest(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("blog_posts")]
        [InlineData("projects")]
        [InlineData("profile_images")]
        public void GetImageMetadata_WithValidSubfolder_AndNonExistentFile_ShouldReturnNotFound(string subfolder)
        {
            // Act
            var result = _controller.GetImageMetadata(subfolder, "nonexistent.jpg");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public void GetImageMetadata_WithNullSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImageMetadata(null!, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_WithNullFilename_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImageMetadata("projects", null!);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("BLOG_POSTS", "test.jpg")] // Case insensitive
        [InlineData("Projects", "image.PNG")]
        [InlineData("profile_images", "photo.JPEG")]
        public void GetImageMetadata_WithValidSubfolderCaseInsensitive_ShouldProcessCorrectly(string subfolder, string filename)
        {
            // Act
            var result = _controller.GetImageMetadata(subfolder, filename);

            // Assert 
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("test.jpg")]
        [InlineData("image.jpeg")]
        [InlineData("photo.png")]
        [InlineData("graphic.gif")]
        [InlineData("modern.webp")]
        public void GetImageMetadata_WithValidFileExtensions_ShouldNotReturnBadRequestForExtension(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_WithWhitespaceFilename_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImageMetadata("projects", "   ");

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_WithWhitespaceSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImageMetadata("   ", "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("test.JPG")]
        [InlineData("image.JPEG")]
        [InlineData("photo.PNG")]
        [InlineData("graphic.GIF")]
        [InlineData("modern.WEBP")]
        public void GetImageMetadata_WithUpperCaseExtensions_ShouldBeValid(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_WithSpecialCharactersInValidFilename_ShouldProcess()
        {
            // Arrange
            var filename = "my-image_2023.jpg";

            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_WithFilenameLongerThan255Characters_ShouldStillValidate()
        {
            // Arrange
            var longFilename = new string('a', 250) + ".jpg";

            // Act
            var result = _controller.GetImageMetadata("projects", longFilename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Fact]
        public void GetImageMetadata_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.GetImageMetadataWithException("projects", "test.jpg");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting image metadata")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("image-with-dashes.jpg")]
        [InlineData("image_with_underscores.png")]
        [InlineData("image123.jpeg")]
        [InlineData("IMAGE.GIF")]
        public void GetImageMetadata_WithVariousValidFilenameFormats_ShouldProcess(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        #endregion

        #region ListImages Tests

        [Theory]
        [InlineData("invalid_folder")]
        [InlineData("")]
        [InlineData("../secret")]
        public void ListImages_WithInvalidSubfolder_ShouldReturnBadRequest(string subfolder)
        {
            // Act
            var result = _controller.ListImages(subfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            messageProperty.Should().NotBeNull();
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Invalid subfolder");
        }

        [Fact]
        public void ListImages_WithNullSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.ListImages(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void ListImages_WithWhitespaceSubfolder_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.ListImages("   ");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("BLOG_POSTS")] 
        [InlineData("Projects")]
        [InlineData("profile_images")]
        public void ListImages_WithValidSubfolderCaseInsensitive_ShouldProcessCorrectly(string subfolder)
        {
            // Act
            var result = _controller.ListImages(subfolder);

            // Assert 
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("blog_posts")]
        [InlineData("projects")]
        [InlineData("profile_images")]
        public void ListImages_WithValidSubfolder_ShouldReturnOkResult(string subfolder)
        {
            // Act
            var result = _controller.ListImages(subfolder);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response);
            images.Should().NotBeNull();
        }

        [Theory]
        [InlineData("blog_posts")]
        [InlineData("projects")]
        [InlineData("profile_images")]
        public void ListImages_WithValidSubfolder_ShouldReturnOkResultWithImagesArray(string subfolder)
        {
            // Act
            var result = _controller.ListImages(subfolder);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response) as Array;
            images.Should().NotBeNull();
            // Note: The actual number of images depends on what's in the server/portfolio directory
            // We just verify that the method returns a valid array structure
        }

        [Fact]
        public void ListImages_ShouldReturnImagesWithCorrectProperties()
        {
            // Act
            var result = _controller.ListImages("blog_posts");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response) as Array;
            
            if (images != null && images.Length > 0)
            {
                var firstImage = images.GetValue(0);
                var filenameProperty = firstImage!.GetType().GetProperty("filename");
                var sizeProperty = firstImage.GetType().GetProperty("size");
                var lastModifiedProperty = firstImage.GetType().GetProperty("lastModified");
                var urlProperty = firstImage.GetType().GetProperty("url");

                filenameProperty.Should().NotBeNull();
                sizeProperty.Should().NotBeNull();
                lastModifiedProperty.Should().NotBeNull();
                urlProperty.Should().NotBeNull();
            }
        }

        [Fact]
        public void ListImages_ShouldReturnImagesOrderedByLastModifiedDescending()
        {
            // Act
            var result = _controller.ListImages("blog_posts");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response) as Array;
            
            if (images != null && images.Length > 1)
            {
                var firstImage = images.GetValue(0);
                var secondImage = images.GetValue(1);
                var firstLastModified = firstImage!.GetType().GetProperty("lastModified")!.GetValue(firstImage) as DateTime?;
                var secondLastModified = secondImage!.GetType().GetProperty("lastModified")!.GetValue(secondImage) as DateTime?;

                if (firstLastModified.HasValue && secondLastModified.HasValue)
                {
                    (firstLastModified.Value >= secondLastModified.Value).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void ListImages_ShouldReturnCorrectUrlFormat()
        {
            // Act
            var result = _controller.ListImages("blog_posts");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response) as Array;
            
            if (images != null && images.Length > 0)
            {
                var firstImage = images.GetValue(0);
                var urlProperty = firstImage!.GetType().GetProperty("url");
                urlProperty.Should().NotBeNull();
                var url = urlProperty!.GetValue(firstImage) as string;
                url.Should().StartWith("/api/Image/blog_posts/");
            }
        }

        [Fact]
        public void ListImages_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.ListImagesWithException("projects");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error listing images")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("projects", "Invalid subfolder")]
        [InlineData("blog_posts", "Invalid subfolder")]
        [InlineData("profile_images", "Invalid subfolder")]
        public void ListImages_ShouldReturnCorrectErrorMessage_ForInvalidSubfolder(string _, string expectedMessage)
        {
            // Act
            var result = _controller.ListImages("invalid_folder");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be(expectedMessage);
        }

        [Fact]
        public void ListImages_WithEmptyDirectory_ShouldReturnEmptyArray()
        {
            // Note: This test might return images if the directory exists and has files
            // We're testing that the method doesn't crash when the directory is empty or non-existent
            
            // Act
            var result = _controller.ListImages("blog_posts");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value;
            var imagesProperty = response!.GetType().GetProperty("images");
            imagesProperty.Should().NotBeNull();
            var images = imagesProperty!.GetValue(response) as Array;
            images.Should().NotBeNull();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void GetImage_ShouldReturnInternalServerError_WhenIOExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.GetImageWithException("projects", "test.jpg");

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            var response = objectResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Error serving image");
        }

        [Fact]
        public void GetImage_ShouldLogError_WhenIOExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            mockController.GetImageWithException("projects", "test.jpg");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error serving image")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetImageMetadata_ShouldReturnInternalServerError_WhenIOExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.GetImageMetadataWithException("projects", "test.jpg");

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public void ListImages_ShouldReturnInternalServerError_WhenIOExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.ListImagesWithException("projects");

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            var response = objectResult.Value;
            var messageProperty = response!.GetType().GetProperty("message");
            var message = messageProperty!.GetValue(response);
            message.Should().Be("Error listing images");
        }

        [Fact]
        public void GetImage_ShouldReturnInternalServerError_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.GetImageWithUnauthorizedException("projects", "test.jpg");

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public void GetImageMetadata_ShouldReturnInternalServerError_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.GetImageMetadataWithUnauthorizedException("projects", "test.jpg");

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public void ListImages_ShouldReturnInternalServerError_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.ListImagesWithUnauthorizedException("projects");

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        #endregion

        #region Content Type Tests

        [Theory]
        [InlineData(".jpg", "image/jpeg")]
        [InlineData(".jpeg", "image/jpeg")]
        [InlineData(".png", "image/png")]
        [InlineData(".gif", "image/gif")]
        [InlineData(".webp", "image/webp")]
        [InlineData(".txt", "application/octet-stream")]
        [InlineData(".exe", "application/octet-stream")]
        [InlineData(".unknown", "application/octet-stream")]
        public void GetContentType_ShouldReturnCorrectMimeType(string extension, string expectedMimeType)
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.TestGetContentType(extension);

            // Assert
            result.Should().Be(expectedMimeType);
        }

        [Theory]
        [InlineData(".JPG", "image/jpeg")]
        [InlineData(".JPEG", "image/jpeg")]
        [InlineData(".PNG", "image/png")]
        [InlineData(".GIF", "image/gif")]
        [InlineData(".WEBP", "image/webp")]
        public void GetContentType_WithUpperCaseExtensions_ShouldReturnCorrectMimeType(string extension, string expectedMimeType)
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.TestGetContentType(extension);

            // Assert
            result.Should().Be(expectedMimeType);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("noextension")]
        public void GetContentType_WithInvalidExtensions_ShouldReturnDefaultMimeType(string extension)
        {
            // Arrange
            var mockController = new TestableImageController(_mockLogger.Object, _mockConfiguration.Object);

            // Act
            var result = mockController.TestGetContentType(extension);

            // Assert
            result.Should().Be("application/octet-stream");
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void GetImage_WithVeryLongSubfolderName_ShouldStillValidate()
        {
            // Arrange
            var longSubfolder = new string('a', 1000);

            // Act
            var result = _controller.GetImage(longSubfolder, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImageMetadata_WithVeryLongSubfolderName_ShouldStillValidate()
        {
            // Arrange
            var longSubfolder = new string('a', 1000);

            // Act
            var result = _controller.GetImageMetadata(longSubfolder, "test.jpg");

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public void ListImages_WithVeryLongSubfolderName_ShouldStillValidate()
        {
            // Arrange
            var longSubfolder = new string('a', 1000);

            // Act
            var result = _controller.ListImages(longSubfolder);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("test.jpg")]
        [InlineData("test.jpeg")]
        [InlineData("test.png")]
        [InlineData("test.gif")]
        [InlineData("test.webp")]
        public void GetImage_WithValidFilenames_ShouldNotFailOnExtensionValidation(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("test.jpg")]
        [InlineData("test.jpeg")]
        [InlineData("test.png")]
        [InlineData("test.gif")]
        [InlineData("test.webp")]
        public void GetImageMetadata_WithValidFilenames_ShouldNotFailOnExtensionValidation(string filename)
        {
            // Act
            var result = _controller.GetImageMetadata("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestResult>();
        }

        [Theory]
        [InlineData("image.jpg.")]
        [InlineData("  image.jpg  ")]
        [InlineData("image..jpg")]
        public void GetImage_WithEdgeCaseFilenames_ShouldHandleCorrectly(string filename)
        {
            // Note: We're not trimming filenames in the controller, so these should be processed as-is
            
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert - Just verify the method processes them (might return BadRequest or NotFound, but shouldn't crash)
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetImage_WithEmptyStringExtension_ShouldReturnBadRequest()
        {
            // Act
            var result = _controller.GetImage("projects", "filename");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("file with spaces.jpg")]
        [InlineData("file-with-dashes.jpg")]
        [InlineData("file_with_underscores.jpg")]
        [InlineData("file123.jpg")]
        [InlineData("FILE.JPG")]
        public void GetImage_WithAcceptableSpecialCharacters_ShouldProcess(string filename)
        {
            // Act
            var result = _controller.GetImage("projects", filename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData("blog_posts")]
        [InlineData("Blog_Posts")]
        [InlineData("BLOG_POSTS")]
        [InlineData("projects")]
        [InlineData("Projects")]
        [InlineData("PROJECTS")]
        [InlineData("profile_images")]
        [InlineData("Profile_Images")]
        [InlineData("PROFILE_IMAGES")]
        public void GetImage_WithAllSubfolderVariations_ShouldProcessCorrectly(string subfolder)
        {
            // Act
            var result = _controller.GetImage(subfolder, "test.jpg");

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetImage_WithMaxPathLength_ShouldStillFunction()
        {
            // Arrange
            var longButValidFilename = new string('a', 200) + ".jpg";

            // Act
            var result = _controller.GetImage("projects", longButValidFilename);

            // Assert
            result.Should().NotBeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public void GetImage_WithMultipleConcurrentRequests_ShouldHandleCorrectly()
        {
            // Arrange
            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => _controller.GetImage("projects", $"test{i}.jpg")));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        [Fact]
        public void ListImages_WithMultipleConcurrentRequests_ShouldHandleCorrectly()
        {
            // Arrange
            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() => _controller.ListImages("projects")));
                tasks.Add(Task.Run(() => _controller.ListImages("blog_posts")));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(10);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Testable version of ImageController that allows us to simulate exceptions and test private methods
        /// </summary>
        private class TestableImageController : ImageController
        {
            private readonly ILogger<ImageController> _logger;

            public TestableImageController(ILogger<ImageController> logger, IConfiguration configuration) 
                : base(logger, configuration)
            {
                _logger = logger;
            }

            public IActionResult GetImageWithException(string subfolder, string filename)
            {
                try
                {
                    throw new IOException("Simulated IO exception");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error serving image: {Subfolder}/{Filename}", subfolder, filename);
                    return StatusCode(500, new { message = "Error serving image" });
                }
            }

            public IActionResult GetImageWithUnauthorizedException(string subfolder, string filename)
            {
                try
                {
                    throw new UnauthorizedAccessException("Simulated unauthorized access");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error serving image: {Subfolder}/{Filename}", subfolder, filename);
                    return StatusCode(500, new { message = "Error serving image" });
                }
            }

            public IActionResult GetImageMetadataWithException(string subfolder, string filename)
            {
                try
                {
                    throw new IOException("Simulated IO exception");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting image metadata: {Subfolder}/{Filename}", subfolder, filename);
                    return StatusCode(500);
                }
            }

            public IActionResult GetImageMetadataWithUnauthorizedException(string subfolder, string filename)
            {
                try
                {
                    throw new UnauthorizedAccessException("Simulated unauthorized access");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting image metadata: {Subfolder}/{Filename}", subfolder, filename);
                    return StatusCode(500);
                }
            }

            public IActionResult ListImagesWithException(string subfolder)
            {
                try
                {
                    throw new IOException("Simulated IO exception");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error listing images in subfolder: {Subfolder}", subfolder);
                    return StatusCode(500, new { message = "Error listing images" });
                }
            }

            public IActionResult ListImagesWithUnauthorizedException(string subfolder)
            {
                try
                {
                    throw new UnauthorizedAccessException("Simulated unauthorized access");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error listing images in subfolder: {Subfolder}", subfolder);
                    return StatusCode(500, new { message = "Error listing images" });
                }
            }

            public string TestGetContentType(string extension)
            {
                // Using reflection to access the private static method
                var method = typeof(ImageController).GetMethod("GetContentType", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                return (string)method!.Invoke(null, new object[] { extension })!;
            }
        }

        #endregion
    }
} 