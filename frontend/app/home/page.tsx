'use client';

import React, { useEffect, useState } from 'react';
import Header from '@/components/header';
import FilterSidebar from '@/components/home-page/filter-sidebar';
import PortfolioGrid from '@/components/home-page/portfolio-grid';
import { HomePageCacheProvider, useHomePageCache } from '@/lib/contexts/home-page-cache-context';
import './style.css';

const HomePageContent: React.FC = () => {
  const { loadPage, setFilters, portfolios, selectedSkills, selectedRoles, featuredOnly, dateFrom } = useHomePageCache();
  const [activeFilters, setActiveFilters] = useState<string[]>(['all-portfolios']);

  // Load initial page on mount
  useEffect(() => {
    loadPage(1);
  }, [loadPage]);

  // Update active filters when context state changes
  useEffect(() => {
    const newActiveFilters: string[] = [];
    
    // Check if any filters are active
    const hasActiveFilters = selectedSkills.length > 0 || selectedRoles.length > 0 || featuredOnly || dateFrom;
    
    if (!hasActiveFilters) {
      newActiveFilters.push('all-portfolios');
    } else {
      if (featuredOnly) {
        newActiveFilters.push('featured');
      }
      
      if (dateFrom) {
        // Check if it's a "new this week" filter (approximately 7 days ago)
        const oneWeekAgo = new Date();
        oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
        const timeDiff = Math.abs(dateFrom.getTime() - oneWeekAgo.getTime());
        const daysDiff = Math.ceil(timeDiff / (1000 * 60 * 60 * 24));
        
        if (daysDiff <= 1) { // Within 1 day of the "one week ago" date
          newActiveFilters.push('new-this-week');
        }
      }
      
      // Add role filters
      selectedRoles.forEach(role => {
        newActiveFilters.push(role);
      });
      
      // Add skill filters (convert spaces back to dashes for display)
      selectedSkills.forEach(skill => {
        newActiveFilters.push(skill.replace(/\s+/g, '-').toLowerCase());
      });
    }
    
    setActiveFilters(newActiveFilters);
  }, [selectedSkills, selectedRoles, featuredOnly, dateFrom]);

  const handleFiltersChange = (filters: string[]) => {
    // Convert filter array to filter object
    const filterObj: {
      featured?: boolean;
      roles?: string[];
      skills?: string[];
      dateFrom?: Date;
      dateTo?: Date;
    } = {};
    
    // If only 'all-portfolios' is selected, clear all filters
    if (filters.length === 1 && filters[0] === 'all-portfolios') {
      setFilters({
        featured: false,
        roles: [],
        skills: [],
        dateFrom: undefined,
        dateTo: undefined
      });
      return;
    }
    
    // Remove 'all-portfolios' from filters array if other filters are present
    const actualFilters = filters.filter(f => f !== 'all-portfolios');
    
    if (actualFilters.includes('featured')) {
      filterObj.featured = true;
    }
    
    if (actualFilters.includes('new-this-week')) {
      const oneWeekAgo = new Date();
      oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
      filterObj.dateFrom = oneWeekAgo;
    }
    
    // Handle role filters
    const roleFilters = actualFilters.filter(filter => 
      ['developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
    );
    if (roleFilters.length > 0) {
      filterObj.roles = roleFilters;
    }
    
    // Handle skill filters (remaining filters that aren't roles or special filters)
    const skillFilters = actualFilters.filter(filter => 
      !['featured', 'new-this-week', 'developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
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
              activeFilters={activeFilters}
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