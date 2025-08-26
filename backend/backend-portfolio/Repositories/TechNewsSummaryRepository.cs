using System;
using System.Linq;
using System.Threading.Tasks;
using backend_portfolio.Data;
using backend_portfolio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_portfolio.Repositories
{
    public class TechNewsSummaryRepository : ITechNewsSummaryRepository
    {
        private readonly PortfolioDbContext _context;
        private readonly ILogger<TechNewsSummaryRepository> _logger;

        public TechNewsSummaryRepository(PortfolioDbContext context, ILogger<TechNewsSummaryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TechNewsSummary?> GetLatestAsync()
        {
            try
            {
                // Get the most recent tech news summary based on created_at timestamp
                // This ensures we always get the newest summary from AirFlow
                return await _context.TechNewsSummaries
                    .OrderByDescending(t => t.CreatedAt)
                    .ThenByDescending(t => t.UpdatedAt) // Secondary sort by updated_at for tie-breaking
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest tech news summary");
                throw;
            }
        }

        public async Task<TechNewsSummary?> GetLatestWithMetadataAsync()
        {
            try
            {
                // Get the most recent tech news summary with additional metadata
                // This is useful for debugging and monitoring
                var latest = await _context.TechNewsSummaries
                    .OrderByDescending(t => t.CreatedAt)
                    .ThenByDescending(t => t.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (latest != null)
                {
                    _logger.LogInformation("Latest tech news summary: ID={Id}, Created={CreatedAt}, Updated={UpdatedAt}, WorkflowCompleted={WorkflowCompleted}", 
                        latest.Id, latest.CreatedAt, latest.UpdatedAt, latest.WorkflowCompleted);
                }

                return latest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest tech news summary with metadata");
                throw;
            }
        }

        public async Task<TechNewsSummary> CreateAsync(TechNewsSummary techNewsSummary)
        {
            try
            {
                techNewsSummary.CreatedAt = DateTime.UtcNow;
                techNewsSummary.UpdatedAt = DateTime.UtcNow;
                
                _context.TechNewsSummaries.Add(techNewsSummary);
                await _context.SaveChangesAsync();
                
                return techNewsSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tech news summary");
                throw;
            }
        }

        public async Task<TechNewsSummary> UpdateAsync(TechNewsSummary techNewsSummary)
        {
            try
            {
                techNewsSummary.UpdatedAt = DateTime.UtcNow;
                
                _context.TechNewsSummaries.Update(techNewsSummary);
                await _context.SaveChangesAsync();
                
                return techNewsSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tech news summary");
                throw;
            }
        }

        public async Task<TechNewsSummary?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.TechNewsSummaries.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tech news summary by ID: {Id}", id);
                throw;
            }
        }
    }
}
