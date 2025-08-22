namespace backend_AI.Services.Abstractions
{
    public interface ITechNewsSummaryStore
    {
        string? Summary { get; }

        void SetSummary(string summary);
    }
}


