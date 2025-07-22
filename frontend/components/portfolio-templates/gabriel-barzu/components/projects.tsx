import React from 'react';
import { Project } from '@/lib/portfolio';
import { getSafeImageUrl } from '@/lib/image';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data: projects }: ProjectsProps) {
  if (!projects || projects.length === 0) {
    return null;
  }

  const featuredProjects = projects.filter(project => project.featured);
  const displayProjects = featuredProjects.length > 0 ? featuredProjects : projects.slice(0, 4);

  return (
    <section className="gb-projects">
      <h3 className="section-title">Featured Projects</h3>
      <div className="projects-grid">
        {displayProjects.map((project) => (
          <div key={project.id} className="project-card">
            <div className="project-image-container">
              <img 
                src={getSafeImageUrl(project.imageUrl)} 
                alt={project.title}
                className="project-image"
                onError={(e) => {
                  // Fallback to placeholder if image fails to load
                  e.currentTarget.src = getSafeImageUrl('');
                }}
              />
              <div className="project-overlay">
                <div className="project-links">
                  {project.demoUrl && (
                    <a 
                      href={project.demoUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="project-link project-demo"
                    >
                      Live Demo
                    </a>
                  )}
                  {project.githubUrl && (
                    <a 
                      href={project.githubUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="project-link project-code"
                    >
                      View Code
                    </a>
                  )}
                </div>
              </div>
            </div>
            <div className="project-info">
              <h4 className="project-title">{project.title}</h4>
              <p className="project-description">{project.description}</p>
              <div className="project-technologies">
                {project.technologies.map((tech, index) => (
                  <span key={index} className="tech-tag">
                    {tech}
                  </span>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
} 