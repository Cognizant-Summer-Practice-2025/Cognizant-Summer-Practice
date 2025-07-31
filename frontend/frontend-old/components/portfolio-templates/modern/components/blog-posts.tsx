import React from 'react';
import Image from 'next/image';
import { BlogPost } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Calendar, ExternalLink, Clock } from 'lucide-react';
import { getSafeImageUrl } from '@/lib/image';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data: posts }: BlogPostsProps) {
  if (!posts || posts.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No blog posts available.
      </div>
    );
  }

  // Filter for published posts only
  const publishedPosts = posts.filter(post => post.isPublished);

  if (publishedPosts.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No published blog posts available.
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  };

  const totalCount = publishedPosts.length;

  return (
    <div className="modern-component-container">
      {/* Count indicator */}
      <div className="mb-4 pb-2 border-b border-border">
        <p className="text-sm text-muted-foreground">
          {totalCount} blog post{totalCount !== 1 ? 's' : ''}
        </p>
      </div>

      <div className="max-h-[800px] overflow-y-auto pr-2">
      <div className="modern-grid">
        {publishedPosts.map((post) => (
        <Card key={post.id} className="modern-card">
          {post.featuredImageUrl && (
            <div className="aspect-video overflow-hidden rounded-t-lg">
              <Image 
                src={getSafeImageUrl(post.featuredImageUrl)} 
                alt={post.title}
                className="w-full h-full object-cover transition-transform hover:scale-105"
                width={400}
                height={225}
              />
            </div>
          )}
          
          <CardContent className="p-6">
            <div className="flex items-center gap-2 text-sm text-muted-foreground mb-3">
              <Calendar size={14} />
              <span>
                {formatDate(post.publishedAt || post.createdAt)}
              </span>
            </div>
            
            <h3 className="text-xl font-semibold text-foreground mb-3 hover:text-primary transition-colors">
              {post.title}
            </h3>
            
            {post.excerpt && (
              <p className="text-muted-foreground mb-4 leading-relaxed">
                {post.excerpt}
              </p>
            )}
            
            {post.tags && post.tags.length > 0 && (
              <div className="flex flex-wrap gap-2 mb-4">
                {post.tags.slice(0, 3).map((tag, index) => (
                  <Badge key={index} variant="secondary" className="text-xs">
                    {tag}
                  </Badge>
                ))}
                {post.tags.length > 3 && (
                  <Badge variant="secondary" className="text-xs">
                    +{post.tags.length - 3} more
                  </Badge>
                )}
              </div>
            )}
            
            <div className="flex items-center justify-between pt-4 border-t border-border">
              <div className="flex items-center gap-1 text-xs text-muted-foreground">
                <Clock size={12} />
                <span>
                  {Math.ceil((post.content?.length || 500) / 200)} min read
                </span>
              </div>
              
              <Button size="sm" variant="ghost" className="p-2">
                <ExternalLink size={14} />
              </Button>
            </div>
          </CardContent>
        </Card>
        ))}
      </div>
      </div>
    </div>
  );
} 