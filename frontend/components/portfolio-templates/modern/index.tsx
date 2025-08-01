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
import { Moon, Sun } from 'lucide-react';
import './styles/main.css';

interface ModernTemplateProps {
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
  experience: 'Experience',
  projects: 'Projects', 
  skills: 'Skills',
  blog_posts: 'Blog',
  contact: 'Contact',
  about: 'About'
};

export default function ModernTemplate({ data }: ModernTemplateProps) {
  const [darkMode, setDarkMode] = useState(false);
  const [activeSection, setActiveSection] = useState('about');
  
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

  useEffect(() => {
    if (darkMode) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [darkMode]);

  const scrollToSection = (sectionId: string) => {
    setActiveSection(sectionId);
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  };

  return (
    <div className="modern-template">
      {/* Background */}
      <div className="modern-background">
        <div className="modern-gradient-1"></div>
        <div className="modern-gradient-2"></div>
        <div className="modern-particles"></div>
      </div>

      {/* Navigation - Right under app header */}
      <nav className="modern-nav">
        <div className="modern-nav-content">
          <div className="modern-nav-brand">
            <span className="modern-nav-name">{validatedData.profile.name}</span>
          </div>
          
          <div className="modern-nav-menu">
            {navComponents.filter(comp => comp).map((comp) => (
              <Button
                key={comp!.type}
                variant={activeSection === comp!.type ? "default" : "ghost"}
                size="sm"
                onClick={() => scrollToSection(comp!.type)}
                className="modern-nav-item"
              >
                {tabLabels[comp!.type] || comp!.type}
              </Button>
            ))}
          </div>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => setDarkMode(!darkMode)}
            className="modern-theme-toggle"
          >
            {darkMode ? <Sun size={16} /> : <Moon size={16} />}
          </Button>
        </div>
      </nav>

      {/* Main Content */}
      <main className="modern-main">
        {/* Hero Section with Header immediately after navigation */}
        <section id="hero" className="modern-hero">
          <Card className="modern-hero-card">
            <Header basicInfo={validatedData.profile} />
            <Stats stats={validatedData.stats} />
          </Card>
        </section>

        {/* Dynamic Sections */}
        {dynamicComponents.filter(componentInfo => componentInfo).map((componentInfo) => {
          const Component = componentInfo!.component;
          return (
            <section
              key={componentInfo!.id}
              id={componentInfo!.type}
              className="modern-section"
            >
              <Card className="modern-section-card">
                <div className="modern-section-header">
                  <h2 className="modern-section-title">
                    {tabLabels[componentInfo!.type] || componentInfo!.type}
                  </h2>
                </div>
                <div className="modern-section-content">
                  <Component data={componentInfo!.data} />
                </div>
              </Card>
            </section>
          );
        })}
      </main>
    </div>
  );
} 