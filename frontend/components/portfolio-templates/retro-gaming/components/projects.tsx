import React, { useState } from 'react';
import { Project } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ExternalLink, Github, Play, Star, Trophy, Gamepad2 } from 'lucide-react';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data: projects }: ProjectsProps) {
  const [selectedProject, setSelectedProject] = useState<string | null>(null);

  const getItemRarity = (index: number) => {
    const rarities = ['legendary', 'epic', 'rare', 'common'];
    return rarities[index % rarities.length];
  };

  const getItemIcon = (rarity: string) => {
    switch (rarity) {
      case 'legendary': return <Trophy className="item-icon" size={20} />;
      case 'epic': return <Star className="item-icon" size={20} />;
      case 'rare': return <Gamepad2 className="item-icon" size={20} />;
      default: return <Play className="item-icon" size={20} />;
    }
  };

  const getRarityColor = (rarity: string) => {
    switch (rarity) {
      case 'legendary': return '#FFD700';
      case 'epic': return '#9D4EDD';
      case 'rare': return '#06FFA5';
      default: return '#FFFFFF';
    }
  };

  return (
    <div className="retro-projects" id="projects">
      <div className="section-header">
        <h2 className="section-title">
          <Trophy className="pixel-icon" size={24} />
          INVENTORY & ARTIFACTS
        </h2>
        <div className="pixel-border"></div>
        <p className="section-subtitle">Legendary Items & Creations</p>
      </div>

      <div className="inventory-grid">
        {projects.map((project, index) => {
          const rarity = getItemRarity(index);
          const isSelected = selectedProject === project.id;
          
          return (
            <Card 
              key={project.id} 
              className={`inventory-item ${rarity} ${isSelected ? 'selected' : ''}`}
              onClick={() => setSelectedProject(isSelected ? null : project.id)}
              style={{ '--rarity-color': getRarityColor(rarity) } as React.CSSProperties}
            >
              <div className="item-header">
                <div className="item-icon-container">
                  {getItemIcon(rarity)}
                  <div className="icon-glow"></div>
                </div>
                <div className="rarity-indicator">
                  <span className={`rarity-text ${rarity}`}>
                    {rarity.toUpperCase()}
                  </span>
                </div>
              </div>

              <div className="item-preview">
                {project.image && (
                  <img
                    src={project.image}
                    alt={project.title}
                    className="project-image pixel-art"
                  />
                )}
                <div className="image-overlay">
                  <Play className="play-icon" size={32} />
                </div>
              </div>

              <div className="item-info">
                <h3 className="item-name">{project.title}</h3>
                <p className="item-description">{project.description}</p>

                {project.technologies && (
                  <div className="item-stats">
                    <h4 className="stats-title">Enchantments:</h4>
                    <div className="tech-tags">
                      {project.technologies.slice(0, 3).map((tech, techIndex) => (
                        <span key={techIndex} className="tech-tag">
                          {tech}
                        </span>
                      ))}
                      {project.technologies.length > 3 && (
                        <span className="tech-more">
                          +{project.technologies.length - 3}
                        </span>
                      )}
                    </div>
                  </div>
                )}

                <div className="item-actions">
                  {project.demoUrl && (
                    <Button className="action-btn demo-btn" size="sm">
                      <ExternalLink size={14} />
                      <span>DEMO</span>
                    </Button>
                  )}
                  {project.githubUrl && (
                    <Button className="action-btn code-btn" size="sm">
                      <Github size={14} />
                      <span>CODE</span>
                    </Button>
                  )}
                </div>

                <div className="item-level">
                  <span className="level-text">LVL {index + 1}</span>
                  <div className="level-bar">
                    <div className="level-fill" style={{ width: '100%' }}></div>
                  </div>
                </div>
              </div>

              <div className="item-effects">
                <div className="sparkle-effect"></div>
                <div className="glow-effect"></div>
              </div>

              <div className={`item-border ${rarity}`}>
                <div className="border-corner tl"></div>
                <div className="border-corner tr"></div>
                <div className="border-corner bl"></div>
                <div className="border-corner br"></div>
              </div>
            </Card>
          );
        })}
      </div>

      {selectedProject && (
        <div className="item-details-modal">
          <Card className="details-card">
            {(() => {
              const project = projects.find(p => p.id === selectedProject);
              if (!project) return null;

              return (
                <>
                  <div className="details-header">
                    <h3 className="details-title">{project.title}</h3>
                    <Button 
                      className="close-btn"
                      onClick={() => setSelectedProject(null)}
                    >
                      ✕
                    </Button>
                  </div>

                  <div className="details-content">
                    {project.image && (
                      <img
                        src={project.image}
                        alt={project.title}
                        className="details-image"
                      />
                    )}
                    
                    <div className="details-info">
                      <p className="details-description">{project.description}</p>
                      
                      {project.technologies && (
                        <div className="details-tech">
                          <h4>Technologies Used:</h4>
                          <div className="tech-grid">
                            {project.technologies.map((tech, index) => (
                              <span key={index} className="tech-item">
                                {tech}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="details-actions">
                        {project.demoUrl && (
                          <Button className="action-button demo">
                            <ExternalLink size={16} />
                            View Demo
                          </Button>
                        )}
                        {project.githubUrl && (
                          <Button className="action-button github">
                            <Github size={16} />
                            View Code
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                </>
              );
            })()}
          </Card>
        </div>
      )}

      <div className="inventory-stats">
        <div className="stats-header">
          <Star className="pixel-icon" size={20} />
          <span>COLLECTION STATS</span>
        </div>
        <div className="stats-grid">
          <div className="stat-item">
            <span className="stat-label">Total Items:</span>
            <span className="stat-value">{projects.length}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Legendary:</span>
            <span className="stat-value">{Math.ceil(projects.length / 4)}</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Epic:</span>
            <span className="stat-value">{Math.floor(projects.length / 4)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Projects;