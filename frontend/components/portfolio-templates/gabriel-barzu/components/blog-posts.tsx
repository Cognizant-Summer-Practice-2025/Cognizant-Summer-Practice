import React from 'react';
import { BlogPost } from '@/lib/portfolio';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data: blogPosts }: BlogPostsProps) {
  if (!blogPosts || blogPosts.length === 0) {
    return null;
  }

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Draft';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  };

  return (
    <section className="gb-blog-posts">
      <h3 className="section-title">Latest Blog Posts</h3>
      <div className="blog-posts-grid">
        {blogPosts.map((post) => (
          <article key={post.id} className="blog-post-card">
            {post.featuredImageUrl && (
              <div className="blog-post-image-container">
                <img 
                  src={post.featuredImageUrl} 
                  alt={post.title}
                  className="blog-post-image"
                />
              </div>
            )}
            <div className="blog-post-content">
              <div className="blog-post-meta">
                <span className="blog-post-date">{formatDate(post.publishedAt)}</span>
              </div>
              <h4 className="blog-post-title">{post.title}</h4>
              <p className="blog-post-excerpt">{post.excerpt}</p>
              {post.tags && post.tags.length > 0 && (
                <div className="blog-post-tags">
                  {post.tags.map((tag, index) => (
                    <span key={index} className="blog-tag">
                      {tag}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </article>
        ))}
      </div>
    </section>
  );
} 