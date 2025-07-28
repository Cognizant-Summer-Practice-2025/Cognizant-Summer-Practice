import React from 'react';
import { Project } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { ExternalLink, Github, Star } from 'lucide-react';
import { getSafeImageUrl } from '@/lib/image';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data: projects }: ProjectsProps) {
  if (!projects || projects.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No projects available.
      </div>
    );
  }

  const totalCount = projects.length;

  return (
    <div className="modern-component-container">
      {/* Count indicator */}
      <div className="mb-4 pb-2 border-b border-border">
        <p className="text-sm text-muted-foreground">
          {totalCount} project{totalCount !== 1 ? 's' : ''}
        </p>
      </div>

      <div className="max-h-[800px] overflow-y-auto pr-2">
      <div className="modern-grid">
        {projects.map((project) => (
        <Card key={project.id} className="modern-project-card">
          {project.imageUrl ? (
            <img 
              src={getSafeImageUrl(project.imageUrl)} 
              alt={project.title}
              className="modern-project-image"
              onError={(e) => {
                // Fallback to placeholder if image fails to load
                e.currentTarget.src = getSafeImageUrl('');
              }}
            />
          ) : (
            <div className="modern-project-image flex items-center justify-center">
              <div className="text-white text-6xl font-bold opacity-50">
                {project.title.charAt(0)}
              </div>
            </div>
          )}
          
          <CardContent className="modern-project-content">
            <div className="flex items-start justify-between mb-2">
              <h3 className="modern-project-title">{project.title}</h3>
              {project.featured && (
                <Star size={16} className="text-yellow-500 fill-current" />
              )}
            </div>
            
            {project.description && (
              <p className="modern-project-description">
                {project.description}
              </p>
            )}
            
            {project.technologies && project.technologies.length > 0 && (
              <div className="modern-project-tech">
                {project.technologies.map((tech, index) => (
                  <Badge key={index} className="modern-tech-tag">
                    {tech}
                  </Badge>
                ))}
              </div>
            )}
            
            <div className="flex gap-2 mt-4">
              {project.demoUrl && (
                <Button size="sm" variant="default" asChild>
                  <a 
                    href={project.demoUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="flex items-center gap-1"
                  >
                    <ExternalLink size={14} />
                    Demo
                  </a>
                </Button>
              )}
              
              {project.githubUrl && (
                <Button size="sm" variant="outline" asChild>
                  <a 
                    href={project.githubUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="flex items-center gap-1"
                  >
                    <Github size={14} />
                    Code
                  </a>
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
        ))}
      </div>
      </div>
    </div>
  );
} 