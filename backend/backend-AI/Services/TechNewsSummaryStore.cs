using System;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    public class TechNewsSummaryStore : ITechNewsSummaryStore
    {
        private readonly object _lock = new object();
        private string? _summary;

        public string? Summary
        {
            get
            {
                lock (_lock)
                {
                    return _summary;
                }
            }
        }

        public void SetSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                throw new ArgumentException("Summary cannot be empty", nameof(summary));
            }

            lock (_lock)
            {
                _summary = summary;
            }
        }
    }
}


