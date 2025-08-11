using Xunit;
using FluentAssertions;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.Models;
using AutoFixture;
using AutoFixture.Kernel;

namespace backend_portfolio.tests.DTO
{
    public class PortfolioDTOTests
    {
        private readonly Fixture _fixture;

        public PortfolioDTOTests()
        {
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

        #region PortfolioCreateRequest Tests

        [Fact]
        public void PortfolioCreateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioCreateRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.Title.Should().NotBeNull();
            dto.TemplateName.Should().NotBeNull();
            dto.Visibility.Should().Be(Visibility.Public);
            dto.IsPublished.Should().BeFalse();
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var title = "Test Portfolio";
            var bio = "Test Bio";

            // Act
            var dto = new PortfolioCreateRequest
            {
                UserId = userId,
                Title = title,
                Bio = bio,
                Visibility = Visibility.Private,
                IsPublished = true
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(Visibility.Private);
            dto.IsPublished.Should().BeTrue();
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldHandleEmptyGuid()
        {
            // Act
            var dto = new PortfolioCreateRequest
            {
                UserId = Guid.Empty
            };

            // Assert
            dto.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldHandleNullBio()
        {
            // Act
            var dto = new PortfolioCreateRequest
            {
                Bio = null
            };

            // Assert
            dto.Bio.Should().BeNull();
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldHandleNullComponents()
        {
            // Act
            var dto = new PortfolioCreateRequest
            {
                Components = null
            };

            // Assert
            dto.Components.Should().BeNull();
        }

        [Fact]
        public void PortfolioCreateRequest_ShouldHandleAllVisibilityTypes()
        {
            // Arrange & Act & Assert
            var publicDto = new PortfolioCreateRequest { Visibility = Visibility.Public };
            publicDto.Visibility.Should().Be(Visibility.Public);

            var privateDto = new PortfolioCreateRequest { Visibility = Visibility.Private };
            privateDto.Visibility.Should().Be(Visibility.Private);

            var unlistedDto = new PortfolioCreateRequest { Visibility = Visibility.Unlisted };
            unlistedDto.Visibility.Should().Be(Visibility.Unlisted);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("A")]
        [InlineData("Very Long Portfolio Title That Should Still Be Accepted")]
        public void PortfolioCreateRequest_ShouldHandleVariousTitleFormats(string title)
        {
            // Act
            var dto = new PortfolioCreateRequest { Title = title };

            // Assert
            dto.Title.Should().Be(title);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("template1")]
        [InlineData("Very-Long_Template.Name")]
        public void PortfolioCreateRequest_ShouldHandleVariousTemplateNames(string templateName)
        {
            // Act
            var dto = new PortfolioCreateRequest { TemplateName = templateName };

            // Assert
            dto.TemplateName.Should().Be(templateName);
        }

        #endregion

        #region PortfolioUpdateRequest Tests

        [Fact]
        public void PortfolioUpdateRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioUpdateRequest();

            // Assert
            dto.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioUpdateRequest_ShouldAcceptValidValues()
        {
            // Arrange
            var title = "Updated Portfolio";
            var bio = "Updated Bio";

            // Act
            var dto = new PortfolioUpdateRequest
            {
                Title = title,
                Bio = bio,
                Visibility = Visibility.Public,
                IsPublished = false
            };

            // Assert
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(Visibility.Public);
            dto.IsPublished.Should().BeFalse();
        }

        [Fact]
        public void PortfolioUpdateRequest_ShouldHandleAllNullValues()
        {
            // Act
            var dto = new PortfolioUpdateRequest
            {
                Title = null,
                Bio = null,
                Visibility = null,
                IsPublished = null,
                Components = null,
                TemplateName = null
            };

            // Assert
            dto.Title.Should().BeNull();
            dto.Bio.Should().BeNull();
            dto.Visibility.Should().BeNull();
            dto.IsPublished.Should().BeNull();
            dto.Components.Should().BeNull();
            dto.TemplateName.Should().BeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void PortfolioUpdateRequest_ShouldHandleNullableIsPublished(bool? isPublished)
        {
            // Act
            var dto = new PortfolioUpdateRequest { IsPublished = isPublished };

            // Assert
            dto.IsPublished.Should().Be(isPublished);
        }

        #endregion

        #region BulkPortfolioContentRequest Tests

        [Fact]
        public void BulkPortfolioContentRequest_ShouldBeInstantiable()
        {
            // Act
            var dto = new BulkPortfolioContentRequest();

            // Assert
            dto.Should().NotBeNull();
            dto.PortfolioId.Should().Be(Guid.Empty);
            dto.PublishPortfolio.Should().BeTrue();
        }

        [Fact]
        public void BulkPortfolioContentRequest_ShouldAcceptValidData()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var projects = new List<ProjectCreateRequest> { new() { Title = "Test Project" } };
            var experiences = new List<ExperienceCreateRequest> { new() { JobTitle = "Test Job" } };
            var skills = new List<SkillCreateRequest> { new() { Name = "Test Skill" } };
            var blogPosts = new List<BlogPostCreateRequest> { new() { Title = "Test Post" } };

            // Act
            var dto = new BulkPortfolioContentRequest
            {
                PortfolioId = portfolioId,
                Projects = projects,
                Experience = experiences,
                Skills = skills,
                BlogPosts = blogPosts,
                PublishPortfolio = false
            };

            // Assert
            dto.PortfolioId.Should().Be(portfolioId);
            dto.Projects.Should().NotBeNull().And.HaveCount(1);
            dto.Experience.Should().NotBeNull().And.HaveCount(1);
            dto.Skills.Should().NotBeNull().And.HaveCount(1);
            dto.BlogPosts.Should().NotBeNull().And.HaveCount(1);
            dto.PublishPortfolio.Should().BeFalse();
        }

        [Fact]
        public void BulkPortfolioContentRequest_ShouldHandleNullCollections()
        {
            // Act
            var dto = new BulkPortfolioContentRequest
            {
                Projects = null,
                Experience = null,
                Skills = null,
                BlogPosts = null
            };

            // Assert
            dto.Projects.Should().BeNull();
            dto.Experience.Should().BeNull();
            dto.Skills.Should().BeNull();
            dto.BlogPosts.Should().BeNull();
        }

        [Fact]
        public void BulkPortfolioContentRequest_ShouldHandleEmptyCollections()
        {
            // Act
            var dto = new BulkPortfolioContentRequest
            {
                Projects = new List<ProjectCreateRequest>(),
                Experience = new List<ExperienceCreateRequest>(),
                Skills = new List<SkillCreateRequest>(),
                BlogPosts = new List<BlogPostCreateRequest>()
            };

            // Assert
            dto.Projects.Should().NotBeNull().And.BeEmpty();
            dto.Experience.Should().NotBeNull().And.BeEmpty();
            dto.Skills.Should().NotBeNull().And.BeEmpty();
            dto.BlogPosts.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region PortfolioResponse Tests

        [Fact]
        public void PortfolioResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(Guid.Empty);
            dto.UserId.Should().Be(Guid.Empty);
            dto.Title.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioResponse_ShouldAcceptValidValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var title = "Test Portfolio";
            var bio = "Test Bio";
            var visibility = Visibility.Public;
            var isPublished = true;
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow;

            // Act
            var dto = new PortfolioResponse
            {
                Id = id,
                UserId = userId,
                Title = title,
                Bio = bio,
                Visibility = visibility,
                IsPublished = isPublished,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            dto.Id.Should().Be(id);
            dto.UserId.Should().Be(userId);
            dto.Title.Should().Be(title);
            dto.Bio.Should().Be(bio);
            dto.Visibility.Should().Be(visibility);
            dto.IsPublished.Should().Be(isPublished);
            dto.CreatedAt.Should().Be(createdAt);
            dto.UpdatedAt.Should().Be(updatedAt);
        }

        [Fact]
        public void PortfolioResponse_ShouldHandleCounterDefaults()
        {
            // Act
            var dto = new PortfolioResponse();

            // Assert
            dto.ViewCount.Should().Be(0);
            dto.LikeCount.Should().Be(0);
            dto.BookmarkCount.Should().Be(0);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void PortfolioResponse_ShouldHandleVariousCountValues(int count)
        {
            // Act
            var dto = new PortfolioResponse
            {
                ViewCount = count,
                LikeCount = count,
                BookmarkCount = count
            };

            // Assert
            dto.ViewCount.Should().Be(count);
            dto.LikeCount.Should().Be(count);
            dto.BookmarkCount.Should().Be(count);
        }

        #endregion

        #region PortfolioDetailResponse Tests

        [Fact]
        public void PortfolioDetailResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioDetailResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Projects.Should().NotBeNull().And.BeEmpty();
            dto.Experience.Should().NotBeNull().And.BeEmpty();
            dto.Skills.Should().NotBeNull().And.BeEmpty();
            dto.BlogPosts.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void PortfolioDetailResponse_ShouldAcceptCollections()
        {
            // Arrange
            var projects = new List<ProjectSummaryResponse> { new() { Title = "Test Project" } };
            var experience = new List<ExperienceSummaryResponse> { new() { JobTitle = "Test Job" } };
            var skills = new List<SkillSummaryResponse> { new() { Name = "Test Skill" } };
            var blogPosts = new List<BlogPostSummaryResponse> { new() { Title = "Test Post" } };
            var template = new PortfolioTemplateSummaryResponse { Name = "Test Template" };

            // Act
            var dto = new PortfolioDetailResponse
            {
                Projects = projects,
                Experience = experience,
                Skills = skills,
                BlogPosts = blogPosts,
                Template = template
            };

            // Assert
            dto.Projects.Should().HaveCount(1);
            dto.Experience.Should().HaveCount(1);
            dto.Skills.Should().HaveCount(1);
            dto.BlogPosts.Should().HaveCount(1);
            dto.Template.Should().NotBeNull();
            dto.Template.Name.Should().Be("Test Template");
        }

        [Fact]
        public void PortfolioDetailResponse_ShouldHandleNullTemplate()
        {
            // Act
            var dto = new PortfolioDetailResponse { Template = null };

            // Assert
            dto.Template.Should().BeNull();
        }

        #endregion

        #region PortfolioSummaryResponse Tests

        [Fact]
        public void PortfolioSummaryResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioSummaryResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Template.Should().BeNull();
        }

        [Fact]
        public void PortfolioSummaryResponse_ShouldAcceptTemplate()
        {
            // Arrange
            var template = new PortfolioTemplateSummaryResponse { Name = "Test Template" };

            // Act
            var dto = new PortfolioSummaryResponse { Template = template };

            // Assert
            dto.Template.Should().NotBeNull();
            dto.Template.Name.Should().Be("Test Template");
        }

        #endregion

        #region PortfolioCardResponse Tests

        [Fact]
        public void PortfolioCardResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new PortfolioCardResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Skills.Should().NotBeNull().And.BeEmpty();
            dto.Name.Should().NotBeNull();
            dto.Role.Should().NotBeNull();
            dto.Location.Should().NotBeNull();
            dto.Description.Should().NotBeNull();
            dto.Date.Should().NotBeNull();
        }

        [Fact]
        public void PortfolioCardResponse_ShouldAcceptValidData()
        {
            // Arrange
            var skills = new List<string> { "C#", "React", "SQL" };

            // Act
            var dto = new PortfolioCardResponse
            {
                Name = "John Doe",
                Role = "Software Developer",
                Location = "New York",
                Description = "Experienced developer",
                Skills = skills,
                Views = 100,
                Likes = 50,
                Comments = 25,
                Bookmarks = 10,
                Featured = true,
                Avatar = "avatar.jpg",
                TemplateName = "Modern"
            };

            // Assert
            dto.Name.Should().Be("John Doe");
            dto.Role.Should().Be("Software Developer");
            dto.Location.Should().Be("New York");
            dto.Description.Should().Be("Experienced developer");
            dto.Skills.Should().HaveCount(3).And.Contain("C#");
            dto.Views.Should().Be(100);
            dto.Likes.Should().Be(50);
            dto.Comments.Should().Be(25);
            dto.Bookmarks.Should().Be(10);
            dto.Featured.Should().BeTrue();
            dto.Avatar.Should().Be("avatar.jpg");
            dto.TemplateName.Should().Be("Modern");
        }

        [Fact]
        public void PortfolioCardResponse_ShouldHandleNullOptionalFields()
        {
            // Act
            var dto = new PortfolioCardResponse
            {
                Avatar = null,
                TemplateName = null
            };

            // Assert
            dto.Avatar.Should().BeNull();
            dto.TemplateName.Should().BeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void PortfolioCardResponse_ShouldHandleVariousCountValues(int count)
        {
            // Act
            var dto = new PortfolioCardResponse
            {
                Views = count,
                Likes = count,
                Comments = count,
                Bookmarks = count
            };

            // Assert
            dto.Views.Should().Be(count);
            dto.Likes.Should().Be(count);
            dto.Comments.Should().Be(count);
            dto.Bookmarks.Should().Be(count);
        }

        #endregion

        #region BulkPortfolioContentResponse Tests

        [Fact]
        public void BulkPortfolioContentResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new BulkPortfolioContentResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Message.Should().NotBeNull();
            dto.ProjectsCreated.Should().Be(0);
            dto.ExperienceCreated.Should().Be(0);
            dto.SkillsCreated.Should().Be(0);
            dto.BlogPostsCreated.Should().Be(0);
            dto.PortfolioPublished.Should().BeFalse();
        }

        [Fact]
        public void BulkPortfolioContentResponse_ShouldAcceptValidData()
        {
            // Act
            var dto = new BulkPortfolioContentResponse
            {
                Message = "Content created successfully",
                ProjectsCreated = 5,
                ExperienceCreated = 3,
                SkillsCreated = 10,
                BlogPostsCreated = 2,
                PortfolioPublished = true
            };

            // Assert
            dto.Message.Should().Be("Content created successfully");
            dto.ProjectsCreated.Should().Be(5);
            dto.ExperienceCreated.Should().Be(3);
            dto.SkillsCreated.Should().Be(10);
            dto.BlogPostsCreated.Should().Be(2);
            dto.PortfolioPublished.Should().BeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1000)]
        public void BulkPortfolioContentResponse_ShouldHandleVariousCreatedCounts(int count)
        {
            // Act
            var dto = new BulkPortfolioContentResponse
            {
                ProjectsCreated = count,
                ExperienceCreated = count,
                SkillsCreated = count,
                BlogPostsCreated = count
            };

            // Assert
            dto.ProjectsCreated.Should().Be(count);
            dto.ExperienceCreated.Should().Be(count);
            dto.SkillsCreated.Should().Be(count);
            dto.BlogPostsCreated.Should().Be(count);
        }

        #endregion

        #region UserPortfolioComprehensiveResponse Tests

        [Fact]
        public void UserPortfolioComprehensiveResponse_ShouldBeInstantiable()
        {
            // Act
            var dto = new UserPortfolioComprehensiveResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.UserId.Should().Be(Guid.Empty);
            dto.Portfolios.Should().NotBeNull().And.BeEmpty();
            dto.Projects.Should().NotBeNull().And.BeEmpty();
            dto.Experience.Should().NotBeNull().And.BeEmpty();
            dto.Skills.Should().NotBeNull().And.BeEmpty();
            dto.BlogPosts.Should().NotBeNull().And.BeEmpty();
            dto.Bookmarks.Should().NotBeNull().And.BeEmpty();
            dto.Templates.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void UserPortfolioComprehensiveResponse_ShouldAcceptCollections()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolios = new List<PortfolioSummaryResponse> { new() };
            var projects = new List<ProjectResponse> { new() };
            var experience = new List<ExperienceResponse> { new() };
            var skills = new List<SkillResponse> { new() };
            var blogPosts = new List<BlogPostResponse> { new() };
            var bookmarks = new List<BookmarkResponse> { new() };
            var templates = new List<PortfolioTemplateSummaryResponse> { new() };

            // Act
            var dto = new UserPortfolioComprehensiveResponse
            {
                UserId = userId,
                Portfolios = portfolios,
                Projects = projects,
                Experience = experience,
                Skills = skills,
                BlogPosts = blogPosts,
                Bookmarks = bookmarks,
                Templates = templates
            };

            // Assert
            dto.UserId.Should().Be(userId);
            dto.Portfolios.Should().HaveCount(1);
            dto.Projects.Should().HaveCount(1);
            dto.Experience.Should().HaveCount(1);
            dto.Skills.Should().HaveCount(1);
            dto.BlogPosts.Should().HaveCount(1);
            dto.Bookmarks.Should().HaveCount(1);
            dto.Templates.Should().HaveCount(1);
        }

        [Fact]
        public void UserPortfolioComprehensiveResponse_ShouldHandleLargeCollections()
        {
            // Arrange
            var portfolios = _fixture.CreateMany<PortfolioSummaryResponse>(100).ToList();
            var projects = _fixture.CreateMany<ProjectResponse>(50).ToList();

            // Act
            var dto = new UserPortfolioComprehensiveResponse
            {
                Portfolios = portfolios,
                Projects = projects
            };

            // Assert
            dto.Portfolios.Should().HaveCount(100);
            dto.Projects.Should().HaveCount(50);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Fact]
        public void AllPortfolioDTOs_ShouldHandleGuidEmpty()
        {
            // Arrange & Act
            var createRequest = new PortfolioCreateRequest { UserId = Guid.Empty };
            var response = new PortfolioResponse { Id = Guid.Empty, UserId = Guid.Empty };
            var detailResponse = new PortfolioDetailResponse { Id = Guid.Empty };

            // Assert
            createRequest.UserId.Should().Be(Guid.Empty);
            response.Id.Should().Be(Guid.Empty);
            response.UserId.Should().Be(Guid.Empty);
            detailResponse.Id.Should().Be(Guid.Empty);
        }

        [Fact]
        public void AllPortfolioDTOs_ShouldHandleExtremeStringLengths()
        {
            // Arrange
            var emptyString = "";
            var longString = new string('a', 10000);

            // Act
            var createRequest = new PortfolioCreateRequest
            {
                Title = longString,
                Bio = emptyString,
                TemplateName = longString
            };

            var updateRequest = new PortfolioUpdateRequest
            {
                Title = longString,
                Bio = emptyString
            };

            // Assert
            createRequest.Title.Should().Be(longString);
            createRequest.Bio.Should().Be(emptyString);
            updateRequest.Title.Should().Be(longString);
            updateRequest.Bio.Should().Be(emptyString);
        }

        [Fact]
        public void AllPortfolioDTOs_ShouldHandleSpecialCharacters()
        {
            // Arrange
            var specialChars = "!@#$%^&*()_+{}|:<>?[];',./\"\\`~";

            // Act
            var dto = new PortfolioCreateRequest
            {
                Title = specialChars,
                Bio = specialChars,
                TemplateName = specialChars
            };

            // Assert
            dto.Title.Should().Be(specialChars);
            dto.Bio.Should().Be(specialChars);
            dto.TemplateName.Should().Be(specialChars);
        }

        [Fact]
        public void AllPortfolioDTOs_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicode = "测试中文字符 🚀 emoji 日本語 العربية";

            // Act
            var dto = new PortfolioCreateRequest
            {
                Title = unicode,
                Bio = unicode
            };

            // Assert
            dto.Title.Should().Be(unicode);
            dto.Bio.Should().Be(unicode);
        }

        #endregion
    }
} 