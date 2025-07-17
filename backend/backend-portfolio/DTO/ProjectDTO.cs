namespace backend_portfolio.DTO
{
    // Project Response DTO
    public class ProjectResponseDto
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

    // Project Request DTO
    public class ProjectRequestDto
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

    // Project Summary DTO
    public class ProjectSummaryDto
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

    // Project Update DTO
    public class ProjectUpdateDto
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