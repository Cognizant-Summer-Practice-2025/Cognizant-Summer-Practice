namespace backend_portfolio.DTO.BlogPost.Request
{
    /// <summary>
    /// DTO for creating a new blog post
    /// </summary>
    public class BlogPostCreateRequest
    {
        public Guid PortfolioId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool IsPublished { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing blog post
    /// </summary>
    public class BlogPostUpdateRequest
    {
        public string? Title { get; set; }
        public string? Excerpt { get; set; }
        public string? Content { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public bool? IsPublished { get; set; }
    }
} 
