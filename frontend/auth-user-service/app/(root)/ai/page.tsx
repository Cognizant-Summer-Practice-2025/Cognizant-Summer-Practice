'use client';

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Brain, Sparkles, Loader2, Newspaper, AlertCircle } from 'lucide-react';
import PortfolioCard from '@/components/home-page/portfolio-card';
import { generateBestPortfolios, convertAIPortfoliosToCards, getLatestTechNews } from '@/lib/ai/api';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import { TechNewsSummary } from '@/components/ai/TechNewsSummary';
import PremiumGuard from '@/components/premium/PremiumGuard';
import PortfolioDeployment from '@/components/deployment/PortfolioDeployment';
import '@/components/home-page/style.css'; // Import home page styles for consistent portfolio card styling

interface AIPageState {
  portfolios: PortfolioCardDto[];
  loading: boolean;
  error: string | null;
  hasGenerated: boolean;
  techNews?: { summary: string } | null;
  techLoading?: boolean;
  showDeployment?: boolean;
}

function AIPageContent() {
  const [state, setState] = useState<AIPageState>({
    portfolios: [],
    loading: false,
    error: null,
    hasGenerated: false,
    techNews: null,
    techLoading: false,
    showDeployment: false
  });

  const handleGeneratePortfolios = async () => {
    // Clear any existing content
    setState(prev => ({ 
      ...prev, 
      loading: true, 
      error: null, 
      techNews: null,
      hasGenerated: false,
      showDeployment: false
    }));

    try {
      // Call AI service to generate best portfolios
      const aiPortfolios = await generateBestPortfolios();
      
      console.log('Raw AI Response:', aiPortfolios); // Debug log
      
      // Check if we have enough portfolios for AI generation
      if (!aiPortfolios || aiPortfolios.length === 0) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'Insufficient portfolio data. The AI requires at least 10 portfolios in the database to generate recommendations. Please add more portfolios and try again.'
        }));
        return;
      }
      
      // Convert to portfolio card format
      const portfolioCards = await convertAIPortfoliosToCards(aiPortfolios);
      
      // Additional check after conversion
      if (portfolioCards.length === 0) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'No valid portfolios could be processed. The AI requires at least 10 portfolios in the database to generate recommendations.'
        }));
        return;
      }
      
      setState({
        portfolios: portfolioCards,
        loading: false,
        error: null,
        hasGenerated: true,
        techNews: null
      });
    } catch (error) {
      console.error('Error generating portfolios:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to generate portfolios. Please try again.';
      
      // Check if the error is related to insufficient data
      if (errorMessage.toLowerCase().includes('insufficient') || errorMessage.toLowerCase().includes('not enough')) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'Insufficient portfolio data. The AI requires at least 10 portfolios in the database to generate meaningful recommendations. Please add more portfolios and try again.'
        }));
      } else {
        setState(prev => ({
          ...prev,
          loading: false,
          error: errorMessage
        }));
      }
    }
  };

  const handleShowTechNews = async () => {
    // Clear any existing content
    setState(prev => ({ 
      ...prev, 
      techLoading: true, 
      portfolios: [],
      hasGenerated: false,
      error: null,
      showDeployment: false
    }));
    
    try {
      const news = await getLatestTechNews();
      
      setState(prev => ({ 
        ...prev, 
        techNews: news, 
        techLoading: false 
      }));
    } catch (e) {
      console.error('Failed to load tech news', e);
      let errorMessage = 'Failed to load tech news. Please try again.';
      
      // Handle specific OpenRouter rate limit errors
      if (e instanceof Error) {
        if (e.message.includes('Rate limit exceeded') || e.message.includes('TooManyRequests')) {
          errorMessage = 'AI service rate limit exceeded. Please try again later or contact support.';
        } else if (e.message.includes('OpenRouter error')) {
          errorMessage = 'AI service temporarily unavailable. Please try again later.';
        }
      }
      
      setState(prev => ({ 
        ...prev, 
        techNews: null, 
        techLoading: false,
        error: errorMessage
      }));
    }
  };

  const handleShowDeployment = () => {
    // Clear any existing content and show deployment
    setState(prev => ({ 
      ...prev, 
      portfolios: [],
      hasGenerated: false,
      techNews: null,
      error: null,
      showDeployment: true
    }));
  };

  const handleDeploymentComplete = (deploymentUrl: string) => {
    console.log('✅ Deployment completed:', deploymentUrl);
  };

  const handleDeploymentError = (error: string) => {
    setState(prev => ({
      ...prev,
      error
    }));
  };

  // Determine what content to show
  const showContent = () => {
    if (state.loading) {
      return (
        <div className="py-12 text-center">
          <div className="flex items-center gap-3 text-gray-600 justify-center">
            <Loader2 className="w-6 h-6 animate-spin" />
            <span className="text-lg">AI is analyzing portfolios and selecting the best ones...</span>
          </div>
          <div className="mt-4 text-sm text-gray-500">
            This may take a few moments while we process the data
          </div>
        </div>
      );
    }

    if (state.techLoading) {
      return (
        <div className="py-12 text-center">
          <div className="flex items-center gap-3 text-gray-600 justify-center">
            <Loader2 className="w-6 h-6 animate-spin" />
            <span className="text-lg">Loading Tech News...</span>
          </div>
          <div className="mt-4 text-sm text-gray-500">
            Fetching the latest tech news summary
          </div>
        </div>
      );
    }



    if (state.portfolios.length > 0) {
      return (
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
      );
    }

    if (state.techNews?.summary) {
      return (
        <div className="bg-white rounded-lg shadow-sm overflow-hidden mt-8">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center gap-2">
            <Newspaper className="w-5 h-5 text-emerald-600" />
            <h2 className="text-xl font-semibold text-gray-900">Tech News Summary</h2>
            {/* no timestamp */}
          </div>
                      <TechNewsSummary summary={state.techNews.summary} />
        </div>
      );
    }

    if (state.showDeployment) {
      return (
        <div className="bg-white rounded-lg shadow-sm overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-gray-900">
              Deploy Your Portfolio
            </h2>
            <p className="text-gray-600 mt-1">
              Deploy your portfolio to Vercel with one click
            </p>
          </div>
          
          <div className="p-6">
            <PortfolioDeployment
              templateName="creative"
              environment="Production"
              onDeploymentComplete={handleDeploymentComplete}
              onError={handleDeploymentError}
            />
          </div>
        </div>
      );
    }

    // Default state - no button pressed yet
    return (
      <div className="py-12 text-center">
        <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mb-4 flex items-center justify-center mx-auto">
          <Brain className="w-8 h-8 text-gray-400" />
        </div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">Choose Your AI Experience</h3>
        <p className="text-gray-600 max-w-md mx-auto">
          Generate the best 10 portfolios from the site or get Tech News summary
        </p>
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50 pt-16">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Generate Button Section - Top Left */}
        <div className="p-6 bg-white border border-gray-300 rounded-lg shadow-sm flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-4">
          <Button
            onClick={handleGeneratePortfolios}
            disabled={state.loading}
            className="px-8 py-4 bg-blue-500 hover:bg-blue-600 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
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

          <Button
            onClick={handleShowTechNews}
            disabled={state.techLoading}
            className="px-8 py-4 bg-emerald-600 hover:bg-emerald-700 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
            size="lg"
          >
            {state.techLoading ? (
              <>
                <Loader2 className="w-6 h-6 animate-spin" />
                Loading Tech News...
              </>
            ) : (
              <>
                <Newspaper className="w-6 h-6" />
                Tech News
              </>
            )}
          </Button>

          <Button
            onClick={handleShowDeployment}
            className="px-8 py-4 bg-purple-600 hover:bg-purple-700 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
            size="lg"
          >
            Deploy Portfolio
          </Button>
        </div>

        {/* Content Area */}
        <div className="mt-12">
          {state.error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
              <div className="flex items-center gap-2 text-red-800">
                <AlertCircle className="w-5 h-5" />
                <span className="font-medium">Error</span>
              </div>
              <p className="text-red-700 mt-1">{state.error}</p>
            </div>
          )}
          
          {showContent()}
        </div>
      </div>
    </div>
  );
}

export default function AIPage() {
  return (
    <PremiumGuard>
      <AIPageContent />
    </PremiumGuard>
  );
} 