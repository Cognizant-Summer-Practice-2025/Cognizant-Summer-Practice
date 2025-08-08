using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend_portfolio.tests.DTO
{
    public class ProjectDTOTests
    {
        private readonly Fixture _fixture;

        public ProjectDTOTests()
        {
            _fixture = new Fixture();
        }

        #region ProjectCreateRequest Tests

        [Fact]
        public void ProjectCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new ProjectCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.Featured.Should().BeFalse();
        }

        [Fact]
        public void ProjectCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var title = "Test Project";
            var description = "Test Description";
            var imageUrl = "https://example.com/image.jpg";
            var demoUrl = "https://example.com/demo";
            var githubUrl = "https://github.com/user/repo";
            var technologies = new[] { "C#", "React", "SQL" };

            // Act
            var dto = new ProjectCreateRequest
            {
                PortfolioId = portfolioId,
                Title = title,
                Description = description,
                ImageUrl = imageUrl,
                DemoUrl = demoUrl,
                GithubUrl = githubUrl,
                Technologies = technologies,
                Featured = true
            };

            // Assert
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Title.Should().Be(title);
            dto.Description.Should().Be(description);
            dto.ImageUrl.Should().Be(imageUrl);
            dto.DemoUrl.Should().Be(demoUrl);
            dto.GithubUrl.Should().Be(githubUrl);
            dto.Technologies.Should().BeEquivalentTo(technologies);
            dto.Featured.Should().BeTrue();
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ProjectCreateRequest
            {
                Description = null,
                ImageUrl = null,
                DemoUrl = null,
                GithubUrl = null,
                Technologies = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.ImageUrl.Should().BeNull();
            dto.DemoUrl.Should().BeNull();
            dto.GithubUrl.Should().BeNull();
            dto.Technologies.Should().BeNull();
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleEmptyStrings()
        {
            // Act
            var dto = new ProjectCreateRequest
            {
                Title = "",
                Description = "",
                ImageUrl = "",
                DemoUrl = "",
                GithubUrl = ""
            };

            // Assert
            dto.Title.Should().Be("");
            dto.Description.Should().Be("");
            dto.ImageUrl.Should().Be("");
            dto.DemoUrl.Should().Be("");
            dto.GithubUrl.Should().Be("");
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleEmptyTechnologiesArray()
        {
            // Act
            var dto = new ProjectCreateRequest
            {
                Technologies = new string[0]
            };

            // Assert
            dto.Technologies.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleLargeTechnologiesArray()
        {
            // Arrange
            var technologies = _fixture.CreateMany<string>(100).ToArray();

            // Act
            var dto = new ProjectCreateRequest
            {
                Technologies = technologies
            };

            // Assert
            dto.Technologies.Should().HaveCount(100);
            dto.Technologies.Should().BeEquivalentTo(technologies);
        }

        [Theory]
        [InlineData("https://example.com/image.jpg")]
        [InlineData("http://example.com/image.png")]
        [InlineData("https://cdn.example.com/images/project.gif")]
        [InlineData("relative/path/image.webp")]
        [InlineData("")]
        public void ProjectCreateRequest_ShouldHandleVariousImageUrls(string imageUrl)
        {
            // Act
            var dto = new ProjectCreateRequest { ImageUrl = imageUrl };

            // Assert
            dto.ImageUrl.Should().Be(imageUrl);
        }

        [Theory]
        [InlineData("https://example.com/demo")]
        [InlineData("http://localhost:3000")]
        [InlineData("https://app.example.com/demo/project")]
        [InlineData("")]
        public void ProjectCreateRequest_ShouldHandleVariousDemoUrls(string demoUrl)
        {
            // Act
            var dto = new ProjectCreateRequest { DemoUrl = demoUrl };

            // Assert
            dto.DemoUrl.Should().Be(demoUrl);
        }

        [Theory]
        [InlineData("https://github.com/user/repo")]
        [InlineData("https://gitlab.com/user/repo")]
        [InlineData("https://bitbucket.org/user/repo")]
        [InlineData("")]
        public void ProjectCreateRequest_ShouldHandleVariousGithubUrls(string githubUrl)
        {
            // Act
            var dto = new ProjectCreateRequest { GithubUrl = githubUrl };

            // Assert
            dto.GithubUrl.Should().Be(githubUrl);
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleSpecialCharactersInTitle()
        {
            // Arrange
            var specialTitle = "My Project! @#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            var dto = new ProjectCreateRequest { Title = specialTitle };

            // Assert
            dto.Title.Should().Be(specialTitle);
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeTitle = "项目测试 🚀 プロジェクト مشروع";
            var unicodeDescription = "تطبيق ويب متقدم 日本語の説明 中文描述";

            // Act
            var dto = new ProjectCreateRequest
            {
                Title = unicodeTitle,
                Description = unicodeDescription
            };

            // Assert
            dto.Title.Should().Be(unicodeTitle);
            dto.Description.Should().Be(unicodeDescription);
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleLongStrings()
        {
            // Arrange
            var longTitle = new string('a', 10000);
            var longDescription = new string('b', 50000);

            // Act
            var dto = new ProjectCreateRequest
            {
                Title = longTitle,
                Description = longDescription
            };

            // Assert
            dto.Title.Should().Be(longTitle);
            dto.Description.Should().Be(longDescription);
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleTechnologiesWithSpecialCharacters()
        {
            // Arrange
            var technologies = new[] { "C#", "F#", ".NET", "ASP.NET Core", "React.js", "Node.js", "Vue.js" };

            // Act
            var dto = new ProjectCreateRequest { Technologies = technologies };

            // Assert
            dto.Technologies.Should().BeEquivalentTo(technologies);
        }

        #endregion

        #region ProjectUpdateRequest Tests

        [Fact]
        public void ProjectUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new ProjectUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void ProjectUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var title = "Updated Project";
            var description = "Updated Description";
            var technologies = new[] { "TypeScript", "Angular" };

            // Act
            var dto = new ProjectUpdateRequest
            {
                Title = title,
                Description = description,
                Technologies = technologies,
                Featured = true
            };

            // Assert
            dto.Title.Should().Be(title);
            dto.Description.Should().Be(description);
            dto.Technologies.Should().BeEquivalentTo(technologies);
            dto.Featured.Should().BeTrue();
        }

        [Fact]
        public void ProjectUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new ProjectUpdateRequest
            {
                Title = null,
                Description = null,
                ImageUrl = null,
                DemoUrl = null,
                GithubUrl = null,
                Technologies = null,
                Featured = null
            };

            // Assert
            dto.Title.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.ImageUrl.Should().BeNull();
            dto.DemoUrl.Should().BeNull();
            dto.GithubUrl.Should().BeNull();
            dto.Technologies.Should().BeNull();
            dto.Featured.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void ProjectUpdateRequest_ShouldHandleNullableFeatured(bool? featured)
        {
            // Act
            var dto = new ProjectUpdateRequest { Featured = featured };

            // Assert
            dto.Featured.Should().Be(featured);
        }

        [Fact]
        public void ProjectUpdateRequest_ShouldHandlePartialUpdates()
        {
            // Act
            var dto = new ProjectUpdateRequest
            {
                Title = "New Title",
                // Leave other fields null for partial update
                Description = null,
                Technologies = null
            };

            // Assert
            dto.Title.Should().Be("New Title");
            dto.Description.Should().BeNull();
            dto.Technologies.Should().BeNull();
        }

        #endregion

        #region ProjectResponse Tests

        [Fact]
        public void ProjectResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ProjectResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.Featured.Should().BeFalse();
            dto.CreatedAt.Should().Be(default(DateTime));
            dto.UpdatedAt.Should().Be(default(DateTime));
        }

        [Fact]
        public void ProjectResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var portfolioId = Guid.NewGuid();
            var title = "Test Project Response";
            var createdAt = DateTime.UtcNow.AddDays(-5);
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new ProjectResponse
            {
                Id = id,
                PortfolioId = portfolioId,
                Title = title,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                Featured = true
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Title.Should().Be(title);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
            dto.Featured.Should().BeTrue();
        }

        [Fact]
        public void ProjectResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ProjectResponse
            {
                Description = null,
                ImageUrl = null,
                DemoUrl = null,
                GithubUrl = null,
                Technologies = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.ImageUrl.Should().BeNull();
            dto.DemoUrl.Should().BeNull();
            dto.GithubUrl.Should().BeNull();
            dto.Technologies.Should().BeNull();
        }

        [Fact]
        public void ProjectResponse_ShouldHandleDateTimeVariations()
        {
            // Arrange
            var minDate = DateTime.MinValue;
            var maxDate = DateTime.MaxValue;
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            // Act
            var dto1 = new ProjectResponse { CreatedAt = minDate, UpdatedAt = maxDate };
            var dto2 = new ProjectResponse { CreatedAt = utcNow, UpdatedAt = localNow };

            // Assert
            dto1.CreatedAt.Should().Be(minDate);
            dto1.UpdatedAt.Should().Be(maxDate);
            dto2.CreatedAt.Should().Be(utcNow);
            dto2.UpdatedAt.Should().Be(localNow);
        }

        #endregion

        #region ProjectSummaryResponse Tests

        [Fact]
        public void ProjectSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new ProjectSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
            dto.Featured.Should().BeFalse();
        }

        [Fact]
        public void ProjectSummaryResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Summary Project";
            var description = "Summary Description";
            var technologies = new[] { "Python", "Django" };

            // Act
            var dto = new ProjectSummaryResponse
            {
                Id = id,
                Title = title,
                Description = description,
                Technologies = technologies,
                Featured = true
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.Title.Should().Be(title);
            dto.Description.Should().Be(description);
            dto.Technologies.Should().BeEquivalentTo(technologies);
            dto.Featured.Should().BeTrue();
        }

        [Fact]
        public void ProjectSummaryResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new ProjectSummaryResponse
            {
                Description = null,
                ImageUrl = null,
                DemoUrl = null,
                GithubUrl = null,
                Technologies = null
            };

            // Assert
            dto.Description.Should().BeNull();
            dto.ImageUrl.Should().BeNull();
            dto.DemoUrl.Should().BeNull();
            dto.GithubUrl.Should().BeNull();
            dto.Technologies.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public void AllProjectDTOs_ShouldHandleGuidEmpty()
        {
            // Act
            var createRequest = new ProjectCreateRequest { PortfolioId = Guid.Empty };
            var response = new ProjectResponse { Id = Guid.Empty, PortfolioId = Guid.Empty };
            var summaryResponse = new ProjectSummaryResponse { Id = Guid.Empty };

            // Assert
            createRequest.PortfolioId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.PortfolioId.Should().Be(Guid.Empty);
            summaryResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllProjectDTOs_ShouldHandleMaxGuidValues()
        {
            // Arrange
            var maxGuid = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");

            // Act
            var createRequest = new ProjectCreateRequest { PortfolioId = maxGuid };
            var response = new ProjectResponse { Id = maxGuid, PortfolioId = maxGuid };

            // Assert
            createRequest.PortfolioId.Should().Be(maxGuid);
            response.Id.Should().Be(maxGuid);
            response.PortfolioId.Should().Be(maxGuid);
        }

        [Fact]
        public void AllProjectDTOs_ShouldHandleWhitespaceStrings()
        {
            // Arrange
            var whitespace = "   \t\n\r   ";

            // Act
            var createRequest = new ProjectCreateRequest
            {
                Title = whitespace,
                Description = whitespace
            };

            var updateRequest = new ProjectUpdateRequest
            {
                Title = whitespace,
                Description = whitespace
            };

            // Assert
            createRequest.Title.Should().Be(whitespace);
            createRequest.Description.Should().Be(whitespace);
            updateRequest.Title.Should().Be(whitespace);
            updateRequest.Description.Should().Be(whitespace);
        }

        [Fact]
        public void AllProjectDTOs_ShouldHandleDuplicateTechnologies()
        {
            // Arrange
            var technologies = new[] { "React", "React", "JavaScript", "JavaScript" };

            // Act
            var createRequest = new ProjectCreateRequest { Technologies = technologies };
            var updateRequest = new ProjectUpdateRequest { Technologies = technologies };

            // Assert
            createRequest.Technologies.Should().HaveCount(4);
            createRequest.Technologies.Should().BeEquivalentTo(technologies);
            updateRequest.Technologies.Should().HaveCount(4);
            updateRequest.Technologies.Should().BeEquivalentTo(technologies);
        }

        [Fact]
        public void AllProjectDTOs_ShouldHandleInvalidUrls()
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
                var dto = new ProjectCreateRequest
                {
                    ImageUrl = url,
                    DemoUrl = url,
                    GithubUrl = url
                };

                dto.ImageUrl.Should().Be(url);
                dto.DemoUrl.Should().Be(url);
                dto.GithubUrl.Should().Be(url);
            }
        }

        [Fact]
        public void AllProjectDTOs_ShouldHandleVeryLongUrls()
        {
            // Arrange
            var longUrl = "https://example.com/" + new string('a', 10000);

            // Act
            var dto = new ProjectCreateRequest
            {
                ImageUrl = longUrl,
                DemoUrl = longUrl,
                GithubUrl = longUrl
            };

            // Assert
            dto.ImageUrl.Should().Be(longUrl);
            dto.DemoUrl.Should().Be(longUrl);
            dto.GithubUrl.Should().Be(longUrl);
        }

        [Fact]
        public void ProjectDTOs_ShouldBeSerializationFriendly()
        {
            // This test ensures DTOs have proper structure for serialization
            
            // Arrange
            var createRequest = new ProjectCreateRequest();
            var updateRequest = new ProjectUpdateRequest();
            var response = new ProjectResponse();
            var summaryResponse = new ProjectSummaryResponse();

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
        public void ProjectDTOs_ShouldHandleMemoryStress()
        {
            // Arrange & Act
            var projects = new List<ProjectResponse>();
            for (int i = 0; i < 1000; i++)
            {
                projects.Add(new ProjectResponse
                {
                    Id = Guid.NewGuid(),
                    Title = _fixture.Create<string>(),
                    Description = _fixture.Create<string>(),
                    Technologies = _fixture.CreateMany<string>(10).ToArray()
                });
            }

            // Assert - Should not throw OutOfMemoryException
            projects.Should().HaveCount(1000);
        }

        [Fact]
        public void ProjectCreateRequest_ShouldHandleConcurrentAccess()
        {
            // This test verifies thread safety of DTO instantiation
            var tasks = new List<Task<ProjectCreateRequest>>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() => new ProjectCreateRequest
                {
                    Title = $"Project {i}",
                    Technologies = new[] { "Tech1", "Tech2" }
                }));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert
            results.Should().HaveCount(100);
            results.Should().AllSatisfy(dto => dto.Should().NotBeNull());
        }

        #endregion
    }
} 