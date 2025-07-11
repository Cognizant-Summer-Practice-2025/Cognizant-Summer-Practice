using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class PortfolioLike
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        [Required]
        public Guid UserId { get; set; } // External reference to User Service

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 