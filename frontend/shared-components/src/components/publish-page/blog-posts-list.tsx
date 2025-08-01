"use client"

import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useDraft } from '@/lib/contexts/draft-context';

export function BlogPostsList() {
  const { getUserBlogPosts } = usePortfolio()
  const { draftBlogPosts, deleteDraftBlogPost } = useDraft();
  const blogPosts = getUserBlogPosts()

  if (blogPosts.length === 0 && draftBlogPosts.length === 0) {
    return (
      <div className="border border-slate-200 rounded-lg p-6 bg-gray-50">
        <p className="text-sm text-gray-500 text-center">
          No blog posts added yet. Add your first blog post above.
        </p>
      </div>
    )
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return ''
    return new Date(dateString).toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short',
      day: 'numeric' 
    })
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-slate-900">Your Blog Posts</h3>
      
      {/* Existing Blog Posts */}
      {blogPosts.map((post) => (
        <div key={post.id} className="border border-slate-200 rounded-lg p-4 bg-white">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{post.title}</h4>
                <span className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                  Published
                </span>
              </div>
              {post.publishedAt && (
                <p className="text-xs text-gray-500 mb-3">
                  Published {formatDate(post.publishedAt)}
                </p>
              )}
              {post.content && (
                <p className="text-sm text-gray-600 mb-3">
                  {post.content.substring(0, 150)}...
                </p>
              )}
              {post.tags && post.tags.length > 0 && (
                <div className="flex flex-wrap gap-1">
                  {post.tags.map((tag: string, index: number) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {tag}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      ))}

      {/* Draft Blog Posts */}
      {draftBlogPosts.map((post) => (
        <div key={post.id} className="border border-amber-200 rounded-lg p-4 bg-amber-50">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{post.title}</h4>
                <span className="px-2 py-1 bg-amber-100 text-amber-800 text-xs rounded-full">
                  Draft
                </span>
              </div>
              {post.content && (
                <p className="text-sm text-gray-600 mb-3">
                  {post.content.substring(0, 150)}...
                </p>
              )}
              {post.tags && post.tags.trim() && (
                <div className="flex flex-wrap gap-1">
                  {post.tags.split(',').map((tag: string, index: number) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {tag.trim()}
                    </span>
                  ))}
                </div>
              )}
            </div>
            <button
              onClick={() => deleteDraftBlogPost(post.id)}
              className="ml-4 text-red-600 hover:text-red-800 text-sm"
            >
              Remove
            </button>
          </div>
        </div>
      ))}
    </div>
  )
} 