namespace backend_portfolio.DTO
{
    // BlogPost Response DTO
    public class BlogPostResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public int? ReadingTimeMinutes { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BlogPostTagSummaryDto> Tags { get; set; } = new();
    }

    // BlogPost Request DTO
    public class BlogPostRequestDto
    {
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public int? ReadingTimeMinutes { get; set; }
        public bool IsPublished { get; set; } = false;
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    // BlogPost Summary DTO
    public class BlogPostSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public int? ReadingTimeMinutes { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BlogPostTagSummaryDto> Tags { get; set; } = new();
    }

    // BlogPost Update DTO
    public class BlogPostUpdateDto
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public int? ReadingTimeMinutes { get; set; }
        public bool? IsPublished { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public List<string>? Tags { get; set; }
    }

    // BlogPostTag Response DTO
    public class BlogPostTagResponseDto
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // BlogPostTag Request DTO
    public class BlogPostTagRequestDto
    {
        public Guid BlogPostId { get; set; }
        public string TagName { get; set; } = string.Empty;
    }

    // BlogPostTag Summary DTO
    public class BlogPostTagSummaryDto
    {
        public Guid Id { get; set; }
        public string TagName { get; set; } = string.Empty;
    }
} 