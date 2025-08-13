'use client';

import { useState, useCallback, useRef, useEffect } from 'react';
import { PortfolioCardDto } from '@/lib/portfolio/api';

export interface SearchResult {
  id: string;
  userId: string;
  name: string;
  role: string;
  location: string;
  description: string;
  skills: string[];
  avatar?: string;
  featured: boolean;
}

interface UsePortfolioSearchReturn {
  searchTerm: string;
  setSearchTerm: (term: string) => void;
  results: SearchResult[];
  loading: boolean;
  error: string | null;
  showResults: boolean;
  setShowResults: (show: boolean) => void;
  searchInputRef: React.RefObject<HTMLInputElement | null>;
  searchContainerRef: React.RefObject<HTMLDivElement | null>;
  handleResultClick: (result: SearchResult) => void;
  clearSearch: () => void;
}

export const usePortfolioSearch = (): UsePortfolioSearchReturn => {
  const [searchTerm, setSearchTerm] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showResults, setShowResults] = useState(false);
  const searchInputRef = useRef<HTMLInputElement>(null);
  const searchContainerRef = useRef<HTMLDivElement>(null);
  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // Debounced search function
  const performSearch = useCallback(async (term: string) => {
    if (!term.trim()) {
      setResults([]);
      setShowResults(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5201';
      const queryParams = new URLSearchParams();
      
      queryParams.append('page', '1');
      queryParams.append('pageSize', '10'); // Limit results for search dropdown
      queryParams.append('searchTerm', term);

      const response = await fetch(`${API_BASE_URL}/api/Portfolio/home-page-cards/paginated?${queryParams}`);
      
      if (!response.ok) {
        throw new Error('Search failed');
      }

      const data = await response.json();
      const searchResults: SearchResult[] = data.data.map((card: PortfolioCardDto) => ({
        id: card.id,
        userId: card.userId,
        name: card.name,
        role: card.role,
        location: card.location,
        description: card.description,
        skills: card.skills,
        avatar: card.avatar,
        featured: card.featured
      }));

      setResults(searchResults);
      setShowResults(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search failed');
      setResults([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // Handle search term changes with debouncing
  const handleSearchTermChange = useCallback((term: string) => {
    setSearchTerm(term);
    
    // Clear previous timeout
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }

    // Set new timeout for debounced search
    debounceTimeoutRef.current = setTimeout(() => {
      performSearch(term);
    }, 300);
  }, [performSearch]);

  // Handle result click - redirect to home portfolio service
  const handleResultClick = useCallback((result: SearchResult) => {
    setShowResults(false);
    setSearchTerm('');
    const homePortfolioService = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    window.location.href = `${homePortfolioService}/portfolio?user=${result.userId}`;
  }, []);

  // Clear search
  const clearSearch = useCallback(() => {
    setSearchTerm('');
    setResults([]);
    setShowResults(false);
    setError(null);
    if (searchInputRef.current) {
      searchInputRef.current.blur();
    }
  }, []);

  // Handle clicks outside search component
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Element;
      
      const isInsideInput = searchInputRef.current && searchInputRef.current.contains(target);
      const isInsideContainer = searchContainerRef.current && searchContainerRef.current.contains(target);
      const isSearchResultButton = target.closest('[data-search-result-button]') !== null;
      
      if (!isInsideInput && !isInsideContainer && !isSearchResultButton) {
        setShowResults(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, []);

  return {
    searchTerm,
    setSearchTerm: handleSearchTermChange,
    results,
    loading,
    error,
    showResults,
    setShowResults,
    searchInputRef,
    searchContainerRef,
    handleResultClick,
    clearSearch
  };
}; 