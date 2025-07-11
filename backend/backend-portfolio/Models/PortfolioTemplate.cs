using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.Models
{
    public class PortfolioTemplate
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? PreviewImageUrl { get; set; }

        public string? CssConfig { get; set; } // JSON stored as string

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
} 