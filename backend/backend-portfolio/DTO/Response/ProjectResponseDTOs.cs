namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// Standard project response DTO
    /// </summary>
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DemoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string[]? Technologies { get; set; }
        public bool Featured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Summary project response for list views
    /// </summary>
    public class ProjectSummaryResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DemoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string[]? Technologies { get; set; }
        public bool Featured { get; set; }
    }
} 