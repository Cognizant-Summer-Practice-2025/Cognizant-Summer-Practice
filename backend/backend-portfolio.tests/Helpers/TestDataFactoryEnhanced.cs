using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using backend_portfolio.Models;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.Skill.Response;

namespace backend_portfolio.tests.Helpers
{
    public static class TestDataFactoryEnhanced
    {
        private static readonly Fixture _fixture = new();

        static TestDataFactoryEnhanced()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    
            _fixture.Customizations.Add(new DateOnlyGenerator());
        }

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

        #region Portfolio Test Data

        public static Portfolio CreateValidPortfolio()
        {
            return _fixture.Build<Portfolio>()
                .With(p => p.Title, "Test Portfolio")
                .With(p => p.Bio, "A comprehensive portfolio showcasing skills and projects.")
                .With(p => p.Visibility, Visibility.Public)
                .With(p => p.IsPublished, true)
                .With(p => p.ViewCount, 0)
                .With(p => p.LikeCount, 0)
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Create();
        }

        public static Portfolio CreatePortfolioWithEdgeCases()
        {
            return _fixture.Build<Portfolio>()
                .With(p => p.Title, new string('A', 255))       
                .With(p => p.Bio, new string('B', 5000)) 
                .With(p => p.Visibility, Visibility.Private)
                .With(p => p.IsPublished, false)
                .With(p => p.ViewCount, int.MaxValue) 
                .With(p => p.LikeCount, int.MaxValue) 
                .Without(p => p.Template)
                .Without(p => p.Projects)
                .Without(p => p.Experience)
                .Without(p => p.Skills)
                .Without(p => p.BlogPosts)
                .Without(p => p.Bookmarks)
                .Create();
        }

        public static PortfolioCreateRequest CreateValidPortfolioRequest()
        {
            return _fixture.Build<PortfolioCreateRequest>()
                .With(r => r.Title, "New Portfolio")
                .With(r => r.Bio, "Portfolio description")
                .With(r => r.Visibility, Visibility.Public)
                .Create();
        }

        public static PortfolioCreateRequest CreateInvalidPortfolioRequest()
        {
            return _fixture.Build<PortfolioCreateRequest>()
                .With(r => r.Title, "") 
                .With(r => r.Bio, new string('X', 10000)) 
                .With(r => r.UserId, Guid.Empty) 
                .With(r => r.TemplateName, "") 
                .Create();
        }

        #endregion

        #region Project Test Data

        public static Project CreateValidProject()
        {
            return _fixture.Build<Project>()
                .With(p => p.Title, "Test Project")
                .With(p => p.Description, "A comprehensive project description.")
                .With(p => p.Technologies, new[] { "C#", ".NET", "React", "TypeScript" })
                .With(p => p.Featured, false)
                .Without(p => p.Portfolio)
                .Create();
        }

        public static Project CreateProjectWithEdgeCases()
        {
            return _fixture.Build<Project>()
                .With(p => p.Title, new string('T', 200)) 
                .With(p => p.Description, new string('D', 2000)) 
                .With(p => p.Technologies, new[] { new string('T', 100), new string('U', 100) })
                .With(p => p.Featured, true)
                .Without(p => p.Portfolio)
                .Create();
        }

        public static ProjectCreateRequest CreateValidProjectRequest()
        {
            return _fixture.Build<ProjectCreateRequest>()
                .With(r => r.Title, "New Project")
                .With(r => r.Description, "Project description")
                .With(r => r.Technologies, new[] { "React", "Node.js" })
                .Create();
        }

        #endregion

        #region Skill Test Data

        public static Skill CreateValidSkill()
        {
            return _fixture.Build<Skill>()
                .With(s => s.Name, "C# Programming")
                .With(s => s.CategoryType, "Technical")
                .With(s => s.Subcategory, "Programming Languages")
                .With(s => s.Category, "Backend Development")
                .With(s => s.ProficiencyLevel, 8)
                .With(s => s.DisplayOrder, 1)
                .Without(s => s.Portfolio)
                .Create();
        }

        public static Skill CreateSkillWithEdgeCases()
        {
            return _fixture.Build<Skill>()
                .With(s => s.Name, new string('S', 100)) 
                .With(s => s.CategoryType, new string('C', 50)) 
                .With(s => s.Subcategory, new string('S', 100))
                .With(s => s.Category, new string('C', 255)) 
                .With(s => s.ProficiencyLevel, 10) 
                .With(s => s.DisplayOrder, int.MaxValue) 
                .Without(s => s.Portfolio)
                .Create();
        }

        public static SkillCreateRequest CreateValidSkillRequest()
        {
            return _fixture.Build<SkillCreateRequest>()
                .With(r => r.Name, "JavaScript")
                .With(r => r.CategoryType, "Technical")
                .With(r => r.ProficiencyLevel, 7)
                .Create();
        }

        #endregion

        #region Experience Test Data

        public static Experience CreateValidExperience()
        {
            return _fixture.Build<Experience>()
                .With(e => e.CompanyName, "Tech Company Inc.")
                .With(e => e.JobTitle, "Senior Software Developer")
                .With(e => e.Description, "Developed and maintained web applications using modern technologies.")
                .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)))
                .With(e => e.EndDate, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)))
                .With(e => e.IsCurrent, false)
                .Without(e => e.Portfolio)
                .Create();
        }

        public static Experience CreateCurrentExperience()
        {
            return _fixture.Build<Experience>()
                .With(e => e.CompanyName, "Current Company")
                .With(e => e.JobTitle, "Lead Developer")
                .With(e => e.Description, "Currently working on cutting-edge projects.")
                .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)))
                .With(e => e.EndDate, (DateOnly?)null) 
                .With(e => e.IsCurrent, true)
                .Without(e => e.Portfolio)
                .Create();
        }

        #endregion

        #region BlogPost Test Data

        public static BlogPost CreateValidBlogPost()
        {
            return _fixture.Build<BlogPost>()
                .With(b => b.Title, "Introduction to Clean Architecture")
                .With(b => b.Content, "Clean architecture is a software design philosophy...")
                .With(b => b.Excerpt, "An overview of clean architecture principles.")
                .With(b => b.Tags, new[] { "architecture", "clean-code", "software-design" })
                .With(b => b.IsPublished, true)
                .With(b => b.PublishedAt, DateTime.UtcNow.AddDays(-7))
                .Without(b => b.Portfolio)
                .Create();
        }

        public static BlogPost CreateUnpublishedBlogPost()
        {
            return _fixture.Build<BlogPost>()
                .With(b => b.Title, "Draft Blog Post")
                .With(b => b.Content, "This is a draft post...")
                .With(b => b.IsPublished, false)
                .With(b => b.PublishedAt, (DateTime?)null)
                .Without(b => b.Portfolio)
                .Create();
        }

        #endregion

        #region Bookmark Test Data

        public static Bookmark CreateValidBookmark()
        {
            return _fixture.Build<Bookmark>()
                .With(b => b.CollectionName, "Development Resources")
                .With(b => b.Notes, "A helpful resource for developers.")
                .Without(b => b.Portfolio)
                .Create();
        }

        #endregion

        #region Template Test Data

        public static PortfolioTemplate CreateValidTemplate()
        {
            return _fixture.Build<PortfolioTemplate>()
                .With(t => t.Name, "Modern Portfolio Template")
                .With(t => t.Description, "A clean and modern portfolio template.")
                .With(t => t.PreviewImageUrl, "https://example.com/preview.jpg")
                .With(t => t.IsActive, true)
                .Without(t => t.Portfolios)
                .Create();
        }

        #endregion

        #region Utility Methods

        public static List<T> CreateMany<T>(int count = 3) where T : class
        {
            return _fixture.CreateMany<T>(count).ToList();
        }

        public static Portfolio CreateCompletePortfolio()
        {
            var template = CreateValidTemplate();
            var portfolio = CreateValidPortfolio();
            portfolio.Template = template;
            portfolio.Projects = new List<Project> { CreateValidProject(), CreateProjectWithEdgeCases() };
            portfolio.Skills = new List<Skill> { CreateValidSkill(), CreateSkillWithEdgeCases() };
            portfolio.Experience = new List<Experience> { CreateValidExperience(), CreateCurrentExperience() };
            portfolio.BlogPosts = new List<BlogPost> { CreateValidBlogPost(), CreateUnpublishedBlogPost() };
            portfolio.Bookmarks = new List<Bookmark> { CreateValidBookmark() };
            
            return portfolio;
        }

        public static string CreateLongString(int length)
        {
            return new string('A', length);
        }

        public static string CreateValidUrl(string domain = "example.com")
        {
            return $"https://{domain}/path/to/resource";
        }

        public static string CreateInvalidUrl()
        {
            return "not-a-valid-url";
        }

        public static DateTime CreatePastDate(int daysAgo = 30)
        {
            return DateTime.UtcNow.AddDays(-daysAgo);
        }

        public static DateTime CreateFutureDate(int daysInFuture = 30)
        {
            return DateTime.UtcNow.AddDays(daysInFuture);
        }

        #endregion

        #region Edge Case Scenarios

        public static class EdgeCases
        {
            public static Portfolio PortfolioWithNullOptionalFields()
            {
                return _fixture.Build<Portfolio>()
                    .With(p => p.Title, "Required Title")
                    .With(p => p.Bio, (string?)null)
                    .With(p => p.Components, (string?)null)
                    .Without(p => p.Template)
                    .Without(p => p.Projects)
                    .Without(p => p.Experience)
                    .Without(p => p.Skills)
                    .Without(p => p.BlogPosts)
                    .Without(p => p.Bookmarks)
                    .Create();
            }

            public static Project ProjectWithMinimalData()
            {
                return _fixture.Build<Project>()
                    .With(p => p.Title, "Minimal Project")
                    .With(p => p.Description, (string?)null)
                    .With(p => p.Technologies, (string[]?)null)
                    .With(p => p.ImageUrl, (string?)null)
                    .With(p => p.DemoUrl, (string?)null)
                    .With(p => p.GithubUrl, (string?)null)
                    .Without(p => p.Portfolio)
                    .Create();
            }

            public static Skill SkillWithMinimalData()
            {
                return _fixture.Build<Skill>()
                    .With(s => s.Name, "Basic Skill")
                    .With(s => s.CategoryType, (string?)null)
                    .With(s => s.Subcategory, (string?)null)
                    .With(s => s.Category, (string?)null)
                    .With(s => s.ProficiencyLevel, (int?)null)
                    .With(s => s.DisplayOrder, (int?)null)
                    .Without(s => s.Portfolio)
                    .Create();
            }

            public static List<string> ValidationErrorMessages()
            {
                return new List<string>
                {
                    "Title is required and cannot be empty.",
                    "Description exceeds maximum length of 2000 characters.",
                    "Email format is invalid.",
                    "URL format is invalid.",
                    "Date cannot be in the future.",
                    "Proficiency level must be between 1 and 10.",
                    "Portfolio ID cannot be empty."
                };
            }
        }

        #endregion
    }
} 