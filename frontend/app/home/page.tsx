'use client';

import React, { useState, useEffect } from 'react';
import Header from '@/components/header';
import FilterSidebar from '@/components/home-page/filter-sidebar';
import PortfolioGrid from '@/components/home-page/portfolio-grid';
import { getPortfolioCardsForHomePage, PortfolioCardDto } from '@/lib/portfolio/api';
import './style.css';

const HomePage: React.FC = () => {
  const [portfolios, setPortfolios] = useState<PortfolioCardDto[]>([]);
  const [filteredPortfolios, setFilteredPortfolios] = useState<PortfolioCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeFilters, setActiveFilters] = useState<string[]>(['all-portfolios']);

  useEffect(() => {
    const fetchPortfolios = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getPortfolioCardsForHomePage();
        setPortfolios(data);
        setFilteredPortfolios(data); // Initially show all portfolios
      } catch (err) {
        console.error('Error fetching portfolios:', err);
        setError('Failed to load portfolios. Please try again later.');
        setPortfolios([]);
        setFilteredPortfolios([]);
      } finally {
        setLoading(false);
      }
    };

    fetchPortfolios();
  }, []);

  // Filter portfolios based on active filters
  useEffect(() => {
    const filterPortfolios = () => {
      if (activeFilters.includes('all-portfolios') || activeFilters.length === 0) {
        setFilteredPortfolios(portfolios);
        return;
      }

      const filtered = portfolios.filter(portfolio => {
        // Check portfolio type filters
        if (activeFilters.includes('featured') && !portfolio.featured) {
          return false;
        }

        if (activeFilters.includes('new-this-week')) {
          const oneWeekAgo = new Date();
          oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
          const portfolioDate = new Date(portfolio.date);
          if (portfolioDate < oneWeekAgo) {
            return false;
          }
        }

        // Check role filters
        const roleFilters = activeFilters.filter(filter => 
          ['developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
        );
        
        if (roleFilters.length > 0) {
          const portfolioRole = portfolio.role.toLowerCase();
          const matchesRole = roleFilters.some(roleFilter => {
            switch (roleFilter) {
              case 'developer':
                return portfolioRole.includes('developer') || portfolioRole.includes('dev');
              case 'designer':
                return portfolioRole.includes('designer') || portfolioRole.includes('design');
              case 'product-manager':
                return portfolioRole.includes('product manager') || portfolioRole.includes('pm') || portfolioRole.includes('project manager');
              case 'engineer':
                return portfolioRole.includes('engineer');
              case 'analyst':
                return portfolioRole.includes('analyst');
              default:
                return portfolioRole.includes(roleFilter);
            }
          });
          
          if (!matchesRole) {
            return false;
          }
        }

        // Check skill filters
        const skillFilters = activeFilters.filter(filter => 
          !['all-portfolios', 'featured', 'new-this-week', 'developer', 'designer', 'product-manager', 'engineer', 'analyst'].includes(filter)
        );

        if (skillFilters.length > 0) {
          const portfolioSkills = portfolio.skills.map(skill => skill.toLowerCase().trim());
          
          const matchesSkills = skillFilters.every(skillFilter => {
            // Convert filter value back to skill name for matching
            const skillName = skillFilter.replace(/-/g, ' ').toLowerCase();
            
            return portfolioSkills.some(portfolioSkill => {
              // Direct match
              if (portfolioSkill === skillName) return true;
              
              // Partial match
              if (portfolioSkill.includes(skillName) || skillName.includes(portfolioSkill)) return true;
              
              // Handle common variations
              if ((skillName === 'javascript' && portfolioSkill === 'js') ||
                  (skillName === 'js' && portfolioSkill === 'javascript') ||
                  (skillName === 'react.js' && portfolioSkill === 'react') ||
                  (skillName === 'react' && portfolioSkill === 'react.js') ||
                  (skillName === 'node.js' && portfolioSkill === 'nodejs') ||
                  (skillName === 'nodejs' && portfolioSkill === 'node.js')) {
                return true;
              }
              
              return false;
            });
          });

          if (!matchesSkills) {
            return false;
          }
        }

        return true;
      });

      setFilteredPortfolios(filtered);
    };

    filterPortfolios();
  }, [portfolios, activeFilters]);

  const handleFiltersChange = (filters: string[]) => {
    setActiveFilters(filters);
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
            <PortfolioGrid 
              portfolios={filteredPortfolios}
              loading={loading}
              error={error}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage; 