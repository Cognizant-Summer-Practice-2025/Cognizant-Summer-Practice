using backend_portfolio.Models;
using backend_portfolio.DTO.PortfolioTemplate.Response;
using backend_portfolio.DTO.Project.Response;
using backend_portfolio.DTO.Experience.Response;
using backend_portfolio.DTO.Skill.Response;
using backend_portfolio.DTO.BlogPost.Response;
using backend_portfolio.DTO.Bookmark.Response;

namespace backend_portfolio.DTO.Portfolio.Response
{
    /// <summary>
    /// Standard portfolio response DTO
    /// </summary>
    public class PortfolioResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int BookmarkCount { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Components { get; set; }
    }

    /// <summary>
    /// Detailed portfolio response with related entities
    /// </summary>
    public class PortfolioDetailResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int BookmarkCount { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Components { get; set; }
        public PortfolioTemplateSummaryResponse? Template { get; set; }
        public List<ProjectSummaryResponse> Projects { get; set; } = new();
        public List<ExperienceSummaryResponse> Experience { get; set; } = new();
        public List<SkillSummaryResponse> Skills { get; set; } = new();
        public List<BlogPostSummaryResponse> BlogPosts { get; set; } = new();
    }

    /// <summary>
    /// Summary portfolio response for list views
    /// </summary>
    public class PortfolioSummaryResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int BookmarkCount { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Components { get; set; }
        public PortfolioTemplateSummaryResponse? Template { get; set; }
    }

    /// <summary>
    /// Portfolio card response for home page display
    /// </summary>
    public class PortfolioCardResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int Bookmarks { get; set; }
        public string Date { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public bool Featured { get; set; }
        public string? TemplateName { get; set; }
    }

    /// <summary>
    /// Bulk portfolio content creation response
    /// </summary>
    public class BulkPortfolioContentResponse
    {
        public string Message { get; set; } = string.Empty;
        public int ProjectsCreated { get; set; }
        public int ExperienceCreated { get; set; }
        public int SkillsCreated { get; set; }
        public int BlogPostsCreated { get; set; }
        public bool PortfolioPublished { get; set; }
    }

    /// <summary>
    /// Comprehensive user portfolio data response
    /// </summary>
    public class UserPortfolioComprehensiveResponse
    {
        public Guid UserId { get; set; }
        public List<PortfolioSummaryResponse> Portfolios { get; set; } = new();
        public List<ProjectResponse> Projects { get; set; } = new();
        public List<ExperienceResponse> Experience { get; set; } = new();
        public List<SkillResponse> Skills { get; set; } = new();
        public List<BlogPostResponse> BlogPosts { get; set; } = new();
        public List<BookmarkResponse> Bookmarks { get; set; } = new();
        public List<PortfolioTemplateSummaryResponse> Templates { get; set; } = new();
    }
} 
