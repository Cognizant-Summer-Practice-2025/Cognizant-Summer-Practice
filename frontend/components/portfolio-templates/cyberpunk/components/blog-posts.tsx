import React from 'react';
import { BlogPost } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { FileText, Calendar, Tag, Eye, ExternalLink } from 'lucide-react';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data }: BlogPostsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="cyberpunk-blog empty">
        <div className="empty-state">
          <FileText size={48} className="empty-icon" />
          <h3>No blog posts found</h3>
          <p>Content database appears to be empty</p>
        </div>
      </div>
    );
  }

  const publishedPosts = data.filter(post => post.isPublished);

  return (
    <div className="cyberpunk-blog">
      <div className="blog-header">
        <h2 className="section-title">
          <FileText size={24} />
          Blog Archive
        </h2>
        <div className="archive-info">
          <span className="archive-text">Published articles and thoughts</span>
          <span className="post-count">{publishedPosts.length} entries</span>
        </div>
      </div>

      <div className="posts-grid">
        {publishedPosts.map((post, index) => (
          <Card key={post.id} className="blog-post-card">
            {post.featuredImageUrl && (
              <div className="post-image">
                <img 
                  src={post.featuredImageUrl} 
                  alt={post.title}
                  className="featured-image"
                />
                <div className="image-overlay">
                  <div className="overlay-badge">
                    <Eye size={14} />
                    <span>Featured</span>
                  </div>
                </div>
              </div>
            )}
            
            <div className="post-content">
              <div className="post-meta">
                <span className="post-id">POST_{String(index + 1).padStart(3, '0')}</span>
                <div className="post-date">
                  <Calendar size={14} />
                  <span>
                    {post.publishedAt ? 
                      new Date(post.publishedAt).toLocaleDateString() : 
                      new Date(post.createdAt).toLocaleDateString()
                    }
                  </span>
                </div>
              </div>
              
              <h3 className="post-title">{post.title}</h3>
              
              {post.excerpt && (
                <p className="post-excerpt">{post.excerpt}</p>
              )}
              
              {post.tags && post.tags.length > 0 && (
                <div className="post-tags">
                  <Tag size={14} className="tags-icon" />
                  <div className="tags-list">
                    {post.tags.map((tag, tagIndex) => (
                      <Badge key={tagIndex} variant="outline" className="tag-badge">
                        #{tag}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}
              
              <div className="post-actions">
                <Button 
                  variant="outline" 
                  size="sm" 
                  className="read-button"
                  onClick={() => {
                    // Here you would typically navigate to the full post
                    console.log('Navigate to post:', post.id);
                  }}
                >
                  <ExternalLink size={16} />
                  Read Article
                </Button>
              </div>
            </div>
            
            <div className="post-footer">
              <div className="post-status">
                <div className="status-indicator published">
                  <div className="status-dot"></div>
                  <span>PUBLISHED</span>
                </div>
                <div className="post-stats">
                  <span className="stat-item">
                    Words: {post.content ? post.content.split(' ').length : 'N/A'}
                  </span>
                </div>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {data.length > publishedPosts.length && (
        <div className="draft-notice">
          <span className="draft-text">
            {data.length - publishedPosts.length} draft(s) in development
          </span>
        </div>
      )}
    </div>
  );
}