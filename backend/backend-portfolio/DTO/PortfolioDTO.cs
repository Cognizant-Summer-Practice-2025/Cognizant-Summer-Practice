using backend_portfolio.Models;

namespace backend_portfolio.DTO
{
    // Portfolio Response DTO
    public class PortfolioResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Components { get; set; }
        public PortfolioTemplateSummaryDto? Template { get; set; }
        public List<ProjectSummaryDto> Projects { get; set; } = new();
        public List<ExperienceSummaryDto> Experience { get; set; } = new();
        public List<SkillSummaryDto> Skills { get; set; } = new();
        public List<BlogPostSummaryDto> BlogPosts { get; set; } = new();
    }

    // Portfolio Request DTO
    public class PortfolioRequestDto
    {
        public Guid UserId { get; set; }
        public string TemplateName { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Public;
        public bool IsPublished { get; set; } = false;
        public string? Components { get; set; }
    }

    // Portfolio Summary DTO (for list views)
    public class PortfolioSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public Visibility Visibility { get; set; }
        public bool IsPublished { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Components { get; set; }
        public PortfolioTemplateSummaryDto? Template { get; set; }
    }

    // Portfolio Update DTO
    public class PortfolioUpdateDto
    {
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public Visibility? Visibility { get; set; }
        public bool? IsPublished { get; set; }
        public string? Components { get; set; }
        public string? TemplateName { get; set; } 
    }

    // Bulk Portfolio Content DTO for publishing
    public class BulkPortfolioContentDto
    {
        public Guid PortfolioId { get; set; }
        public List<ProjectRequestDto>? Projects { get; set; }
        public List<ExperienceRequestDto>? Experience { get; set; }
        public List<SkillRequestDto>? Skills { get; set; }
        public List<BlogPostRequestDto>? BlogPosts { get; set; }
        public bool PublishPortfolio { get; set; } = true;
    }

    // Bulk Portfolio Response DTO
    public class BulkPortfolioResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int ProjectsCreated { get; set; }
        public int ExperienceCreated { get; set; }
        public int SkillsCreated { get; set; }
        public int BlogPostsCreated { get; set; }
        public bool PortfolioPublished { get; set; }
    }

    // User Portfolio Comprehensive DTO 
    public class UserPortfolioComprehensiveDto
    {
        public Guid UserId { get; set; }
        public List<PortfolioSummaryDto> Portfolios { get; set; } = new();
        public List<ProjectResponseDto> Projects { get; set; } = new();
        public List<ExperienceResponseDto> Experience { get; set; } = new();
        public List<SkillResponseDto> Skills { get; set; } = new();
        public List<BlogPostResponseDto> BlogPosts { get; set; } = new();
        public List<BookmarkResponseDto> Bookmarks { get; set; } = new();
        public List<PortfolioTemplateSummaryDto> Templates { get; set; } = new();
    }
}
