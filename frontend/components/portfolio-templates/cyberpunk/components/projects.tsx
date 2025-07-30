import React from 'react';
import { Project } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ExternalLink, Github, Database, Star } from 'lucide-react';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data }: ProjectsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="cyberpunk-projects empty">
        <div className="empty-state">
          <Database size={48} className="empty-icon" />
          <h3>No projects found</h3>
          <p>Database query returned 0 results</p>
        </div>
      </div>
    );
  }

  return (
    <div className="cyberpunk-projects">
      <div className="projects-header">
        <h2 className="section-title">
          <Database size={24} />
          Project Database
        </h2>
        <div className="query-info">
          <span className="query-text">SELECT * FROM projects WHERE status = 'active';</span>
          <span className="result-count">{data.length} records found</span>
        </div>
      </div>

      <div className="projects-grid">
        {data.map((project, index) => (
          <Card key={project.id} className="project-card">
            <div className="card-header">
              <div className="project-meta">
                <span className="project-id">ID: {String(index + 1).padStart(3, '0')}</span>
                {project.featured && (
                  <Badge className="featured-badge">
                    <Star size={12} />
                    FEATURED
                  </Badge>
                )}
              </div>
              <h3 className="project-title">{project.title}</h3>
            </div>
            
            {project.imageUrl && (
              <div className="project-image">
                <img 
                  src={project.imageUrl} 
                  alt={project.title}
                  className="image"
                />
                <div className="image-overlay">
                  <div className="overlay-content">
                    <span>Preview Available</span>
                  </div>
                </div>
              </div>
            )}
            
            <div className="project-content">
              {project.description && (
                <p className="project-description">{project.description}</p>
              )}
              
              {project.technologies && project.technologies.length > 0 && (
                <div className="tech-stack">
                  <span className="tech-label">Tech Stack:</span>
                  <div className="tech-tags">
                    {project.technologies.map((tech, techIndex) => (
                      <Badge key={techIndex} variant="outline" className="tech-tag">
                        {tech}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}
              
              <div className="project-actions">
                {project.demoUrl && (
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="action-button demo-button"
                    onClick={() => window.open(project.demoUrl, '_blank')}
                  >
                    <ExternalLink size={16} />
                    Live Demo
                  </Button>
                )}
                {project.githubUrl && (
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="action-button github-button"
                    onClick={() => window.open(project.githubUrl, '_blank')}
                  >
                    <Github size={16} />
                    Source
                  </Button>
                )}
              </div>
            </div>
            
            <div className="card-footer">
              <div className="project-dates">
                <span className="date-label">Created:</span>
                <span className="date-value">
                  {new Date(project.createdAt).toLocaleDateString()}
                </span>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}