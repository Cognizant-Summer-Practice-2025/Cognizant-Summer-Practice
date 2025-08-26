using System;

namespace backend_portfolio.Models
{
    public class TechNewsSummary
    {
        public Guid Id { get; set; }
        public string Summary { get; set; } = string.Empty;
        public bool WorkflowCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
