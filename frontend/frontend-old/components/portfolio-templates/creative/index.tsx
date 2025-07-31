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
import { Minimize2, Maximize2, X, Folder, FileText, Code2, Terminal, User } from 'lucide-react';
import './styles/main.css';

interface CreativeTemplateProps {
  data: PortfolioDataFromDB;
}

interface ComponentInfo {
  component: React.ComponentType<{ data: unknown }>;
  data: unknown;
  id: string;
  type: string;
}

// Component mapping for the template manager
const componentMap: ComponentMap = {
  experience: Experience as React.ComponentType<{ data: unknown }>,
  projects: Projects as React.ComponentType<{ data: unknown }>,
  skills: Skills as React.ComponentType<{ data: unknown }>,
  blog_posts: BlogPosts as React.ComponentType<{ data: unknown }>,
  about: About as React.ComponentType<{ data: unknown }>
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
  const [hasAutoScrolled, setHasAutoScrolled] = useState(false);

  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(data);

  // Create component lookup
  const componentLookup = dynamicComponents.reduce((acc, comp) => {
    if (comp) {
    acc[comp.type] = comp;
    }
    return acc;
  }, {} as Record<string, unknown>);

  // If no components are available, create a default about component
  if (dynamicComponents.length === 0) {
    // Add a default about component
    componentLookup['about'] = {
      id: 'default-about',
      type: 'about',
      order: 1,
      component: About,
      data: data.quotes || [
        {
          id: 1,
          text: data.profile.bio || "Welcome to my portfolio. I'm passionate about creating amazing digital experiences.",
          author: data.profile.name || 'Developer',
          context: "About me"
        }
      ]
    };
  }

  // Filter file structure to only show enabled components
  let fileStructure = allFileStructure.filter(file => 
    componentLookup[file.component]
  );

  // Ensure we always have at least about.md available
  if (fileStructure.length === 0) {
    fileStructure = [{ name: 'about.md', icon: FileText, component: 'about' }];
  }

  // Ensure we have a valid active file when fileStructure changes
  React.useEffect(() => {
    if (fileStructure.length > 0 && !fileStructure.find(f => f.name === activeFile)) {
      const firstFile = fileStructure[0].name;
      setActiveFile(firstFile);
      setDesktopOpenTabs([firstFile]);
    }
  }, [fileStructure, activeFile]);

  // Determine which tabs to show based on screen size
  const isMobile = typeof window !== 'undefined' && window.innerWidth <= 480;
  const openTabs = isMobile 
    ? fileStructure.map(file => file.name) // Mobile: all tabs always open
    : desktopOpenTabs.filter(tab => fileStructure.find(file => file.name === tab));

  // Auto-scroll to components on desktop load
  React.useEffect(() => {
    if (!hasAutoScrolled && typeof window !== 'undefined') {
      const isMobile = window.innerWidth <= 480;
      const isDesktop = window.innerWidth > 768;
      
      if (isDesktop) {
        setTimeout(() => {
          const contentArea = document.querySelector('.content-area');
          const dynamicContentContainer = document.querySelector('.dynamic-content-container');
          
          if (contentArea && dynamicContentContainer) {
            // Scroll to the dynamic content section
            const dynamicContentTop = (dynamicContentContainer as HTMLElement).offsetTop;
            contentArea.scrollTo({
              top: dynamicContentTop,
              behavior: 'smooth'
            });
          }
          setHasAutoScrolled(true);
        }, 1000); // Delay to allow animations to settle
      } else if (isMobile) {
        // Auto-scroll on mobile to show content beyond header
        setTimeout(() => {
          const contentArea = document.querySelector('.content-area');
          const portfolioHeader = document.querySelector('.portfolio-header-container');
          const dynamicContent = document.querySelector('.dynamic-content');
          
          if (contentArea) {
            let scrollTarget = 0;
            
            if (portfolioHeader) {
              // Scroll to just past the header to show dynamic content
              const headerHeight = (portfolioHeader as HTMLElement).offsetHeight;
              scrollTarget = Math.max(headerHeight - 20, 0); // Show a bit of header for context
            } else if (dynamicContent) {
              // Fallback: scroll to dynamic content
              scrollTarget = (dynamicContent as HTMLElement).offsetTop - 20;
            }
            
            contentArea.scrollTo({
              top: scrollTarget,
              behavior: 'smooth'
            });
          }
          setHasAutoScrolled(true);
        }, 1500); // Longer delay for mobile to ensure everything is rendered
      } else {
        setHasAutoScrolled(true); // Skip auto-scroll on tablet
      }
    }
  }, [hasAutoScrolled, fileStructure.length]);

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', darkMode ? 'dark' : 'light');
  }, [darkMode]);

  // Reset auto-scroll state on window resize to handle device rotation
  useEffect(() => {
    const handleResize = () => {
      setHasAutoScrolled(false);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const handleFileClick = (fileName: string) => {
    setActiveFile(fileName);
    
    // On desktop, add tab to open tabs if not already open
    const currentMobile = window.innerWidth <= 480;
    if (!currentMobile && !desktopOpenTabs.includes(fileName)) {
      setDesktopOpenTabs([...desktopOpenTabs, fileName]);
    }
    
    // Auto-scroll to the dynamic content area (works on all devices)
    setTimeout(() => {
      if (currentMobile) {
        // Mobile: Enhanced scrolling with multiple fallback methods
        const contentArea = document.querySelector('.content-area');
        const dynamicContentElement = document.querySelector('.dynamic-content');
        const dynamicContentContainer = document.querySelector('.dynamic-content-container');
        const portfolioHeader = document.querySelector('.portfolio-header-container');
        
        if (contentArea) {
          let scrollTarget = 0;
          
          // Try to find the best scroll target
          if (portfolioHeader) {
            // Method 1: Scroll to just past the header (preferred)
            const headerHeight = (portfolioHeader as HTMLElement).offsetHeight;
            scrollTarget = headerHeight - 20; // Show a bit of header for context
          } else if (dynamicContentContainer) {
            // Method 2: Scroll to dynamic content container
            scrollTarget = (dynamicContentContainer as HTMLElement).offsetTop;
          } else if (dynamicContentElement) {
            // Method 3: Scroll to dynamic content element
            scrollTarget = (dynamicContentElement as HTMLElement).offsetTop;
          } else {
            // Method 4: Simple scroll down from top
            scrollTarget = 300; // Approximate header height
          }
          
          // Apply smooth scrolling
          contentArea.scrollTo({ 
            top: scrollTarget, 
            behavior: 'smooth' 
          });
          
          // Fallback: direct scrollTop assignment for older browsers
          setTimeout(() => {
            if (contentArea.scrollTop === 0) {
              contentArea.scrollTop = scrollTarget;
            }
          }, 300);
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
    }, 200); // Delay for content to update
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
    if (!fileInfo) {
      // Fallback to first available component if activeFile not found
      if (fileStructure.length > 0) {
        const firstFile = fileStructure[0];
        const componentInfo = componentLookup[firstFile.component] as ComponentInfo;
        if (componentInfo) {
          const Component = componentInfo.component;
          return <Component data={componentInfo.data} />;
        }
      }
      // Last resort: render about component with default data
      return (
        <div className="component-card">
          <div className="component-title">
            <User size={20} />
            About Me
          </div>
          <div className="code-block">
            <div className="code-line">
              <span className="syntax-comment">{`// Welcome to my portfolio`}</span>
            </div>
            <div className="code-line">
              <span className="syntax-keyword">const</span>{' '}
              <span className="syntax-highlight">about</span> = {'{'}
            </div>
            <div className="code-line" style={{ marginLeft: '20px' }}>
              <span className="syntax-highlight">message</span>: 
              <span className="syntax-string">&quot;{data.profile?.bio || 'Portfolio content loading...'}&quot;</span>
            </div>
            <div className="code-line" style={{ marginLeft: '20px' }}>
              <span className="syntax-highlight">name</span>: 
              <span className="syntax-string">&quot;{data.profile?.name || 'Developer'}&quot;</span>
            </div>
            <div className="code-line">{'}'}</div>
          </div>
        </div>
      );
    }
    
    const componentInfo = componentLookup[fileInfo.component] as ComponentInfo;
    if (!componentInfo) {
      return (
        <div className="component-card">
          <div className="component-title">Component Not Found</div>
          <p>The component &quot;{fileInfo.component}&quot; could not be loaded.</p>
        </div>
      );
    }

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
                      
                      // Mobile: scroll to dynamic content after tab click
                      if (window.innerWidth <= 480) {
                        setTimeout(() => {
                          const contentArea = document.querySelector('.content-area');
                          const dynamicContentContainer = document.querySelector('.dynamic-content-container');
                          const portfolioHeader = document.querySelector('.portfolio-header-container');
                          
                          if (contentArea) {
                            let scrollTarget = 0;
                            
                            if (portfolioHeader) {
                              // Scroll to just past the header to show dynamic content
                              const headerHeight = (portfolioHeader as HTMLElement).offsetHeight;
                              scrollTarget = headerHeight - 20; // Show a bit of header for context
                            } else if (dynamicContentContainer) {
                              // Fallback: scroll to dynamic content container
                              scrollTarget = (dynamicContentContainer as HTMLElement).offsetTop;
                            }
                            
                            contentArea.scrollTo({
                              top: scrollTarget,
                              behavior: 'smooth'
                            });
                          }
                        }, 100); // Short delay for tab change to complete
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
              {/* Fixed Portfolio Header */}
              <div className="portfolio-header-container">
                <div className="portfolio-header">
                  <Header basicInfo={data.profile} />
                  <Stats stats={data.stats} />
                </div>
              </div>

              {/* Scrollable Dynamic Content */}
              <div className="dynamic-content-container">
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