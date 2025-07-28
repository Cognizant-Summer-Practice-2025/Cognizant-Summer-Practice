import React from 'react';
import { BookOpen, Calendar, Clock, Tag, ExternalLink, Eye } from 'lucide-react';

interface BlogPost {
  id: number;
  title: string;
  content: string;
  summary?: string;
  published_date: string;
  tags?: string;
  read_time?: number;
  views?: number;
  featured?: boolean;
  external_url?: string;
}

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data }: BlogPostsProps) {
  const blogPosts = data || [];

  const formatDate = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'long',
      day: 'numeric'
    });
  };

  const getTagArray = (tags: string | null | undefined = '') => {
    if (!tags || typeof tags !== 'string') {
      return [];
    }
    return tags.split(',').map(tag => tag.trim()).filter(Boolean);
  };

  const truncateContent = (content: string, maxLength: number = 150) => {
    if (content.length <= maxLength) return content;
    return content.substring(0, maxLength).trim() + '...';
  };

  // Sort blog posts by date (most recent first)
  const sortedPosts = [...blogPosts].sort((a, b) => 
    new Date(b.published_date).getTime() - new Date(a.published_date).getTime()
  );

  const featuredPosts = sortedPosts.filter(post => post.featured);
  const regularPosts = sortedPosts.filter(post => !post.featured);

  return (
    <div className="component-card">
      <div className="component-title">
        <BookOpen size={20} />
        Blog & Articles
      </div>

      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">blogData</span> = {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">totalPosts</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{blogPosts.length}</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">featuredPosts</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{featuredPosts.length}</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">topics</span>: 
          <span className="syntax-string">"Tech, Development, Innovation"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">purpose</span>: 
          <span className="syntax-string">"Sharing knowledge and insights"</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      {blogPosts.length === 0 ? (
        <div style={{ 
          textAlign: 'center', 
          padding: '40px',
          color: 'var(--text-secondary)',
          fontStyle: 'italic'
        }}>
          <BookOpen size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
          <div>No blog posts available yet.</div>
          <div style={{ fontSize: '14px', marginTop: '8px' }}>
            Check back soon for interesting articles and insights!
          </div>
        </div>
      ) : (
        <div style={{ marginTop: '24px' }}>
          {/* Featured Posts */}
          {featuredPosts.length > 0 && (
            <div style={{ marginBottom: '32px' }}>
              <h4 style={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: '8px', 
                marginBottom: '16px',
                color: 'var(--text-primary)',
                fontSize: '16px',
                fontWeight: '600'
              }}>
                ⭐ Featured Articles
              </h4>

              <div style={{ 
                display: 'grid', 
                gap: '20px',
                gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))'
              }}>
                {featuredPosts.slice(0, 2).map((post) => (
                  <div key={post.id} style={{
                    background: 'linear-gradient(135deg, var(--bg-tertiary) 0%, var(--bg-secondary) 100%)',
                    borderRadius: '12px',
                    padding: '24px',
                    border: '2px solid var(--bg-accent)',
                    transition: 'all 0.3s ease',
                    position: 'relative',
                    overflow: 'hidden'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-4px)';
                    e.currentTarget.style.boxShadow = '0 12px 30px rgba(0, 120, 212, 0.2)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
                  }}>
                    <div style={{
                      position: 'absolute',
                      top: '12px',
                      right: '12px',
                      background: '#ff6b6b',
                      color: 'white',
                      padding: '4px 8px',
                      borderRadius: '12px',
                      fontSize: '10px',
                      fontWeight: '600',
                      textTransform: 'uppercase'
                    }}>
                      Featured
                    </div>

                    <h3 style={{ 
                      margin: '0 0 12px 0',
                      fontSize: '18px',
                      fontWeight: '700',
                      color: 'var(--text-primary)',
                      lineHeight: '1.3',
                      paddingRight: '60px'
                    }}>
                      {post.title}
                    </h3>

                    <div style={{
                      fontSize: '14px',
                      color: 'var(--text-secondary)',
                      marginBottom: '16px',
                      lineHeight: '1.5'
                    }}>
                      {post.summary || truncateContent(post.content)}
                    </div>

                    <div style={{ 
                      display: 'flex', 
                      justifyContent: 'space-between', 
                      alignItems: 'center',
                      marginBottom: '16px'
                    }}>
                      <div style={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: '12px',
                        fontSize: '12px',
                        color: 'var(--text-secondary)'
                      }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                          <Calendar size={12} />
                          {formatDate(post.published_date)}
                        </div>
                        {post.read_time && (
                          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            <Clock size={12} />
                            {post.read_time} min read
                          </div>
                        )}
                        {post.views && (
                          <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                            <Eye size={12} />
                            {post.views.toLocaleString()} views
                          </div>
                        )}
                      </div>
                    </div>

                    {post.tags && (
                      <div style={{ marginBottom: '16px' }}>
                        <div style={{ 
                          display: 'flex', 
                          flexWrap: 'wrap', 
                          gap: '6px'
                        }}>
                          {getTagArray(post.tags).slice(0, 3).map((tag, index) => (
                            <span
                              key={index}
                              style={{
                                fontSize: '10px',
                                padding: '4px 8px',
                                background: 'var(--bg-accent)',
                                color: 'white',
                                borderRadius: '12px',
                                fontWeight: '500'
                              }}
                            >
                              #{tag}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {post.external_url && (
                      <a
                        href={post.external_url}
                        target="_blank"
                        rel="noopener noreferrer"
                        style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '6px',
                          padding: '8px 16px',
                          background: 'var(--bg-accent)',
                          color: 'white',
                          textDecoration: 'none',
                          borderRadius: '6px',
                          fontSize: '12px',
                          fontWeight: '500',
                          transition: 'all 0.2s ease'
                        }}
                        onMouseEnter={(e) => {
                          e.currentTarget.style.transform = 'translateY(-1px)';
                          e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 120, 212, 0.3)';
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.transform = 'translateY(0)';
                          e.currentTarget.style.boxShadow = 'none';
                        }}
                      >
                        Read Article
                        <ExternalLink size={12} />
                      </a>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Regular Posts */}
          {regularPosts.length > 0 && (
            <div>
              <h4 style={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: '8px', 
                marginBottom: '16px',
                color: 'var(--text-primary)',
                fontSize: '16px',
                fontWeight: '600'
              }}>
                📝 All Articles
              </h4>

              <div style={{ 
                display: 'grid', 
                gap: '16px',
                gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))'
              }}>
                {regularPosts.map((post) => (
                  <div key={post.id} style={{
                    background: 'var(--bg-tertiary)',
                    borderRadius: '8px',
                    padding: '20px',
                    border: '1px solid var(--border-color)',
                    transition: 'all 0.3s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-2px)';
                    e.currentTarget.style.boxShadow = '0 8px 25px rgba(0, 0, 0, 0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
                  }}>
                    <h3 style={{ 
                      margin: '0 0 8px 0',
                      fontSize: '16px',
                      fontWeight: '600',
                      color: 'var(--text-primary)',
                      lineHeight: '1.3'
                    }}>
                      {post.title}
                    </h3>

                    <div style={{
                      fontSize: '13px',
                      color: 'var(--text-secondary)',
                      marginBottom: '12px',
                      lineHeight: '1.4'
                    }}>
                      {post.summary || truncateContent(post.content, 100)}
                    </div>

                    <div style={{ 
                      display: 'flex', 
                      justifyContent: 'space-between', 
                      alignItems: 'center',
                      marginBottom: '12px'
                    }}>
                      <div style={{ 
                        fontSize: '11px',
                        color: 'var(--text-secondary)',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '8px'
                      }}>
                        <Calendar size={10} />
                        {formatDate(post.published_date)}
                        {post.read_time && (
                          <>
                            • <Clock size={10} />
                            {post.read_time}m
                          </>
                        )}
                      </div>
                    </div>

                    {post.tags && (
                      <div style={{ marginBottom: '12px' }}>
                        <div style={{ 
                          display: 'flex', 
                          flexWrap: 'wrap', 
                          gap: '4px'
                        }}>
                          {getTagArray(post.tags).slice(0, 2).map((tag, index) => (
                            <span
                              key={index}
                              style={{
                                fontSize: '9px',
                                padding: '2px 6px',
                                background: 'var(--bg-secondary)',
                                color: 'var(--text-secondary)',
                                borderRadius: '8px',
                                border: '1px solid var(--border-color)'
                              }}
                            >
                              #{tag}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {post.external_url && (
                      <a
                        href={post.external_url}
                        target="_blank"
                        rel="noopener noreferrer"
                        style={{
                          fontSize: '11px',
                          color: 'var(--bg-accent)',
                          textDecoration: 'none',
                          fontWeight: '500',
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '4px'
                        }}
                      >
                        Read more
                        <ExternalLink size={10} />
                      </a>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      <div style={{ marginTop: '32px' }}>
        <div className="code-block">
          <div className="code-line">
            <span className="syntax-comment">// Content creation loop</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">async function</span> <span className="syntax-highlight">createContent</span>() {'{'}
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-keyword">const</span> <span className="syntax-highlight">inspiration</span> = <span className="syntax-keyword">await</span> <span className="syntax-highlight">findNewTopics</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-keyword">const</span> <span className="syntax-highlight">article</span> = <span className="syntax-highlight">writeWithPassion</span>(inspiration);
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-keyword">return</span> <span className="syntax-highlight">shareKnowledge</span>(article);
          </div>
          <div className="code-line">{'}'}</div>
        </div>
      </div>
    </div>
  );
} 