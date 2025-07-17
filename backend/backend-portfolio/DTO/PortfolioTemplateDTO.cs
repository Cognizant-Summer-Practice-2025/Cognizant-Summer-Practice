namespace backend_portfolio.DTO
{
    // PortfolioTemplate Response DTO
    public class PortfolioTemplateResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // PortfolioTemplate Request DTO
    public class PortfolioTemplateRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // PortfolioTemplate Update DTO
    public class PortfolioTemplateUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    // PortfolioTemplate Summary DTO (for list views)
    public class PortfolioTemplateSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
