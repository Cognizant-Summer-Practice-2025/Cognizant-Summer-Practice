'use client';

import React, { useEffect } from 'react';
import { 
  ProfileBasicInfo as BasicInfo, 
  ProfileProjects as Projects, 
  ProfileExperience as Experience, 
  ProfileSkills as Skills, 
  ProfileTemplate as Template, 
  ProfileSettings as Settings,
  Loading,
  useProfile,
  usePortfolio,
  useUser
} from '@cognizant-summer-practice/shared-components';

const ProfilePage = () => {
  const { activeTab } = useProfile();
  const { user, loading: userLoading } = useUser();
  const { userPortfolioData, loading, loadUserPortfolios, refreshUserPortfolios } = usePortfolio();

  // Load user's portfolio data when component mounts
  useEffect(() => {
    if (user?.id && !userPortfolioData) {
      loadUserPortfolios();
    }
  }, [user?.id, userPortfolioData, loadUserPortfolios]);

  // Show loading state while initial data is being fetched
  if (userLoading || (loading && !userPortfolioData)) {
    return (
      <div className="min-h-full">
        <div className="p-4 sm:p-6 lg:p-8">
          <div className="max-w-7xl mx-auto">
            <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[400px]">
              <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Profile</h1>
              <div className="flex flex-col items-center justify-center py-8">
                <Loading className="scale-50" backgroundColor="white" />
                <span className="mt-4 text-gray-600">Loading profile...</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

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