"use client";

import React, { useState, useEffect, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { useUser } from '@/lib/contexts/user-context';
import { usePortfolio } from '@/lib/contexts/portfolio-context';
import { loadTemplateComponent, getDefaultTemplate } from '@/lib/templates';
import { PortfolioDataFromDB, PortfolioData } from '@/lib/portfolio';

// Loading component
function TemplateLoader() {
  return (
    <div className="portfolio-loading">
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading portfolio...</p>
      </div>
    </div>
  );
}

// No Portfolio component
function NoPortfolioFound({ 
  isOwnProfile, 
  userName, 
  onCreatePortfolio 
}: { 
  isOwnProfile: boolean; 
  userName?: string;
  onCreatePortfolio: () => void;
}) {
  return (
    <div className="portfolio-error">
      <div className="error-container no-portfolio-container">
        <div className="no-portfolio-icon">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M3 7V17C3 18.1046 3.89543 19 5 19H19C20.1046 19 21 18.1046 21 17V7C21 5.89543 20.1046 5 19 5H5C3.89543 5 3 5.89543 3 7Z" stroke="#94A3B8" strokeWidth="2"/>
            <path d="M8 12H16" stroke="#94A3B8" strokeWidth="2" strokeLinecap="round"/>
            <path d="M8 15H13" stroke="#94A3B8" strokeWidth="2" strokeLinecap="round"/>
            <path d="M8 9H10" stroke="#94A3B8" strokeWidth="2" strokeLinecap="round"/>
          </svg>
        </div>
        
        {isOwnProfile ? (
          <>
            <h2>No Portfolio Yet</h2>
            <p>You haven&apos;t created a portfolio yet. Start building your professional presence!</p>
            <button 
              className="publish-button primary"
              onClick={onCreatePortfolio}
            >
              <span className="button-icon">🚀</span>
              Create Your Portfolio
            </button>
          </>
        ) : (
          <>
            <h2>Portfolio Not Available</h2>
            <p>
              {userName ? `${userName} hasn't` : 'This user hasn&apos;t'} published a portfolio yet.
            </p>
            <p className="secondary-text">Check back later or contact them directly.</p>
          </>
        )}
      </div>
    </div>
  );
}

const PortfolioPage = () => {
  const [TemplateComponent, setTemplateComponent] = useState<React.ComponentType<{data: PortfolioDataFromDB | PortfolioData}> | null>(null);
  const [templateLoading, setTemplateLoading] = useState(false);
  
  const router = useRouter();
  const { user: currentUser } = useUser();
  const { 
    currentPortfolio, 
    currentPortfolioEntities,
    portfolioLoading, 
    portfolioError, 
    loadPortfolioByUserId,
    clearCurrentPortfolio,
    isViewingOwnPortfolio 
  } = usePortfolio();
  
  // Get user ID from URL params or search params
  const searchParams = useSearchParams();
  const userId = searchParams.get('user');
  const portfolioId = searchParams.get('portfolio');

  const handleCreatePortfolio = () => {
    router.push('/publish');
  };

  // Create portfolio data structure for templates (compatibility layer)
  const createPortfolioDataForTemplate = (): PortfolioDataFromDB | PortfolioData | null => {
    if (!currentPortfolio || !currentPortfolioEntities) return null;

    // For templates that expect PortfolioDataFromDB (like Gabriel Barzu)
    const portfolioDataFromDB: PortfolioDataFromDB = {
      portfolio: {
        id: currentPortfolio.id,
        userId: currentPortfolio.userId,
        templateId: currentPortfolio.templateId,
        title: currentPortfolio.title || '',
        bio: currentPortfolio.bio,
        isPublished: currentPortfolio.isPublished,
        visibility: currentPortfolio.visibility,
        viewCount: currentPortfolio.viewCount,
        likeCount: currentPortfolio.likeCount,
        customConfig: {},
        components: [], // Would need to be populated from template configuration
        createdAt: '', // Not available in new structure
        updatedAt: currentPortfolio.updatedAt,
      },
      profile: {
        id: currentUser?.id || '',
        name: currentUser ? `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim() : '',
        title: currentUser?.jobTitle || '',
        bio: currentPortfolio.bio || '',
        profileImage: currentUser?.profileImageUrl || '',
        location: currentUser?.location || '',
        email: currentUser?.email || '',
      },
      stats: [], // Would need to be calculated or configured
      contacts: {
        email: currentUser?.email || '',
        location: currentUser?.location || '',
      },
      quotes: [], // Would need to be configured or fetched separately
      experience: currentPortfolioEntities.experience,
      projects: currentPortfolioEntities.projects,
      skills: currentPortfolioEntities.skills,
      socialLinks: [], // Would need to be configured or fetched separately
      blogPosts: currentPortfolioEntities.blogPosts,
    };

    return portfolioDataFromDB;
  };

  // Load portfolio data when URL parameters change
  useEffect(() => {
    async function loadPortfolio() {
      // Clear any existing portfolio when URL changes
      clearCurrentPortfolio();
      
      if (portfolioId) {
        // For now, portfolioId loading would need to be implemented differently
        // since we removed loadPortfolioById from context
        console.warn('Portfolio ID loading not yet implemented with new context structure');
      } else if (userId) {
        // Load user's published portfolio
        await loadPortfolioByUserId(userId, true); // increment views
      }
    }

    if (portfolioId || userId) {
      loadPortfolio();
    }
  }, [portfolioId, userId, loadPortfolioByUserId, clearCurrentPortfolio]);

  // Load template component when portfolio data is available
  useEffect(() => {
    async function loadTemplate() {
      if (!currentPortfolio) {
        setTemplateComponent(null);
        return;
      }

      try {
        setTemplateLoading(true);
        
        // Get template ID from portfolio configuration
        const templateId = currentPortfolio.templateId || getDefaultTemplate().id;
        
        // Load the template component
        const templateModule = await loadTemplateComponent(templateId);
        setTemplateComponent(() => templateModule.default as React.ComponentType<{data: PortfolioDataFromDB | PortfolioData}>);
        
      } catch (err) {
        console.error('Error loading template:', err);
      } finally {
        setTemplateLoading(false);
      }
    }

    loadTemplate();
  }, [currentPortfolio]);

  // Show loading state
  if (portfolioLoading || templateLoading) {
    return <TemplateLoader />;
  }

  // Show no portfolio found
  if (portfolioError || (!currentPortfolio && !portfolioLoading)) {
    return (
      <NoPortfolioFound 
        isOwnProfile={isViewingOwnPortfolio}
        userName={currentUser?.firstName && currentUser?.lastName ? 
          `${currentUser.firstName} ${currentUser.lastName}` : undefined}
        onCreatePortfolio={handleCreatePortfolio}
      />
    );
  }

  // Create template data
  const templateData = createPortfolioDataForTemplate();

  // Show general error
  if (!TemplateComponent || !templateData) {
    return (
      <div className="portfolio-error">
        <div className="error-container">
          <h2>Portfolio Not Found</h2>
          <p>Unable to load the portfolio data or template.</p>
          <button onClick={() => window.location.reload()}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return (
    <Suspense fallback={<TemplateLoader />}>
      <TemplateComponent data={templateData} />
    </Suspense>
  );
};

export default PortfolioPage;