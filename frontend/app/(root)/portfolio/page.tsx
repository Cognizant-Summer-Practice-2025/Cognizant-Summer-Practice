"use client";

import React, { useState } from 'react';
import PortfolioInfo from '@/components/portfolio-page/portfolio-info';
import PortfolioStats from '@/components/portfolio-page/portfolio-stats';
import PortfolioTabs from '@/components/portfolio-page/portfolio-tabs';
import ProjectsList from '@/components/portfolio-page/projects-list';
import SkillsList from '@/components/portfolio-page/skills-list';
import BlogList from '@/components/portfolio-page/blog-list';
import SocialLinks from '@/components/portfolio-page/social-links';

const PortfolioPage = () => {
  const [activeTab, setActiveTab] = useState('projects');

  const renderTabContent = () => {
    switch (activeTab) {
      case 'projects':
        return <ProjectsList />;
      case 'skills':
        return <SkillsList />;
      case 'blog':
        return <BlogList />;
      default:
        return <ProjectsList />;
    }
  };

  return (
      <div className="h-full p-4 sm:p-6 lg:p-8 overflow-hidden">
        <div className="max-w-7xl mx-auto h-full flex flex-col items-start">
          <PortfolioInfo />
          <PortfolioStats />
          <PortfolioTabs activeTab={activeTab} setActiveTab={setActiveTab} />
          {renderTabContent()}
          <SocialLinks />
        </div>
      </div>
  );
};

export default PortfolioPage;