using Xunit;
using FluentAssertions;
using backend_portfolio.Services.Mappers;
using backend_portfolio.Models;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.tests.Helpers;
using System.Collections.Generic;

namespace backend_portfolio.tests.Services
{
    public class ProjectMapperTests
    {
        private readonly ProjectMapper _mapper;

        public ProjectMapperTests()
        {
            _mapper = new ProjectMapper();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act
            var mapper = new ProjectMapper();

            // Assert
            mapper.Should().NotBeNull();
        }

        #endregion

        #region MapToResponseDto Tests

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenValidProjectProvided()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Id = Guid.NewGuid();
            project.PortfolioId = Guid.NewGuid();
            project.Title = "Test Project";
            project.Description = "Test Description";
            project.Technologies = new string[] { "JavaScript", "React", "Node.js" };
            project.GithubUrl = "https://github.com/test/project";
            project.DemoUrl = "https://test-project.com";
            project.ImageUrl = "https://example.com/project-image.jpg";
            project.Featured = true;
            project.CreatedAt = DateTime.UtcNow.AddDays(-1);
            project.UpdatedAt = DateTime.UtcNow;

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(project.Id);
            result.PortfolioId.Should().Be(project.PortfolioId);
            result.Title.Should().Be(project.Title);
            result.Description.Should().Be(project.Description);
            result.Technologies.Should().BeEquivalentTo(project.Technologies);
            result.GithubUrl.Should().Be(project.GithubUrl);
            result.DemoUrl.Should().Be(project.DemoUrl);
            result.ImageUrl.Should().Be(project.ImageUrl);
            result.Featured.Should().Be(project.Featured);
            result.CreatedAt.Should().Be(project.CreatedAt);
            result.UpdatedAt.Should().Be(project.UpdatedAt);
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectHasNullValues()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Description = null;
            project.Technologies = null;
            project.GithubUrl = null;
            project.DemoUrl = null;
            project.ImageUrl = null;

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().BeNull();
            result.Technologies.Should().BeNull();
            result.GithubUrl.Should().BeNull();
            result.DemoUrl.Should().BeNull();
            result.ImageUrl.Should().BeNull();
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectHasEmptyStrings()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Title = "";
            project.Description = "";
            project.Technologies = new string[] { };
            project.GithubUrl = "";
            project.DemoUrl = "";
            project.ImageUrl = "";

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("");
            result.Description.Should().Be("");
            result.Technologies.Should().BeEquivalentTo(new string[] { });
            result.GithubUrl.Should().Be("");
            result.DemoUrl.Should().Be("");
            result.ImageUrl.Should().Be("");
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectHasWhitespaceStrings()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Title = "   ";
            project.Description = "   ";
            project.Technologies = new string[] { "   " };
            project.GithubUrl = "   ";
            project.DemoUrl = "   ";
            project.ImageUrl = "   ";

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("   ");
            result.Description.Should().Be("   ");
            result.Technologies.Should().BeEquivalentTo(new string[] { "   " });
            result.GithubUrl.Should().Be("   ");
            result.DemoUrl.Should().Be("   ");
            result.ImageUrl.Should().Be("   ");
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectIsNotFeatured()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Featured = false;

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Featured.Should().BeFalse();
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectHasSpecialCharacters()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Title = "Project with special chars: áéíóú ñ & < > \" '";
            project.Description = "Description with emojis: 🚀 💻 📱";
            project.Technologies = new string[] { "Technologies: React", "Node.js", "MongoDB" };
            project.GithubUrl = "https://github.com/user/project-with-special-chars";
            project.DemoUrl = "https://project-with-special-chars.com";
            project.ImageUrl = "https://example.com/project-image.jpg?size=large";

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Project with special chars: áéíóú ñ & < > \" '");
            result.Description.Should().Be("Description with emojis: 🚀 💻 📱");
            result.Technologies.Should().BeEquivalentTo(new string[] { "Technologies: React", "Node.js", "MongoDB" });
            result.GithubUrl.Should().Be("https://github.com/user/project-with-special-chars");
            result.DemoUrl.Should().Be("https://project-with-special-chars.com");
            result.ImageUrl.Should().Be("https://example.com/project-image.jpg?size=large");
        }

        [Fact]
        public void MapToResponseDto_ShouldMapProjectToResponseDto_WhenProjectHasEmptyGuid()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Id = Guid.Empty;
            project.PortfolioId = Guid.Empty;

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Guid.Empty);
            result.PortfolioId.Should().Be(Guid.Empty);
        }

        #endregion

        #region MapToResponseDtos Tests

        [Fact]
        public void MapToResponseDtos_ShouldMapProjectsToResponseDtos_WhenValidProjectsProvided()
        {
            // Arrange
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject(),
                TestDataFactory.CreateProject()
            };

            // Act
            var result = _mapper.MapToResponseDtos(projects);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        [Fact]
        public void MapToResponseDtos_ShouldReturnEmptyEnumerable_WhenEmptyProjectsProvided()
        {
            // Arrange
            var projects = new List<Project>();

            // Act
            var result = _mapper.MapToResponseDtos(projects);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToResponseDtos_ShouldReturnEmptyEnumerable_WhenNullProjectsProvided()
        {
            // Arrange
            IEnumerable<Project> projects = null!;

            // Act
            var result = _mapper.MapToResponseDtos(projects);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void MapToResponseDtos_ShouldHandleProjectsWithNullValues()
        {
            // Arrange
            var projects = new List<Project>
            {
                TestDataFactory.CreateProject(),
                null!,
                TestDataFactory.CreateProject()
            };

            // Act
            var result = _mapper.MapToResponseDtos(projects);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        #endregion

        #region MapFromCreateDto Tests

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenValidRequestProvided()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project",
                Description = "New Description",
                Technologies = new string[] { "React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/new-project",
                DemoUrl = "https://new-project.com",
                ImageUrl = "https://example.com/new-project-image.jpg",
                Featured = true
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.PortfolioId.Should().Be(request.PortfolioId);
            result.Title.Should().Be(request.Title);
            result.Description.Should().Be(request.Description);
            result.Technologies.Should().BeEquivalentTo(request.Technologies);
            result.GithubUrl.Should().Be(request.GithubUrl);
            result.DemoUrl.Should().Be(request.DemoUrl);
            result.ImageUrl.Should().Be(request.ImageUrl);
            result.Featured.Should().Be(request.Featured);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenRequestHasNullValues()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project",
                Description = null,
                Technologies = null,
                GithubUrl = null,
                DemoUrl = null,
                ImageUrl = null
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().BeNull();
            result.Technologies.Should().BeNull();
            result.GithubUrl.Should().BeNull();
            result.DemoUrl.Should().BeNull();
            result.ImageUrl.Should().BeNull();
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenRequestHasEmptyStrings()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "",
                Description = "",
                Technologies = new string[] { },
                GithubUrl = "",
                DemoUrl = "",
                ImageUrl = ""
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("");
            result.Description.Should().Be("");
            result.Technologies.Should().BeEquivalentTo(new string[] { });
            result.GithubUrl.Should().Be("");
            result.DemoUrl.Should().Be("");
            result.ImageUrl.Should().Be("");
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenRequestHasWhitespaceStrings()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "   ",
                Description = "   ",
                Technologies = new string[] { "   " },
                GithubUrl = "   ",
                DemoUrl = "   ",
                ImageUrl = "   "
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("   ");
            result.Description.Should().Be("   ");
            result.Technologies.Should().BeEquivalentTo(new string[] { "   " });
            result.GithubUrl.Should().Be("   ");
            result.DemoUrl.Should().Be("   ");
            result.ImageUrl.Should().Be("   ");
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenRequestHasEmptyPortfolioId()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.Empty,
                Title = "New Project"
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.PortfolioId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenRequestHasSpecialCharacters()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Project with special chars: áéíóú ñ & < > \" '",
                Description = "Description with emojis: 🚀 💻 📱",
                Technologies = new string[] { "Technologies: React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/project-with-special-chars",
                DemoUrl = "https://project-with-special-chars.com",
                ImageUrl = "https://example.com/project-image.jpg?size=large"
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Project with special chars: áéíóú ñ & < > \" '");
            result.Description.Should().Be("Description with emojis: 🚀 💻 📱");
            result.Technologies.Should().BeEquivalentTo(new string[] { "Technologies: React", "Node.js", "MongoDB" });
            result.GithubUrl.Should().Be("https://github.com/user/project-with-special-chars");
            result.DemoUrl.Should().Be("https://project-with-special-chars.com");
            result.ImageUrl.Should().Be("https://example.com/project-image.jpg?size=large");
        }

        [Fact]
        public void MapFromCreateDto_ShouldMapCreateRequestToProject_WhenProjectIsNotFeatured()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "New Project",
                Featured = false
            };

            // Act
            var result = _mapper.MapFromCreateDto(request);

            // Assert
            result.Should().NotBeNull();
            result.Featured.Should().BeFalse();
        }

        #endregion

        #region UpdateEntityFromDto Tests

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenValidRequestProvided()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            var originalTechnologies = project.Technologies;
            var originalGithubUrl = project.GithubUrl;
            var originalDemoUrl = project.DemoUrl;
            var originalImageUrl = project.ImageUrl;
            var originalFeatured = project.Featured;
            var originalUpdatedAt = project.UpdatedAt;

            var request = new ProjectUpdateRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Technologies = new string[] { "Updated Technologies" },
                GithubUrl = "https://github.com/user/updated-project",
                DemoUrl = "https://updated-project.com",
                ImageUrl = "https://example.com/updated-project-image.jpg",
                Featured = false
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be("Updated Title");
            project.Description.Should().Be("Updated Description");
            project.Technologies.Should().BeEquivalentTo(new string[] { "Updated Technologies" });
            project.GithubUrl.Should().Be("https://github.com/user/updated-project");
            project.DemoUrl.Should().Be("https://updated-project.com");
            project.ImageUrl.Should().Be("https://example.com/updated-project-image.jpg");
            project.Featured.Should().BeFalse();
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenRequestHasNullValues()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            var originalTechnologies = project.Technologies;
            var originalGithubUrl = project.GithubUrl;
            var originalDemoUrl = project.DemoUrl;
            var originalImageUrl = project.ImageUrl;
            var originalFeatured = project.Featured;

            var request = new ProjectUpdateRequest
            {
                Title = null,
                Description = null,
                Technologies = null,
                GithubUrl = null,
                DemoUrl = null,
                ImageUrl = null,
                Featured = null
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be(originalTitle);
            project.Description.Should().Be(originalDescription);
            project.Technologies.Should().BeEquivalentTo(originalTechnologies);
            project.GithubUrl.Should().Be(originalGithubUrl);
            project.DemoUrl.Should().Be(originalDemoUrl);
            project.ImageUrl.Should().Be(originalImageUrl);
            project.Featured.Should().Be(originalFeatured);
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenRequestHasPartialUpdates()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            var originalTechnologies = project.Technologies;
            var originalGithubUrl = project.GithubUrl;
            var originalDemoUrl = project.DemoUrl;
            var originalImageUrl = project.ImageUrl;
            var originalFeatured = project.Featured;

            var request = new ProjectUpdateRequest
            {
                Title = "Updated Title"
                // Only updating title, other fields should remain unchanged
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be("Updated Title");
            project.Description.Should().Be(originalDescription);
            project.Technologies.Should().BeEquivalentTo(originalTechnologies);
            project.GithubUrl.Should().Be(originalGithubUrl);
            project.DemoUrl.Should().Be(originalDemoUrl);
            project.ImageUrl.Should().Be(originalImageUrl);
            project.Featured.Should().Be(originalFeatured);
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenRequestHasEmptyStrings()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            var originalTechnologies = project.Technologies;
            var originalGithubUrl = project.GithubUrl;
            var originalDemoUrl = project.DemoUrl;
            var originalImageUrl = project.ImageUrl;

            var request = new ProjectUpdateRequest
            {
                Title = "",
                Description = "",
                Technologies = new string[] { },
                GithubUrl = "",
                DemoUrl = "",
                ImageUrl = ""
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be("");
            project.Description.Should().Be("");
            project.Technologies.Should().BeEquivalentTo(new string[] { });
            project.GithubUrl.Should().Be("");
            project.DemoUrl.Should().Be("");
            project.ImageUrl.Should().Be("");
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenRequestHasWhitespaceStrings()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalTitle = project.Title;
            var originalDescription = project.Description;
            var originalTechnologies = project.Technologies;
            var originalGithubUrl = project.GithubUrl;
            var originalDemoUrl = project.DemoUrl;
            var originalImageUrl = project.ImageUrl;

            var request = new ProjectUpdateRequest
            {
                Title = "   ",
                Description = "   ",
                Technologies = new string[] { "   " },
                GithubUrl = "   ",
                DemoUrl = "   ",
                ImageUrl = "   "
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be("   ");
            project.Description.Should().Be("   ");
            project.Technologies.Should().BeEquivalentTo(new string[] { "   " });
            project.GithubUrl.Should().Be("   ");
            project.DemoUrl.Should().Be("   ");
            project.ImageUrl.Should().Be("   ");
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldUpdateProjectFromUpdateRequest_WhenRequestHasSpecialCharacters()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();

            var request = new ProjectUpdateRequest
            {
                Title = "Updated with special chars: áéíóú ñ & < > \" '",
                Description = "Updated description with emojis: 🚀 💻 📱",
                Technologies = new string[] { "Updated technologies: React", "Node.js", "MongoDB" },
                GithubUrl = "https://github.com/user/updated-project-with-special-chars",
                DemoUrl = "https://updated-project-with-special-chars.com",
                ImageUrl = "https://example.com/updated-project-image.jpg?size=large"
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Title.Should().Be("Updated with special chars: áéíóú ñ & < > \" '");
            project.Description.Should().Be("Updated description with emojis: 🚀 💻 📱");
            project.Technologies.Should().BeEquivalentTo(new string[] { "Updated technologies: React", "Node.js", "MongoDB" });
            project.GithubUrl.Should().Be("https://github.com/user/updated-project-with-special-chars");
            project.DemoUrl.Should().Be("https://updated-project-with-special-chars.com");
            project.ImageUrl.Should().Be("https://example.com/updated-project-image.jpg?size=large");
            project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public void MapToResponseDto_ShouldHandleLargeProjectData()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Title = new string('A', 1000);
            project.Description = new string('B', 5000);
            project.Technologies = new string[] { new string('C', 2000) };
            project.GithubUrl = new string('D', 500);
            project.DemoUrl = new string('E', 500);
            project.ImageUrl = new string('F', 500);

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().HaveLength(1000);
            result.Description.Should().HaveLength(5000);
            result.Technologies.Should().HaveCount(1);
            result.Technologies![0].Should().HaveLength(2000);
            result.GithubUrl.Should().HaveLength(500);
            result.DemoUrl.Should().HaveLength(500);
            result.ImageUrl.Should().HaveLength(500);
        }

        [Fact]
        public void MapToResponseDtos_ShouldHandleLargeCollection()
        {
            // Arrange
            var projects = Enumerable.Range(0, 1000)
                .Select(_ => TestDataFactory.CreateProject())
                .ToList();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = _mapper.MapToResponseDtos(projects);
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(1000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
        }

        [Fact]
        public void MapFromCreateDto_ShouldGenerateUniqueIds()
        {
            // Arrange
            var request = new ProjectCreateRequest
            {
                PortfolioId = Guid.NewGuid(),
                Title = "Test Project"
            };

            // Act
            var result1 = _mapper.MapFromCreateDto(request);
            var result2 = _mapper.MapFromCreateDto(request);

            // Assert
            result1.Id.Should().NotBe(result2.Id);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyId()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalId = project.Id;
            var request = new ProjectUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.Id.Should().Be(originalId);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyPortfolioId()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalPortfolioId = project.PortfolioId;
            var request = new ProjectUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.PortfolioId.Should().Be(originalPortfolioId);
        }

        [Fact]
        public void UpdateEntityFromDto_ShouldNotModifyCreatedAt()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            var originalCreatedAt = project.CreatedAt;
            var request = new ProjectUpdateRequest
            {
                Title = "Updated Title"
            };

            // Act
            _mapper.UpdateEntityFromDto(project, request);

            // Assert
            project.CreatedAt.Should().Be(originalCreatedAt);
        }

        [Fact]
        public void MapToResponseDto_ShouldHandleVeryLongUrls()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.GithubUrl = "https://github.com/very-long-username/very-long-repository-name-with-many-characters-to-test-url-length-handling-in-the-mapper";
            project.DemoUrl = "https://very-long-domain-name-with-many-characters-to-test-url-length-handling-in-the-mapper.com/very-long-path/with-many-segments";
            project.ImageUrl = "https://very-long-cdn-domain-name-with-many-characters-to-test-url-length-handling-in-the-mapper.com/very-long-path/to/image.jpg?size=large&quality=high&format=webp";

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.GithubUrl.Should().Be(project.GithubUrl);
            result.DemoUrl.Should().Be(project.DemoUrl);
            result.ImageUrl.Should().Be(project.ImageUrl);
        }

        [Fact]
        public void MapToResponseDto_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var project = TestDataFactory.CreateProject();
            project.Title = "项目标题：React 项目";
            project.Description = "项目描述：这是一个使用 React 和 Node.js 构建的现代化 Web 应用程序";
            project.Technologies = new string[] { "React", "Node.js", "MongoDB", "中文支持" };

            // Act
            var result = _mapper.MapToResponseDto(project);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("项目标题：React 项目");
            result.Description.Should().Be("项目描述：这是一个使用 React 和 Node.js 构建的现代化 Web 应用程序");
            result.Technologies.Should().BeEquivalentTo(new string[] { "React", "Node.js", "MongoDB", "中文支持" });
        }

        #endregion
    }
} 