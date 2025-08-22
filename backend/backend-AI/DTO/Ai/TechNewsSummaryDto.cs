namespace backend_AI.DTO.Ai
{
    public class TechNewsSummaryRequest
    {
        public string Summary { get; set; } = string.Empty;
        public bool? WorkflowCompleted { get; set; }
    }

    public class TechNewsSummaryResponse
    {
        public string? Summary { get; set; }
    }
}


