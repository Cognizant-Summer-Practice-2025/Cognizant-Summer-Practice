using backend_portfolio.Models;

namespace backend_portfolio.DTO
{
    // Skill Response DTO
    public class SkillResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SkillCategory Category { get; set; }
        public string? IconUrl { get; set; }
        public string? Color { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Skill Request DTO
    public class SkillRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public SkillCategory Category { get; set; } = SkillCategory.Other;
        public string? IconUrl { get; set; }
        public string? Color { get; set; }
    }

    // Skill Summary DTO
    public class SkillSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SkillCategory Category { get; set; }
        public string? IconUrl { get; set; }
        public string? Color { get; set; }
        public int UsageCount { get; set; }
    }

    // PortfolioSkill Response DTO
    public class PortfolioSkillResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public Guid SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsExperience { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public SkillSummaryDto? Skill { get; set; }
    }

    // PortfolioSkill Request DTO
    public class PortfolioSkillRequestDto
    {
        public Guid PortfolioId { get; set; }
        public Guid SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;
        public int? YearsExperience { get; set; }
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    // PortfolioSkill Summary DTO
    public class PortfolioSkillSummaryDto
    {
        public Guid Id { get; set; }
        public Guid SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsExperience { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public SkillSummaryDto? Skill { get; set; }
    }

    // PortfolioSkill Update DTO
    public class PortfolioSkillUpdateDto
    {
        public ProficiencyLevel? ProficiencyLevel { get; set; }
        public int? YearsExperience { get; set; }
        public bool? IsFeatured { get; set; }
        public int? SortOrder { get; set; }
    }
} 