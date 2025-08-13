'use client';

import React from 'react';
import Image from 'next/image';
import { SearchResult } from '@/hooks/usePortfolioSearch';
import { Loader2, Search } from 'lucide-react';

interface SearchResultsProps {
  results: SearchResult[];
  loading: boolean;
  error: string | null;
  showResults: boolean;
  onResultClick: (result: SearchResult) => void;
  searchTerm: string;
  searchContainerRef?: React.RefObject<HTMLDivElement | null>;
}

const SearchResults: React.FC<SearchResultsProps> = ({
  results,
  loading,
  error,
  showResults,
  onResultClick,
  searchTerm,
  searchContainerRef,
}) => {
  if (!showResults) return null;

  // Helper function to get user avatar with fallback
  const getUserAvatar = (result: SearchResult) => {
    if (result.avatar) {
      return result.avatar;
    }
    // Generate default avatar with user's initials
    const initials = result.name
      .split(' ')
      .map(name => name.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&size=40&background=f0f0f0&color=666`;
  };

  return (
    <div 
      ref={searchContainerRef}
      className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-50 max-h-96 overflow-y-auto"
    >
      {loading && (
        <div className="flex items-center justify-center p-4">
          <Loader2 className="w-5 h-5 animate-spin text-gray-400 mr-2" />
          <span className="text-sm text-gray-500">Searching...</span>
        </div>
      )}

      {error && (
        <div className="p-4 text-center">
          <p className="text-sm text-red-500">Search failed. Please try again.</p>
        </div>
      )}

      {!loading && !error && results.length === 0 && searchTerm.trim() && (
        <div className="p-4 text-center">
          <Search className="w-8 h-8 text-gray-300 mx-auto mb-2" />
          <p className="text-sm text-gray-500">No portfolios found for "{searchTerm}"</p>
          <p className="text-xs text-gray-400 mt-1">Try searching for names, skills, or roles</p>
        </div>
      )}

      {!loading && !error && results.length > 0 && (
        <>
          <div className="px-3 py-2 border-b border-gray-100">
            <p className="text-xs text-gray-500">
              {results.length} result{results.length !== 1 ? 's' : ''} for "{searchTerm}"
            </p>
          </div>
          <div className="max-h-80 overflow-y-auto">
            {results.map((result) => (
              <button
                key={result.id}
                data-search-result-button="true"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  onResultClick(result);
                }}
                onMouseDown={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                }}
                className="w-full px-3 py-3 flex items-start gap-3 hover:bg-gray-50 hover:shadow-sm active:bg-gray-100 transition-all duration-200 border-b border-gray-50 last:border-b-0 text-left cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-gray-50"
                type="button"
                tabIndex={0}
                role="button"
                aria-label={`View ${result.name}'s portfolio`}
              >
                <div className="flex-shrink-0">
                  <Image
                    src={getUserAvatar(result)}
                    alt={result.name}
                    width={40}
                    height={40}
                    className="rounded-full"
                  />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {result.name}
                    </h4>
                    {result.featured && (
                      <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                        Featured
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-gray-600 truncate">{result.role}</p>
                  {result.location && (
                    <p className="text-xs text-gray-500 truncate">{result.location}</p>
                  )}
                  {result.skills.length > 0 && (
                    <div className="mt-1 flex flex-wrap gap-1">
                      {result.skills.slice(0, 3).map((skill, index) => (
                        <span
                          key={index}
                          className="inline-flex items-center px-2 py-0.5 rounded text-xs bg-gray-100 text-gray-600"
                        >
                          {skill}
                        </span>
                      ))}
                      {result.skills.length > 3 && (
                        <span className="text-xs text-gray-400">
                          +{result.skills.length - 3} more
                        </span>
                      )}
                    </div>
                  )}
                </div>
              </button>
            ))}
          </div>
        </>
      )}
    </div>
  );
};

export default SearchResults; 