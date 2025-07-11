using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.Models
{
    public class Skill
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public SkillCategory Category { get; set; } = SkillCategory.Other;

        public string? IconUrl { get; set; }

        public string? Color { get; set; }

        public int UsageCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PortfolioSkill> PortfolioSkills { get; set; } = new List<PortfolioSkill>();
    }
} 