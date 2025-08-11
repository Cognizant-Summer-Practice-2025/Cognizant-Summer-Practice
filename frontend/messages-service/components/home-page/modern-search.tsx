import React from 'react';
import { SearchOutlined, CloseOutlined } from '@ant-design/icons';

// Simple cn utility fallback if needed
const cn = (...classes: (string | undefined | false)[]) => {
  return classes.filter(Boolean).join(' ');
};

interface ModernSearchProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
  isCollapsed?: boolean;
}

const ModernSearch: React.FC<ModernSearchProps> = ({
  value,
  onChange,
  placeholder = "Search skills...",
  className,
  isCollapsed = false
}) => {
  const handleClear = () => {
    onChange('');
  };

  return (
    <div className={cn(
      "skills-search-modern",
      isCollapsed && "collapsed",
      className
    )}>
      <div className="search-container">
        <div className="search-input-wrapper">
          <SearchOutlined className="search-icon" />
          <input
            type="text"
            value={value}
            onChange={(e) => onChange(e.target.value)}
            placeholder={placeholder}
            className="search-input"
          />
          {value && (
            <button
              onClick={handleClear}
              className="clear-button"
              type="button"
            >
              <CloseOutlined className="clear-icon" />
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ModernSearch; 