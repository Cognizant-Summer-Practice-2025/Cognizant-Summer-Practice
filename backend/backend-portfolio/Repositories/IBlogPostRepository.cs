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

namespace backend_portfolio.Repositories
{
    public interface IBlogPostRepository
    {
        Task<List<BlogPost>> GetAllBlogPostsAsync();
        Task<BlogPost?> GetBlogPostByIdAsync(Guid id);
        Task<List<BlogPost>> GetBlogPostsByPortfolioIdAsync(Guid portfolioId);
        Task<BlogPost> CreateBlogPostAsync(BlogPostCreateRequest request);
        Task<BlogPost?> UpdateBlogPostAsync(Guid id, BlogPostUpdateRequest request);
        Task<bool> DeleteBlogPostAsync(Guid id);
        Task<List<BlogPost>> GetPublishedBlogPostsAsync(Guid portfolioId);
        Task<List<BlogPost>> GetBlogPostsByTagAsync(string tag);
    }
}
