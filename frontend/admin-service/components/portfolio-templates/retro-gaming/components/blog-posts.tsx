import React, { useState } from 'react';
import Image from 'next/image';
import { BlogPost } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ExternalLink, Calendar, BookOpen, Star, Gamepad2 } from 'lucide-react';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data: blogPosts }: BlogPostsProps) {
  const [selectedPost, setSelectedPost] = useState<string | null>(null);

  if (!blogPosts || blogPosts.length === 0) {
    return (
      <div className="retro-blog-posts" id="blog">
        <div className="section-header">
          <h2 className="section-title">
            <BookOpen className="pixel-icon" size={24} />
            QUEST JOURNAL
          </h2>
          <div className="pixel-border"></div>
          <p className="section-subtitle">Adventure Logs & Chronicles</p>
        </div>

        <Card className="empty-state">
          <div className="empty-content">
            <Gamepad2 className="empty-icon pixel-icon" size={48} />
            <h3 className="empty-title">No Quest Logs Found</h3>
            <p className="empty-description">
              The journal is empty. New adventures await to be documented!
            </p>
          </div>
        </Card>
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric'
    });
  };

  const getPostRarity = (index: number) => {
    const rarities = ['legendary', 'epic', 'rare', 'common'];
    return rarities[index % rarities.length];
  };

  const getRarityIcon = (rarity: string) => {
    switch (rarity) {
      case 'legendary': return '👑';
      case 'epic': return '⭐';
      case 'rare': return '💎';
      default: return '📖';
    }
  };

  return (
    <div className="retro-blog-posts" id="blog">
      <div className="section-header">
        <h2 className="section-title">
          <BookOpen className="pixel-icon" size={24} />
          QUEST JOURNAL
        </h2>
        <div className="pixel-border"></div>
        <p className="section-subtitle">Adventure Logs & Chronicles</p>
      </div>

      <div className="journal-grid">
        {blogPosts.map((post, index) => {
          const rarity = getPostRarity(index);
          const isSelected = selectedPost === post.id;
          
          return (
            <Card 
              key={post.id} 
              className={`journal-entry ${rarity} ${isSelected ? 'selected' : ''}`}
              onClick={() => setSelectedPost(isSelected ? null : post.id)}
            >
              <div className="entry-header">
                <div className="rarity-indicator">
                  <span className="rarity-icon">
                    {getRarityIcon(rarity)}
                  </span>
                  <span className={`rarity-text ${rarity}`}>
                    {rarity.toUpperCase()}
                  </span>
                </div>
                
                <div className="entry-meta">
                  <div className="date-info">
                    <Calendar className="meta-icon" size={14} />
                    <span>{post.publishedAt ? formatDate(post.publishedAt) : 'No date'}</span>
                  </div>

                </div>
              </div>

              {post.featuredImageUrl && (
                <div className="entry-image">
                  <Image
                    src={post.featuredImageUrl}
                    alt={post.title}
                    className="post-image pixel-art"
                    width={300}
                    height={200}
                  />
                  <div className="image-overlay">
                    <BookOpen className="overlay-icon" size={24} />
                  </div>
                </div>
              )}

              <div className="entry-content">
                <h3 className="entry-title">{post.title}</h3>
                <p className="entry-excerpt">{post.excerpt}</p>

                {post.tags && post.tags.length > 0 && (
                  <div className="entry-tags">
                    {post.tags.slice(0, 3).map((tag, tagIndex) => (
                      <span key={tagIndex} className="tag">
                        #{tag}
                      </span>
                    ))}
                    {post.tags.length > 3 && (
                      <span className="tag-more">
                        +{post.tags.length - 3}
                      </span>
                    )}
                  </div>
                )}

                <div className="entry-stats">
                  <div className="stat-item">
                    <Star className="stat-icon" size={14} />
                    <span>XP: +{(index + 1) * 50}</span>
                  </div>
                  <div className="stat-item">
                    <span className="difficulty">
                      Difficulty: {rarity === 'legendary' ? 'Expert' : 
                                  rarity === 'epic' ? 'Advanced' : 
                                  rarity === 'rare' ? 'Intermediate' : 'Beginner'}
                    </span>
                  </div>
                </div>

                <Button className="read-button pixel-button" size="sm">
                  <ExternalLink size={14} />
                  <span>READ QUEST</span>
                </Button>
              </div>

              <div className="entry-effects">
                <div className="sparkle-particle"></div>
                <div className="sparkle-particle"></div>
                <div className="sparkle-particle"></div>
              </div>

              <div className={`entry-border ${rarity}`}>
                <div className="border-glow"></div>
              </div>
            </Card>
          );
        })}
      </div>

      {selectedPost && (
        <div className="post-details-modal">
          <Card className="details-card">
            {(() => {
              const post = blogPosts.find(p => p.id === selectedPost);
              if (!post) return null;

              return (
                <>
                  <div className="details-header">
                    <h3 className="details-title">{post.title}</h3>
                    <Button 
                      className="close-btn pixel-button"
                      onClick={() => setSelectedPost(null)}
                    >
                      ✕
                    </Button>
                  </div>

                  <div className="details-content">
                    {post.featuredImageUrl && (
                      <Image
                        src={post.featuredImageUrl}
                        alt={post.title}
                        className="details-image"
                        width={500}
                        height={300}
                      />
                    )}
                    
                    <div className="details-info">
                      <div className="details-meta">
                        <span className="publish-date">
                          Published: {post.publishedAt ? formatDate(post.publishedAt) : 'No date'}
                        </span>

                      </div>
                      
                      <p className="details-excerpt">{post.excerpt}</p>
                      
                      {post.tags && (
                        <div className="details-tags">
                          <h4>Quest Tags:</h4>
                          <div className="tag-list">
                            {post.tags.map((tag, index) => (
                              <span key={index} className="detail-tag">
                                #{tag}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="details-actions">
                        <Button className="action-button read">
                          <ExternalLink size={16} />
                          Read Full Quest
                        </Button>
                      </div>
                    </div>
                  </div>
                </>
              );
            })()}
          </Card>
        </div>
      )}

      <div className="journal-stats">
        <Card className="stats-card">
          <div className="stats-header">
            <BookOpen className="pixel-icon" size={20} />
            <h3>JOURNAL STATISTICS</h3>
          </div>
          
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-label">Total Entries:</span>
              <span className="stat-value">{blogPosts.length}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Total XP:</span>
              <span className="stat-value">{blogPosts.length * 75}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Legendary Entries:</span>
              <span className="stat-value">
                {Math.ceil(blogPosts.length / 4)}
              </span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Knowledge Level:</span>
              <span className="stat-value">Master</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}

export default BlogPosts;