using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    [Table("experience")]
    public class Experience
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("portfolio_id")]
        public Guid PortfolioId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("job_title")]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("company_name")]
        public string CompanyName { get; set; } = string.Empty;

        [Column("start_date")]
        public DateOnly StartDate { get; set; }

        [Column("end_date")]
        public DateOnly? EndDate { get; set; }

        [Column("is_current")]
        public bool IsCurrent { get; set; } = false;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("skills_used")]
        public string[]? SkillsUsed { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;
    }
} 