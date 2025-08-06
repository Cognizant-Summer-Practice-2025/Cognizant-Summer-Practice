'use client';

import React, { createContext, useContext, useState, ReactNode, useCallback } from 'react';
import { bookmarkApi, BookmarkToggleRequest } from '@/lib/bookmark/api';
import { useUser } from './user-context';

interface BookmarkContextType {
  bookmarkedPortfolios: Set<string>;
  isBookmarked: (portfolioId: string) => boolean;
  toggleBookmark: (portfolioId: string, portfolioTitle?: string) => Promise<boolean>;
  loading: boolean;
  error: string | null;
  refreshBookmarks: () => Promise<void>;
}

const BookmarkContext = createContext<BookmarkContextType | undefined>(undefined);

export function BookmarkProvider({ children }: { children: ReactNode }) {
  const { user } = useUser();
  const [bookmarkedPortfolios, setBookmarkedPortfolios] = useState<Set<string>>(new Set());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isBookmarked = useCallback((portfolioId: string): boolean => {
    return bookmarkedPortfolios.has(portfolioId);
  }, [bookmarkedPortfolios]);

  const refreshBookmarks = useCallback(async () => {
    if (!user?.id) return;

    try {
      setLoading(true);
      setError(null);
      const userBookmarks = await bookmarkApi.getUserBookmarks(user.id);
      const portfolioIds = new Set(userBookmarks.map(b => b.portfolioId));
      setBookmarkedPortfolios(portfolioIds);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch bookmarks';
      setError(errorMessage);
      console.error('Error refreshing bookmarks:', err);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  const toggleBookmark = useCallback(async (portfolioId: string, portfolioTitle?: string): Promise<boolean> => {
    // Check if user is logged in
    if (!user?.id) {
      // Preserve current page when redirecting to login
      const currentPath = window.location.pathname + window.location.search;
      const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
      window.location.href = `${authServiceUrl}/login?callbackUrl=${encodeURIComponent(currentPath)}`;
      return false;
    }

    try {
      setLoading(true);
      setError(null);

      const request: BookmarkToggleRequest = {
        userId: user.id,
        portfolioId,
        collectionName: 'Default',
        notes: portfolioTitle ? `Bookmarked: ${portfolioTitle}` : undefined,
      };

      const response = await bookmarkApi.toggleBookmark(request);

      // Update local state
      setBookmarkedPortfolios(prev => {
        const newSet = new Set(prev);
        if (response.isBookmarked) {
          newSet.add(portfolioId);
        } else {
          newSet.delete(portfolioId);
        }
        return newSet;
      });

      return response.isBookmarked;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to toggle bookmark';
      setError(errorMessage);
      console.error('Error toggling bookmark:', err);
      return false;
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  // Load user bookmarks when user is present and injection is complete
  React.useEffect(() => {
    if (user?.id) {
      // Add a small delay to ensure user injection is complete
      const timer = setTimeout(() => {
        refreshBookmarks();
      }, 100);
      
      return () => clearTimeout(timer);
    } else {
      setBookmarkedPortfolios(new Set());
    }
  }, [user?.id, refreshBookmarks]);

  const value: BookmarkContextType = {
    bookmarkedPortfolios,
    isBookmarked,
    toggleBookmark,
    loading,
    error,
    refreshBookmarks,
  };

  return (
    <BookmarkContext.Provider value={value}>
      {children}
    </BookmarkContext.Provider>
  );
}

export function useBookmarks(): BookmarkContextType {
  const context = useContext(BookmarkContext);
  if (context === undefined) {
    throw new Error('useBookmarks must be used within a BookmarkProvider');
  }
  return context;
} 