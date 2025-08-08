using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class PortfolioTemplateDTOTests
    {
        private readonly Fixture _fixture;

        public PortfolioTemplateDTOTests()
        {
            _fixture = new Fixture();
        }

        #region PortfolioTemplateCreateRequest Tests

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.Name.Should().NotBeNull();
            dto.IsActive.Should().BeTrue();
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var name = "Modern Portfolio Template";
            var description = "A clean and modern portfolio template with responsive design";
            var previewImageUrl = "https://example.com/templates/modern-preview.jpg";

            // Act
            var dto = new PortfolioTemplateCreateRequest
            {
                Name = name,
                Description = description,
                PreviewImageUrl = previewImageUrl,
                IsActive = true
            };

            // Assert
            dto.Name.Should().Be(name);
            dto.Description.Should().Be(description);
            dto.PreviewImageUrl.Should().Be(previewImageUrl);
            dto.IsActive.Should().BeTrue();
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest
            {
                Description = null,
                PreviewImageUrl = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.PreviewImageUrl.Should().BeNull();
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest
            {
                Name = "",
                Description = "",
                PreviewImageUrl = ""
            };

            // Assert
            dto.Name.Should().Be("");
            dto.Description.Should().Be("");
            dto.PreviewImageUrl.Should().Be("");
        }

        [Theory]
        [InlineData("Modern")]
        [InlineData("Classic")]
        [InlineData("Minimalist")]
        [InlineData("Creative")]
        [InlineData("Professional")]
        [InlineData("Dark Theme")]
        [InlineData("Portfolio Pro")]
        [InlineData("Developer Focus")]
        public void PortfolioTemplateCreateRequest_ShouldHandleCommonTemplateNames(string templateName)
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest { Name = templateName };

            // Assert
            dto.Name.Should().Be(templateName);
        }

        [Theory]
        [InlineData("https://example.com/preview.jpg")]
        [InlineData("http://example.com/preview.png")]
        [InlineData("https://cdn.example.com/templates/preview.gif")]
        [InlineData("relative/path/preview.webp")]
        [InlineData("")]
        public void PortfolioTemplateCreateRequest_ShouldHandleVariousPreviewImageUrls(string imageUrl)
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest { PreviewImageUrl = imageUrl };

            // Assert
            dto.PreviewImageUrl.Should().Be(imageUrl);
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleSpecialCharactersInName()
        {
            // Arrange
            var specialName = "Portfolio Template v2.0 - Modern & Responsive (2024)";

            // Act
            var dto = new PortfolioTemplateCreateRequest { Name = specialName };

            // Assert
            dto.Name.Should().Be(specialName);
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeName = "现代作品集模板 🎨 モダンポートフォリオ";
            var unicodeDescription = "适用于开发者的现代模板 開発者向けのモダンテンプレート";

            // Act
            var dto = new PortfolioTemplateCreateRequest
            {
                Name = unicodeName,
                Description = unicodeDescription
            };

            // Assert
            dto.Name.Should().Be(unicodeName);
            dto.Description.Should().Be(unicodeDescription);
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleLongStrings()
        {
            // Arrange
            var longName = new string('a', 5000);
            var longDescription = new string('b', 50000);

            // Act
            var dto = new PortfolioTemplateCreateRequest
            {
                Name = longName,
                Description = longDescription
            };

            // Assert
            dto.Name.Should().Be(longName);
            dto.Description.Should().Be(longDescription);
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleDetailedDescription()
        {
            // Arrange
            var detailedDescription = @"
Modern Portfolio Template Features:
- Fully responsive design that works on all devices
- Dark/Light theme toggle
- Smooth animations and transitions
- Contact form integration
- SEO optimized structure
- Fast loading performance
- Clean typography and spacing
- Mobile-first approach
- Accessibility compliant
- Cross-browser compatible

Ideal for:
✓ Web developers
✓ UI/UX designers
✓ Creative professionals
✓ Freelancers
✓ Students

Technologies used:
• HTML5
• CSS3 with Flexbox/Grid
• JavaScript ES6+
• Responsive images
• Web fonts
";

            // Act
            var dto = new PortfolioTemplateCreateRequest { Description = detailedDescription };

            // Assert
            dto.Description.Should().Be(detailedDescription);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PortfolioTemplateCreateRequest_ShouldHandleIsActiveValues(bool isActive)
        {
            // Act
            var dto = new PortfolioTemplateCreateRequest { IsActive = isActive };

            // Assert
            dto.IsActive.Should().Be(isActive);
        }

        #endregion

        #region PortfolioTemplateUpdateRequest Tests

        [Fact]
        public void PortfolioTemplateUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioTemplateUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioTemplateUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var name = "Updated Modern Template";
            var description = "Updated description with new features";
            var previewImageUrl = "https://example.com/updated-preview.jpg";

            // Act
            var dto = new PortfolioTemplateUpdateRequest
            {
                Name = name,
                Description = description,
                PreviewImageUrl = previewImageUrl,
                IsActive = false
            };

            // Assert
            dto.Name.Should().Be(name);
            dto.Description.Should().Be(description);
            dto.PreviewImageUrl.Should().Be(previewImageUrl);
            dto.IsActive.Should().BeFalse();
        }

        [Fact]
        public void PortfolioTemplateUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new PortfolioTemplateUpdateRequest
            {
                Name = null,
                Description = null,
                PreviewImageUrl = null,
                IsActive = null
            };

            // Assert
            dto.Name.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.PreviewImageUrl.Should().BeNull();
            dto.IsActive.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void PortfolioTemplateUpdateRequest_ShouldHandleNullableIsActive(bool? isActive)
        {
            // Act
            var dto = new PortfolioTemplateUpdateRequest { IsActive = isActive };

            // Assert
            dto.IsActive.Should().Be(isActive);
        }

        [Fact]
        public void PortfolioTemplateUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new PortfolioTemplateUpdateRequest
            {
                Name = "New Template Name",
                // Leave other fields null for partial update
                Description = null,
                PreviewImageUrl = null
            };

            // Assert
            dto.Name.Should().Be("New Template Name");
            dto.Description.Should().BeNull();
            dto.PreviewImageUrl.Should().BeNull();
        }

        [Fact]
        public void PortfolioTemplateUpdateRequest_ShouldHandleActivationToggle()
        {
            // Act
            var activateDto = new PortfolioTemplateUpdateRequest { IsActive = true };
            var deactivateDto = new PortfolioTemplateUpdateRequest { IsActive = false };

            // Assert
            activateDto.IsActive.Should().BeTrue();
            deactivateDto.IsActive.Should().BeFalse();
        }

        #endregion

        #region PortfolioTemplateResponse Tests

        [Fact]
        public void PortfolioTemplateResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioTemplateResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.Name.Should().NotBeNull();
            dto.IsActive.Should().BeFalse();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void PortfolioTemplateResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Test Template Response";
            var description = "Test description";
            var previewImageUrl = "https://example.com/test-preview.jpg";
            var createdAt = DateTime.UtcNow.AddDays(-10);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new PortfolioTemplateResponse
            {
                Id = id,
                Name = name,
                Description = description,
                PreviewImageUrl = previewImageUrl,
                IsActive = true,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.Name.Should().Be(name);
            dto.Description.Should().Be(description);
            dto.PreviewImageUrl.Should().Be(previewImageUrl);
            dto.IsActive.Should().BeTrue();
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void PortfolioTemplateResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new PortfolioTemplateResponse
            {
                Description = null,
                PreviewImageUrl = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.PreviewImageUrl.Should().BeNull();
        }

        [Fact]
        public void PortfolioTemplateResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new PortfolioTemplateResponse { CreatedAt = minDate, UpdatedAt = maxDate };
            var dto2 = new PortfolioTemplateResponse { CreatedAt = utcNow, UpdatedAt = localNow };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto1.UpdatedAt.Should().Be(maxDate);
            dto2.CreatedAt.Should().Be(utcNow);
            dto2.UpdatedAt.Should().Be(localNow);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PortfolioTemplateResponse_ShouldHandleActiveStatus(bool isActive)
        {
            // Act
            var dto = new PortfolioTemplateResponse { IsActive = isActive };

            // Assert
            dto.IsActive.Should().Be(isActive);
        }

        #endregion

        #region PortfolioTemplateSummaryResponse Tests

        [Fact]
        public void PortfolioTemplateSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioTemplateSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.Name.Should().NotBeNull();
            dto.IsActive.Should().BeFalse();
        }

        [Fact]
        public void PortfolioTemplateSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Summary Template";
            var description = "Summary description";
            var previewImageUrl = "https://example.com/summary-preview.jpg";

            // Act
            var dto = new PortfolioTemplateSummaryResponse
            {
                Id = id,
                Name = name,
                Description = description,
                PreviewImageUrl = previewImageUrl,
                IsActive = true
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.Name.Should().Be(name);
            dto.Description.Should().Be(description);
            dto.PreviewImageUrl.Should().Be(previewImageUrl);
            dto.IsActive.Should().BeTrue();
        }

        [Fact]
        public void PortfolioTemplateSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new PortfolioTemplateSummaryResponse
            {
                Description = null,
                PreviewImageUrl = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.PreviewImageUrl.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllPortfolioTemplateDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var response = new PortfolioTemplateResponse { Id = Guid.Empty };
            var summaryResponse = new PortfolioTemplateSummaryResponse { Id = Guid.Empty };

            // Assert
            response.Id.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllPortfolioTemplateDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var response = new PortfolioTemplateResponse { Id = maxGuid };
            var summaryResponse = new PortfolioTemplateSummaryResponse { Id = maxGuid };

            // Assert
            response.Id.Should().Be(maxGuid);
            summaryResponse.Id.Should().Be(maxGuid);
        }

        [Fact]
        public void AllPortfolioTemplateDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new PortfolioTemplateCreateRequest
            {
                Name = whitespace,
                Description = whitespace,
                PreviewImageUrl = whitespace
            };

            var updateRequest = new PortfolioTemplateUpdateRequest
            {
                Name = whitespace,
                Description = whitespace,
                PreviewImageUrl = whitespace
            };

            // Assert
            createRequest.Name.Should().Be(whitespace);
            createRequest.Description.Should().Be(whitespace);
            createRequest.PreviewImageUrl.Should().Be(whitespace);
            updateRequest.Name.Should().Be(whitespace);
            updateRequest.Description.Should().Be(whitespace);
            updateRequest.PreviewImageUrl.Should().Be(whitespace);
        }

        [Fact]
        public void AllPortfolioTemplateDTOs_ShouldHandleInvalidUrls()
        {
            // Arrange
            var invalidUrls = new[]
            {
                "not-a-url",
                "ftp://invalid.com",
                "javascript:alert('xss')",
                "file:///etc/passwd",
                "data:text/html,<script>alert('xss')</script>"
            };

            // Act & Assert - DTOs should accept any string, validation happens elsewhere
            foreach (var url in invalidUrls)
            {
                var createDto = new PortfolioTemplateCreateRequest { PreviewImageUrl = url };
                var updateDto = new PortfolioTemplateUpdateRequest { PreviewImageUrl = url };

                createDto.PreviewImageUrl.Should().Be(url);
                updateDto.PreviewImageUrl.Should().Be(url);
            }
        }

        [Fact]
        public void AllPortfolioTemplateDTOs_ShouldHandleVeryLongUrls()
        {
            // Arrange
            var longUrl = "https://example.com/" + new string('a', 10000);

            // Act
            var createDto = new PortfolioTemplateCreateRequest { PreviewImageUrl = longUrl };
            var updateDto = new PortfolioTemplateUpdateRequest { PreviewImageUrl = longUrl };

            // Assert
            createDto.PreviewImageUrl.Should().Be(longUrl);
            updateDto.PreviewImageUrl.Should().Be(longUrl);
        }

        [Fact]
        public void PortfolioTemplateDTOs_ShouldHandleCommonTemplateTypes()
        {
            // Arrange
            var templateTypes = new[]
            {
                "Modern",
                "Classic",
                "Minimalist",
                "Creative",
                "Professional",
                "Dark Theme",
                "Light Theme",
                "Corporate",
                "Portfolio Pro",
                "Developer",
                "Designer",
                "Photographer",
                "Artist",
                "Writer",
                "Freelancer"
            };

            // Act & Assert
            foreach (var templateType in templateTypes)
            {
                var createRequest = new PortfolioTemplateCreateRequest { Name = templateType };
                var updateRequest = new PortfolioTemplateUpdateRequest { Name = templateType };

                createRequest.Name.Should().Be(templateType);
                updateRequest.Name.Should().Be(templateType);
            }
        }

        [Fact]
        public void PortfolioTemplateDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new PortfolioTemplateCreateRequest();
            var updateRequest = new PortfolioTemplateUpdateRequest();
            var response = new PortfolioTemplateResponse();
            var summaryResponse = new PortfolioTemplateSummaryResponse();

            // Act & Assert - Should not throw
            var createRequestProperties = createRequest.GetType().GetProperties();
            var updateRequestProperties = updateRequest.GetType().GetProperties();
            var responseProperties = response.GetType().GetProperties();
            var summaryResponseProperties = summaryResponse.GetType().GetProperties();

            createRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            updateRequestProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            responseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
            summaryResponseProperties.Should().AllSatisfy(p => p.CanRead.Should().BeTrue());
        }

        [Fact]
        public void PortfolioTemplateDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var templates = new List<PortfolioTemplateResponse>();
            for (int i = 0; i < 1000; i++)
            {
                templates.Add(new PortfolioTemplateResponse
                {
                    Id = Guid.NewGuid(),
                    Name = _fixture.Create<string>(),
                    Description = _fixture.Create<string>(),
                    PreviewImageUrl = _fixture.Create<string>(),
                    IsActive = i % 2 == 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Assert - Should not throw OutOfMemoryException
            templates.Should().HaveCount(1000);
        }

        [Fact]
        public void PortfolioTemplateCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<PortfolioTemplateCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                var index = i; // Capture for closure
                tasks.Add(Task.Run(() => new PortfolioTemplateCreateRequest
                {
                    Name = $"Template {index}",
                    Description = $"Description for template {index}",
                    PreviewImageUrl = $"https://example.com/template-{index}.jpg",
                    IsActive = index % 2 == 0
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void PortfolioTemplateDTOs_ShouldHandleTemplateLifecycle()
        {
            // This test simulates a typical template lifecycle
            
            // Arrange - Create template
            var createRequest = new PortfolioTemplateCreateRequest
            {
                Name = "New Modern Template",
                Description = "A brand new modern template",
                PreviewImageUrl = "https://example.com/new-template.jpg",
                IsActive = false // Start as inactive
            };

            // Act - Update to activate and refine
            var updateRequest = new PortfolioTemplateUpdateRequest
            {
                Description = "Updated description with more features",
                IsActive = true // Activate the template
            };

            var response = new PortfolioTemplateResponse
            {
                Name = createRequest.Name,
                Description = updateRequest.Description,
                PreviewImageUrl = createRequest.PreviewImageUrl,
                IsActive = updateRequest.IsActive.Value,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            var summaryResponse = new PortfolioTemplateSummaryResponse
            {
                Name = response.Name,
                Description = response.Description,
                PreviewImageUrl = response.PreviewImageUrl,
                IsActive = response.IsActive
            };

            // Assert
            createRequest.IsActive.Should().BeFalse();
            updateRequest.IsActive.Should().BeTrue();
            response.IsActive.Should().BeTrue();
            summaryResponse.IsActive.Should().BeTrue();
            response.Description.Should().Be("Updated description with more features");
            summaryResponse.Description.Should().Be(response.Description);
        }

        [Fact]
        public void PortfolioTemplateDTOs_ShouldHandleTemplateVersioning()
        {
            // This test simulates template versioning scenarios
            
            // Arrange
            var baseTemplateName = "Modern Portfolio";
            var versions = new[]
            {
                $"{baseTemplateName} v1.0",
                $"{baseTemplateName} v2.0",
                $"{baseTemplateName} v2.1",
                $"{baseTemplateName} v3.0 Beta"
            };

            // Act
            var templates = versions.Select(version => new PortfolioTemplateCreateRequest
            {
                Name = version,
                Description = $"Version {version.Split(' ').Last()} of the {baseTemplateName} template",
                IsActive = !version.Contains("Beta") // Beta versions start inactive
            }).ToList();

            // Assert
            templates.Should().HaveCount(4);
            templates.Where(t => t.IsActive).Should().HaveCount(3); // All except beta
            templates.Where(t => !t.IsActive).Should().HaveCount(1); // Only beta
            templates.Should().AllSatisfy(t => t.Name.Should().StartWith(baseTemplateName));
        }

        #endregion
    }
} 