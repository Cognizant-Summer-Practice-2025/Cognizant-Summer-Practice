using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    [Table("projects")]
    public class Project
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

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("image_url", TypeName = "text")]
        public string? ImageUrl { get; set; }

        [Column("demo_url", TypeName = "text")]
        public string? DemoUrl { get; set; }

        [Column("github_url", TypeName = "text")]
        public string? GithubUrl { get; set; }

        [Column("technologies")]
        public string[]? Technologies { get; set; }

        [Column("featured")]
        public bool Featured { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 