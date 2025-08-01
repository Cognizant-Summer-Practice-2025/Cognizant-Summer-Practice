"use client";

import React, { useState } from 'react';
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
import { Footer } from './components/footer';
import './styles/main.css';

interface GabrielBarzuTemplateProps {
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
  blog_posts: 'Blog'
};

export default function GabrielBarzuTemplate({ data }: GabrielBarzuTemplateProps) {
  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Validate and transform the data before use
  const validatedData = TemplateManager.validateAndTransformPortfolioData(data);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(validatedData);

  // Filter middle components (experience, projects, skills, blog_posts) that should be in the right column
  const middleComponents = dynamicComponents.filter(comp => 
    comp && ['experience', 'projects', 'skills', 'blog_posts'].includes(comp.type)
  );

  // Get first component and remaining components
  const firstComponent = middleComponents[0];
  const remainingComponents = middleComponents.slice(1);

  // State for selected tab - default to first available tab
  const [selectedTab, setSelectedTab] = useState<string | null>(
    remainingComponents.length > 0 && remainingComponents[0] ? remainingComponents[0].type : null
  );

  // Generate dynamic tabs for remaining components only
  const remainingTabs = remainingComponents.filter(comp => comp).map(comp => ({
    id: comp!.type,
    label: tabLabels[comp!.type] || comp!.type,
    hasData: comp!.data && (Array.isArray(comp!.data) ? comp!.data.length > 0 : true)
  }));

  // Filter components to show based on selected tab
  const componentsToShow = selectedTab 
    ? remainingComponents.filter(comp => comp && comp.type === selectedTab)
    : remainingComponents;

  const handleTabClick = (tabId: string) => {
    setSelectedTab(selectedTab === tabId ? null : tabId);
  };

  return (
    <div className="gabriel-barzu-template">
      <div className="template-container">
        {/* Fixed Header and Stats */}
        <Header basicInfo={validatedData.profile} />
        <Stats stats={validatedData.stats} />
        
        <div className="content-grid">
          <div className="left-column">
            {/* Contact and About components */}
            <Contact data={validatedData.contacts} />
            <About data={validatedData.quotes} />
          </div>
          
          <div className="right-column">
            {/* Render first component */}
            {firstComponent && (
              <div key={firstComponent.id} className="dynamic-component">
                <firstComponent.component data={firstComponent.data} />
              </div>
            )}

            {/* Dynamic Interactive Tab Menu - Only for remaining components */}
            {remainingTabs.length > 0 && (
              <div className="portfolio-tabs-container">
                <div className="portfolio-tabs">
                  {remainingTabs.map((tab) => (
                    <div
                      key={tab.id}
                      className={`portfolio-tab ${selectedTab === tab.id ? 'active' : ''} ${tab.hasData ? 'has-data' : 'no-data'}`}
                      onClick={() => handleTabClick(tab.id)}
                    >
                      {tab.label}
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Render filtered components based on selection */}
            {componentsToShow.filter(componentInfo => componentInfo).map((componentInfo) => {
              const Component = componentInfo!.component;
              return (
                <div 
                  key={componentInfo!.id} 
                  className={`dynamic-component ${selectedTab && selectedTab !== componentInfo!.type ? 'component-hidden' : 'component-visible'}`}
                >
                  <Component data={componentInfo!.data} />
                </div>
              );
            })}
          </div>
        </div>
        
        {/* Fixed Footer */}
        <Footer socialLinks={validatedData.socialLinks} />
      </div>
    </div>
  );
} 