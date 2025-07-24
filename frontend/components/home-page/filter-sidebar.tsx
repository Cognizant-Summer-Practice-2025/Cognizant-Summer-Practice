'use client';

import React, { useState, useEffect } from 'react';
import { Divider } from 'antd';
import { FilterOutlined, UserOutlined } from '@ant-design/icons';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import FilterGroup from './filter-group';
import SelectedFilters from './selected-filters';
import SkillsFilter from './skills-filter';
import MobileFilter from './mobile-filter';
import { FilterSidebarProps, SkillCategory } from './filter-types';
import { 
  calculatePortfolioFilters, 
  calculateRoleFilters, 
  generateCategorizedSkillFilters 
} from './filter-utils';
import './style.css';

const FilterSidebar: React.FC<FilterSidebarProps> = ({ portfolios, onFiltersChange, activeFilters }) => {
  const [selectedFilters, setSelectedFilters] = useState<string[]>(activeFilters);
  const [isMobileFilterOpen, setIsMobileFilterOpen] = useState(false);
  const [collapsedSections, setCollapsedSections] = useState<Record<string, boolean>>({});

  useEffect(() => {
    setSelectedFilters(activeFilters);
  }, [activeFilters]);

  // Calculate filter options
  const portfolioFilters = calculatePortfolioFilters(portfolios);
  const roleFilters = calculateRoleFilters(portfolios);
  const skillCategories: SkillCategory[] = generateCategorizedSkillFilters(portfolios);

  // Event handlers

  const handleFilterChange = (value: string, checked: boolean) => {
    let newFilters: string[];
    
    if (checked) {
      // When adding a filter, remove 'all-portfolios' if it exists
      newFilters = selectedFilters.includes('all-portfolios') 
        ? [value] 
        : [...selectedFilters, value];
    } else {
      newFilters = selectedFilters.filter(filter => filter !== value);
      
      // If no filters left, add 'all-portfolios' back
      if (newFilters.length === 0) {
        newFilters = ['all-portfolios'];
      }
    }
    
    setSelectedFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const toggleSection = (sectionKey: string) => {
    setCollapsedSections(prev => ({
      ...prev,
      [sectionKey]: !prev[sectionKey]
    }));
  };

  const clearAllFilters = () => {
    const newFilters = ['all-portfolios'];
    setSelectedFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const removeFilter = (filterValue: string) => {
    let newFilters = selectedFilters.filter(filter => filter !== filterValue);
    
    // If no filters left, add 'all-portfolios' back
    if (newFilters.length === 0) {
      newFilters = ['all-portfolios'];
    }
    
    setSelectedFilters(newFilters);
    onFiltersChange(newFilters);
  };

  const toggleMobileFilter = () => {
    setIsMobileFilterOpen(!isMobileFilterOpen);
  };

  // Helper function to get filter labels for display
  const getFilterLabel = (filterValue: string) => {
    // Check portfolio filters
    const portfolioFilter = portfolioFilters.find(f => f.value === filterValue);
    if (portfolioFilter) return portfolioFilter.label;
    
    // Check role filters
    const roleFilter = roleFilters.find(f => f.value === filterValue);
    if (roleFilter) return roleFilter.label;
    
    // Check skill filters across all categories
    for (const category of skillCategories) {
      for (const subcategory of category.subcategories) {
        const skillFilter = subcategory.skills.find(f => f.value === filterValue);
        if (skillFilter) return skillFilter.label;
      }
    }
    
    return filterValue;
  };

  const getSelectedFiltersCount = () => {
    // Don't count 'all-portfolios' as it's the default
    return selectedFilters.filter(filter => filter !== 'all-portfolios').length;
  };

  // The complete filter sidebar content
  const filterSidebarContent = (
    <div className="filter-sidebar-content">
      <SelectedFilters
        selectedFilters={selectedFilters}
        onRemoveFilter={removeFilter}
        onClearAll={clearAllFilters}
        getFilterLabel={getFilterLabel}
      />

      <FilterGroup
        title="Filters"
        icon={<FilterOutlined />}
        filters={portfolioFilters}
        groupKey="portfolios"
        selectedFilters={selectedFilters}
        collapsedSections={collapsedSections}
        onFilterChange={handleFilterChange}
        onToggleSection={toggleSection}
      />
      
      <Divider className="filter-divider" />
      
      <FilterGroup
        title="Role"
        icon={<UserOutlined />}
        filters={roleFilters}
        groupKey="roles"
        selectedFilters={selectedFilters}
        collapsedSections={collapsedSections}
        onFilterChange={handleFilterChange}
        onToggleSection={toggleSection}
      />
      
      <Divider className="filter-divider" />
      
      <SkillsFilter
        skillCategories={skillCategories}
        selectedFilters={selectedFilters}
        collapsedSections={collapsedSections}
        onFilterChange={handleFilterChange}
        onToggleSection={toggleSection}
      />
    </div>
  );

  return (
    <>
      {/* Desktop Sidebar */}
      <div className="filter-sidebar">
        {filterSidebarContent}
      </div>

      {/* Mobile Filter Container */}
      <MobileFilter
        isOpen={isMobileFilterOpen}
        selectedFiltersCount={getSelectedFiltersCount()}
        onToggle={toggleMobileFilter}
      >
          {filterSidebarContent}
      </MobileFilter>
    </>
  );
};

export default FilterSidebar; 