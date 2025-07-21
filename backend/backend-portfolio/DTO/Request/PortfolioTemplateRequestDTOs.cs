namespace backend_portfolio.DTO.Request
{
    /// <summary>
    /// DTO for creating a new portfolio template
    /// </summary>
    public class PortfolioTemplateCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing portfolio template
    /// </summary>
    public class PortfolioTemplateUpdateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PreviewImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }
} 