using System;
using System.Threading.Tasks;
using backend_portfolio.DTO.TechNews;
using backend_portfolio.Models;
using backend_portfolio.Repositories;
using backend_portfolio.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace backend_portfolio.Services
{
    public class TechNewsSummaryService : ITechNewsSummaryService
    {
        private readonly ITechNewsSummaryRepository _repository;
        private readonly ILogger<TechNewsSummaryService> _logger;

        public TechNewsSummaryService(
            ITechNewsSummaryRepository repository,
            ILogger<TechNewsSummaryService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TechNewsSummaryResponseDto?> GetLatestAsync()
        {
            try
            {
                var latest = await _repository.GetLatestAsync();
                if (latest == null)
                {
                    return null;
                }

                return MapToDto(latest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest tech news summary");
                throw;
            }
        }

        public async Task<TechNewsSummaryResponseDto> UpsertAsync(TechNewsSummaryRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Summary))
                {
                    throw new ArgumentException("Summary cannot be empty", nameof(request));
                }

                // Get the latest summary to see if we should update or create
                var existing = await _repository.GetLatestAsync();
                TechNewsSummary result;

                if (existing != null)
                {
                    // Update existing summary
                    existing.Summary = request.Summary;
                    existing.WorkflowCompleted = request.WorkflowCompleted;
                    result = await _repository.UpdateAsync(existing);
                }
                else
                {
                    // Create new summary
                    var newSummary = new TechNewsSummary
                    {
                        Summary = request.Summary,
                        WorkflowCompleted = request.WorkflowCompleted
                    };
                    result = await _repository.CreateAsync(newSummary);
                }

                return MapToDto(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting tech news summary");
                throw;
            }
        }

        private static TechNewsSummaryResponseDto MapToDto(TechNewsSummary entity)
        {
            return new TechNewsSummaryResponseDto
            {
                Id = entity.Id,
                Summary = entity.Summary,
                WorkflowCompleted = entity.WorkflowCompleted,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
