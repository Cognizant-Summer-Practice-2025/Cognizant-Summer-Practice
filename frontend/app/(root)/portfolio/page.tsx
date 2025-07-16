"use client";

import React, { useState, useEffect, Suspense } from 'react';
import { useParams, useSearchParams } from 'next/navigation';
import { loadTemplateComponent, getDefaultTemplate } from '@/lib/templates';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { createMockPortfolioData } from '@/lib/template-manager';

// Loading component
function TemplateLoader() {
  return (
    <div className="portfolio-loading">
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading portfolio...</p>
      </div>
    </div>
  );
}

const PortfolioPage = () => {
  const [TemplateComponent, setTemplateComponent] = useState<React.ComponentType<any> | null>(null);
  const [portfolioData, setPortfolioData] = useState<PortfolioDataFromDB | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Get user ID from URL params or search params (for future use)
  const searchParams = useSearchParams();
  const userId = searchParams.get('user') || 'default-user'; // Default for demo

  useEffect(() => {
    async function loadUserPortfolio() {
      try {
        setLoading(true);
        
        // TODO: Replace with actual API call to get user's portfolio
        // const response = await fetch(`/api/portfolio/${userId}`);
        // const portfolioData = await response.json();
        
        // For now, use mock data
        const mockData = createMockPortfolioData(userId);
        setPortfolioData(mockData);
        
        // Get template ID from portfolio configuration
        const templateId = mockData.portfolio.templateId;
        
        // Load the template component
        const templateModule = await loadTemplateComponent(templateId);
        setTemplateComponent(() => templateModule.default);
        
      } catch (err) {
        console.error('Error loading portfolio:', err);
        setError('Failed to load portfolio. Please try again.');
      } finally {
        setLoading(false);
      }
    }

    loadUserPortfolio();
  }, [userId]);

  if (loading) {
    return <TemplateLoader />;
  }

  if (error) {
    return (
      <div className="portfolio-error">
        <div className="error-container">
          <h2>Oops! Something went wrong</h2>
          <p>{error}</p>
          <button onClick={() => window.location.reload()}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  if (!TemplateComponent || !portfolioData) {
    return (
      <div className="portfolio-error">
        <div className="error-container">
          <h2>Portfolio Not Found</h2>
          <p>Unable to load the portfolio data or template.</p>
        </div>
      </div>
    );
  }

  return (
    <Suspense fallback={<TemplateLoader />}>
      <TemplateComponent data={portfolioData} />
    </Suspense>
  );
};

export default PortfolioPage;