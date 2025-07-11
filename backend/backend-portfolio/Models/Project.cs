using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string? DemoUrl { get; set; }

        public string? GithubUrl { get; set; }

        public string[]? Technologies { get; set; } // Array of technology names

        public bool Featured { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 