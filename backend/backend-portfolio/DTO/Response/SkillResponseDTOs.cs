namespace backend_portfolio.DTO.Response
{
    /// <summary>
    /// Standard skill response DTO
    /// </summary>
    public class SkillResponse
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Summary skill response for list views
    /// </summary>
    public class SkillSummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }
} 