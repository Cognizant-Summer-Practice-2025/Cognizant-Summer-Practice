using System.Threading.Tasks;
using backend_AI.DTO.Ai;

namespace backend_AI.Services.External
{
    public interface ITechNewsPortfolioClient
    {
        Task<bool> UpsertSummaryAsync(TechNewsSummaryDto request);
        Task<string?> GetLatestSummaryAsync();
    }
}
