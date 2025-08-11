using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Portfolio.Response;
using backend_portfolio.Models;

namespace backend_portfolio.Services.Abstractions
{
    public interface IPortfolioMapper : IEntityMapper<Portfolio, PortfolioCreateRequest, PortfolioResponse, PortfolioUpdateRequest>
    {
        PortfolioSummaryResponse MapToSummaryDto(Portfolio entity);
        IEnumerable<PortfolioSummaryResponse> MapToSummaryDtos(IEnumerable<Portfolio> entities);
        PortfolioDetailResponse MapToDetailDto(Portfolio entity);
    }
} 