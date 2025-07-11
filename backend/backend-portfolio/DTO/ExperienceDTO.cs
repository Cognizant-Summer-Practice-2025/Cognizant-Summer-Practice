using backend_portfolio.Models;

namespace backend_portfolio.DTO
{
    // Experience Response DTO
    public class ExperienceResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyUrl { get; set; }
        public string? Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
        public string[]? Achievements { get; set; }
        public string[]? SkillsUsed { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Experience Request DTO
    public class ExperienceRequestDto
    {
        public Guid PortfolioId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyUrl { get; set; }
        public string? Location { get; set; }
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public string? Description { get; set; }
        public string[]? Achievements { get; set; }
        public string[]? SkillsUsed { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    // Experience Summary DTO
    public class ExperienceSummaryDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public EmploymentType EmploymentType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public int SortOrder { get; set; }
    }

    // Experience Update DTO
    public class ExperienceUpdateDto
    {
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyUrl { get; set; }
        public string? Location { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsCurrent { get; set; }
        public string? Description { get; set; }
        public string[]? Achievements { get; set; }
        public string[]? SkillsUsed { get; set; }
        public int? SortOrder { get; set; }
    }
} 