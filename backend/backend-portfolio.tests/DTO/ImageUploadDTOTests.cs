using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.DTO.ImageUpload.Response;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Threading;

namespace backend_portfolio.tests.DTO
{
    public class ImageUploadDTOTests
    {
        private readonly Fixture _fixture;

        public ImageUploadDTOTests()
        {
            _fixture = new Fixture();
        }

        #region ImageUploadRequest Tests

        [Fact]
        public void ImageUploadRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new ImageUploadRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.Subfolder.Should().NotBeNull();
        }

        [Fact]
        public void ImageUploadRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);
            var subfolder = "projects";

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = subfolder
            };

            // Assert
            dto.ImageFile.Should().Be(mockFile);
            dto.Subfolder.Should().Be(subfolder);
        }

        [Theory]
        [InlineData("projects")]
        [InlineData("blog_posts")]
        [InlineData("profile_images")]
        public void ImageUploadRequest_ShouldHandleValidSubfolders(string subfolder)
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = subfolder
            };

            // Assert
            dto.Subfolder.Should().Be(subfolder);
        }

        [Fact]
        public void ImageUploadRequest_ShouldHandleEmptySubfolder()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = ""
            };

            // Assert
            dto.Subfolder.Should().Be("");
        }

        [Fact]
        public void ImageUploadRequest_ShouldHandleNullImageFile()
        {
            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = null!,
                Subfolder = "projects"
            };

            // Assert
            dto.ImageFile.Should().BeNull();
        }

        [Theory]
        [InlineData("test.jpg", "image/jpeg")]
        [InlineData("image.png", "image/png")]
        [InlineData("photo.gif", "image/gif")]
        [InlineData("picture.webp", "image/webp")]
        public void ImageUploadRequest_ShouldHandleVariousImageTypes(string fileName, string contentType)
        {
            // Arrange
            var mockFile = CreateMockFormFile(fileName, contentType, 2048);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = "projects"
            };

            // Assert
            dto.ImageFile.FileName.Should().Be(fileName);
            dto.ImageFile.ContentType.Should().Be(contentType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)] // 1MB
        [InlineData(5 * 1024 * 1024)] // 5MB
        public void ImageUploadRequest_ShouldHandleVariousFileSizes(long fileSize)
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", fileSize);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = "projects"
            };

            // Assert
            dto.ImageFile.Length.Should().Be(fileSize);
        }

        [Fact]
        public void ImageUploadRequest_ShouldHandleSpecialCharactersInSubfolder()
        {
            // Arrange
            var specialSubfolder = "projects_2024-test";
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = specialSubfolder
            };

            // Assert
            dto.Subfolder.Should().Be(specialSubfolder);
        }

        [Fact]
        public void ImageUploadRequest_ShouldHandleLongSubfolderName()
        {
            // Arrange
            var longSubfolder = new string('a', 100); // Longer than 50 characters
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

            // Act
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = longSubfolder
            };

            // Assert
            dto.Subfolder.Should().Be(longSubfolder);
        }

        [Fact]
        public void ImageUploadRequest_Validation_ShouldFailForNullImageFile()
        {
            // Arrange
            var dto = new ImageUploadRequest
            {
                ImageFile = null!,
                Subfolder = "projects"
            };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("ImageFile"));
        }

        [Fact]
        public void ImageUploadRequest_Validation_ShouldFailForEmptySubfolder()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = ""
            };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Subfolder"));
        }

        [Fact]
        public void ImageUploadRequest_Validation_ShouldFailForLongSubfolder()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);
            var longSubfolder = new string('a', 51); // Longer than 50 characters
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = longSubfolder
            };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.MemberNames.Contains("Subfolder"));
        }

        [Fact]
        public void ImageUploadRequest_Validation_ShouldPassForValidData()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);
            var dto = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = "projects"
            };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }

        #endregion

        #region ImageUploadResponse Tests

        [Fact]
        public void ImageUploadResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ImageUploadResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.ImagePath.Should().NotBeNull();
            dto.FileName.Should().NotBeNull();
            dto.Subfolder.Should().NotBeNull();
            dto.FileSize.Should().Be(0);
            dto.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            dto.Message.Should().Be("Image uploaded successfully");
        }

        [Fact]
        public void ImageUploadResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var imagePath = "/uploads/projects/test-image.jpg";
            var fileName = "test-image.jpg";
            var subfolder = "projects";
            var fileSize = 2048L;
            var uploadedAt = DateTime.UtcNow.AddMinutes(-5);
            var message = "Custom upload success message";

            // Act
            var dto = new ImageUploadResponse
            {
                ImagePath = imagePath,
                FileName = fileName,
                Subfolder = subfolder,
                FileSize = fileSize,
                UploadedAt = uploadedAt,
                Message = message
            };

            // Assert
            dto.ImagePath.Should().Be(imagePath);
            dto.FileName.Should().Be(fileName);
            dto.Subfolder.Should().Be(subfolder);
            dto.FileSize.Should().Be(fileSize);
            dto.UploadedAt.Should().Be(uploadedAt);
            dto.Message.Should().Be(message);
        }

        [Fact]
        public void ImageUploadResponse_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new ImageUploadResponse
            {
                ImagePath = "",
                FileName = "",
                Subfolder = "",
                Message = ""
            };

            // Assert
            dto.ImagePath.Should().Be("");
            dto.FileName.Should().Be("");
            dto.Subfolder.Should().Be("");
            dto.Message.Should().Be("");
        }

        [Theory]
        [InlineData("/uploads/projects/image.jpg")]
        [InlineData("/uploads/blog_posts/article-image.png")]
        [InlineData("/uploads/profile_images/avatar.gif")]
        [InlineData("https://cdn.example.com/uploads/image.webp")]
        public void ImageUploadResponse_ShouldHandleVariousImagePaths(string imagePath)
        {
            // Act
            var dto = new ImageUploadResponse { ImagePath = imagePath };

            // Assert
            dto.ImagePath.Should().Be(imagePath);
        }

        [Theory]
        [InlineData("simple.jpg")]
        [InlineData("image-with-dashes.png")]
        [InlineData("image_with_underscores.gif")]
        [InlineData("Image With Spaces.webp")]
        [InlineData("测试图片.jpg")]
        public void ImageUploadResponse_ShouldHandleVariousFileNames(string fileName)
        {
            // Act
            var dto = new ImageUploadResponse { FileName = fileName };

            // Assert
            dto.FileName.Should().Be(fileName);
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(1024L)]
        [InlineData(1024 * 1024L)] // 1MB
        [InlineData(long.MaxValue)]
        public void ImageUploadResponse_ShouldHandleVariousFileSizes(long fileSize)
        {
            // Act
            var dto = new ImageUploadResponse { FileSize = fileSize };

            // Assert
            dto.FileSize.Should().Be(fileSize);
        }

        [Fact]
        public void ImageUploadResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new ImageUploadResponse { UploadedAt = minDate };
            var dto2 = new ImageUploadResponse { UploadedAt = maxDate };
            var dto3 = new ImageUploadResponse { UploadedAt = utcNow };
            var dto4 = new ImageUploadResponse { UploadedAt = localNow };

            // Assert
            dto1.UploadedAt.Should().Be(minDate);
            dto2.UploadedAt.Should().Be(maxDate);
            dto3.UploadedAt.Should().Be(utcNow);
            dto4.UploadedAt.Should().Be(localNow);
        }

        [Theory]
        [InlineData("Image uploaded successfully")]
        [InlineData("Upload completed")]
        [InlineData("File saved to server")]
        [InlineData("Success: Image processed and stored")]
        public void ImageUploadResponse_ShouldHandleVariousMessages(string message)
        {
            // Act
            var dto = new ImageUploadResponse { Message = message };

            // Assert
            dto.Message.Should().Be(message);
        }

        [Fact]
        public void ImageUploadResponse_ShouldHandleUnicodeInFields()
        {
            // Arrange
            var unicodeFileName = "测试图片_テスト画像.jpg";
            var unicodeMessage = "上传成功 アップロード成功";

            // Act
            var dto = new ImageUploadResponse
            {
                FileName = unicodeFileName,
                Message = unicodeMessage
            };

            // Assert
            dto.FileName.Should().Be(unicodeFileName);
            dto.Message.Should().Be(unicodeMessage);
        }

        #endregion

        #region ImageUploadErrorResponse Tests

        [Fact]
        public void ImageUploadErrorResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ImageUploadErrorResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Error.Should().NotBeNull();
            dto.Message.Should().NotBeNull();
            dto.SupportedFormats.Should().NotBeNull().And.BeEmpty();
            dto.SupportedSubfolders.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var error = "INVALID_FILE_TYPE";
            var message = "The uploaded file type is not supported";
            var supportedFormats = new List<string> { "jpg", "png", "gif", "webp" };
            var supportedSubfolders = new List<string> { "projects", "blog_posts", "profile_images" };

            // Act
            var dto = new ImageUploadErrorResponse
            {
                Error = error,
                Message = message,
                SupportedFormats = supportedFormats,
                SupportedSubfolders = supportedSubfolders
            };

            // Assert
            dto.Error.Should().Be(error);
            dto.Message.Should().Be(message);
            dto.SupportedFormats.Should().BeEquivalentTo(supportedFormats);
            dto.SupportedSubfolders.Should().BeEquivalentTo(supportedSubfolders);
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new ImageUploadErrorResponse
            {
                Error = "",
                Message = ""
            };

            // Assert
            dto.Error.Should().Be("");
            dto.Message.Should().Be("");
        }

        [Theory]
        [InlineData("INVALID_FILE_TYPE")]
        [InlineData("FILE_TOO_LARGE")]
        [InlineData("INVALID_SUBFOLDER")]
        [InlineData("UPLOAD_FAILED")]
        [InlineData("INSUFFICIENT_PERMISSIONS")]
        public void ImageUploadErrorResponse_ShouldHandleCommonErrorCodes(string errorCode)
        {
            // Act
            var dto = new ImageUploadErrorResponse { Error = errorCode };

            // Assert
            dto.Error.Should().Be(errorCode);
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleDetailedErrorMessages()
        {
            // Arrange
            var detailedMessage = @"Upload failed due to multiple issues:
1. File size exceeds 5MB limit (current: 8.2MB)
2. File type 'bmp' is not supported
3. Subfolder 'invalid_folder' does not exist

Please check the supported formats and try again.";

            // Act
            var dto = new ImageUploadErrorResponse { Message = detailedMessage };

            // Assert
            dto.Message.Should().Be(detailedMessage);
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleEmptyCollections()
        {
            // Act
            var dto = new ImageUploadErrorResponse
            {
                SupportedFormats = new List<string>(),
                SupportedSubfolders = new List<string>()
            };

            // Assert
            dto.SupportedFormats.Should().NotBeNull().And.BeEmpty();
            dto.SupportedSubfolders.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleLargeCollections()
        {
            // Arrange
            var formats = _fixture.CreateMany<string>(100).ToList();
            var subfolders = _fixture.CreateMany<string>(50).ToList();

            // Act
            var dto = new ImageUploadErrorResponse
            {
                SupportedFormats = formats,
                SupportedSubfolders = subfolders
            };

            // Assert
            dto.SupportedFormats.Should().HaveCount(100);
            dto.SupportedSubfolders.Should().HaveCount(50);
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleCommonSupportedFormats()
        {
            // Arrange
            var commonFormats = new List<string> { "jpg", "jpeg", "png", "gif", "webp", "svg", "bmp", "tiff" };

            // Act
            var dto = new ImageUploadErrorResponse { SupportedFormats = commonFormats };

            // Assert
            dto.SupportedFormats.Should().BeEquivalentTo(commonFormats);
        }

        [Fact]
        public void ImageUploadErrorResponse_ShouldHandleCommonSupportedSubfolders()
        {
            // Arrange
            var commonSubfolders = new List<string> 
            { 
                "projects", "blog_posts", "profile_images", "gallery", 
                "thumbnails", "temp", "assets", "uploads" 
            };

            // Act
            var dto = new ImageUploadErrorResponse { SupportedSubfolders = commonSubfolders };

            // Assert
            dto.SupportedSubfolders.Should().BeEquivalentTo(commonSubfolders);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllImageUploadDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

            // Act
            var request = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = whitespace
            };

            var response = new ImageUploadResponse
            {
                ImagePath = whitespace,
                FileName = whitespace,
                Subfolder = whitespace,
                Message = whitespace
            };

            var errorResponse = new ImageUploadErrorResponse
            {
                Error = whitespace,
                Message = whitespace
            };

            // Assert
            request.Subfolder.Should().Be(whitespace);
            response.ImagePath.Should().Be(whitespace);
            response.FileName.Should().Be(whitespace);
            response.Subfolder.Should().Be(whitespace);
            response.Message.Should().Be(whitespace);
            errorResponse.Error.Should().Be(whitespace);
            errorResponse.Message.Should().Be(whitespace);
        }

        [Fact]
        public void ImageUploadDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var request = new ImageUploadRequest();
            var response = new ImageUploadResponse();
            var errorResponse = new ImageUploadErrorResponse();

            // Act & Assert - Should not throw
            var requestProperties = request.GetType().GetProperties();
            var responseProperties = response.GetType().GetProperties();
            var errorResponseProperties = errorResponse.GetType().GetProperties();

            requestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            responseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            errorResponseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
        }

        [Fact]
        public void ImageUploadDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var responses = new List<ImageUploadResponse>();
            for (int i = 0; i < 1000; i++)
            {
                responses.Add(new ImageUploadResponse
                {
                    ImagePath = $"/uploads/test/image-{i}.jpg",
                    FileName = $"image-{i}.jpg",
                    Subfolder = "test",
                    FileSize = i * 1024,
                    UploadedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }

            // Assert - Should not throw OutOfMemoryException
            responses.Should().HaveCount(1000);
        }

        [Fact]
        public void ImageUploadRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<ImageUploadRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() =>
                {
                    var mockFile = CreateMockFormFile($"test-{index}.jpg", "image/jpeg", 1024);
                    return new ImageUploadRequest
                    {
                        ImageFile = mockFile,
                        Subfolder = $"test-{index}"
                    };
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void ImageUploadDTOs_ShouldHandleUploadWorkflow()
        {
            // This test simulates a typical image upload workflow
            
            // Arrange - Create upload request
            var mockFile = CreateMockFormFile("portfolio-image.jpg", "image/jpeg", 2048);
            var request = new ImageUploadRequest
            {
                ImageFile = mockFile,
                Subfolder = "projects"
            };

            // Act - Simulate successful upload
            var successResponse = new ImageUploadResponse
            {
                ImagePath = "/uploads/projects/portfolio-image.jpg",
                FileName = "portfolio-image.jpg",
                Subfolder = request.Subfolder,
                FileSize = mockFile.Length,
                UploadedAt = DateTime.UtcNow,
                Message = "Image uploaded successfully"
            };

            // Simulate failed upload
            var errorResponse = new ImageUploadErrorResponse
            {
                Error = "FILE_TOO_LARGE",
                Message = "File size exceeds the maximum allowed limit of 5MB",
                SupportedFormats = new List<string> { "jpg", "png", "gif", "webp" },
                SupportedSubfolders = new List<string> { "projects", "blog_posts", "profile_images" }
            };

            // Assert
            request.ImageFile.FileName.Should().Be("portfolio-image.jpg");
            request.Subfolder.Should().Be("projects");
            successResponse.FileName.Should().Be(request.ImageFile.FileName);
            successResponse.Subfolder.Should().Be(request.Subfolder);
            successResponse.FileSize.Should().Be(mockFile.Length);
            errorResponse.Error.Should().Be("FILE_TOO_LARGE");
            errorResponse.SupportedFormats.Should().Contain("jpg");
            errorResponse.SupportedSubfolders.Should().Contain("projects");
        }

        #endregion

        #region Helper Methods

        private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.ContentType).Returns(contentType);
            mock.Setup(f => f.Length).Returns(length);
            
            // Create a simple stream with some content
            var content = Encoding.UTF8.GetBytes("fake image content for testing");
            var stream = new MemoryStream(content);
            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return mock.Object;
        }

        #endregion
    }
} 