import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import { Project } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { ExternalLink, Github, Folder, File, Terminal, Star, GitBranch } from 'lucide-react';

interface ProjectsProps {
  data: Project[];
}

export function Projects({ data: projects }: ProjectsProps) {
  const [selectedProject, setSelectedProject] = useState<string | null>(null);
  const [lsOutput, setLsOutput] = useState<string[]>([]);
  const [showDetails, setShowDetails] = useState(false);

  useEffect(() => {
    // Simulate ls -la command output
    const safeProjects = Array.isArray(projects) ? projects : [];
    const displayProjects = safeProjects.slice(0, 5);
    const lsCommand = [
      '$ ls -la ~/projects',
      'total ' + safeProjects.length,
      'drwxr-xr-x 3 user user 4096 ' + new Date().toDateString() + ' .',
      'drwxr-xr-x 3 user user 4096 ' + new Date().toDateString() + ' ..',
      '',
      ...displayProjects.map((project, index) => {
        const permissions = 'drwxr-xr-x';
        const size = Math.floor(Math.random() * 9000) + 1000;
        const date = new Date().toDateString();
        const title = (project?.title ?? 'untitled').toString();
        return `${permissions} ${index + 1} user user ${size} ${date} ${title.toLowerCase().replace(/\s+/g, '-')}`;
      })
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < lsCommand.length) {
        setLsOutput(prev => [...prev, lsCommand[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 200);

    return () => clearInterval(interval);
  }, [projects]);

  const handleProjectSelect = (projectId: string) => {
    setSelectedProject(projectId);
    setShowDetails(true);
  };

  return (
    <div className="terminal-projects" id="projects">
      <div className="projects-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">projects_directory.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="ls-output">
            {lsOutput.map((line, index) => (
              <div key={index} className="output-line">
                {typeof line === 'string' && line.startsWith('$') ? (
                  <span className="command-line">
                    <Terminal className="cmd-icon" size={12} />
                    {line}
                  </span>
                ) : typeof line === 'string' && line.startsWith('drwxr-xr-x') ? (
                  <span className="file-line">
                    <Folder className="folder-icon" size={12} />
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

      <div className="projects-grid">
        <div className="grid-header">
          <Folder className="grid-icon" size={20} />
          <span>Project Repository</span>
          <span className="project-count">({projects.length} repositories)</span>
        </div>
        
        <div className="project-cards">
          {(Array.isArray(projects) ? projects : []).map((project, index) => (
            <div 
              key={project.id} 
              className="project-card"
              onClick={() => handleProjectSelect(project.id)}
            >
              <div className="card-header">
                <div className="repo-info">
                  <GitBranch className="repo-icon" size={16} />
                  <span className="repo-name">{(project.title ?? 'untitled').toLowerCase().replace(/\s+/g, '-')}</span>
                </div>
                <div className="repo-stats">
                  <Star className="star-icon" size={14} />
                  <span className="star-count">{Math.floor(Math.random() * 100) + 10}</span>
                </div>
              </div>

              {project.imageUrl && (
                <div className="project-preview">
                  <Image 
                    src={project.imageUrl} 
                    alt={project.title ?? 'Project image'}
                    className="preview-image"
                    width={400}
                    height={250}
                  />
                  <div className="preview-overlay">
                    <div className="file-structure">
                      <File className="file-icon" size={12} />
                      <span>README.md</span>
                    </div>
                    <div className="file-structure">
                      <File className="file-icon" size={12} />
                      <span>package.json</span>
                    </div>
                    <div className="file-structure">
                      <Folder className="folder-icon" size={12} />
                      <span>src/</span>
                    </div>
                  </div>
                </div>
              )}

              <div className="card-content">
                <h3 className="project-title">{project.title ?? 'Untitled Project'}</h3>
                <p className="project-description">{project.description ?? ''}</p>

                {project.technologies && (
                  <div className="tech-stack">
                    <div className="tech-header">
                      <File className="tech-icon" size={12} />
                      <span>Languages & Tools:</span>
                    </div>
                    <div className="tech-list">
                      {project.technologies.slice(0, 4).map((tech, techIndex) => (
                        <span key={techIndex} className="tech-badge">
                          {tech}
                        </span>
                      ))}
                      {project.technologies.length > 4 && (
                        <span className="tech-more">+{project.technologies.length - 4}</span>
                      )}
                    </div>
                  </div>
                )}

                <div className="project-actions">
                  {project.demoUrl && (
                    <Button className="action-btn demo-btn" size="sm">
                      <ExternalLink size={12} />
                      <span>Demo</span>
                    </Button>
                  )}
                  {project.githubUrl && (
                    <Button className="action-btn github-btn" size="sm">
                      <Github size={12} />
                      <span>Code</span>
                    </Button>
                  )}
                </div>
              </div>

              <div className="card-footer">
                <div className="commit-info">
                  <span className="commit-hash">#{(index + 1).toString().padStart(7, '0')}</span>
                  <span className="commit-message">Latest commit</span>
                  <span className="commit-time">{Math.floor(Math.random() * 30) + 1} days ago</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {selectedProject && showDetails && (
        <div className="project-details-modal">
          <div className="modal-terminal">
            {(() => {
              const project = projects.find(p => p.id === selectedProject);
              if (!project) return null;

              return (
                <>
                  <div className="terminal-header">
                    <div className="window-controls">
                      <span className="control red"></span>
                      <span className="control yellow"></span>
                      <span className="control green"></span>
                    </div>
                    <span className="window-title">{project.title.toLowerCase().replace(/\s+/g, '-')}/README.md</span>
                    <Button 
                      className="close-btn"
                      onClick={() => setShowDetails(false)}
                    >
                      ✕
                    </Button>
                  </div>
                  
                  <div className="modal-content">
                    <div className="readme-content">
                      <div className="readme-header">
                        <h1 className="readme-title"># {project.title}</h1>
                        <p className="readme-description">{project.description}</p>
                      </div>

                      {project.imageUrl && (
                        <div className="readme-image">
                          <Image src={project.imageUrl} alt={project.title} width={500} height={300} />
                        </div>
                      )}

                      {project.technologies && (
                        <div className="readme-section">
                          <h2 className="section-title">## Tech Stack</h2>
                          <div className="tech-grid">
                            {project.technologies.map((tech, index) => (
                              <span key={index} className="tech-item">
                                `{tech}`
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="readme-section">
                        <h2 className="section-title">## Installation</h2>
                        <div className="code-block">
                          <div className="code-header">
                            <Terminal className="code-icon" size={14} />
                            <span>bash</span>
                          </div>
                          <pre className="code-content">
                            {`git clone ${project.githubUrl || 'https://github.com/user/repo.git'}
cd ${project.title.toLowerCase().replace(/\s+/g, '-')}
npm install
npm start`}
                          </pre>
                        </div>
                      </div>

                      <div className="readme-actions">
                        {project.demoUrl && (
                          <Button className="readme-btn demo">
                            <ExternalLink size={16} />
                            View Live Demo
                          </Button>
                        )}
                        {project.githubUrl && (
                          <Button className="readme-btn github">
                            <Github size={16} />
                            View Source Code
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                </>
              );
            })()}
          </div>
        </div>
      )}

      <div className="projects-summary">
        <div className="summary-terminal">
          <div className="summary-header">
            <Terminal className="summary-icon" size={16} />
            <span>Repository Statistics</span>
          </div>
          
          <div className="summary-stats">
            <div className="stat-line">
              <span className="prompt">$</span>
              <span className="command">find ~/projects -name &ldquo;*.md&rdquo; | wc -l</span>
              <span className="output">{projects.length}</span>
            </div>
            <div className="stat-line">
              <span className="prompt">$</span>
              <span className="command">git log --oneline | wc -l</span>
              <span className="output">{projects.length * 47}</span>
            </div>
            <div className="stat-line">
              <span className="prompt">$</span>
              <span className="command">du -sh ~/projects</span>
              <span className="output">{projects.length * 23}MB</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Projects;