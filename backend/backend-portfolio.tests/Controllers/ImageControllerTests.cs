using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Controllers;

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
            _fixture = new Fixture();
        }

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

            // Assert - Should not return BadRequest for invalid subfolder
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

            // Assert - Should not return BadRequest for file extension
            result.Should().NotBeOfType<BadRequestObjectResult>();
            // Note: May still return NotFound if file doesn't exist, but that's expected
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
        public void GetImage_WithValidParams_ShouldNotLogWarnings(string subfolder, string filename)
        {
            // Act
            _controller.GetImage(subfolder, filename);

            // Assert - Should not log warnings for valid parameters
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
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
    }
} 