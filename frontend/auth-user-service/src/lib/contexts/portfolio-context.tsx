'use client';

import React, { createContext, useContext, useEffect, useState, useCallback, ReactNode } from 'react';
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
  getPortfolioComprehensive,
  incrementViewCount,
  getUserPortfolioInfo,
  UserPortfolioInfo
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
  // Portfolio owner information (when viewing someone else's portfolio)
  currentPortfolioOwner: UserPortfolioInfo | null;
  // Loading states
  loading: boolean;
  portfolioLoading: boolean;
  // Error states
  error: string | null;
  portfolioError: string | null;
  // Portfolio management
  loadUserPortfolios: () => Promise<void>;
  loadPortfolioByUserId: (userId: string, incrementViews?: boolean) => Promise<void>;
  loadPortfolioById: (portfolioId: string, incrementViews?: boolean) => Promise<void>;
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
  // Navigation support for home page cache
  setHomePageReturnContext: (context: HomePageReturnContext) => void;
  getHomePageReturnContext: () => HomePageReturnContext | null;
  clearHomePageReturnContext: () => void;
}

interface HomePageReturnContext {
  page: number;
  scrollPosition: number;
  timestamp: number;
  filters?: {
    searchTerm?: string;
    sortBy?: string;
    sortDirection?: string;
    selectedSkills?: string[];
    selectedRoles?: string[];
    featuredOnly?: boolean;
  };
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
  const [currentPortfolioOwner, setCurrentPortfolioOwner] = useState<UserPortfolioInfo | null>(null);
  const [portfolioLoading, setPortfolioLoading] = useState(false);
  const [portfolioError, setPortfolioError] = useState<string | null>(null);

  // Home page return context for navigation
  const [homePageReturnContext, setHomePageReturnContextState] = useState<HomePageReturnContext | null>(null);

  // Load user's comprehensive portfolio data
  const loadUserPortfolios = useCallback(async () => {
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
  }, [user?.id]);

  // Load portfolio by user ID (first published one)
  const loadPortfolioByUserId = useCallback(async (userId: string, incrementViews = false) => {
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
        setCurrentPortfolioOwner(null);
        return;
      }

      // Fetch portfolio owner information
      const portfolioOwner = await getUserPortfolioInfo(userId);
      setCurrentPortfolioOwner(portfolioOwner);
      
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
      setCurrentPortfolioOwner(null);
    } finally {
      setPortfolioLoading(false);
    }
  }, []);

  // Load portfolio by portfolio ID
  const loadPortfolioById = useCallback(async (portfolioId: string, incrementViews = false) => {
    try {
      setPortfolioLoading(true);
      setPortfolioError(null);
      
      // Get comprehensive portfolio data by ID
      const comprehensiveData = await getPortfolioComprehensive(portfolioId);
      
              if (!comprehensiveData.portfolio.isPublished) {
          setPortfolioError(`Portfolio ${portfolioId} is not published`);
          setCurrentPortfolio(null);
          setCurrentPortfolioEntities(null);
          setCurrentPortfolioOwner(null);
          return;
        }

        // Fetch portfolio owner information
        const portfolioOwner = await getUserPortfolioInfo(comprehensiveData.portfolio.userId);
        setCurrentPortfolioOwner(portfolioOwner);
        
        setCurrentPortfolio(comprehensiveData.portfolio);
        
        // Set the comprehensive portfolio entities
        setCurrentPortfolioEntities({
          projects: comprehensiveData.projects,
          experience: comprehensiveData.experience,
          skills: comprehensiveData.skills,
          blogPosts: comprehensiveData.blogPosts,
          bookmarks: comprehensiveData.bookmarks,
        });
        
        // Increment view count if requested
        if (incrementViews) {
          try {
            await incrementViewCount(portfolioId);
          } catch (err) {
            console.warn('Failed to increment view count:', err);
          }
        }
    } catch (err) {
      console.error('Error loading portfolio by ID:', err);
      setPortfolioError(err instanceof Error ? err.message : `Portfolio ${portfolioId} not found`);
      setCurrentPortfolio(null);
      setCurrentPortfolioEntities(null);
      setCurrentPortfolioOwner(null);
    } finally {
      setPortfolioLoading(false);
    }
  }, []);

  // Refresh user portfolios
  const refreshUserPortfolios = useCallback(async () => {
    setUserPortfolioData(null); // Clear cache to force reload
    await loadUserPortfolios();
  }, [loadUserPortfolios]);

  // Clear current portfolio
  const clearCurrentPortfolio = useCallback(() => {
    setCurrentPortfolio(null);
    setCurrentPortfolioEntities(null);
    setCurrentPortfolioOwner(null);
    setPortfolioError(null);
  }, []);

  // Invalidate all cached data
  const invalidateCache = useCallback(() => {
    setUserPortfolioData(null);
    setCurrentPortfolio(null);
    setCurrentPortfolioEntities(null);
    setCurrentPortfolioOwner(null);
    setError(null);
    setPortfolioError(null);
  }, []);

  // Home page return context management
  const setHomePageReturnContext = useCallback((context: HomePageReturnContext) => {
    setHomePageReturnContextState(context);
  }, []);

  const getHomePageReturnContext = useCallback(() => {
    return homePageReturnContext;
  }, [homePageReturnContext]);

  const clearHomePageReturnContext = useCallback(() => {
    setHomePageReturnContextState(null);
  }, []);

  // Load user portfolio data when user changes
  useEffect(() => {
    if (!userLoading && user?.id && !userPortfolioData) {
      loadUserPortfolios();
    }
  }, [user, userLoading, userPortfolioData, loadUserPortfolios]);

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
    currentPortfolioOwner,
    // Loading states
    loading: loading || userLoading,
    portfolioLoading,
    // Error states
    error,
    portfolioError,
    // Portfolio management methods
    loadUserPortfolios,
    loadPortfolioByUserId,
    loadPortfolioById,
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
    // Home page navigation support
    setHomePageReturnContext,
    getHomePageReturnContext,
    clearHomePageReturnContext,
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