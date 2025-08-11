'use client';

import React, { useState, useEffect } from 'react';
import { useUser } from '@/lib/contexts/user-context';
import { useBookmarks } from '@/lib/contexts/bookmark-context';
import { bookmarkApi } from '@/lib/bookmark/api';
import { getPortfolioCardsForHomePage, PortfolioCardDto } from '@/lib/portfolio/api';
import PortfolioCard from '@/components/home-page/portfolio-card';
import '@/components/home-page/style.css'; // Import home page styles for consistent portfolio card styling
import { Loading } from '@/components/loader';
import { Bookmark, Heart } from 'lucide-react';

interface BookmarkPageData {
  portfolios: PortfolioCardDto[];
  loading: boolean;
  error: string | null;
}

export default function BookmarksPage() {
  const { user } = useUser();
  const { bookmarkedPortfolios, refreshBookmarks } = useBookmarks();
  const [data, setData] = useState<BookmarkPageData>({
    portfolios: [],
    loading: true,
    error: null
  });

  // Load bookmarked portfolios
  useEffect(() => {
    async function loadBookmarkedPortfolios() {
      if (!user?.id) return;

      try {
        setData(prev => ({ ...prev, loading: true, error: null }));

        // Get user's bookmarks
        const bookmarks = await bookmarkApi.getUserBookmarks(user.id);
        
        if (bookmarks.length === 0) {
          setData({
            portfolios: [],
            loading: false,
            error: null
          });
          return;
        }

        // Get all portfolio cards from home page
        const allPortfolios = await getPortfolioCardsForHomePage();
        
        // Filter to only show bookmarked portfolios
        const bookmarkedPortfolioIds = new Set(bookmarks.map(b => b.portfolioId));
        const bookmarkedPortfolios = allPortfolios.filter(portfolio => 
          bookmarkedPortfolioIds.has(portfolio.id)
        );

        setData({
          portfolios: bookmarkedPortfolios,
          loading: false,
          error: null
        });

      } catch (error) {
        console.error('Error loading bookmarked portfolios:', error);
        setData(prev => ({
          ...prev,
          loading: false,
          error: error instanceof Error ? error.message : 'Failed to load bookmarks'
        }));
      }
    }

    loadBookmarkedPortfolios();
  }, [user?.id, bookmarkedPortfolios]);

  // Refresh bookmarks when component mounts
  useEffect(() => {
    if (user?.id) {
      refreshBookmarks();
    }
  }, [user?.id, refreshBookmarks]);

  if (!user) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Please log in to view your bookmarks.</p>
        </div>
      </div>
    );
  }

  if (data.loading) {
    return (
      <div className="min-h-screen bg-gray-50 pt-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="flex items-center justify-center py-12">
              <Loading className="scale-75" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (data.error) {
    return (
      <div className="min-h-screen bg-gray-50 pt-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-lg shadow-sm p-6">
            <div className="text-center py-12">
              <div className="text-red-600 mb-4">
                <Heart className="w-12 h-12 mx-auto opacity-50" />
              </div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">Error Loading Bookmarks</h3>
              <p className="text-gray-600 mb-4">{data.error}</p>
              <button
                onClick={() => window.location.reload()}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 pt-16">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center gap-3 mb-2">
            <Bookmark className="w-8 h-8 text-blue-600" />
            <h1 className="text-3xl font-bold text-gray-900">My Bookmarks</h1>
          </div>
          <p className="text-gray-600">
            {data.portfolios.length} saved {data.portfolios.length === 1 ? 'portfolio' : 'portfolios'}
          </p>
        </div>

        {/* Portfolio Grid */}
        {data.portfolios.length > 0 ? (
          <div className="bg-white rounded-lg shadow-sm overflow-hidden">
            <div className="p-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {data.portfolios.map((portfolio) => (
                  <PortfolioCard
                    key={portfolio.id}
                    id={portfolio.id}
                    name={portfolio.name}
                    role={portfolio.role}
                    location={portfolio.location}
                    description={portfolio.description}
                    skills={portfolio.skills}
                    views={portfolio.views}
                    likes={portfolio.likes}
                    comments={portfolio.comments}
                    bookmarks={portfolio.bookmarks}
                    date={portfolio.date}
                    avatar={portfolio.avatar}
                    featured={portfolio.featured}
                  />
                ))}
              </div>
            </div>
          </div>
        ) : (
          <div className="bg-white rounded-lg shadow-sm">
            <div className="text-center py-16">
              <div className="text-gray-400 mb-6">
                <Bookmark className="w-16 h-16 mx-auto opacity-50" />
              </div>
              <h3 className="text-xl font-medium text-gray-900 mb-2">No Bookmarks Yet</h3>
              <p className="text-gray-600 mb-6 max-w-md mx-auto">
                Start exploring portfolios and bookmark the ones you find inspiring. 
                You can bookmark portfolios by clicking the bookmark icon on any portfolio card.
              </p>
              <button
                onClick={() => {
                  const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
                  window.location.href = homeServiceUrl;
                }}
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Explore Portfolios
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
} 