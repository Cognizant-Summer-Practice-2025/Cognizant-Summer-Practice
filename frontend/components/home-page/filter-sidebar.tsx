'use client';

import React, { useState } from 'react';
import { Checkbox, Divider } from 'antd';
import { FilterOutlined, UserOutlined, CodeOutlined, DownOutlined } from '@ant-design/icons';
import type { CheckboxChangeEvent } from 'antd/es/checkbox';
import './style.css';

interface FilterOption {
  value: string;
  label: string;
  count: number;
}

const FilterSidebar: React.FC = () => {
  const [selectedFilters, setSelectedFilters] = useState<string[]>(['all-portfolios']);
  const [isMobileFilterOpen, setIsMobileFilterOpen] = useState(false);

  const portfolioFilters: FilterOption[] = [
    { value: 'all-portfolios', label: 'All Portfolios', count: 847 },
    { value: 'featured', label: 'Featured', count: 23 },
    { value: 'new-this-week', label: 'New This Week', count: 156 },
  ];

  const roleFilters: FilterOption[] = [
    { value: 'developer', label: 'Developer', count: 324 },
    { value: 'designer', label: 'Designer', count: 198 },
    { value: 'product-manager', label: 'Product Manager', count: 87 },
  ];

  const skillFilters: FilterOption[] = [
    { value: 'react', label: 'React', count: 156 },
    { value: 'nodejs', label: 'Node.js', count: 98 },
    { value: 'figma', label: 'Figma', count: 203 },
  ];

  const handleFilterChange = (value: string, checked: boolean) => {
    if (checked) {
      setSelectedFilters([...selectedFilters, value]);
    } else {
      setSelectedFilters(selectedFilters.filter(filter => filter !== value));
    }
  };

  const getSelectedFiltersCount = () => {
    // Don't count 'all-portfolios' as it's the default
    return selectedFilters.filter(filter => filter !== 'all-portfolios').length;
  };

  const toggleMobileFilter = () => {
    setIsMobileFilterOpen(!isMobileFilterOpen);
  };

  const renderFilterGroup = (
    title: string,
    icon: React.ReactNode,
    filters: FilterOption[],
    groupKey: string
  ) => (
    <div className="filter-group">
      <div className="filter-group-header">
        {icon}
        <h4>{title}</h4>
      </div>
      <div className="filter-options">
        {filters.map((filter) => (
          <div key={filter.value} className="filter-option">
            <Checkbox
              checked={selectedFilters.includes(filter.value)}
              onChange={(e: CheckboxChangeEvent) => 
                handleFilterChange(filter.value, e.target.checked)
              }
            >
              <span className="filter-label">{filter.label}</span>
            </Checkbox>
            <span className="filter-count">{filter.count}</span>
          </div>
        ))}
      </div>
    </div>
  );

  // The complete filter sidebar content
  const filterSidebarContent = (
    <div className="filter-sidebar-content">
      {renderFilterGroup(
        'Filters',
        <FilterOutlined />,
        portfolioFilters,
        'portfolios'
      )}
      
      <Divider className="filter-divider" />
      
      {renderFilterGroup(
        'Role',
        <UserOutlined />,
        roleFilters,
        'roles'
      )}
      
      <Divider className="filter-divider" />
      
      {renderFilterGroup(
        'Skills',
        <CodeOutlined />,
        skillFilters,
        'skills'
      )}
    </div>
  );

  return (
    <>
      {/* Desktop Sidebar */}
      <div className="filter-sidebar">
        {filterSidebarContent}
      </div>

      {/* Mobile Filter Container */}
      <div className="mobile-filter-container">
        <div 
          className={`mobile-filter-button ${isMobileFilterOpen ? 'open' : ''}`}
          onClick={toggleMobileFilter}
        >
          <div className="mobile-filter-button-left">
            <FilterOutlined />
            <span>Filters</span>
            {getSelectedFiltersCount() > 0 && (
              <span className="mobile-filter-badge">{getSelectedFiltersCount()}</span>
            )}
          </div>
          <DownOutlined className="mobile-filter-button-arrow" />
        </div>
        
        <div className={`mobile-filter-content ${isMobileFilterOpen ? 'open' : ''}`}>
          {filterSidebarContent}
        </div>
      </div>
    </>
  );
};

export default FilterSidebar; 