import React from 'react';
import { CloseOutlined } from '@ant-design/icons';
import { SelectedFiltersProps } from './filter-types';

const SelectedFilters: React.FC<SelectedFiltersProps> = ({
  selectedFilters,
  onRemoveFilter,
  onClearAll,
  getFilterLabel
}) => {
  // Filter out 'all-portfolios' as it's the default state
  const displaySelectedFilters = selectedFilters.filter(filter => filter !== 'all-portfolios');

  if (displaySelectedFilters.length === 0) {
    return null;
  }

  return (
    <div className="selected-filters-summary">
      <div className="selected-filters-title">
        Active Filters ({displaySelectedFilters.length})
      </div>
      <div className="selected-filters-tags">
        {displaySelectedFilters.map((filterValue) => (
          <div key={filterValue} className="selected-filter-tag">
            {getFilterLabel(filterValue)}
            <button
              className="remove-filter"
              onClick={() => onRemoveFilter(filterValue)}
            >
              <CloseOutlined />
            </button>
          </div>
        ))}
      </div>
      <button className="clear-all-filters" onClick={onClearAll}>
        Clear All
      </button>
    </div>
  );
};

export default SelectedFilters;
