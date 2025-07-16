namespace backend_portfolio.DTO
{
    // Experience Response DTO
    public class ExperienceResponseDto
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
        public string[]? SkillsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Experience Request DTO
    public class ExperienceRequestDto
    {
        public Guid PortfolioId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public string? Description { get; set; }
        public string[]? SkillsUsed { get; set; }
    }

    // Experience Summary DTO
    public class ExperienceSummaryDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
        public string[]? SkillsUsed { get; set; }
    }

    // Experience Update DTO
    public class ExperienceUpdateDto
    {
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsCurrent { get; set; }
        public string? Description { get; set; }
        public string[]? SkillsUsed { get; set; }
    }
}
