namespace backend_portfolio.DTO.Project.Request
{
    /// <summary>
    /// DTO for creating a new project
    /// </summary>
    public class ProjectCreateRequest
    {
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DemoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string[]? Technologies { get; set; }
        public bool Featured { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing project
    /// </summary>
    public class ProjectUpdateRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? DemoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string[]? Technologies { get; set; }
        public bool? Featured { get; set; }
    }
} 
