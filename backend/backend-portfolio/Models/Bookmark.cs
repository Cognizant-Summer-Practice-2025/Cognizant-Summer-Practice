using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class Bookmark
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        public string? CollectionName { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 