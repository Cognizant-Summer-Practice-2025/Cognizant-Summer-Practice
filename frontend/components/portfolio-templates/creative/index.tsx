"use client";

import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { TemplateManager, ComponentMap } from '@/lib/template-manager';
import { Header } from './components/header';
import { Stats } from './components/stats';
import { About } from './components/about';
import { Experience } from './components/experience';
import { Projects } from './components/projects';
import { Skills } from './components/skills';
import { BlogPosts } from './components/blog-posts';
import { FileExplorer } from './components/file-explorer';
import { TerminalTab } from './components/terminal-tab';
import { Minimize2, Maximize2, X, Folder, FileText, Code2, Terminal, User, Briefcase, Lightbulb, BookOpen, Mail } from 'lucide-react';
import './styles/main.css';

interface CreativeTemplateProps {
  data: PortfolioDataFromDB;
}

// Component mapping for the template manager
const componentMap: ComponentMap = {
  experience: Experience,
  projects: Projects,
  skills: Skills,
  blog_posts: BlogPosts,
  about: About
};

// File structure mapping
const allFileStructure = [
  { name: 'about.md', icon: FileText, component: 'about' },
  { name: 'experience.json', icon: Code2, component: 'experience' },
  { name: 'projects/', icon: Folder, component: 'projects' },
  { name: 'skills.js', icon: Code2, component: 'skills' },
  { name: 'blog/', icon: Folder, component: 'blog_posts' }
];

export default function CreativeTemplate({ data }: CreativeTemplateProps) {
  const [activeFile, setActiveFile] = useState('about.md');
  const [darkMode, setDarkMode] = useState(true);
  const [openTabs, setOpenTabs] = useState(['about.md']);
  const [showTerminal, setShowTerminal] = useState(false);

  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(data);

  // Create component lookup
  const componentLookup = dynamicComponents.reduce((acc, comp) => {
    acc[comp.type] = comp;
    return acc;
  }, {} as Record<string, any>);

  // Filter file structure to only show enabled components
  const fileStructure = allFileStructure.filter(file => 
    componentLookup[file.component]
  );

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', darkMode ? 'dark' : 'light');
  }, [darkMode]);

  const handleFileClick = (fileName: string, componentType: string) => {
    setActiveFile(fileName);
    if (!openTabs.includes(fileName)) {
      setOpenTabs([...openTabs, fileName]);
    }
    
    // Auto-scroll to the dynamic content area
    setTimeout(() => {
      const dynamicContentElement = document.querySelector('.dynamic-content');
      if (dynamicContentElement) {
        dynamicContentElement.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'start' 
        });
      }
    }, 100); // Small delay to ensure content is rendered
  };

  const closeTab = (fileName: string, e: React.MouseEvent) => {
    e.stopPropagation();
    const newTabs = openTabs.filter(tab => tab !== fileName);
    setOpenTabs(newTabs);
    
    if (activeFile === fileName && newTabs.length > 0) {
      setActiveFile(newTabs[newTabs.length - 1]);
    }
  };

  const getActiveComponent = () => {
    const fileInfo = fileStructure.find(f => f.name === activeFile);
    if (!fileInfo) return null;
    
    const componentInfo = componentLookup[fileInfo.component];
    if (!componentInfo) return null;

    const Component = componentInfo.component;
    return <Component data={componentInfo.data} />;
  };

  const getFileIcon = (fileName: string) => {
    const fileInfo = fileStructure.find(f => f.name === fileName);
    return fileInfo ? fileInfo.icon : FileText;
  };

  return (
    <div className={`creative-template ${darkMode ? 'dark' : 'light'}`}>
      {/* VS Code Window Chrome */}
      <div className="vscode-window">
        <div className="title-bar">
          <div className="window-controls">
            <button className="control close" onClick={() => console.log('Close')}>
              <X size={12} />
            </button>
            <button className="control minimize" onClick={() => console.log('Minimize')}>
              <Minimize2 size={12} />
            </button>
            <button className="control maximize" onClick={() => console.log('Maximize')}>
              <Maximize2 size={12} />
            </button>
          </div>
          <div className="title">
            Portfolio - Visual Studio Code
          </div>
          <div className="theme-toggle">
            <button onClick={() => setDarkMode(!darkMode)}>
              {darkMode ? '☀️' : '🌙'}
            </button>
          </div>
        </div>

        {/* Menu Bar */}
        <div className="menu-bar">
          <span>File</span>
          <span>Edit</span>
          <span>View</span>
          <span>Terminal</span>
          <span>Help</span>
        </div>

        {/* Main Layout */}
        <div className="main-layout">
          {/* Sidebar with File Explorer */}
          <div className="sidebar">
            <div className="activity-bar">
              <div className="activity-item active">
                <Folder size={24} />
              </div>
              <div className="activity-item">
                <User size={24} />
              </div>
              <div className="activity-item">
                <Terminal size={24} onClick={() => setShowTerminal(!showTerminal)} />
              </div>
            </div>
            
            <FileExplorer
              files={fileStructure}
              onFileClick={handleFileClick}
              activeFile={activeFile}
            />
          </div>

          {/* Main Content Area */}
          <div className="editor-area">
            {/* Tab Bar */}
            <div className="tab-bar">
              {openTabs.map((tab) => {
                const IconComponent = getFileIcon(tab);
                return (
                  <div
                    key={tab}
                    className={`tab ${activeFile === tab ? 'active' : ''}`}
                    onClick={() => setActiveFile(tab)}
                  >
                    <IconComponent size={16} />
                    <span>{tab}</span>
                    {openTabs.length > 1 && (
                      <button
                        className="close-tab"
                        onClick={(e) => closeTab(tab, e)}
                      >
                        <X size={12} />
                      </button>
                    )}
                  </div>
                );
              })}
            </div>

            {/* Content Area */}
            <div className="content-area">
              <div className="editor-content">
                {/* Header Section - Always visible */}
                <div className="portfolio-header">
                  <Header basicInfo={data.profile} />
                  <Stats stats={data.stats} />
                </div>

                {/* Dynamic Content based on active file */}
                <div className="dynamic-content">
                  {getActiveComponent()}
                </div>
              </div>
            </div>

            {/* Terminal Section */}
            {showTerminal && (
              <div className="terminal-section">
                <TerminalTab onClose={() => setShowTerminal(false)} />
              </div>
            )}
          </div>
        </div>

        {/* Status Bar */}
        <div className="status-bar">
          <div className="status-left">
            <span>Portfolio</span>
            <span>•</span>
            <span>{activeFile}</span>
          </div>
          <div className="status-right">
            <span>Ready</span>
            <span>•</span>
            <span>{data.profile.name || 'Developer'}</span>
          </div>
        </div>
      </div>
    </div>
  );
} 