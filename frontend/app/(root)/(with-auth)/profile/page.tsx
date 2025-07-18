'use client';

import React, { useEffect } from 'react';
import BasicInfo from '@/components/profile-page/basic-info';
import Projects from '@/components/profile-page/projects';
import Experience from '@/components/profile-page/experience';
import Skills from '@/components/profile-page/skills';
import Template from '@/components/profile-page/template';
import Settings from '@/components/profile-page/settings';
import { useProfile } from './layout';
import { usePortfolio } from '@/lib/contexts/portfolio-context';
import { useUser } from '@/lib/contexts/user-context';

const ProfilePage = () => {
  const { activeTab } = useProfile();
  const { user } = useUser();
  const { userPortfolioData, loading, loadUserPortfolios } = usePortfolio();

  // Load user's portfolio data when component mounts
  useEffect(() => {
    if (user?.id && !userPortfolioData) {
      loadUserPortfolios();
    }
  }, [user?.id, userPortfolioData, loadUserPortfolios]);

  const renderContent = () => {
    // Get the user's published portfolio if available
    const publishedPortfolio = userPortfolioData?.portfolios.find(p => p.isPublished);
    
    switch (activeTab) {
      case 'basic-info':
        return <BasicInfo />;
      case 'projects':
        return <Projects 
          projects={userPortfolioData?.projects || []} 
          portfolioId={publishedPortfolio?.id}
          loading={loading}
        />;
      case 'experience':
        return <Experience 
          experiences={userPortfolioData?.experience || []} 
          portfolioId={publishedPortfolio?.id}
          loading={loading}
        />;
      case 'skills':
        return <Skills 
          initialSkills={userPortfolioData?.skills || []}
          portfolioId={publishedPortfolio?.id}
          readOnly={false}
        />;
      case 'template':
        return <Template />;
      case 'settings':
        return <Settings />;
      default:
        return <BasicInfo />;
    }
  };

  return (
    <div className="h-full p-4 sm:p-6 lg:p-8 overflow-hidden">
      <div className="max-w-7xl mx-auto h-full flex items-start">
        <div className="w-full">
          {renderContent()}
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;