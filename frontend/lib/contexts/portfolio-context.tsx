'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useUser } from './user-context';
import { 
  Portfolio,
  Project,
  Experience,
  Skill,
  BlogPost,
  Bookmark,
  PortfolioTemplate,
  UserPortfolioComprehensive
} from '@/lib/portfolio';
import { 
  getUserPortfolioComprehensive,
  getPortfolioById,
  incrementViewCount 
} from '@/lib/portfolio/api';

interface PortfolioContextType {
  // User's comprehensive portfolio data
  userPortfolioData: UserPortfolioComprehensive | null;
  // Currently viewed portfolio (for portfolio page)
  currentPortfolio: Portfolio | null;
  currentPortfolioEntities: {
    projects: Project[];
    experience: Experience[];
    skills: Skill[];
    blogPosts: BlogPost[];
    bookmarks: Bookmark[];
  } | null;
  // Loading states
  loading: boolean;
  portfolioLoading: boolean;
  // Error states
  error: string | null;
  portfolioError: string | null;
  // Portfolio management
  loadUserPortfolios: () => Promise<void>;
  loadPortfolioByUserId: (userId: string, incrementViews?: boolean) => Promise<void>;
  refreshUserPortfolios: () => Promise<void>;
  clearCurrentPortfolio: () => void;
  invalidateCache: () => void;
  // Portfolio status
  hasPublishedPortfolio: boolean;
  isViewingOwnPortfolio: boolean;
  // Entity getters (filtered by current portfolio if viewing specific portfolio)
  getUserProjects: () => Project[];
  getUserExperience: () => Experience[];
  getUserSkills: () => Skill[];
  getUserBlogPosts: () => BlogPost[];
  getUserBookmarks: () => Bookmark[];
  getUserPortfolios: () => Portfolio[];
  getPortfolioTemplates: () => PortfolioTemplate[];
}

const PortfolioContext = createContext<PortfolioContextType | undefined>(undefined);

export function PortfolioProvider({ children }: { children: ReactNode }) {
  const { user, loading: userLoading } = useUser();
  
  // User's comprehensive portfolio data
  const [userPortfolioData, setUserPortfolioData] = useState<UserPortfolioComprehensive | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Currently viewed portfolio (could be own or others)
  const [currentPortfolio, setCurrentPortfolio] = useState<Portfolio | null>(null);
  const [currentPortfolioEntities, setCurrentPortfolioEntities] = useState<{
    projects: Project[];
    experience: Experience[];
    skills: Skill[];
    blogPosts: BlogPost[];
    bookmarks: Bookmark[];
  } | null>(null);
  const [portfolioLoading, setPortfolioLoading] = useState(false);
  const [portfolioError, setPortfolioError] = useState<string | null>(null);

  // Load user's comprehensive portfolio data
  const loadUserPortfolios = async () => {
    if (!user?.id) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const comprehensiveData = await getUserPortfolioComprehensive(user.id);
      setUserPortfolioData(comprehensiveData);
    } catch (err) {
      console.error('Error loading user portfolio data:', err);
      setError(err instanceof Error ? err.message : 'Failed to load portfolio data');
    } finally {
      setLoading(false);
    }
  };

  // Load portfolio by user ID (first published one)
  const loadPortfolioByUserId = async (userId: string, incrementViews = false) => {
    try {
      setPortfolioLoading(true);
      setPortfolioError(null);
      
      // Get comprehensive data for the user
      const comprehensiveData = await getUserPortfolioComprehensive(userId);
      const publishedPortfolio = comprehensiveData.portfolios.find(p => p.isPublished);
      
      if (!publishedPortfolio) {
        setPortfolioError(`No published portfolio found for user ${userId}`);
        setCurrentPortfolio(null);
        setCurrentPortfolioEntities(null);
        return;
      }
      
      setCurrentPortfolio(publishedPortfolio);
      
      // Filter entities by portfolio ID
      setCurrentPortfolioEntities({
        projects: comprehensiveData.projects.filter(p => p.portfolioId === publishedPortfolio.id),
        experience: comprehensiveData.experience.filter(e => e.portfolioId === publishedPortfolio.id),
        skills: comprehensiveData.skills.filter(s => s.portfolioId === publishedPortfolio.id),
        blogPosts: comprehensiveData.blogPosts.filter(b => b.portfolioId === publishedPortfolio.id),
        bookmarks: comprehensiveData.bookmarks.filter(b => b.portfolioId === publishedPortfolio.id),
      });
      
      // Increment view count if requested
      if (incrementViews) {
        try {
          await incrementViewCount(publishedPortfolio.id);
        } catch (err) {
          console.warn('Failed to increment view count:', err);
        }
      }
    } catch (err) {
      console.error('Error loading portfolio by user ID:', err);
      setPortfolioError(err instanceof Error ? err.message : `No portfolio found for user ${userId}`);
      setCurrentPortfolio(null);
      setCurrentPortfolioEntities(null);
    } finally {
      setPortfolioLoading(false);
    }
  };

  // Refresh user portfolios
  const refreshUserPortfolios = async () => {
    setUserPortfolioData(null); // Clear cache to force reload
    await loadUserPortfolios();
  };

  // Clear current portfolio
  const clearCurrentPortfolio = () => {
    setCurrentPortfolio(null);
    setCurrentPortfolioEntities(null);
    setPortfolioError(null);
  };

  // Invalidate all cached data
  const invalidateCache = () => {
    setUserPortfolioData(null);
    setCurrentPortfolio(null);
    setCurrentPortfolioEntities(null);
    setError(null);
    setPortfolioError(null);
  };

  // Load user portfolio data when user changes
  useEffect(() => {
    if (!userLoading && user?.id && !userPortfolioData) {
      loadUserPortfolios();
    }
  }, [user, userLoading, userPortfolioData]);

  // Computed values
  const hasPublishedPortfolio = userPortfolioData?.portfolios?.some(p => p.isPublished) ?? false;
  const isViewingOwnPortfolio = !!(user && currentPortfolio && 
    userPortfolioData?.portfolios?.some(p => p.id === currentPortfolio.id));

  // Entity getters
  const getUserProjects = (): Project[] => {
    if (currentPortfolio && currentPortfolioEntities) {
      return currentPortfolioEntities.projects;
    }
    return userPortfolioData?.projects || [];
  };

  const getUserExperience = (): Experience[] => {
    if (currentPortfolio && currentPortfolioEntities) {
      return currentPortfolioEntities.experience;
    }
    return userPortfolioData?.experience || [];
  };

  const getUserSkills = (): Skill[] => {
    if (currentPortfolio && currentPortfolioEntities) {
      return currentPortfolioEntities.skills;
    }
    return userPortfolioData?.skills || [];
  };

  const getUserBlogPosts = (): BlogPost[] => {
    if (currentPortfolio && currentPortfolioEntities) {
      return currentPortfolioEntities.blogPosts;
    }
    return userPortfolioData?.blogPosts || [];
  };

  const getUserBookmarks = (): Bookmark[] => {
    if (currentPortfolio && currentPortfolioEntities) {
      return currentPortfolioEntities.bookmarks;
    }
    return userPortfolioData?.bookmarks || [];
  };

  const getUserPortfolios = (): Portfolio[] => {
    return userPortfolioData?.portfolios || [];
  };

  const getPortfolioTemplates = (): PortfolioTemplate[] => {
    return userPortfolioData?.templates || [];
  };

  const value: PortfolioContextType = {
    // Portfolio data
    userPortfolioData,
    currentPortfolio,
    currentPortfolioEntities,
    // Loading states
    loading: loading || userLoading,
    portfolioLoading,
    // Error states
    error,
    portfolioError,
    // Portfolio management methods
    loadUserPortfolios,
    loadPortfolioByUserId,
    refreshUserPortfolios,
    clearCurrentPortfolio,
    invalidateCache,
    // Status flags
    hasPublishedPortfolio,
    isViewingOwnPortfolio,
    // Entity getters
    getUserProjects,
    getUserExperience,
    getUserSkills,
    getUserBlogPosts,
    getUserBookmarks,
    getUserPortfolios,
    getPortfolioTemplates,
  };

  return (
    <PortfolioContext.Provider value={value}>
      {children}
    </PortfolioContext.Provider>
  );
}

export function usePortfolio(): PortfolioContextType {
  const context = useContext(PortfolioContext);
  if (context === undefined) {
    throw new Error('usePortfolio must be used within a PortfolioProvider');
  }
  return context;
} 