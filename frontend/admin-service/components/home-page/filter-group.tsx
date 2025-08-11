import React from 'react';
import { Checkbox } from 'antd';
import { DownOutlined } from '@ant-design/icons';
import type { CheckboxChangeEvent } from 'antd/es/checkbox';
import { FilterGroupProps } from './filter-types';

const FilterGroup: React.FC<FilterGroupProps> = ({
  title,
  icon,
  filters,
  groupKey,
  selectedFilters,
  collapsedSections,
  onFilterChange,
  onToggleSection
}) => {
  const isCollapsed = collapsedSections[groupKey];
  const selectedCount = filters.filter(filter => selectedFilters.includes(filter.value)).length;

  return (
    <div className="filter-group">
      <div className="filter-group-header">
        <div className="filter-group-header-left">
          {icon}
          <h4>{title}</h4>
          {selectedCount > 0 && (
            <span className="skills-count-badge">{selectedCount}</span>
          )}
        </div>
        <button
          className={`filter-group-toggle ${isCollapsed ? 'collapsed' : ''}`}
          onClick={() => onToggleSection(groupKey)}
        >
          <DownOutlined />
        </button>
      </div>
      
      <div className={`filter-options ${isCollapsed ? 'collapsed' : ''}`}>
        {filters.map((filter) => (
          <div key={filter.value} className="filter-option">
            <Checkbox
              checked={selectedFilters.includes(filter.value)}
              onChange={(e: CheckboxChangeEvent) => 
                onFilterChange(filter.value, e.target.checked)
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
};

export default FilterGroup; 