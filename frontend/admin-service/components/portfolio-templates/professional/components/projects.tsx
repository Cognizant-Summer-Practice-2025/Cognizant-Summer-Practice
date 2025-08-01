"use client";

import React from 'react';
import Image from 'next/image';
import { Project } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ExternalLink, Github, Folder } from 'lucide-react';
import { getSafeImageUrl } from '@/lib/image';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data }: ProjectsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="prof-projects">
        <div className="prof-empty-state">
          <Folder size={48} />
          <h3>No Projects Data</h3>
          <p>Project portfolio will be displayed here when available.</p>
        </div>
      </div>
    );
  }

  const totalCount = data.length;
  const needsScrolling = data.length > 6;

  return (
    <div className="prof-projects">
      {/* Count indicator */}
      <div className="prof-count-indicator">
        <p className="prof-count-text">
          {totalCount} project{totalCount !== 1 ? 's' : ''} in my portfolio
        </p>
      </div>

      <div className={`prof-projects-container ${needsScrolling ? 'prof-scrollable' : ''}`}>
        <div className="prof-projects-grid">
          {data.map((project, index) => (
            <Card key={project.id || index} className="prof-project-card">
              {project.imageUrl && (
                <div className="prof-project-image">
                  <Image 
                    src={getSafeImageUrl(project.imageUrl)} 
                    alt={project.title}
                    className="prof-project-img"
                    width={400}
                    height={220}
                  />
                  <div className="prof-project-overlay">
                    <div className="prof-project-actions">
                      {project.demoUrl && (
                        <Button size="sm" variant="secondary">
                          <ExternalLink size={14} />
                          Live Demo
                        </Button>
                      )}
                      {project.githubUrl && (
                        <Button size="sm" variant="outline">
                          <Github size={14} />
                          Code
                        </Button>
                      )}
                    </div>
                  </div>
                </div>
              )}
              
              <div className="prof-project-content">
                <div className="prof-project-header">
                  <h3 className="prof-project-title">{project.title}</h3>
                </div>

                {project.description && (
                  <p className="prof-project-description">{project.description}</p>
                )}

                {project.technologies && project.technologies.length > 0 && (
                  <div className="prof-project-tech">
                    <div className="prof-tech-stack">
                      {project.technologies.slice(0, 4).map((tech, techIndex) => (
                        <span key={techIndex} className="prof-tech-badge">
                          {tech}
                        </span>
                      ))}
                      {project.technologies.length > 4 && (
                        <span className="prof-tech-more">
                          +{project.technologies.length - 4} more
                        </span>
                      )}
                    </div>
                  </div>
                )}

                <div className="prof-project-footer">
                  <div className="prof-project-links">
                    {project.demoUrl && (
                      <a 
                        href={project.demoUrl} 
                        target="_blank" 
                        rel="noopener noreferrer"
                        className="prof-project-link"
                      >
                        <ExternalLink size={14} />
                        Live Demo
                      </a>
                    )}
                    {project.githubUrl && (
                      <a 
                        href={project.githubUrl} 
                        target="_blank" 
                        rel="noopener noreferrer"
                        className="prof-project-link"
                      >
                        <Github size={14} />
                        Source Code
                      </a>
                    )}
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