'use client';

import React, { createContext, useContext, useCallback, useState, ReactNode } from 'react';
import { PortfolioCardDto } from '@/lib/portfolio/api';

export interface PaginationRequest {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: string;
  searchTerm?: string;
  skills?: string[];
  roles?: string[];
  featured?: boolean;
  dateFrom?: Date;
  dateTo?: Date;
}

export interface PaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
  sortBy?: string;
  sortDirection?: string;
  nextPage?: number;
  previousPage?: number;
}

export interface PaginatedResponse<T> {
  data: T[];
  pagination: PaginationMetadata;
  cacheKey?: string;
  cachedAt: string;
}

interface CacheEntry<T> {
  data: PaginatedResponse<T>;
  timestamp: number;
  expiry: number;
}

interface HomePageCacheContextType {
  // Pagination state
  currentPage: number;
  pageSize: number;
  portfolios: PortfolioCardDto[];
  pagination: PaginationMetadata | null;
  loading: boolean;
  error: string | null;
  
  // Filters and sorting
  searchTerm: string;
  sortBy: string;
  sortDirection: string;
  selectedSkills: string[];
  selectedRoles: string[];
  featuredOnly: boolean;
  dateFrom: Date | null;
  dateTo: Date | null;
  
  // Cache management
  
  // Actions
  loadPage: (page: number, useCache?: boolean) => Promise<void>;
  setFilters: (filters: Partial<PaginationRequest>) => void;
  setSort: (sortBy: string, sortDirection?: string) => void;
  setSearch: (searchTerm: string) => void;
  clearFilters: () => void;
  
  // Navigation
  goToPage: (page: number) => Promise<void>;
  goToNextPage: () => Promise<void>;
  goToPreviousPage: () => Promise<void>;
  goToFirstPage: () => Promise<void>;
  goToLastPage: () => Promise<void>;
  
  // Cache management
  clearCache: () => void;
  preloadPage: (page: number) => Promise<void>;

}

const HomePageCacheContext = createContext<HomePageCacheContextType | undefined>(undefined);

// In-memory cache with TTL
class PageCache {
  private cache = new Map<string, CacheEntry<PortfolioCardDto>>();
  private readonly TTL = 5 * 60 * 1000; // 5 minutes
  private hits = 0;
  private misses = 0;
  private lastClearTime: Date | null = null;

  generateKey(request: PaginationRequest): string {
    const params = [
      `page:${request.page}`,
      `size:${request.pageSize}`,
      `sort:${request.sortBy || 'most-recent'}:${request.sortDirection || 'desc'}`,
      `search:${request.searchTerm || ''}`,
      `skills:${request.skills?.join(',') || ''}`,
      `roles:${request.roles?.join(',') || ''}`,
      `featured:${request.featured || ''}`,
      `from:${request.dateFrom?.toISOString() || ''}`,
      `to:${request.dateTo?.toISOString() || ''}`
    ];
    return params.join('|');
  }

  get<T>(key: string): PaginatedResponse<T> | null {
    const entry = this.cache.get(key) as CacheEntry<T> | undefined;
    
    if (!entry) {
      this.misses++;
      return null;
    }

    if (Date.now() > entry.expiry) {
      this.cache.delete(key);
      this.misses++;
      return null;
    }

    this.hits++;
    return entry.data;
  }

  set<T>(key: string, value: PaginatedResponse<T>): void {
    const now = Date.now();
    this.cache.set(key, {
      data: value,
      timestamp: now,
      expiry: now + this.TTL
    } as CacheEntry<PortfolioCardDto>);
  }

  clear(): void {
    this.cache.clear();
    this.hits = 0;
    this.misses = 0;
    this.lastClearTime = new Date();
  }



  // Clean expired entries
  cleanup(): void {
    const now = Date.now();
    for (const [key, entry] of this.cache.entries()) {
      if (now > entry.expiry) {
        this.cache.delete(key);
      }
    }
  }
}

export function HomePageCacheProvider({ children }: { children: ReactNode }) {
  const [cache] = useState(() => new PageCache());
  
  // State
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [portfolios, setPortfolios] = useState<PortfolioCardDto[]>([]);
  const [pagination, setPagination] = useState<PaginationMetadata | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Filters
  const [searchTerm, setSearchTermState] = useState('');
  const [sortBy, setSortByState] = useState('most-recent');
  const [sortDirection, setSortDirectionState] = useState('desc');
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [featuredOnly, setFeaturedOnly] = useState(false);
  const [dateFrom, setDateFrom] = useState<Date | null>(null);
  const [dateTo, setDateTo] = useState<Date | null>(null);

  // Create request object
  const createRequest = useCallback((page: number): PaginationRequest => {
    return {
      page,
      pageSize,
      sortBy,
      sortDirection,
      searchTerm: searchTerm || undefined,
      skills: selectedSkills.length > 0 ? selectedSkills : undefined,
      roles: selectedRoles.length > 0 ? selectedRoles : undefined,
      featured: featuredOnly || undefined,
      dateFrom: dateFrom || undefined,
      dateTo: dateTo || undefined,
    };
  }, [pageSize, sortBy, sortDirection, searchTerm, selectedSkills, selectedRoles, featuredOnly, dateFrom, dateTo]);

  // Preload single page function
  const preloadPage = useCallback(async (page: number) => {
    const request = createRequest(page);
    const cacheKey = cache.generateKey(request);
    
    // Don't preload if already cached
    if (cache.get<PortfolioCardDto>(cacheKey)) {
      return;
    }

    try {
      // Load page without updating UI state
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const queryParams = new URLSearchParams();
      
      queryParams.append('page', request.page.toString());
      queryParams.append('pageSize', request.pageSize.toString());
      
      if (request.sortBy) queryParams.append('sortBy', request.sortBy);
      if (request.sortDirection) queryParams.append('sortDirection', request.sortDirection);
      if (request.searchTerm) queryParams.append('searchTerm', request.searchTerm);
      if (request.skills) request.skills.forEach(skill => queryParams.append('skills', skill));
      if (request.roles) request.roles.forEach(role => queryParams.append('roles', role));
      if (request.featured !== undefined) queryParams.append('featured', request.featured.toString());
      if (request.dateFrom) queryParams.append('dateFrom', request.dateFrom.toISOString().split('T')[0]);
      if (request.dateTo) queryParams.append('dateTo', request.dateTo.toISOString().split('T')[0]);

      const response = await fetch(`${API_BASE_URL}/api/Portfolio/home-page-cards/paginated?${queryParams}`);
      
      if (response.ok) {
        const data: PaginatedResponse<PortfolioCardDto> = await response.json();
        cache.set(cacheKey, data);
        console.log(`📦 Preloaded page ${page}`);
      }
    } catch (err) {
      console.warn(`Failed to preload page ${page}:`, err);
    }
  }, [createRequest, cache]);

  // Preload adjacent pages for smoother navigation
  const preloadAdjacentPages = useCallback(async (currentPageNum: number, totalPages: number) => {
    const pagesToPreload: number[] = [];
    
    // Determine which pages to preload based on current page
    if (currentPageNum === 1) {
      // If on page 1, preload the first several pages for initial browsing
      for (let i = 2; i <= Math.min(4, totalPages); i++) {
        pagesToPreload.push(i);
      }
    } else if (currentPageNum === 2) {
      // If on page 2, preload pages 1, 3, and 4
      pagesToPreload.push(1); // Users often go back to page 1
      if (currentPageNum + 1 <= totalPages) pagesToPreload.push(currentPageNum + 1);
      if (currentPageNum + 2 <= totalPages) pagesToPreload.push(currentPageNum + 2);
    } else {
      // For other pages, preload adjacent pages
      if (currentPageNum > 1) {
        pagesToPreload.push(currentPageNum - 1);
      }
      if (currentPageNum < totalPages) {
        pagesToPreload.push(currentPageNum + 1);
      }
      
      // Preload one more page in the direction user is likely going
      if (currentPageNum + 2 <= totalPages) {
        pagesToPreload.push(currentPageNum + 2);
      }
    }
    
    // Filter out pages that are already cached
    const uncachedPages = pagesToPreload.filter(pageNum => {
      const request = createRequest(pageNum);
      const cacheKey = cache.generateKey(request);
      return !cache.get<PortfolioCardDto>(cacheKey);
    });
    
    if (uncachedPages.length === 0) {
      console.log(`📦 All adjacent pages already cached for page ${currentPageNum}`);
      return;
    }
    
    // Preload pages in the background with staggered timing
    uncachedPages.forEach((pageNum, index) => {
      setTimeout(() => {
        preloadPage(pageNum).catch(err => {
          console.debug(`Background preload failed for page ${pageNum}:`, err);
        });
      }, 150 + (index * 100)); // Stagger requests to avoid overwhelming the server
    });
    
    console.log(`🚀 Preloading ${uncachedPages.length} pages: ${uncachedPages.join(', ')}`);
  }, [preloadPage, createRequest, cache]);

  // Load page with cache support
  const loadPage = useCallback(async (page: number, useCache = true) => {
    const request = createRequest(page);
    const cacheKey = cache.generateKey(request);
    
    // Try cache first
    if (useCache) {
      const cachedData = cache.get<PortfolioCardDto>(cacheKey);
      if (cachedData) {
        setPortfolios(cachedData.data);
        setPagination(cachedData.pagination);
        setCurrentPage(page);
        setError(null);
        
        // Scroll to top when page changes (cached data)
        window.scrollTo({ top: 0, behavior: 'smooth' });
        
        // Also trigger preloading for cached hits
        setTimeout(() => {
          preloadAdjacentPages(page, cachedData.pagination.totalPages);
        }, 50); // Smaller delay for cached data
        
        return;
      }
    }

    try {
      setLoading(true);
      setError(null);
      
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const queryParams = new URLSearchParams();
      
      queryParams.append('page', request.page.toString());
      queryParams.append('pageSize', request.pageSize.toString());
      
      if (request.sortBy) queryParams.append('sortBy', request.sortBy);
      if (request.sortDirection) queryParams.append('sortDirection', request.sortDirection);
      if (request.searchTerm) queryParams.append('searchTerm', request.searchTerm);
      if (request.skills) request.skills.forEach(skill => queryParams.append('skills', skill));
      if (request.roles) request.roles.forEach(role => queryParams.append('roles', role));
      if (request.featured !== undefined) queryParams.append('featured', request.featured.toString());
      if (request.dateFrom) queryParams.append('dateFrom', request.dateFrom.toISOString().split('T')[0]);
      if (request.dateTo) queryParams.append('dateTo', request.dateTo.toISOString().split('T')[0]);

      const fullUrl = `${API_BASE_URL}/api/Portfolio/home-page-cards/paginated?${queryParams}`;
      const response = await fetch(fullUrl);
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const data: PaginatedResponse<PortfolioCardDto> = await response.json();
      
      // Cache the result
      cache.set(cacheKey, data);
      
      // Update state
      setPortfolios(data.data);
      setPagination(data.pagination);
      setCurrentPage(page);
      
      // Scroll to top when page changes
      window.scrollTo({ top: 0, behavior: 'smooth' });
      
      // Trigger preloading after successful load (use data from response)
      setTimeout(() => {
        preloadAdjacentPages(page, data.pagination.totalPages);
      }, 100); // Small delay to ensure state updates complete
      
    } catch (err) {
      console.error('Error loading portfolios:', err);
      setError(err instanceof Error ? err.message : 'Failed to load portfolios');
      setPortfolios([]);
      setPagination(null);
    } finally {
      setLoading(false);
    }
  }, [createRequest, cache, preloadAdjacentPages]);

  // Filter and search actions
  const setFilters = useCallback((filters: Partial<PaginationRequest>) => {
    if (filters.searchTerm !== undefined) setSearchTermState(filters.searchTerm);
    if (filters.skills !== undefined) setSelectedSkills(filters.skills);
    if (filters.roles !== undefined) setSelectedRoles(filters.roles);
    if (filters.featured !== undefined) setFeaturedOnly(filters.featured);
    if (filters.dateFrom !== undefined) setDateFrom(filters.dateFrom || null);
    if (filters.dateTo !== undefined) setDateTo(filters.dateTo || null);
    
    // Reset to page 1 when filters change
    setCurrentPage(1);
    loadPage(1, false); // Skip cache when filters change
  }, [loadPage]);

  const setSort = useCallback((newSortBy: string, newSortDirection = 'desc') => {
    setSortByState(newSortBy);
    setSortDirectionState(newSortDirection);
    loadPage(1, false); // Skip cache when sort changes
  }, [loadPage]);

  const setSearch = useCallback((newSearchTerm: string) => {
    setSearchTermState(newSearchTerm);
    loadPage(1, false); // Skip cache when search changes
  }, [loadPage]);

  const clearFilters = useCallback(() => {
    setSearchTermState('');
    setSelectedSkills([]);
    setSelectedRoles([]);
    setFeaturedOnly(false);
    setDateFrom(null);
    setDateTo(null);
    setSortByState('most-recent');
    setSortDirectionState('desc');
    loadPage(1, false);
  }, [loadPage]);

  // Navigation actions
  const goToPage = useCallback((page: number) => {
    if (page >= 1 && (pagination?.totalPages === undefined || page <= pagination.totalPages)) {
      return loadPage(page);
    }
    return Promise.resolve();
  }, [loadPage, pagination?.totalPages]);

  const goToNextPage = useCallback(() => {
    if (pagination?.hasNext) {
      return loadPage(currentPage + 1);
    }
    return Promise.resolve();
  }, [loadPage, currentPage, pagination?.hasNext]);

  const goToPreviousPage = useCallback(() => {
    if (pagination?.hasPrevious) {
      return loadPage(currentPage - 1);
    }
    return Promise.resolve();
  }, [loadPage, currentPage, pagination?.hasPrevious]);

  const goToFirstPage = useCallback(() => {
    if (currentPage !== 1) {
      return loadPage(1);
    }
    return Promise.resolve();
  }, [loadPage, currentPage]);

  const goToLastPage = useCallback(() => {
    if (pagination?.totalPages && currentPage !== pagination.totalPages) {
      return loadPage(pagination.totalPages);
    }
    return Promise.resolve();
  }, [loadPage, currentPage, pagination?.totalPages]);

  // Cache management
  const clearCache = useCallback(() => {
    cache.clear();
    console.log('🗑️ Cache cleared');
  }, [cache]);



  const value: HomePageCacheContextType = {
    // State
    currentPage,
    pageSize,
    portfolios,
    pagination,
    loading,
    error,
    
    // Filters
    searchTerm,
    sortBy,
    sortDirection,
    selectedSkills,
    selectedRoles,
    featuredOnly,
    dateFrom,
    dateTo,
    

    
    // Actions
    loadPage,
    setFilters,
    setSort,
    setSearch,
    clearFilters,
    
    // Navigation
    goToPage,
    goToNextPage,
    goToPreviousPage,
    goToFirstPage,
    goToLastPage,
    
    // Cache management
    clearCache,
    preloadPage,

  };

  return (
    <HomePageCacheContext.Provider value={value}>
      {children}
    </HomePageCacheContext.Provider>
  );
}

export function useHomePageCache(): HomePageCacheContextType {
  const context = useContext(HomePageCacheContext);
  if (context === undefined) {
    throw new Error('useHomePageCache must be used within a HomePageCacheProvider');
  }
  return context;
}
