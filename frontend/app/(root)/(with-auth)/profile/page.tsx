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
  const { userPortfolioData, loading, loadUserPortfolios, refreshUserPortfolios } = usePortfolio();

  // Load user's portfolio data when component mounts
  useEffect(() => {
    if (user?.id && !userPortfolioData) {
      loadUserPortfolios();
    }
  }, [user?.id, userPortfolioData, loadUserPortfolios]);

  const handleProjectsUpdate = async () => {
    await refreshUserPortfolios();
  };

  const handleExperiencesUpdate = async () => {
    await refreshUserPortfolios();
  };

  const handleSkillsUpdate = async () => {
    await refreshUserPortfolios();
  };

  const renderContent = () => {
    // Get the user's published portfolio if available, otherwise use the first available portfolio
    const publishedPortfolio = userPortfolioData?.portfolios.find(p => p.isPublished);
    const availablePortfolio = publishedPortfolio || userPortfolioData?.portfolios[0];
    
    switch (activeTab) {
      case 'basic-info':
        return <BasicInfo />;
      case 'projects':
        return <Projects 
          projects={userPortfolioData?.projects || []} 
          portfolioId={availablePortfolio?.id}
          loading={loading}
          onProjectsUpdate={handleProjectsUpdate}
        />;
      case 'experience':
        return <Experience 
          experiences={userPortfolioData?.experience || []} 
          portfolioId={availablePortfolio?.id}
          loading={loading}
          onExperiencesUpdate={handleExperiencesUpdate}
        />;
      case 'skills':
        return <Skills 
          initialSkills={userPortfolioData?.skills || []}
          portfolioId={availablePortfolio?.id}
          readOnly={false}
          onSkillsUpdate={handleSkillsUpdate}
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
    <div className="min-h-full">
      <div className="p-4 sm:p-6 lg:p-8">
        <div className="max-w-7xl mx-auto">
          <div className="w-full">
            {renderContent()}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;