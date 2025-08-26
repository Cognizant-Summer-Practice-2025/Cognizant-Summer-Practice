using System;

namespace backend_portfolio.DTO.TechNews
{
    public class TechNewsSummaryRequestDto
    {
        public string Summary { get; set; } = string.Empty;
        public bool WorkflowCompleted { get; set; }
    }

    public class TechNewsSummaryResponseDto
    {
        public Guid Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public bool WorkflowCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
