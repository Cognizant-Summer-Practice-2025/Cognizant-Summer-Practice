"use client";

import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { TemplateManager, ComponentMap } from '@/lib/template-manager';
import AppHeader from '@/components/header';
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
import { Menu, X, Sun, Moon, ChevronUp } from 'lucide-react';
import './styles/main.css';

interface ProfessionalTemplateProps {
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

export default function ProfessionalTemplate({ data }: ProfessionalTemplateProps) {
  const [darkMode, setDarkMode] = useState(false);
  const [activeSection, setActiveSection] = useState('about');
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showScrollTop, setShowScrollTop] = useState(false);
  
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

  // Handle scroll effects
  useEffect(() => {
    const handleScroll = () => {
      setShowScrollTop(window.scrollY > 400);
      
      // Update active section based on scroll position
      const sections = navComponents.map(comp => comp!.type);
      for (const section of sections) {
        const element = document.getElementById(section);
        if (element) {
          const rect = element.getBoundingClientRect();
          if (rect.top <= 100 && rect.bottom >= 100) {
            setActiveSection(section);
            break;
          }
        }
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [navComponents]);

  const scrollToSection = (sectionId: string) => {
    setActiveSection(sectionId);
    setMobileMenuOpen(false);
    const element = document.getElementById(sectionId);
    if (element) {
      // Account for both app header (64px) and template nav (80px)
      const offsetTop = element.offsetTop - 144;
      window.scrollTo({ top: offsetTop, behavior: 'smooth' });
    }
  };

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="professional-template">
      {/* App Header */}
      <AppHeader />
      
      {/* Fixed Navigation - positioned below app header */}
      <nav className="prof-nav">
        <div className="prof-nav-container">
          <div className="prof-nav-brand">
            <h2 className="prof-brand-name">{validatedData.profile.name}</h2>
            <span className="prof-brand-title">{validatedData.profile.title}</span>
          </div>
          
          {/* Desktop Navigation */}
          <div className="prof-nav-menu">
            {navComponents.filter(comp => comp).map((comp) => (
              <Button
                key={comp!.type}
                variant="ghost"
                size="sm"
                onClick={() => scrollToSection(comp!.type)}
                className={`prof-nav-item ${activeSection === comp!.type ? 'active' : ''}`}
              >
                {tabLabels[comp!.type] || comp!.type}
              </Button>
            ))}
          </div>

          {/* Theme Toggle & Mobile Menu */}
          <div className="prof-nav-actions">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setDarkMode(!darkMode)}
              className="prof-theme-toggle"
            >
              {darkMode ? <Sun size={16} /> : <Moon size={16} />}
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="prof-mobile-toggle"
            >
              {mobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
            </Button>
          </div>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <div className="prof-mobile-menu">
            {navComponents.filter(comp => comp).map((comp) => (
              <Button
                key={comp!.type}
                variant="ghost"
                onClick={() => scrollToSection(comp!.type)}
                className={`prof-mobile-item ${activeSection === comp!.type ? 'active' : ''}`}
              >
                {tabLabels[comp!.type] || comp!.type}
              </Button>
            ))}
          </div>
        )}
      </nav>

      {/* Main Content */}
      <main className="prof-main">
        {/* Hero Section */}
        <section id="hero" className="prof-hero">
          <div className="prof-hero-container">
            <div className="prof-hero-content">
              <Header basicInfo={validatedData.profile} />
              <Stats stats={validatedData.stats} />
            </div>
          </div>
        </section>

        {/* Dynamic Sections */}
        {dynamicComponents.filter(componentInfo => componentInfo).map((componentInfo) => {
          const Component = componentInfo!.component;
          return (
            <section
              key={componentInfo!.id}
              id={componentInfo!.type}
              className={`prof-section prof-section-${componentInfo!.type}`}
            >
              <div className="prof-section-container">
                <Card className="prof-section-card">
                  <div className="prof-section-header">
                    <h2 className="prof-section-title">
                      {tabLabels[componentInfo!.type] || componentInfo!.type}
                    </h2>
                    <div className="prof-section-divider"></div>
                  </div>
                  <div className="prof-section-content">
                    {componentInfo!.type === 'contact' ? (
                      <Component data={componentInfo!.data} {...({ basicInfo: validatedData.profile } as Record<string, unknown>)} />
                    ) : (
                      <Component data={componentInfo!.data} />
                    )}
                  </div>
                </Card>
              </div>
            </section>
          );
        })}
      </main>

      {/* Scroll to Top Button */}
      {showScrollTop && (
        <Button
          onClick={scrollToTop}
          className="prof-scroll-top"
          size="sm"
        >
          <ChevronUp size={20} />
        </Button>
      )}

      {/* Background Elements */}
      <div className="prof-background">
        <div className="prof-bg-gradient-1"></div>
        <div className="prof-bg-gradient-2"></div>
      </div>
    </div>
  );
} 