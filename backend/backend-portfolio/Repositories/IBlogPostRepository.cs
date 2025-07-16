using backend_portfolio.Models;
using backend_portfolio.DTO;

namespace backend_portfolio.Repositories
{
    public interface IBlogPostRepository
    {
        Task<List<BlogPost>> GetAllBlogPostsAsync();
        Task<BlogPost?> GetBlogPostByIdAsync(Guid id);
        Task<List<BlogPost>> GetBlogPostsByPortfolioIdAsync(Guid portfolioId);
        Task<BlogPost> CreateBlogPostAsync(BlogPostRequestDto request);
        Task<BlogPost?> UpdateBlogPostAsync(Guid id, BlogPostUpdateDto request);
        Task<bool> DeleteBlogPostAsync(Guid id);
        Task<List<BlogPost>> GetPublishedBlogPostsAsync(Guid portfolioId);
        Task<List<BlogPost>> GetBlogPostsByTagAsync(string tag);
    }
}
