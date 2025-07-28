import React from 'react';
import { ExternalLink, Github, Folder, Calendar, Tag } from 'lucide-react';

interface Project {
  id: number;
  title: string;
  description: string;
  technologies?: string;
  github_url?: string;
  live_url?: string;
  image_url?: string;
  start_date?: string;
  end_date?: string;
  status?: string;
}

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data }: ProjectsProps) {
  const projects = data || [];

  const getTechArray = (technologies: string | null | undefined = '') => {
    if (!technologies || typeof technologies !== 'string') {
      return [];
    }
    return technologies.split(',').map(tech => tech.trim()).filter(Boolean);
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short' 
    });
  };

  const getStatusColor = (status: string = '') => {
    switch (status.toLowerCase()) {
      case 'completed':
        return '#43e97b';
      case 'in-progress':
        return '#ffbd2e';
      case 'planning':
        return '#ff5f57';
      default:
        return 'var(--bg-accent)';
    }
  };

  return (
    <div className="component-card">
      <div className="component-title">
        <Folder size={20} />
        Projects Portfolio
      </div>

      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">projectsData</span> = [
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-comment">// {projects.length} amazing projects and counting...</span>
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-string">"Each project tells a unique story"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-string">"Built with passion and attention to detail"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-string">"Always pushing the boundaries of what's possible"</span>
        </div>
        <div className="code-line">];</div>
      </div>

      {projects.length === 0 ? (
        <div style={{ 
          textAlign: 'center', 
          padding: '40px',
          color: 'var(--text-secondary)',
          fontStyle: 'italic'
        }}>
          <Folder size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
          <div>No projects to display yet.</div>
          <div style={{ fontSize: '14px', marginTop: '8px' }}>
            Check back soon for exciting updates!
          </div>
        </div>
      ) : (
        <div className="projects-grid" style={{ marginTop: '24px' }}>
          {projects.map((project) => (
            <div key={project.id} className="project-card">
              <div style={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'flex-start',
                marginBottom: '12px'
              }}>
                <h3 style={{ 
                  margin: 0,
                  fontSize: '18px',
                  fontWeight: '600',
                  color: 'var(--text-primary)'
                }}>
                  {project.title}
                </h3>
                {project.status && (
                  <span style={{
                    fontSize: '10px',
                    padding: '4px 8px',
                    borderRadius: '12px',
                    background: getStatusColor(project.status),
                    color: 'white',
                    fontWeight: '500',
                    textTransform: 'uppercase',
                    letterSpacing: '0.5px'
                  }}>
                    {project.status}
                  </span>
                )}
              </div>

              <div style={{
                fontSize: '14px',
                color: 'var(--text-secondary)',
                marginBottom: '16px',
                lineHeight: '1.5'
              }}>
                {project.description}
              </div>

              {project.technologies && (
                <div style={{ marginBottom: '16px' }}>
                  <div style={{ 
                    display: 'flex', 
                    flexWrap: 'wrap', 
                    gap: '6px'
                  }}>
                    {getTechArray(project.technologies).map((tech, index) => (
                      <span
                        key={index}
                        style={{
                          fontSize: '11px',
                          padding: '4px 8px',
                          background: 'var(--bg-tertiary)',
                          color: 'var(--text-primary)',
                          borderRadius: '4px',
                          border: '1px solid var(--border-color)',
                          fontFamily: 'Consolas, Monaco, monospace'
                        }}
                      >
                        {tech}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {(project.start_date || project.end_date) && (
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '8px',
                  marginBottom: '16px',
                  fontSize: '12px',
                  color: 'var(--text-secondary)'
                }}>
                  <Calendar size={14} />
                  <span>
                    {formatDate(project.start_date || '')} 
                    {project.end_date && ` - ${formatDate(project.end_date)}`}
                  </span>
                </div>
              )}

              <div style={{ 
                display: 'flex', 
                gap: '12px',
                marginTop: 'auto'
              }}>
                {project.github_url && (
                  <a
                    href={project.github_url}
                    target="_blank"
                    rel="noopener noreferrer"
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      padding: '8px 12px',
                      background: 'var(--bg-tertiary)',
                      color: 'var(--text-primary)',
                      textDecoration: 'none',
                      borderRadius: '6px',
                      fontSize: '12px',
                      border: '1px solid var(--border-color)',
                      transition: 'all 0.2s ease'
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.background = 'var(--hover-bg)';
                      e.currentTarget.style.transform = 'translateY(-1px)';
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.background = 'var(--bg-tertiary)';
                      e.currentTarget.style.transform = 'translateY(0)';
                    }}
                  >
                    <Github size={14} />
                    Code
                  </a>
                )}
                
                {project.live_url && (
                  <a
                    href={project.live_url}
                    target="_blank"
                    rel="noopener noreferrer"
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '6px',
                      padding: '8px 12px',
                      background: 'var(--bg-accent)',
                      color: 'white',
                      textDecoration: 'none',
                      borderRadius: '6px',
                      fontSize: '12px',
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
                    <ExternalLink size={14} />
                    Live Demo
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <div style={{ marginTop: '24px' }}>
        <div className="code-block">
          <div className="code-line">
            <span className="syntax-comment">// Always working on something new</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">setInterval</span>(() => {'{'}
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">createAmazingProject</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">learnNewTechnology</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">pushBoundaries</span>();
          </div>
          <div className="code-line">{'}'}, <span style={{ color: '#79c0ff' }}>EVERY_DAY</span>);</div>
        </div>
      </div>
    </div>
  );
} 