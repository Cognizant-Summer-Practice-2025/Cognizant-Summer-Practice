"use client";

import React, { useState, useEffect, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { useUser } from '@/lib/contexts/user-context';
import { usePortfolio } from '@/lib/contexts/portfolio-context';
import { loadTemplateComponent, getDefaultTemplate, convertTemplateUuidToId } from '@/lib/templates';
import { PortfolioDataFromDB, PortfolioData } from '@/lib/portfolio';
import Header from '@/components/header';
import { PortfolioDataFromDB, PortfolioData, ComponentConfig } from '@/lib/portfolio';
import { LoadingOverlay } from '@/components/loader';

// Loading component
function TemplateLoader() {
  return (
    <LoadingOverlay 
      isOpen={true}
      title="Loading Portfolio..."
      message="Please wait while we load your portfolio"
      showBackdrop={false}
      preventBodyScroll={false}
      textColor="black"
    />
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
    currentPortfolioOwner,
    portfolioLoading, 
    portfolioError, 
    loadPortfolioByUserId,
    loadPortfolioById,
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

    // Use portfolio owner information if available, fallback to current user for own portfolio
    const portfolioOwner = currentPortfolioOwner || (isViewingOwnPortfolio ? currentUser : null);
    
    // Helper function to get the display name
    const getDisplayName = (owner: typeof portfolioOwner) => {
      if (!owner) return 'Portfolio Owner';
      if ('name' in owner) return owner.name;
      return `${owner.firstName || ''} ${owner.lastName || ''}`.trim() || 'Portfolio Owner';
    };
    
    // Helper function to get the email
    const getEmail = (owner: typeof portfolioOwner) => {
      return owner?.email || 'contact@example.com';
    };

    // Ensure components is always an array - handle both array and string cases
    let portfolioComponents: ComponentConfig[] = [];
    try {
      if (Array.isArray(currentPortfolio.components)) {
        portfolioComponents = currentPortfolio.components;
      } else if (typeof currentPortfolio.components === 'string') {
        portfolioComponents = JSON.parse(currentPortfolio.components);
      } else if (currentPortfolio.components) {
        // If it's some other type, try to convert it
        console.warn('Unexpected components type:', typeof currentPortfolio.components, currentPortfolio.components);
        portfolioComponents = [];
      }
    } catch (error) {
      console.error('Error parsing portfolio components:', error, currentPortfolio.components);
      // Fallback to default components
      portfolioComponents = [];
    }

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
        components: portfolioComponents,
        createdAt: '', // Not available in new structure
        updatedAt: currentPortfolio.updatedAt,
      },
      profile: {
        id: currentPortfolio.userId,
        name: getDisplayName(portfolioOwner),
        title: portfolioOwner?.professionalTitle || 'Professional',
        bio: currentPortfolio.bio || 'Welcome to my portfolio',
        profileImage: portfolioOwner?.avatarUrl || 'https://placehold.co/120x120',
        location: portfolioOwner?.location || '',
        email: 'contact@example.com', // Don't expose real email
      },
      stats: [
        { id: '1', label: 'Portfolio Views', value: currentPortfolio.viewCount?.toString() || '0', icon: '👁️' },
        { id: '2', label: 'Portfolio Likes', value: currentPortfolio.likeCount?.toString() || '0', icon: '❤️' },
        { id: '3', label: 'Projects', value: currentPortfolioEntities.projects.length.toString(), icon: '🚀' },
        { id: '4', label: 'Skills', value: currentPortfolioEntities.skills.length.toString(), icon: '🎯' }
      ],
      contacts: {
        email: getEmail(portfolioOwner),
        location: portfolioOwner?.location || 'Location not specified',
      },
      quotes: [
        {
          id: 'default-1',
          text: currentPortfolio.bio || 'Passionate about creating amazing experiences and solving complex problems.',
          author: getDisplayName(portfolioOwner),
          position: portfolioOwner?.professionalTitle
        }
      ],
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
          // Load portfolio by ID
          await loadPortfolioById(portfolioId, true); // increment views
        } else if (userId) {
          // Load user's published portfolio
          await loadPortfolioByUserId(userId, true); // increment views
        }
    }

          if (portfolioId || userId) {
        loadPortfolio();
      } else if (currentUser?.id) {
        // If no user ID provided but we have a current user, load their portfolio
        loadPortfolioByUserId(currentUser.id, true);
      }
  }, [portfolioId, userId, loadPortfolioByUserId, loadPortfolioById, clearCurrentPortfolio, currentUser]);

  // Load template component when portfolio data is available
  useEffect(() => {
    async function loadTemplate() {
      if (!currentPortfolio) {
        setTemplateComponent(null);
        return;
      }

      try {
        setTemplateLoading(true);
        
        // Get template ID from portfolio configuration, converting UUID to string ID if needed
        const rawTemplateId = currentPortfolio.templateId || getDefaultTemplate().id;
        const templateId = await convertTemplateUuidToId(rawTemplateId);
        

        
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
  if (portfolioLoading || templateLoading || !currentPortfolio || !currentPortfolioEntities) {
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

  // Show loading if template data is not ready
  if (!TemplateComponent || !templateData) {
    return <TemplateLoader />;
  }

  return (
    <>
      <Header />
      <div style={{ position: 'relative', paddingTop: '64px' }}>
        <Suspense fallback={<TemplateLoader />}>
          <TemplateComponent data={templateData} />
        </Suspense>
      </div>
    </>
  );
};

export default PortfolioPage;