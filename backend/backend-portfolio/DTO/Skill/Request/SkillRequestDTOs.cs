namespace backend_portfolio.DTO.Skill.Request
{
    /// <summary>
    /// DTO for creating a new skill
    /// </summary>
    public class SkillCreateRequest
    {
        public Guid PortfolioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing skill
    /// </summary>
    public class SkillUpdateRequest
    {
        public string? Name { get; set; }
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }
} 
