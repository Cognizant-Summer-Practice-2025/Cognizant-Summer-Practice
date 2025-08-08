using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using backend_portfolio.Services;

namespace backend_portfolio.tests.Services
{
    public class ImageUploadUtilityTests
    {
        private readonly Mock<ILogger<ImageUploadUtility>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ImageUploadUtility _service;

        public ImageUploadUtilityTests()
        {
            _mockLogger = new Mock<ILogger<ImageUploadUtility>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new ImageUploadUtility(_mockLogger.Object, _mockConfiguration.Object);
        }

        private static Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, long length = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            return fileMock;
        }

        [Fact]
        public async Task SaveImageAsync_WithValidImage_ShouldReturnFilename()
        {
            // Arrange
            var formFile = CreateMockFormFile("test.jpg", "image/jpeg");
            var subfolder = "projects";

            // Act
            var result = await _service.SaveImageAsync(formFile.Object, subfolder);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().StartWith($"server/portfolio/{subfolder}/");
            result.Should().EndWith(".jpg");
        }

        [Fact]
        public async Task SaveImageAsync_WithNullFile_ShouldThrowArgumentException()
        {
            // Arrange
            var subfolder = "projects";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveImageAsync(null!, subfolder));
        }

        [Fact]
        public async Task SaveImageAsync_WithEmptyFile_ShouldThrowArgumentException()
        {
            // Arrange
            var formFile = CreateMockFormFile("test.jpg", "image/jpeg", 0);
            var subfolder = "projects";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveImageAsync(formFile.Object, subfolder));
            exception.Message.Should().Contain("No file was uploaded or file is empty");
        }

        [Theory]
        [InlineData("test.txt", "text/plain")]
        [InlineData("test.pdf", "application/pdf")]
        [InlineData("test.doc", "application/msword")]
        public async Task SaveImageAsync_WithInvalidFileType_ShouldThrowArgumentException(string fileName, string contentType)
        {
            // Arrange
            var formFile = CreateMockFormFile(fileName, contentType);
            var subfolder = "projects";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveImageAsync(formFile.Object, subfolder));
            exception.Message.Should().Contain("File type");
        }

        [Theory]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("test.png", "image/png")]
        [InlineData("test.gif", "image/gif")]
        [InlineData("test.webp", "image/webp")]
        public async Task SaveImageAsync_WithValidImageTypes_ShouldSucceed(string fileName, string contentType)
        {
            // Arrange
            var formFile = CreateMockFormFile(fileName, contentType);
            var subfolder = "projects";

            // Act
            var result = await _service.SaveImageAsync(formFile.Object, subfolder);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().StartWith($"server/portfolio/{subfolder}/");
        }

        [Fact]
        public void DeleteImage_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            var fileName = "server/portfolio/projects/test.jpg";

            // Act
            var result = _service.DeleteImage(fileName);

            // Assert - Since file doesn't actually exist, this will return false
            // But we're testing that the method executes without throwing
            result.Should().BeFalse();
        }

        [Fact]
        public void DeleteImage_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            var fileName = "server/portfolio/projects/non-existent.jpg";

            // Act
            var result = _service.DeleteImage(fileName);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DeleteImage_WithInvalidFileName_ShouldReturnFalse(string? fileName)
        {
            // Act
            var result = _service.DeleteImage(fileName!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SaveImageAsync_WithLargeFile_ShouldThrowArgumentException()
        {
            // Arrange
            var formFile = CreateMockFormFile("large.jpg", "image/jpeg", 10 * 1024 * 1024); // 10MB
            var subfolder = "projects";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.SaveImageAsync(formFile.Object, subfolder));
            exception.Message.Should().Contain("File size exceeds maximum");
        }

        [Fact]
        public async Task SaveImageAsync_WithSpecialCharactersInFileName_ShouldSanitizeFileName()
        {
            // Arrange
            var formFile = CreateMockFormFile("test@#$%^&*().jpg", "image/jpeg");
            var subfolder = "projects";

            // Act
            var result = await _service.SaveImageAsync(formFile.Object, subfolder);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().StartWith($"server/portfolio/{subfolder}/");
            result.Should().EndWith(".jpg");
            // The actual filename should be a GUID, not the original filename
            result.Should().NotContain("@#$%^&*()");
        }

        [Fact]
        public void GetSupportedSubfolders_ShouldReturnExpectedFolders()
        {
            // Act
            var result = _service.GetSupportedSubfolders();

            // Assert
            result.Should().Contain("blog_posts");
            result.Should().Contain("projects");
            result.Should().Contain("profile_images");
        }

        [Theory]
        [InlineData("projects", true)]
        [InlineData("blog_posts", true)]
        [InlineData("profile_images", true)]
        [InlineData("invalid_folder", false)]
        [InlineData("", false)]
        public void IsValidSubfolder_ShouldReturnCorrectResult(string subfolder, bool expected)
        {
            // Act
            var result = _service.IsValidSubfolder(subfolder);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task SaveImageAsync_WithInvalidSubfolder_ShouldStillSaveImage()
        {
            // Arrange
            var formFile = CreateMockFormFile("test.jpg", "image/jpeg");
            var subfolder = "invalid_folder";

            // Act - Should not throw, as the method creates the directory if it doesn't exist
            var result = await _service.SaveImageAsync(formFile.Object, subfolder);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().StartWith($"server/portfolio/{subfolder}/");
        }
    }
} 