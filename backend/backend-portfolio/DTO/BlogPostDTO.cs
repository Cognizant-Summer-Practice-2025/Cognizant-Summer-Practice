namespace backend_portfolio.DTO
{
    // BlogPost Response DTO
    public class BlogPostResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // BlogPost Request DTO
    public class BlogPostRequestDto
    {
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool IsPublished { get; set; } = false;
    }

    // BlogPost Summary DTO
    public class BlogPostSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // BlogPost Update DTO
    public class BlogPostUpdateDto
    {
        public string? Title { get; set; }
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool? IsPublished { get; set; }
    }
}
