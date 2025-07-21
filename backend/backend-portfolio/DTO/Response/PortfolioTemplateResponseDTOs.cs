namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// Standard portfolio template response DTO
    /// </summary>
    public class PortfolioTemplateResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Summary portfolio template response for list views
    /// </summary>
    public class PortfolioTemplateSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
} 