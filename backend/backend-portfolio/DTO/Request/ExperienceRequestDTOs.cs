namespace backend_portfolio.DTO.Request
{
    /// <summary>
    /// DTO for creating a new experience entry
    /// </summary>
    public class ExperienceCreateRequest
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

    /// <summary>
    /// DTO for updating an existing experience entry
    /// </summary>
    public class ExperienceUpdateRequest
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