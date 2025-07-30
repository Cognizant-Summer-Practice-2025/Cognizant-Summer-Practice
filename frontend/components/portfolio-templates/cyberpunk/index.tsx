"use client";

import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { TemplateManager, ComponentMap } from '@/lib/template-manager';
import { Header } from './components/header';
import { Stats } from './components/stats';
import { Contact } from './components/contact';
import { About } from './components/about';
import { Experience } from './components/experience';
import { Projects } from './components/projects';
import { Skills } from './components/skills';
import { BlogPosts } from './components/blog-posts';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Menu, X, Zap, Terminal, Code, Database, Cpu } from 'lucide-react';
import './styles/main.css';

interface CyberpunkTemplateProps {
  data: PortfolioDataFromDB;
}

// Component mapping for the template manager
const componentMap: ComponentMap = {
  experience: Experience as React.ComponentType<{ data: unknown }>,
  projects: Projects as React.ComponentType<{ data: unknown }>,
  skills: Skills as React.ComponentType<{ data: unknown }>,
  blog_posts: BlogPosts as React.ComponentType<{ data: unknown }>,
  contact: Contact as React.ComponentType<{ data: unknown }>,
  about: About as React.ComponentType<{ data: unknown }>
};

// Tab labels mapping
const tabLabels: Record<string, string> = {
  experience: 'Experience.exe',
  projects: 'Projects.sys', 
  skills: 'Skills.dll',
  blog_posts: 'Blog.log',
  contact: 'Contact.ini',
  about: 'About.me'
};

// Tab icons mapping
const tabIcons: Record<string, React.ComponentType> = {
  experience: Code,
  projects: Database,
  skills: Cpu,
  blog_posts: Terminal,
  contact: Zap,
  about: Menu
};

export default function CyberpunkTemplate({ data }: CyberpunkTemplateProps) {
  const [activeSection, setActiveSection] = useState('about');
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [isMatrixVisible, setIsMatrixVisible] = useState(true);
  const [terminalLines, setTerminalLines] = useState<string[]>([]);
  
  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Validate and transform the data before use
  const validatedData = TemplateManager.validateAndTransformPortfolioData(data);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(validatedData);

  // Filter components for navigation
  const navComponents = dynamicComponents.filter(comp => 
    comp && ['about', 'experience', 'projects', 'skills', 'blog_posts', 'contact'].includes(comp.type)
  );

  // Matrix rain effect
  useEffect(() => {
    const canvas = document.getElementById('matrix-canvas') as HTMLCanvasElement;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;

    const matrix = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789@#$%^&*()*&^%+-/~{[|`]}";
    const matrixArray = matrix.split("");

    const fontSize = 10;
    const columns = canvas.width / fontSize;

    const drops: number[] = [];
    for (let x = 0; x < columns; x++) {
      drops[x] = 1;
    }

    function draw() {
      if (!ctx || !canvas) return;
      
      ctx.fillStyle = 'rgba(0, 0, 0, 0.04)';
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      ctx.fillStyle = '#00ff41';
      ctx.font = fontSize + 'px arial';

      for (let i = 0; i < drops.length; i++) {
        const text = matrixArray[Math.floor(Math.random() * matrixArray.length)];
        ctx.fillText(text, i * fontSize, drops[i] * fontSize);

        if (drops[i] * fontSize > canvas.height && Math.random() > 0.975) {
          drops[i] = 0;
        }
        drops[i]++;
      }
    }

    const interval = setInterval(draw, 35);

    return () => clearInterval(interval);
  }, []);

  // Terminal simulation effect
  useEffect(() => {
    const commands = [
      '> Initializing neural interface...',
      '> Connecting to mainframe...',
      '> Loading portfolio data...',
      '> Decrypting user credentials...',
      '> Access granted. Welcome to the matrix.',
      '> System ready.'
    ];

    let currentIndex = 0;
    const interval = setInterval(() => {
      if (currentIndex < commands.length) {
        setTerminalLines(prev => [...prev, commands[currentIndex]]);
        currentIndex++;
      } else {
        clearInterval(interval);
        setTimeout(() => setIsMatrixVisible(false), 1000);
      }
    }, 500);

    return () => clearInterval(interval);
  }, []);

  // Handle window resize for matrix
  useEffect(() => {
    const handleResize = () => {
      const canvas = document.getElementById('matrix-canvas') as HTMLCanvasElement;
      if (canvas) {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
      }
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const renderActiveComponent = () => {
    const activeComponent = navComponents.find(comp => comp && comp.type === activeSection);
    if (!activeComponent) return null;

    const Component = activeComponent.component;
    return <Component data={activeComponent.data} />;
  };

  return (
    <div className="cyberpunk-template">
      {/* Matrix Loading Screen */}
      {isMatrixVisible && (
        <div className="matrix-overlay">
          <canvas id="matrix-canvas" className="matrix-canvas" />
          <div className="terminal-startup">
            <div className="terminal-header">
              <span className="terminal-title">NEURAL_INTERFACE v2.0.77</span>
            </div>
            <div className="terminal-body">
              {terminalLines.map((line, index) => (
                <div key={index} className="terminal-line">
                  {line}
                  <span className="cursor">_</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Main Interface */}
      <div className={`main-interface ${isMatrixVisible ? 'hidden' : 'visible'}`}>
        {/* Left Panel: Header + Sidebar */}
        <div className="cyberpunk-left-panel">
          {/* Header */}
          <Header data={validatedData} />

          {/* Navigation Sidebar */}
          <div className="cyberpunk-sidebar">
            <div className="sidebar-header">
              <div className="logo-section">
                <Terminal className="logo-icon" />
                <span className="logo-text">NEURAL.NET</span>
              </div>
            </div>

            <nav className="sidebar-nav">
              {navComponents.map((component) => {
                if (!component) return null;
                const IconComponent = tabIcons[component.type] || Code;
                return (
                  <button
                    key={component.type}
                    onClick={() => setActiveSection(component.type)}
                    className={`nav-item ${activeSection === component.type ? 'active' : ''}`}
                  >
                    <IconComponent className="nav-icon" size={16} />
                    <span className="nav-label">{tabLabels[component.type] || component.type}</span>
                    <div className="nav-glow"></div>
                  </button>
                );
              })}
            </nav>

            {/* System Status */}
            <div className="system-status">
              <div className="status-item">
                <span className="status-label">CPU:</span>
                <div className="status-bar">
                  <div className="status-fill" style={{ width: '73%' }}></div>
                </div>
              </div>
              <div className="status-item">
                <span className="status-label">RAM:</span>
                <div className="status-bar">
                  <div className="status-fill" style={{ width: '45%' }}></div>
                </div>
              </div>
              <div className="status-item">
                <span className="status-label">NET:</span>
                <div className="status-bar">
                  <div className="status-fill" style={{ width: '89%' }}></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Main Content Area */}
        <div className="content-area">
          {/* Content Header */}
          <div className="content-header">
            <div className="breadcrumb">
              <span className="breadcrumb-root">~/neural-net/</span>
              <span className="breadcrumb-current">{tabLabels[activeSection] || activeSection}</span>
            </div>
            <div className="header-actions">
              <Button 
                variant="outline" 
                size="sm" 
                onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                className="mobile-menu-toggle md:hidden"
              >
                {mobileMenuOpen ? <X size={16} /> : <Menu size={16} />}
              </Button>
            </div>
          </div>
          {/* Content Body */}
          <div className="content-body">
            <div className="content-window">
              <div className="window-header">
                <div className="window-controls">
                  <div className="control minimize"></div>
                  <div className="control maximize"></div>
                  <div className="control close"></div>
                </div>
                <div className="window-title">{tabLabels[activeSection]}</div>
              </div>
              <div className="window-content">
                {renderActiveComponent()}
              </div>
            </div>
          </div>
        </div>

        {/* Mobile Menu Overlay */}
        {mobileMenuOpen && (
          <div className="mobile-menu-overlay" onClick={() => setMobileMenuOpen(false)}>
            <div className="mobile-menu">
              {navComponents.map((component) => {
                if (!component) return null;
                
                const IconComponent = tabIcons[component.type] || Code;
                
                return (
                  <button
                    key={component.type}
                    onClick={() => {
                      setActiveSection(component.type);
                      setMobileMenuOpen(false);
                    }}
                    className={`mobile-nav-item ${activeSection === component.type ? 'active' : ''}`}
                  >
                    <IconComponent className="nav-icon" size={20} />
                    <span className="nav-label">{tabLabels[component.type] || component.type}</span>
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Floating Particles */}
        <div className="floating-particles">
          {[...Array(20)].map((_, i) => (
            <div
              key={i}
              className="particle"
              style={{
                animationDelay: `${Math.random() * 10}s`,
                left: `${Math.random() * 100}%`,
                animationDuration: `${10 + Math.random() * 20}s`
              }}
            />
          ))}
        </div>
      </div>
    </div>
  );
}