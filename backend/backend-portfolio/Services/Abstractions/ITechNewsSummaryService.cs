using System.Threading.Tasks;
using backend_portfolio.DTO.TechNews;

namespace backend_portfolio.Services.Abstractions
{
    public interface ITechNewsSummaryService
    {
        Task<TechNewsSummaryResponseDto?> GetLatestAsync();
        Task<TechNewsSummaryResponseDto> UpsertAsync(TechNewsSummaryRequestDto request);
    }
}
