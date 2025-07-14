import React, { useState } from 'react';
import './style.css';

interface ProjectCardProps {
  title: string;
  description: string;
  image: string;
  technologies: string[];
  demoLink: string;
  codeLink: string;
}

const ProjectCard: React.FC<ProjectCardProps> = ({ 
  title, 
  description, 
  image, 
  technologies
}) => {
  return (
    <div className="project-card">
      <img className="project-image" src={image} alt={title} />
      <div className="project-content">
        <div className="project-title-container">
          <div className="project-title">{title}</div>
        </div>
        <div className="project-description-container">
          <div className="project-description">{description}</div>
        </div>
        <div className="project-technologies">
          {technologies.map((tech, index) => (
            <div key={index} className="technology-tag">
              <div className="technology-text">{tech}</div>
            </div>
          ))}
        </div>
        <div className="project-links">
          <div className="project-link-demo">
            <div className="link-text">Live Demo</div>
          </div>
          <div className="project-link-code">
            <div className="link-text">Code</div>
          </div>
        </div>
      </div>
    </div>
  );
};

const TabsSection = () => {
  const [activeTab, setActiveTab] = useState('projects');

  const projects = [
    {
      title: "E-Commerce Platform",
      description: "A full-featured e-commerce solution built with React, Node.js, and MongoDB.",
      image: "https://placehold.co/550x200",
      technologies: ["React", "Node.js", "MongoDB", "Stripe"],
      demoLink: "#",
      codeLink: "#"
    },
    {
      title: "Task Management App",
      description: "Collaborative task management tool with real-time updates and team collaboration features.",
      image: "https://placehold.co/550x200",
      technologies: ["Vue.js", "Socket.io", "PostgreSQL"],
      demoLink: "#",
      codeLink: "#"
    }
  ];

  const renderTabContent = () => {
    switch (activeTab) {
      case 'projects':
        return (
          <div className="tab-content-projects">
            {projects.map((project, index) => (
              <ProjectCard
                key={index}
                title={project.title}
                description={project.description}
                image={project.image}
                technologies={project.technologies}
                demoLink={project.demoLink}
                codeLink={project.codeLink}
              />
            ))}
          </div>
        );
      case 'skills':
        return (
          <div className="tab-content-skills">
            <p>Skills content coming soon...</p>
          </div>
        );
      case 'blog':
        return (
          <div className="tab-content-blog">
            <p>Blog content coming soon...</p>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="tabs-container">
      <div className="tabs-header">
        <div 
          className={`tab-item ${activeTab === 'projects' ? 'tab-active' : ''}`}
          onClick={() => setActiveTab('projects')}
        >
          <div className="tab-text">Projects</div>
        </div>
        <div 
          className={`tab-item ${activeTab === 'skills' ? 'tab-active' : ''}`}
          onClick={() => setActiveTab('skills')}
        >
          <div className="tab-text">Skills</div>
        </div>
        <div 
          className={`tab-item ${activeTab === 'blog' ? 'tab-active' : ''}`}
          onClick={() => setActiveTab('blog')}
        >
          <div className="tab-text">Blog</div>
        </div>
      </div>
      <div className="tabs-content">
        {renderTabContent()}
      </div>
    </div>
  );
};

export default TabsSection;