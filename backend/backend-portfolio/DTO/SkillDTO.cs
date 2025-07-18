namespace backend_portfolio.DTO
{
    // Skill Response DTO
    public class SkillResponseDto
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

    // Skill Request DTO
    public class SkillRequestDto
    {
        public Guid PortfolioId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }

    // Skill Summary DTO
    public class SkillSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }

    // Skill Update DTO
    public class SkillUpdateDto
    {
        public string? Name { get; set; }
        public string? CategoryType { get; set; }
        public string? Subcategory { get; set; }
        public string? Category { get; set; }
        public int? ProficiencyLevel { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
