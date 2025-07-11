using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class BlogPost
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty;

        public string? Excerpt { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? FeaturedImageUrl { get; set; }

        public int? ReadingTimeMinutes { get; set; }

        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
        public virtual ICollection<BlogPostTag> Tags { get; set; } = new List<BlogPostTag>();
    }
} 