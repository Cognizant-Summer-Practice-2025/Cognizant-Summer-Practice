"use client";

import React from 'react';
import Image from 'next/image';
import { BlogPost } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Clock, ExternalLink, BookOpen } from 'lucide-react';
import { getSafeImageUrl } from '@/lib/image';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data }: BlogPostsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="prof-blog">
        <div className="prof-empty-state">
          <BookOpen size={48} />
          <h3>No Blog Posts</h3>
          <p>Blog articles and insights will be displayed here when available.</p>
        </div>
      </div>
    );
  }

  const calculateReadTime = (content: string) => {
    const wordsPerMinute = 200;
    const words = content.split(' ').length;
    const readTime = Math.ceil(words / wordsPerMinute);
    return readTime;
  };

  const truncateExcerpt = (text: string, maxLength: number = 150) => {
    if (text.length <= maxLength) return text;
    return text.substr(0, maxLength).trim() + '...';
  };

  const totalCount = data.length;
  const needsScrolling = data.length > 6;

  return (
    <div className="prof-blog">
      {/* Count indicator */}
      <div className="prof-count-indicator">
        <p className="prof-count-text">
          {totalCount} blog post{totalCount !== 1 ? 's' : ''} published
        </p>
      </div>

      <div className={`prof-blog-container ${needsScrolling ? 'prof-scrollable' : ''}`}>
        <div className="prof-blog-grid">
          {data.map((post, index) => (
            <Card key={post.id || index} className="prof-blog-card">
              {post.featuredImageUrl && (
                <div className="prof-blog-image">
                  <Image 
                    src={getSafeImageUrl(post.featuredImageUrl)} 
                    alt={post.title}
                    className="prof-blog-img"
                    width={400}
                    height={200}
                  />
                  <div className="prof-blog-overlay">
                    <Button size="sm" variant="secondary">
                      <ExternalLink size={14} />
                      Read Article
                    </Button>
                  </div>
                </div>
              )}
              
              <div className="prof-blog-content">
                <div className="prof-blog-meta">
                  {post.content && (
                    <div className="prof-blog-read-time">
                      <Clock size={14} />
                      <span>{calculateReadTime(post.content)} min read</span>
                    </div>
                  )}
                </div>

                <h3 className="prof-blog-title">{post.title}</h3>
                
                {post.excerpt && (
                  <p className="prof-blog-excerpt">
                    {truncateExcerpt(post.excerpt)}
                  </p>
                )}

                {post.tags && post.tags.length > 0 && (
                  <div className="prof-blog-tags">
                    {post.tags.slice(0, 3).map((tag, tagIndex) => (
                      <span key={tagIndex} className="prof-blog-tag">
                        {tag}
                      </span>
                    ))}
                    {post.tags.length > 3 && (
                      <span className="prof-blog-more-tags">
                        +{post.tags.length - 3} more
                      </span>
                    )}
                  </div>
                )}

                <div className="prof-blog-footer">
                  <div className="prof-blog-author">
                    <span>By Author</span>
                  </div>
                  
                  <div className="prof-blog-link">
                    <ExternalLink size={14} />
                    Read More
                  </div>
                </div>
              </div>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
} 