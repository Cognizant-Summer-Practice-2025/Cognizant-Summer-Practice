'use client';

import { useCallback } from 'react';
import { usePortfolio } from './portfolio-context';
import { useHomePageCache } from './home-page-cache-context';

export interface NavigationState {
  page: number;
  scrollPosition: number;
  filters: {
    searchTerm?: string;
    sortBy?: string;
    sortDirection?: string;
    selectedSkills?: string[];
    selectedRoles?: string[];
    featuredOnly?: boolean;
  };
}

export function usePortfolioNavigation() {
  const { setHomePageReturnContext, getHomePageReturnContext, clearHomePageReturnContext } = usePortfolio();
  const homePageCache = useHomePageCache();

  // Navigate to a portfolio from home page, saving current state
  const navigateToPortfolio = useCallback((portfolioId: string, userId?: string) => {
    // Save current home page state
    if (homePageCache) {
      const context = {
        page: homePageCache.currentPage,
        scrollPosition: window.scrollY,
        timestamp: new Date().getTime(),
        filters: {
          searchTerm: homePageCache.searchTerm || undefined,
          sortBy: homePageCache.sortBy,
          sortDirection: homePageCache.sortDirection,
          selectedSkills: homePageCache.selectedSkills.length > 0 ? homePageCache.selectedSkills : undefined,
          selectedRoles: homePageCache.selectedRoles.length > 0 ? homePageCache.selectedRoles : undefined,
          featuredOnly: homePageCache.featuredOnly || undefined,
        }
      };
      
      setHomePageReturnContext(context);
      console.log('💾 Saved home page context:', context);
    }

    // Navigate to portfolio in the home-portfolio-service
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    const portfolioUrl = userId 
      ? `${homeServiceUrl}/portfolio?user=${userId}`
      : `${homeServiceUrl}/portfolio?portfolio=${portfolioId}`;
    
    window.location.href = portfolioUrl;
  }, [setHomePageReturnContext, homePageCache]);

  // Navigate back to home page, restoring previous state
  const navigateBackToHome = useCallback(async () => {
    const returnContext = getHomePageReturnContext();
    
    // Get the home service URL
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    
    if (returnContext && homePageCache) {
      console.log('🔄 Restoring home page context:', returnContext);
      
      // Check if context is still valid (within 10 minutes)
      const isContextValid = new Date().getTime() - returnContext.timestamp < 10 * 60 * 1000;
      
      if (isContextValid) {
        // For cross-service navigation, we'll pass the context as URL parameters
        const params = new URLSearchParams();
        
        if (returnContext.page) params.set('page', returnContext.page.toString());
        if (returnContext.scrollPosition) params.set('scroll', returnContext.scrollPosition.toString());
        if (returnContext.filters?.searchTerm) params.set('search', returnContext.filters.searchTerm);
        if (returnContext.filters?.sortBy) params.set('sortBy', returnContext.filters.sortBy);
        if (returnContext.filters?.sortDirection) params.set('sortDirection', returnContext.filters.sortDirection);
        if (returnContext.filters?.selectedSkills?.length) params.set('skills', returnContext.filters.selectedSkills.join(','));
        if (returnContext.filters?.selectedRoles?.length) params.set('roles', returnContext.filters.selectedRoles.join(','));
        if (returnContext.filters?.featuredOnly) params.set('featured', 'true');
        
        const homeUrl = params.toString() ? `${homeServiceUrl}?${params.toString()}` : homeServiceUrl;
        window.location.href = homeUrl;
      } else {
        // Context is too old, just navigate to home without restoration
        console.log('⏰ Context expired, navigating to home without restoration');
        window.location.href = homeServiceUrl;
      }
    } else {
      // No context or cache available, just navigate
      window.location.href = homeServiceUrl;
    }

    // Clear the context after use
    clearHomePageReturnContext();
  }, [getHomePageReturnContext, clearHomePageReturnContext, homePageCache]);

  // Simple navigation to home page without state restoration
  const navigateToHome = useCallback(() => {
    clearHomePageReturnContext();
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    window.location.href = homeServiceUrl;
  }, [clearHomePageReturnContext]);

  // Check if we have a return context
  const hasReturnContext = useCallback(() => {
    const context = getHomePageReturnContext();
    if (!context) return false;
    
    // Check if context is still valid (within 10 minutes)
    return new Date().getTime() - context.timestamp < 10 * 60 * 1000;
  }, [getHomePageReturnContext]);

  return {
    navigateToPortfolio,
    navigateBackToHome,
    navigateToHome,
    hasReturnContext
  };
}
