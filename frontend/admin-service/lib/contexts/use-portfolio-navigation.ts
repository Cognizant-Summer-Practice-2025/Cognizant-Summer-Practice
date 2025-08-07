'use client';

import { useCallback } from 'react';
import { useRouter } from 'next/navigation';
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
  const router = useRouter();
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

    // Navigate to portfolio
    if (userId) {
      router.push(`/portfolio?user=${userId}`);
    } else {
      router.push(`/portfolio?portfolio=${portfolioId}`);
    }
  }, [router, setHomePageReturnContext, homePageCache]);

  // Navigate back to home page, restoring previous state
  const navigateBackToHome = useCallback(async () => {
    const returnContext = getHomePageReturnContext();
    
    if (returnContext && homePageCache) {
      console.log('🔄 Restoring home page context:', returnContext);
      
      // Check if context is still valid (within 10 minutes)
      const isContextValid = new Date().getTime() - returnContext.timestamp < 10 * 60 * 1000;
      
      if (isContextValid) {
        // Restore filters first
        if (returnContext.filters) {
          const { searchTerm, sortBy, sortDirection, selectedSkills, selectedRoles, featuredOnly } = returnContext.filters;
          
          // Apply filters
          homePageCache.setFilters({
            searchTerm,
            skills: selectedSkills,
            roles: selectedRoles,
            featured: featuredOnly,
          });
          
          // Apply sorting
          if (sortBy) {
            homePageCache.setSort(sortBy, sortDirection);
          }
        }
        
        // Navigate to home page
        router.push('/');
        
        // Wait for navigation and then restore page and scroll
        setTimeout(async () => {
          try {
            // Load the specific page
            await homePageCache.loadPage(returnContext.page);
            
            // Restore scroll position after a short delay to allow content to load
            setTimeout(() => {
              window.scrollTo({
                top: returnContext.scrollPosition,
                behavior: 'smooth'
              });
              console.log('📜 Restored scroll position:', returnContext.scrollPosition);
            }, 100);
          } catch (error) {
            console.error('Failed to restore home page state:', error);
            // Fallback to first page
            await homePageCache.loadPage(1);
          }
        }, 100);
      } else {
        // Context is too old, just navigate to home without restoration
        console.log('⏰ Context expired, navigating to home without restoration');
        router.push('/');
      }
    } else {
      // No context or cache available, just navigate
      router.push('/');
    }

    // Clear the context after use
    clearHomePageReturnContext();
  }, [router, getHomePageReturnContext, clearHomePageReturnContext, homePageCache]);

  // Simple navigation to home page without state restoration
  const navigateToHome = useCallback(() => {
    clearHomePageReturnContext();
    router.push('/');
  }, [router, clearHomePageReturnContext]);

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
