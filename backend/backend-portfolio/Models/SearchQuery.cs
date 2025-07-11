using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class SearchQuery
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; } // External reference to User Service (nullable for anonymous searches)

        [ForeignKey("ClickedPortfolio")]
        public Guid? ClickedPortfolioId { get; set; }

        public string? QueryText { get; set; }

        public string? FiltersApplied { get; set; } // JSON stored as string

        public int ResultsCount { get; set; } = 0;

        public string? SessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Portfolio? ClickedPortfolio { get; set; }
    }
} 