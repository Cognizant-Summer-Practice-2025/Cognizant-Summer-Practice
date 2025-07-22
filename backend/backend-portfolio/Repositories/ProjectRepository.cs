using backend_portfolio.Data;
using backend_portfolio.Models;
using backend_portfolio.DTO;
using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using Microsoft.EntityFrameworkCore;

namespace backend_portfolio.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly PortfolioDbContext _context;

        public ProjectRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Portfolio)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            return await _context.Projects
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Project>> GetProjectsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Projects
                .Where(p => p.PortfolioId == portfolioId)
                .OrderBy(p => p.Featured ? 0 : 1)
                .ThenBy(p => p.Title)
                .ToListAsync();
        }

        public async Task<Project> CreateProjectAsync(ProjectCreateRequest request)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                PortfolioId = request.PortfolioId,
                Title = request.Title,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                DemoUrl = request.DemoUrl,
                GithubUrl = request.GithubUrl,
                Technologies = request.Technologies,
                Featured = request.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdateProjectAsync(Guid id, ProjectUpdateRequest request)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return null;

            if (request.Title != null) project.Title = request.Title;
            if (request.Description != null) project.Description = request.Description;
            if (request.ImageUrl != null) project.ImageUrl = request.ImageUrl;
            if (request.DemoUrl != null) project.DemoUrl = request.DemoUrl;
            if (request.GithubUrl != null) project.GithubUrl = request.GithubUrl;
            if (request.Technologies != null) project.Technologies = request.Technologies;
            if (request.Featured.HasValue) project.Featured = request.Featured.Value;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Project>> GetFeaturedProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.Portfolio)
                .Where(p => p.Featured)
                .ToListAsync();
        }

        public async Task<List<Project>> GetFeaturedProjectsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.Projects
                .Where(p => p.PortfolioId == portfolioId && p.Featured)
                .ToListAsync();
        }
    }
}
