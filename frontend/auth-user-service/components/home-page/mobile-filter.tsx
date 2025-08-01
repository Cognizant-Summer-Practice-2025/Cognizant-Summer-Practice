import React from 'react';
import { FilterOutlined, DownOutlined } from '@ant-design/icons';
import { MobileFilterProps } from './filter-types';

const MobileFilter: React.FC<MobileFilterProps> = ({
  isOpen,
  selectedFiltersCount,
  onToggle,
  children
}) => {
  return (
    <div className="mobile-filter-container">
      <div 
        className={`mobile-filter-button ${isOpen ? 'open' : ''}`}
        onClick={onToggle}
      >
        <div className="mobile-filter-button-left">
          <FilterOutlined />
          <span>Filters</span>
          {selectedFiltersCount > 0 && (
            <span className="mobile-filter-badge">{selectedFiltersCount}</span>
          )}
        </div>
        <DownOutlined className="mobile-filter-button-arrow" />
      </div>
      
      <div className={`mobile-filter-content ${isOpen ? 'open' : ''}`}>
        {children}
      </div>
    </div>
  );
};

export default MobileFilter; 