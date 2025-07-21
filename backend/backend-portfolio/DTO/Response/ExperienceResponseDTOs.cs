namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// Standard experience response DTO
    /// </summary>
    public class ExperienceResponse
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

    /// <summary>
    /// Summary experience response for list views
    /// </summary>
    public class ExperienceSummaryResponse
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
} 