using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    [Table("blog_posts")]
    public class BlogPost
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("portfolio_id")]
        public Guid PortfolioId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("excerpt", TypeName = "text")]
        public string? Excerpt { get; set; }

        [Column("content", TypeName = "text")]
        public string? Content { get; set; }

        [Column("featured_image_url", TypeName = "text")]
        public string? FeaturedImageUrl { get; set; }

        [Column("tags")]
        public string[]? Tags { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 