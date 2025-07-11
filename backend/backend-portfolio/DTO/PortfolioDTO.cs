using backend_portfolio.Models;

namespace backend_portfolio.DTO
{
    // Portfolio Response DTO
    public class PortfolioResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Phone { get; set; }
        public Visibility Visibility { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PortfolioTemplateSummaryDto? Template { get; set; }
        public List<ProjectSummaryDto> Projects { get; set; } = new();
        public List<ExperienceSummaryDto> Experiences { get; set; } = new();
        public List<PortfolioSkillSummaryDto> Skills { get; set; } = new();
        public List<BlogPostSummaryDto> BlogPosts { get; set; } = new();
    }

    // Portfolio Request DTO
    public class PortfolioRequestDto
    {
        public Guid UserId { get; set; }
        public Guid? TemplateId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Phone { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Public;
        public bool IsPublished { get; set; } = false;
    }

    // Portfolio Summary DTO (for list views)
    public class PortfolioSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public Visibility Visibility { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProjectCount { get; set; }
        public int ExperienceCount { get; set; }
        public int SkillCount { get; set; }
    }

    // Portfolio Update DTO
    public class PortfolioUpdateDto
    {
        public Guid? TemplateId { get; set; }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Phone { get; set; }
        public Visibility? Visibility { get; set; }
        public bool? IsPublished { get; set; }
    }
} 