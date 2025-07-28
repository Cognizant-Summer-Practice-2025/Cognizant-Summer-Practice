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
  const [showTerminal, setShowTerminal] = useState(false);
  const [desktopOpenTabs, setDesktopOpenTabs] = useState(['about.md']);

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

  // Determine which tabs to show based on screen size
  const isMobile = typeof window !== 'undefined' && window.innerWidth <= 480;
  const openTabs = isMobile 
    ? fileStructure.map(file => file.name) // Mobile: all tabs always open
    : desktopOpenTabs.filter(tab => fileStructure.find(file => file.name === tab)); // Desktop: user-controlled tabs

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', darkMode ? 'dark' : 'light');
  }, [darkMode]);

  const handleFileClick = (fileName: string, componentType: string) => {
    setActiveFile(fileName);
    
    // On desktop, add tab to open tabs if not already open
    const currentMobile = window.innerWidth <= 480;
    if (!currentMobile && !desktopOpenTabs.includes(fileName)) {
      setDesktopOpenTabs([...desktopOpenTabs, fileName]);
    }
    
    // Auto-scroll to the dynamic content area (works on all devices)
    setTimeout(() => {
      if (currentMobile) {
        // Mobile: Try multiple scroll methods for better compatibility
        const contentArea = document.querySelector('.content-area');
        const dynamicContentElement = document.querySelector('.dynamic-content');
        const portfolioHeader = document.querySelector('.portfolio-header');
        
        if (contentArea && dynamicContentElement) {
          // Method 1: Try scrolling to the dynamic content element position
          const elementPosition = dynamicContentElement.offsetTop;
          
          // Try both scrollTo and scrollTop for maximum compatibility
          contentArea.scrollTo({ 
            top: elementPosition, 
            behavior: 'smooth' 
          });
          
          // Fallback: direct scrollTop assignment
          setTimeout(() => {
            contentArea.scrollTop = elementPosition;
          }, 100);
          
        } else if (contentArea && portfolioHeader) {
          // Method 2: Scroll past header
          const headerHeight = portfolioHeader.offsetHeight + portfolioHeader.offsetTop;
          contentArea.scrollTo({ 
            top: headerHeight + 20, 
            behavior: 'smooth' 
          });
        } else if (contentArea) {
          // Method 3: Simple scroll down from top
          contentArea.scrollTo({ 
            top: 300, // Approximate header height
            behavior: 'smooth' 
          });
        }
      } else {
        // Desktop: scroll to the dynamic content section
        const dynamicContentElement = document.querySelector('.dynamic-content');
        if (dynamicContentElement) {
          dynamicContentElement.scrollIntoView({ 
            behavior: 'smooth', 
            block: 'start' 
          });
        }
      }
    }, 200); // Longer delay for mobile
  };

  const closeTab = (fileName: string, e: React.MouseEvent) => {
    e.stopPropagation();
    
    // On mobile (≤480px), tabs cannot be closed
    if (window.innerWidth <= 480) {
      return;
    }
    
    // Desktop: Remove tab from openTabs and update state
    const newTabs = desktopOpenTabs.filter(tab => tab !== fileName);
    setDesktopOpenTabs(newTabs);
    
    // If we're closing the active tab, switch to the last remaining tab
    if (activeFile === fileName && newTabs.length > 0) {
      setActiveFile(newTabs[newTabs.length - 1]);
    }
    
    // If no tabs remain, default to the first available component
    if (newTabs.length === 0 && fileStructure.length > 0) {
      const firstFile = fileStructure[0].name;
      setActiveFile(firstFile);
      setDesktopOpenTabs([firstFile]);
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
                    onClick={() => {
                      setActiveFile(tab);
                      
                      // Mobile: immediate scroll after tab click
                      if (window.innerWidth <= 480) {
                        setTimeout(() => {
                          const contentArea = document.querySelector('.content-area');
                          const dynamicContent = document.querySelector('.dynamic-content');
                          
                          if (contentArea && dynamicContent) {
                            const scrollPosition = dynamicContent.offsetTop;
                            contentArea.scrollTo({
                              top: scrollPosition,
                              behavior: 'smooth'
                            });
                          }
                        }, 50);
                      }
                    }}
                  >
                    <IconComponent size={16} />
                    <span>{tab}</span>
                    {/* Close button hidden on mobile via CSS */}
                    <button
                      className="close-tab"
                      onClick={(e) => closeTab(tab, e)}
                    >
                      <X size={12} />
                    </button>
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