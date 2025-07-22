'use client';

import React, { useEffect } from 'react';
import Header from '@/components/header';
import FilterSidebar from '@/components/home-page/filter-sidebar';
import PortfolioGrid from '@/components/home-page/portfolio-grid';
import { HomePageCacheProvider, useHomePageCache } from '@/lib/contexts/home-page-cache-context';
import './style.css';

const HomePageContent: React.FC = () => {
  const { loadPage, setFilters, portfolios } = useHomePageCache();

  // Load initial page on mount
  useEffect(() => {
    loadPage(1);
  }, [loadPage]);

  const handleFiltersChange = (filters: string[]) => {
    // Convert filter array to filter object
    const filterObj: {
      featured?: boolean;
      roles?: string[];
      skills?: string[];
    } = {};
    
    if (filters.includes('featured')) {
      filterObj.featured = true;
    }
    
    // Handle role filters
    const roleFilters = filters.filter(filter => 
      ['developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
    );
    if (roleFilters.length > 0) {
      filterObj.roles = roleFilters;
    }
    
    // Handle skill filters (remaining filters that aren't roles or special filters)
    const skillFilters = filters.filter(filter => 
      !['all-portfolios', 'featured', 'new-this-week', 'developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
    );
    if (skillFilters.length > 0) {
      filterObj.skills = skillFilters.map(skill => skill.replace(/-/g, ' '));
    }
    
    setFilters(filterObj);
  };

  return (
    <div className="home-page">
      <Header />
      <div className="home-main">
        <div className="home-container">
          <div className="home-content">
            <FilterSidebar 
              portfolios={portfolios} 
              onFiltersChange={handleFiltersChange}
              activeFilters={['all-portfolios']}
            />
            <PortfolioGrid />
          </div>
        </div>
      </div>
    </div>
  );
};

const HomePage: React.FC = () => {
  return (
    <HomePageCacheProvider>
      <HomePageContent />
    </HomePageCacheProvider>
  );
};

export default HomePage; 