namespace backend_portfolio.DTO
{
    // PortfolioTemplate Response DTO
    public class PortfolioTemplateResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public string? CssConfig { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UsageCount { get; set; }
    }

    // PortfolioTemplate Request DTO
    public class PortfolioTemplateRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public string? CssConfig { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // PortfolioTemplate Summary DTO
    public class PortfolioTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 