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
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly PortfolioDbContext _context;

        public BlogPostRepository(PortfolioDbContext context)
        {
            _context = context;
        }

        public async Task<List<BlogPost>> GetAllBlogPostsAsync()
        {
            return await _context.BlogPosts
                .Include(b => b.Portfolio)
                .ToListAsync();
        }

        public async Task<BlogPost?> GetBlogPostByIdAsync(Guid id)
        {
            return await _context.BlogPosts
                .Include(b => b.Portfolio)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BlogPost>> GetBlogPostsByPortfolioIdAsync(Guid portfolioId)
        {
            return await _context.BlogPosts
                .Where(b => b.PortfolioId == portfolioId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<BlogPost> CreateBlogPostAsync(BlogPostCreateRequest request)
        {
            var blogPost = new BlogPost
            {
                Id = Guid.NewGuid(),
                PortfolioId = request.PortfolioId,
                Title = request.Title,
                Excerpt = request.Excerpt,
                Content = request.Content,
                FeaturedImageUrl = request.FeaturedImageUrl,
                Tags = request.Tags,
                IsPublished = request.IsPublished,
                PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.BlogPosts.AddAsync(blogPost);
            await _context.SaveChangesAsync();
            return blogPost;
        }

        public async Task<BlogPost?> UpdateBlogPostAsync(Guid id, BlogPostUpdateRequest request)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return null;

            if (request.Title != null) blogPost.Title = request.Title;
            if (request.Excerpt != null) blogPost.Excerpt = request.Excerpt;
            if (request.Content != null) blogPost.Content = request.Content;
            if (request.FeaturedImageUrl != null) blogPost.FeaturedImageUrl = request.FeaturedImageUrl;
            if (request.Tags != null) blogPost.Tags = request.Tags;
            if (request.IsPublished.HasValue) 
            {
                blogPost.IsPublished = request.IsPublished.Value;
                if (request.IsPublished.Value && !blogPost.PublishedAt.HasValue)
                {
                    blogPost.PublishedAt = DateTime.UtcNow;
                }
            }
            blogPost.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return blogPost;
        }

        public async Task<bool> DeleteBlogPostAsync(Guid id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return false;

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BlogPost>> GetPublishedBlogPostsAsync(Guid portfolioId)
        {
            return await _context.BlogPosts
                .Where(b => b.PortfolioId == portfolioId && b.IsPublished)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetBlogPostsByTagAsync(string tag)
        {
            return await _context.BlogPosts
                .Where(b => b.Tags != null && b.Tags.Contains(tag))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }
    }
}
