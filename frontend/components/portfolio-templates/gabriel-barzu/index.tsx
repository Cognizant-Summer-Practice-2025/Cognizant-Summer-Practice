"use client";

import React from 'react';
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
  experience: Experience,
  projects: Projects,
  skills: Skills,
  blog_posts: BlogPosts,
  contact: Contact,
  about: About
};

export default function GabrielBarzuTemplate({ data }: GabrielBarzuTemplateProps) {
  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(data);

  // Filter middle components (experience, projects, skills, blog_posts) that should be in the right column
  const middleComponents = dynamicComponents.filter(comp => 
    ['experience', 'projects', 'skills', 'blog_posts'].includes(comp.type)
  );

  return (
    <div className="gabriel-barzu-template">
      <div className="template-container">
        {/* Fixed Header and Stats */}
        <Header basicInfo={data.profile} />
        <Stats stats={data.stats} />
        
        <div className="content-grid">
          <div className="left-column">
            {/* Contact and About components */}
            <Contact data={data.contacts} />
            <About data={data.quotes} />
          </div>
          
          <div className="right-column">
            {/* Dynamic middle components (experience, projects, skills, blog_posts) */}
            {middleComponents.map((componentInfo) => {
              const Component = componentInfo.component;
              return (
                <div key={componentInfo.id} className="dynamic-component">
                  <Component data={componentInfo.data} />
                </div>
              );
            })}
          </div>
        </div>
        
        {/* Fixed Footer */}
        <Footer socialLinks={data.socialLinks} />
      </div>
    </div>
  );
} 