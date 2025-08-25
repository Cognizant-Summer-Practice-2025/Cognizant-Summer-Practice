using System;
using System.Threading.Tasks;
using backend_portfolio.Models;

namespace backend_portfolio.Repositories
{
    public interface ITechNewsSummaryRepository
    {
        Task<TechNewsSummary?> GetLatestAsync();
        Task<TechNewsSummary?> GetLatestWithMetadataAsync();
        Task<TechNewsSummary> CreateAsync(TechNewsSummary techNewsSummary);
        Task<TechNewsSummary> UpdateAsync(TechNewsSummary techNewsSummary);
        Task<TechNewsSummary?> GetByIdAsync(Guid id);
    }
}
