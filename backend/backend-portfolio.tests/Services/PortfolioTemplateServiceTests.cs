using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.tests.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.Services
{
    public class PortfolioTemplateServiceTests : IDisposable
    {
        private readonly DbContextOptions<PortfolioDbContext> _options;
        private readonly Mock<ILogger<PortfolioTemplateService>> _mockLogger;
        private readonly PortfolioTemplateService _service;
        private readonly PortfolioDbContext _context;

        public PortfolioTemplateServiceTests()
        {
            _options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PortfolioDbContext(_options);
            _mockLogger = new Mock<ILogger<PortfolioTemplateService>>();
            _service = new PortfolioTemplateService(_context, _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllParametersAreValid()
        {
            // Act
            var service = new PortfolioTemplateService(_context, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenContextIsNull()
        {
            // Act
            var service = new PortfolioTemplateService(null!, _mockLogger.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenLoggerIsNull()
        {
            // Act
            var service = new PortfolioTemplateService(_context, null!);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GetAllTemplatesAsync Tests

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldReturnTemplates_WhenTemplatesExist()
        {
            // Arrange
            var templates = new List<PortfolioTemplate>
            {
                TestDataFactory.CreatePortfolioTemplate(),
                TestDataFactory.CreatePortfolioTemplate()
            };

            await _context.PortfolioTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTemplatesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(t => t.Name);
        }

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldReturnEmptyList_WhenNoTemplatesExist()
        {
            // Act
            var result = await _service.GetAllTemplatesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldReturnTemplatesInOrderByName()
        {
            // Arrange
            var template1 = TestDataFactory.CreatePortfolioTemplate();
            template1.Name = "Zebra Template";

            var template2 = TestDataFactory.CreatePortfolioTemplate();
            template2.Name = "Alpha Template";

            var template3 = TestDataFactory.CreatePortfolioTemplate();
            template3.Name = "Beta Template";

            await _context.PortfolioTemplates.AddRangeAsync(template1, template2, template3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTemplatesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.ElementAt(0).Name.Should().Be("Alpha Template");
            result.ElementAt(1).Name.Should().Be("Beta Template");
            result.ElementAt(2).Name.Should().Be("Zebra Template");
        }

        #endregion

        #region GetActiveTemplatesAsync Tests

        [Fact]
        public async Task GetActiveTemplatesAsync_ShouldReturnOnlyActiveTemplates()
        {
            // Arrange
            var activeTemplate1 = TestDataFactory.CreatePortfolioTemplate();
            activeTemplate1.IsActive = true;
            activeTemplate1.Name = "Active Template 1";

            var activeTemplate2 = TestDataFactory.CreatePortfolioTemplate();
            activeTemplate2.IsActive = true;
            activeTemplate2.Name = "Active Template 2";

            var inactiveTemplate = TestDataFactory.CreatePortfolioTemplate();
            inactiveTemplate.IsActive = false;
            inactiveTemplate.Name = "Inactive Template";

            await _context.PortfolioTemplates.AddRangeAsync(activeTemplate1, activeTemplate2, inactiveTemplate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveTemplatesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.IsActive);
            result.Should().BeInAscendingOrder(t => t.Name);
        }

        [Fact]
        public async Task GetActiveTemplatesAsync_ShouldReturnEmptyList_WhenNoActiveTemplatesExist()
        {
            // Arrange
            var inactiveTemplate = TestDataFactory.CreatePortfolioTemplate();
            inactiveTemplate.IsActive = false;

            await _context.PortfolioTemplates.AddAsync(inactiveTemplate);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveTemplatesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetTemplateByIdAsync Tests

        [Fact]
        public async Task GetTemplateByIdAsync_ShouldReturnTemplate_WhenTemplateExists()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTemplateByIdAsync(template.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(template.Id);
            result.Name.Should().Be(template.Name);
            result.Description.Should().Be(template.Description);
            result.IsActive.Should().Be(template.IsActive);
        }

        [Fact]
        public async Task GetTemplateByIdAsync_ShouldReturnNull_WhenTemplateDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _service.GetTemplateByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTemplateByIdAsync_ShouldHandleEmptyGuid()
        {
            // Act
            var result = await _service.GetTemplateByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetTemplateByNameAsync Tests

        [Fact]
        public async Task GetTemplateByNameAsync_ShouldReturnTemplate_WhenTemplateExists()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            template.Name = "Test Template";
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTemplateByNameAsync("Test Template");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Template");
        }

        [Fact]
        public async Task GetTemplateByNameAsync_ShouldReturnTemplate_WhenNameMatchesCaseInsensitive()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            template.Name = "Test Template";
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTemplateByNameAsync("test template");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Template");
        }

        [Fact]
        public async Task GetTemplateByNameAsync_ShouldReturnNull_WhenTemplateDoesNotExist()
        {
            // Act
            var result = await _service.GetTemplateByNameAsync("Non-existent Template");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetTemplateByNameAsync_ShouldHandleInvalidNames(string name)
        {
            // Act
            var result = await _service.GetTemplateByNameAsync(name);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateTemplateAsync Tests

        [Fact]
        public async Task CreateTemplateAsync_ShouldCreateTemplate_WhenValidRequestProvided()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "New Template",
                Description = "A new template for testing",
                PreviewImageUrl = "https://example.com/preview.jpg",
                IsActive = true
            };

            // Act
            var result = await _service.CreateTemplateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.PreviewImageUrl.Should().Be(request.PreviewImageUrl);
            result.IsActive.Should().Be(request.IsActive);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify template was saved to database
            var savedTemplate = await _context.PortfolioTemplates.FindAsync(result.Id);
            savedTemplate.Should().NotBeNull();
            savedTemplate!.Name.Should().Be(request.Name);
        }

        [Fact]
        public async Task CreateTemplateAsync_ShouldCreateTemplate_WhenRequestHasNullValues()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Template with null values",
                Description = null,
                PreviewImageUrl = null,
                IsActive = false
            };

            // Act
            var result = await _service.CreateTemplateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().BeNull();
            result.PreviewImageUrl.Should().BeNull();
            result.IsActive.Should().Be(request.IsActive);
        }

        [Fact]
        public async Task CreateTemplateAsync_ShouldCreateTemplate_WhenRequestHasEmptyStrings()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Template with empty strings",
                Description = "",
                PreviewImageUrl = "",
                IsActive = true
            };

            // Act
            var result = await _service.CreateTemplateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be("");
            result.PreviewImageUrl.Should().Be("");
        }

        [Fact]
        public async Task CreateTemplateAsync_ShouldLogInformation_WhenTemplateIsCreated()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Test Template",
                Description = "Test Description",
                IsActive = true
            };

            // Act
            await _service.CreateTemplateAsync(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created new portfolio template")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region UpdateTemplateAsync Tests

        [Fact]
        public async Task UpdateTemplateAsync_ShouldUpdateTemplate_WhenTemplateExists()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            template.Name = "Original Name";
            template.Description = "Original Description";
            template.IsActive = true;

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Updated Name",
                Description = "Updated Description",
                IsActive = false
            };

            // Act
            var result = await _service.UpdateTemplateAsync(template.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");
            result.Description.Should().Be("Updated Description");
            result.IsActive.Should().Be(false);
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify template was updated in database
            var updatedTemplate = await _context.PortfolioTemplates.FindAsync(template.Id);
            updatedTemplate.Should().NotBeNull();
            updatedTemplate!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldReturnNull_WhenTemplateDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            var result = await _service.UpdateTemplateAsync(nonExistentId, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldUpdateOnlyProvidedFields()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            template.Name = "Original Name";
            template.Description = "Original Description";
            template.PreviewImageUrl = "https://original.com/image.jpg";
            template.IsActive = true;

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Updated Name"
                // Only updating name, other fields should remain unchanged
            };

            // Act
            var result = await _service.UpdateTemplateAsync(template.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Name");
            result.Description.Should().Be("Original Description");
            result.PreviewImageUrl.Should().Be("https://original.com/image.jpg");
            result.IsActive.Should().Be(true);
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldHandleNullValuesInRequest()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            template.Name = "Original Name";
            template.Description = "Original Description";

            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new PortfolioTemplateUpdateRequest
            {
                Name = null,
                Description = null,
                PreviewImageUrl = null,
                IsActive = null
            };

            // Act
            var result = await _service.UpdateTemplateAsync(template.Id, request);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Original Name");
            result.Description.Should().Be("Original Description");
        }

        [Fact]
        public async Task UpdateTemplateAsync_ShouldLogInformation_WhenTemplateIsUpdated()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            var request = new PortfolioTemplateUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            await _service.UpdateTemplateAsync(template.Id, request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated portfolio template")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region DeleteTemplateAsync Tests

        [Fact]
        public async Task DeleteTemplateAsync_ShouldDeleteTemplate_WhenTemplateExists()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteTemplateAsync(template.Id);

            // Assert
            result.Should().BeTrue();

            // Verify template was completely deleted since no portfolios are using it
            var updatedTemplate = await _context.PortfolioTemplates.FindAsync(template.Id);
            updatedTemplate.Should().BeNull();
        }

        [Fact]
        public async Task DeleteTemplateAsync_ShouldReturnFalse_WhenTemplateDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _service.DeleteTemplateAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteTemplateAsync_ShouldLogInformation_WhenTemplateIsDeleted()
        {
            // Arrange
            var template = TestDataFactory.CreatePortfolioTemplate();
            await _context.PortfolioTemplates.AddAsync(template);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteTemplateAsync(template.Id);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted portfolio template")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }



        #endregion

        #region SeedDefaultTemplatesAsync Tests

        [Fact]
        public async Task SeedDefaultTemplatesAsync_ShouldCreateDefaultTemplates_WhenNoTemplatesExist()
        {
            // Act
            await _service.SeedDefaultTemplatesAsync();

            // Assert
            var templates = await _context.PortfolioTemplates.ToListAsync();
            templates.Should().HaveCount(4);
            templates.Should().Contain(t => t.Name == "Gabriel Bârzu");
            templates.Should().Contain(t => t.Name == "Modern");
            templates.Should().Contain(t => t.Name == "Creative");
            templates.Should().Contain(t => t.Name == "Professional");
        }

        [Fact]
        public async Task SeedDefaultTemplatesAsync_ShouldNotCreateDuplicateTemplates_WhenTemplatesAlreadyExist()
        {
            // Arrange
            var existingTemplate = TestDataFactory.CreatePortfolioTemplate();
            existingTemplate.Name = "Gabriel Bârzu";
            await _context.PortfolioTemplates.AddAsync(existingTemplate);
            await _context.SaveChangesAsync();

            // Act
            await _service.SeedDefaultTemplatesAsync();

            // Assert
            var templates = await _context.PortfolioTemplates.ToListAsync();
            templates.Should().HaveCount(4); // 1 existing (Gabriel Bârzu already exists) + 3 new
            templates.Should().Contain(t => t.Name == "Gabriel Bârzu");
            templates.Should().Contain(t => t.Name == "Modern");
            templates.Should().Contain(t => t.Name == "Creative");
            templates.Should().Contain(t => t.Name == "Professional");
        }

        [Fact]
        public async Task SeedDefaultTemplatesAsync_ShouldLogInformation_WhenTemplatesAreSeeded()
        {
            // Act
            await _service.SeedDefaultTemplatesAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeded template:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Edge Cases and Error Handling

        // Note: Database exception tests have been removed as they require mocking non-virtual DbContext properties
        // which is not supported by Moq. Real database exception testing would be done in integration tests.







        [Fact]
        public async Task GetAllTemplatesAsync_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var templates = new List<PortfolioTemplate>
            {
                TestDataFactory.CreatePortfolioTemplate(),
                TestDataFactory.CreatePortfolioTemplate()
            };

            await _context.PortfolioTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();

            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _service.GetAllTemplatesAsync())
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(5);
            results.Should().AllSatisfy(result => result.Should().HaveCount(2));
        }

        [Fact]
        public async Task CreateTemplateAsync_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var request = new PortfolioTemplateCreateRequest
            {
                Name = "Concurrent Template",
                IsActive = true
            };

            // Act
            var tasks = Enumerable.Range(0, 3)
                .Select(_ => _service.CreateTemplateAsync(request))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().HaveCount(3);
            results.Should().AllSatisfy(result => result.Should().NotBeNull());
        }

        #endregion

        #region Performance Tests

        [Fact]
        public async Task GetAllTemplatesAsync_ShouldCompleteWithinReasonableTime()
        {
            // Arrange
            var templates = Enumerable.Range(0, 100)
                .Select(i => TestDataFactory.CreatePortfolioTemplate())
                .ToList();

            await _context.PortfolioTemplates.AddRangeAsync(templates);
            await _context.SaveChangesAsync();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _service.GetAllTemplatesAsync();
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(100);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 