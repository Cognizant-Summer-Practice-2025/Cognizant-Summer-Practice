using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_portfolio.Models
{
    public class PortfolioSkill
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Portfolio")]
        public Guid PortfolioId { get; set; }

        [Required]
        [ForeignKey("Skill")]
        public Guid SkillId { get; set; }

        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;

        public int? YearsExperience { get; set; }

        public bool IsFeatured { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Portfolio Portfolio { get; set; } = null!;
        public virtual Skill Skill { get; set; } = null!;
    }
} 