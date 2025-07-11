using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace backend_portfolio.Models
{
    public class PortfolioView
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        public Guid? ViewerId { get; set; } // External reference to User Service (nullable for anonymous views)

        public IPAddress? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? ReferrerUrl { get; set; }

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 