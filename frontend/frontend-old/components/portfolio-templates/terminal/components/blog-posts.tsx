import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import { BlogPost } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { ExternalLink, Calendar, FileText, Terminal, Search } from 'lucide-react';

interface BlogPostsProps {
  data: BlogPost[];
}

export function BlogPosts({ data: blogPosts }: BlogPostsProps) {
  const [terminalOutput, setTerminalOutput] = useState<string[]>([]);
  const [selectedPost, setSelectedPost] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    if (!blogPosts || blogPosts.length === 0) return;

    // Simulate ls and grep commands for blog posts
    const commands = [
      '$ ls -la ~/blog/',
      'total ' + blogPosts.length,
      'drwxr-xr-x 3 user user 4096 ' + new Date().toDateString() + ' .',
      'drwxr-xr-x 3 user user 4096 ' + new Date().toDateString() + ' ..',
      '',
      ...blogPosts.map((post) => {
        const date = post.publishedAt ? new Date(post.publishedAt).toDateString() : 'No date';
        const size = Math.floor(Math.random() * 5000) + 1000;
        return `-rw-r--r-- 1 user user ${size} ${date} ${post.title.toLowerCase().replace(/\s+/g, '-')}.md`;
      }),
      '',
      '$ wc -w ~/blog/*.md',
      ...blogPosts.map(post => `${Math.floor(Math.random() * 2000) + 500} ${post.title.toLowerCase().replace(/\s+/g, '-')}.md`),
      `${blogPosts.reduce((sum) => sum + Math.floor(Math.random() * 2000) + 500, 0)} total`
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < commands.length) {
        setTerminalOutput(prev => [...prev, commands[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 150);

    return () => clearInterval(interval);
  }, [blogPosts]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric'
    });
  };

  const filteredPosts = blogPosts?.filter(post =>
    post.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    post.excerpt?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    post.tags?.some(tag => tag.toLowerCase().includes(searchTerm.toLowerCase()))
  ) || [];

  if (!blogPosts || blogPosts.length === 0) {
    return (
      <div className="terminal-blog-posts" id="blog">
        <div className="empty-blog-terminal">
          <div className="terminal-header">
            <div className="window-controls">
              <span className="control red"></span>
              <span className="control yellow"></span>
              <span className="control green"></span>
            </div>
            <span className="window-title">blog_posts.sh</span>
          </div>
          
          <div className="terminal-content">
            <div className="empty-output">
              <div className="command-line">
                <Terminal className="cmd-icon" size={12} />
                <span>$ ls ~/blog/</span>
              </div>
              <div className="output-line">ls: ~/blog/: No such file or directory</div>
              <div className="output-line"></div>
              <div className="output-line">Blog directory is empty. No posts found.</div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="terminal-blog-posts" id="blog">
      <div className="blog-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">blog_directory.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="blog-output">
            {terminalOutput.map((line, index) => (
              <div key={index} className="output-line">
                {line.startsWith('$') ? (
                  <span className="command-line">
                    <Terminal className="cmd-icon" size={12} />
                    {line}
                  </span>
                ) : line.startsWith('-rw-r--r--') ? (
                  <span className="file-line">
                    <FileText className="file-icon" size={12} />
                    {line}
                  </span>
                ) : (
                  line
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="blog-interface">
        <div className="interface-header">
          <FileText className="interface-icon" size={20} />
          <span>Blog Posts</span>
          <span className="post-count">({blogPosts.length} articles)</span>
          
          <div className="search-controls">
            <div className="search-field">
              <Search className="search-icon" size={16} />
              <input
                type="text"
                placeholder="grep -i 'search term'"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
              />
            </div>
          </div>
        </div>
        
        <div className="blog-list">
          {filteredPosts.map((post) => (
            <div 
              key={post.id} 
              className={`blog-entry ${selectedPost === post.id ? 'selected' : ''}`}
              onClick={() => setSelectedPost(selectedPost === post.id ? null : post.id)}
            >
              <div className="entry-header">
                <div className="file-info">
                  <FileText className="file-icon" size={16} />
                  <span className="filename">{post.title.toLowerCase().replace(/\s+/g, '-')}.md</span>
                </div>
                
                <div className="entry-meta">
                  <div className="date-info">
                    <Calendar className="meta-icon" size={14} />
                    <span>{post.publishedAt ? formatDate(post.publishedAt) : 'No date'}</span>
                  </div>

                </div>
              </div>

              {post.featuredImageUrl && (
                <div className="entry-preview">
                  <Image
                    src={post.featuredImageUrl}
                    alt={post.title}
                    className="preview-image"
                    width={300}
                    height={200}
                  />
                  <div className="preview-overlay">
                    <div className="file-stats">
                      <span>Size: {Math.floor(Math.random() * 5000) + 1000}B</span>
                      <span>Modified: {post.publishedAt ? formatDate(post.publishedAt) : 'No date'}</span>
                    </div>
                  </div>
                </div>
              )}

              <div className="entry-content">
                <h3 className="entry-title"># {post.title}</h3>
                <p className="entry-excerpt">{post.excerpt}</p>

                {post.tags && post.tags.length > 0 && (
                  <div className="entry-tags">
                    <span className="tags-label">Tags:</span>
                    <div className="tag-list">
                      {post.tags.map((tag, tagIndex) => (
                        <span key={tagIndex} className="tag">
                          #{tag}
                        </span>
                      ))}
                    </div>
                  </div>
                )}

                <div className="entry-stats">
                  <div className="stat-item">
                    <span className="stat-label">Lines:</span>
                    <span className="stat-value">{Math.floor(Math.random() * 200) + 50}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-label">Words:</span>
                    <span className="stat-value">{Math.floor(Math.random() * 2000) + 500}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-label">Chars:</span>
                    <span className="stat-value">{Math.floor(Math.random() * 10000) + 2000}</span>
                  </div>
                </div>

                <div className="entry-actions">
                  <Button className="read-button terminal-button" size="sm">
                    <ExternalLink size={14} />
                    <span>cat {post.title.toLowerCase().replace(/\s+/g, '-')}.md</span>
                  </Button>
                </div>
              </div>

              <div className="entry-footer">
                <div className="file-permissions">
                  <span className="permissions">-rw-r--r--</span>
                  <span className="owner">user:user</span>
                  <span className="size">{Math.floor(Math.random() * 5000) + 1000}B</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedPost && (
        <div className="post-details-modal">
          <div className="modal-terminal">
            {(() => {
              const post = blogPosts.find(p => p.id === selectedPost);
              if (!post) return null;

              return (
                <>
                  <div className="terminal-header">
                    <div className="window-controls">
                      <span className="control red"></span>
                      <span className="control yellow"></span>
                      <span className="control green"></span>
                    </div>
                    <span className="window-title">cat {post.title.toLowerCase().replace(/\s+/g, '-')}.md</span>
                    <Button 
                      className="close-btn"
                      onClick={() => setSelectedPost(null)}
                    >
                      ✕
                    </Button>
                  </div>
                  
                  <div className="modal-content">
                    <div className="file-header">
                      <div className="command-line">
                        <Terminal className="cmd-icon" size={12} />
                        <span>$ cat ~/blog/{post.title.toLowerCase().replace(/\s+/g, '-')}.md</span>
                      </div>
                    </div>
                    
                    <div className="markdown-content">
                      <div className="md-header">
                        <span className="md-syntax">---</span>
                        <div className="md-frontmatter">
                          <div>title: &ldquo;{post.title}&rdquo;</div>
                          <div>date: {post.publishedAt}</div>
                          {post.tags && <div>tags: [{post.tags.map(tag => `"${tag}"`).join(', ')}]</div>}

                        </div>
                        <span className="md-syntax">---</span>
                      </div>
                      
                      <div className="md-body">
                        <h1 className="md-title"># {post.title}</h1>
                        
                        {post.featuredImageUrl && (
                          <div className="md-image">
                            <span className="md-syntax">![{post.title}]({post.featuredImageUrl})</span>
                            <Image src={post.featuredImageUrl} alt={post.title} width={400} height={250} />
                          </div>
                        )}
                        
                        <div className="md-content">
                          <p>{post.excerpt}</p>
                          
                          <div className="md-footer">
                            <span className="md-syntax">---</span>
                            <div className="publish-info">
                              Published: {post.publishedAt ? formatDate(post.publishedAt) : 'No date'}

                            </div>
                          </div>
                        </div>
                      </div>
                      
                      <div className="external-link">
                        <Button className="external-button">
                          <ExternalLink size={16} />
                          Read Full Article
                        </Button>
                      </div>
                    </div>
                  </div>
                </>
              );
            })()}
          </div>
        </div>
      )}

      <div className="blog-summary">
        <div className="summary-terminal">
          <div className="summary-header">
            <Terminal className="summary-icon" size={16} />
            <span>Blog Statistics</span>
          </div>
          
          <div className="summary-stats">
            <div className="stat-command">
              <span className="prompt">$</span>
              <span className="command">find ~/blog -name &ldquo;*.md&rdquo; | wc -l</span>
              <span className="output">{blogPosts.length}</span>
            </div>
            <div className="stat-command">
              <span className="prompt">$</span>
              <span className="command">grep -o &apos;#[a-zA-Z]*&apos; ~/blog/*.md | sort | uniq | wc -l</span>
              <span className="output">{new Set(blogPosts.flatMap(post => post.tags || [])).size}</span>
            </div>
            <div className="stat-command">
              <span className="prompt">$</span>
              <span className="command">wc -w ~/blog/*.md | tail -1</span>
              <span className="output">{blogPosts.reduce((sum) => sum + Math.floor(Math.random() * 2000) + 500, 0)} total words</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default BlogPosts;