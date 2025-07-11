using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class Experience
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        [Required]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyUrl { get; set; }

        public string? Location { get; set; }

        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; } = false;

        public string? Description { get; set; }

        public string[]? Achievements { get; set; } // Array of achievement strings

        public string[]? SkillsUsed { get; set; } // Array of skill names

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 