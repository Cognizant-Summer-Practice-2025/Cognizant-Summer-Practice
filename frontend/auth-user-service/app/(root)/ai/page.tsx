'use client';

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Brain, Sparkles, Loader2 } from 'lucide-react';
import PortfolioCard from '@/components/home-page/portfolio-card';
import { generateBestPortfolios, convertAIPortfoliosToCards } from '@/lib/ai/api';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import '@/components/home-page/style.css'; // Import home page styles for consistent portfolio card styling

interface AIPageState {
  portfolios: PortfolioCardDto[];
  loading: boolean;
  error: string | null;
  hasGenerated: boolean;
  rawResponse: any; // Store raw JSON response for debugging
}

export default function AIPage() {
  const [state, setState] = useState<AIPageState>({
    portfolios: [],
    loading: false,
    error: null,
    hasGenerated: false,
    rawResponse: null
  });

  const handleGeneratePortfolios = async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      // Call AI service to generate best portfolios
      const aiPortfolios = await generateBestPortfolios();
      
      console.log('Raw AI Response:', aiPortfolios); // Debug log
      
      // Convert to portfolio card format
      const portfolioCards = await convertAIPortfoliosToCards(aiPortfolios);
      
      setState({
        portfolios: portfolioCards,
        loading: false,
        error: null,
        hasGenerated: true,
        rawResponse: aiPortfolios
      });
    } catch (error) {
      console.error('Error generating portfolios:', error);
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Failed to generate portfolios. Please try again.'
      }));
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 pt-16">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Generate Button Section - Top Left */}
        <div className="mb-12">
          <Button
            onClick={handleGeneratePortfolios}
            disabled={state.loading}
            className="px-8 py-4 bg-blue-600 hover:bg-blue-700 text-white text-lg font-medium rounded-lg flex items-center gap-3"
            size="lg"
          >
            {state.loading ? (
              <>
                <Loader2 className="w-6 h-6 animate-spin" />
                Generating Portfolios...
              </>
            ) : (
              <>
                <Sparkles className="w-6 h-6" />
                Generate Portfolio
              </>
            )}
          </Button>
          
          {state.hasGenerated && !state.loading && !state.error && (
            <p className="text-sm text-green-600 mt-3 font-medium">
              ✨ Generated {state.portfolios.length} top portfolios
            </p>
          )}
        </div>

        {/* Error Message */}
        {state.error && (
          <div className="mb-8">
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <p className="text-red-700">{state.error}</p>
              <Button
                onClick={handleGeneratePortfolios}
                variant="outline"
                className="mt-3 border-red-200 text-red-700 hover:bg-red-50"
              >
                Try Again
              </Button>
            </div>
          </div>
        )}

        {/* Loading State */}
        {state.loading && (
          <div className="py-12">
            <div className="flex items-center gap-3 text-gray-600">
              <Loader2 className="w-6 h-6 animate-spin" />
              <span className="text-lg">AI is analyzing portfolios and selecting the best ones...</span>
            </div>
            <div className="mt-4 text-sm text-gray-500">
              This may take a few moments while we process the data
            </div>
          </div>
        )}

        {/* Generated Portfolios Grid */}
        {state.portfolios.length > 0 && !state.loading && (
          <div className="bg-white rounded-lg shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                <Sparkles className="w-5 h-5 text-blue-600" />
                AI-Generated Top Portfolios
              </h2>
              <p className="text-gray-600 mt-1">
                {state.portfolios.length} portfolios selected based on content quality, skills, and engagement
              </p>
            </div>
            
            <div className="p-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {state.portfolios.map((portfolio) => (
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
        )}

        {/* Raw JSON Response Debug Display */}
        {state.rawResponse && !state.loading && (
          <div className="bg-white rounded-lg shadow-sm overflow-hidden mt-8">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                <Brain className="w-5 h-5 text-green-600" />
                Raw Backend AI Response
              </h2>
              <p className="text-gray-600 mt-1">
                JSON data returned from backend-AI service
              </p>
            </div>
            
            <div className="p-6">
              <div className="bg-gray-50 rounded-lg p-4 border">
                <pre className="text-sm text-gray-800 overflow-x-auto whitespace-pre-wrap">
                  {JSON.stringify(state.rawResponse, null, 2)}
                </pre>
              </div>
              
              {Array.isArray(state.rawResponse) && state.rawResponse.length > 0 && (
                <div className="mt-6">
                  <h3 className="text-lg font-medium text-gray-900 mb-4">
                    Individual Portfolio Cards
                  </h3>
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    {state.rawResponse.map((item: any, index: number) => (
                      <div key={index} className="ant-card-body bg-white border border-gray-200 rounded-lg p-4">
                        <div className="flex justify-between items-start mb-2">
                          <h4 className="font-medium text-gray-900">Portfolio #{index + 1}</h4>
                          <span className="text-xs text-gray-500">ID: {item.id || 'No ID'}</span>
                        </div>
                        <div className="space-y-2 text-sm">
                          {Object.entries(item).map(([key, value]) => (
                            <div key={key} className="flex">
                              <span className="font-medium text-gray-600 w-32 flex-shrink-0">{key}:</span>
                              <span className="text-gray-800 break-words">
                                {typeof value === 'object' ? JSON.stringify(value) : String(value)}
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Empty State - Show only if user hasn't generated yet */}
        {!state.hasGenerated && !state.loading && !state.error && (
          <div className="py-12">
            <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mb-4 flex items-center justify-center">
              <Brain className="w-8 h-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Ready to Generate</h3>
            <p className="text-gray-600 max-w-md">
              Click the generate button above to let our AI curate the best portfolios for you.
            </p>
          </div>
        )}
      </div>
    </div>
  );
} 