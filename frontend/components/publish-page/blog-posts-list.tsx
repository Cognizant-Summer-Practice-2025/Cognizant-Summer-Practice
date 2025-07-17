"use client"

import { usePortfolio } from '@/lib/contexts/portfolio-context';

export function BlogPostsList() {
  const { getUserBlogPosts, loading } = usePortfolio();
  
  // Get blog posts using the new entity getter
  const blogPosts = getUserBlogPosts();

  if (loading) {
    return (
      <div className="w-full px-4 py-4 md:px-6 md:py-8 bg-white rounded-lg border border-slate-200 flex flex-col gap-3 md:gap-4">
        <div className="pb-1 flex flex-col">
          <h3 className="text-slate-900 text-lg font-semibold">Your Blog Posts</h3>
        </div>
        <div className="px-3 py-6 text-center text-slate-500">
          Loading blog posts...
        </div>
      </div>
    );
  }

  return (
    <div className="w-full px-4 py-4 md:px-6 md:py-8 bg-white rounded-lg border border-slate-200 flex flex-col gap-3 md:gap-4">
      <div className="pb-1 flex flex-col">
        <h3 className="text-slate-900 text-lg font-semibold">Your Blog Posts</h3>
      </div>
      
      {blogPosts.length === 0 ? (
        <div className="px-3 py-6 text-center text-slate-500">
          <p>No blog posts yet. Share your thoughts and expertise with the world!</p>
        </div>
      ) : (
        blogPosts.map((post) => (
          <div
            key={post.id}
            className="px-3 py-3 md:px-4 md:py-4 bg-white rounded-lg border border-slate-200 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0"
          >
            <div className="flex flex-col gap-1 min-w-0 flex-1">
              <div className="pb-1 flex flex-col">
                <h4 className="text-slate-900 text-base font-semibold truncate">{post.title}</h4>
                {post.publishedAt && (
                  <span className="text-slate-400 text-xs">
                    {new Date(post.publishedAt).toLocaleDateString()}
                  </span>
                )}
              </div>
              <div className="flex flex-col">
                <p className="text-slate-500 text-sm leading-relaxed line-clamp-2">
                  {post.excerpt || post.content?.substring(0, 150) + '...' || 'No excerpt available'}
                </p>
              </div>
              {post.tags && post.tags.length > 0 && (
                <div className="flex flex-wrap gap-1 mt-1">
                  {post.tags.slice(0, 3).map((tag, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-600 text-xs rounded"
                    >
                      {tag}
                    </span>
                  ))}
                  {post.tags.length > 3 && (
                    <span className="text-slate-400 text-xs">+{post.tags.length - 3} more</span>
                  )}
                </div>
              )}
            </div>
            
            <button className="p-2 rounded-lg flex flex-col justify-center items-center hover:bg-slate-50 transition-colors self-start sm:self-center">
              <span className="text-slate-500 text-sm">⋯</span>
            </button>
          </div>
        ))
      )}
    </div>
  )
} 