import React, { useState, useMemo } from 'react';
import { Checkbox } from 'antd';
import { CodeOutlined, DownOutlined } from '@ant-design/icons';
import type { CheckboxChangeEvent } from 'antd/es/checkbox';
import { SkillsFilterProps } from './filter-types';
import { getTotalSkillsCount } from './filter-utils';
import ModernSearch from './modern-search';

const SkillsFilter: React.FC<SkillsFilterProps> = ({
  skillCategories,
  selectedFilters,
  collapsedSections,
  onFilterChange,
  onToggleSection
}) => {
  const [skillsSearchTerm, setSkillsSearchTerm] = useState('');

  // Filter skills based on search term (across all categories)
  const filteredSkillCategories = useMemo(() => {
    if (!skillsSearchTerm) return skillCategories;
    
    return skillCategories.map(category => ({
      ...category,
      subcategories: category.subcategories.map(subcategory => ({
        ...subcategory,
        skills: subcategory.skills.filter(skill =>
          skill.label.toLowerCase().includes(skillsSearchTerm.toLowerCase())
        )
      })).filter(subcategory => subcategory.skills.length > 0)
    })).filter(category => category.subcategories.length > 0);
  }, [skillCategories, skillsSearchTerm]);

  const totalSkillsCount = getTotalSkillsCount(filteredSkillCategories);
  const isMainCollapsed = collapsedSections['skills-main'];
  
  // Count selected skills across all categories
  const selectedSkillsCount = filteredSkillCategories.reduce((total, category) =>
    total + category.subcategories.reduce((subTotal, subcategory) =>
      subTotal + subcategory.skills.filter(skill => selectedFilters.includes(skill.value)).length, 0
    ), 0
  );

  return (
    <div className="filter-group skills-filter">
      <div className="filter-group-header">
        <div className="filter-group-header-left">
          <CodeOutlined />
          <h4>Skills ({totalSkillsCount})</h4>
          {selectedSkillsCount > 0 && (
            <span className="skills-count-badge">{selectedSkillsCount}</span>
          )}
        </div>
        <button
          className={`filter-group-toggle ${isMainCollapsed ? 'collapsed' : ''}`}
          onClick={() => onToggleSection('skills-main')}
        >
          <DownOutlined />
        </button>
      </div>
      
      <ModernSearch
        value={skillsSearchTerm}
        onChange={setSkillsSearchTerm}
        placeholder="Search skills..."
        isCollapsed={isMainCollapsed}
      />
      
      <div className={`skills-categories ${isMainCollapsed ? 'collapsed' : ''}`}>
        {filteredSkillCategories.map((category) => {
          const isCategoryCollapsed = collapsedSections[`category-${category.id}`];
          
          return (
            <div key={category.id} className={`skill-category ${category.type}-skills`}>
              <div 
                className="skill-category-header"
                onClick={() => onToggleSection(`category-${category.id}`)}
              >
                <div className="skill-category-header-left">
                  <div className={`skill-category-icon ${category.type}`}>
                    {category.type === 'hard' ? '⚙️' : '🧠'}
                  </div>
                  <h5 className="skill-category-title">{category.label}</h5>
                </div>
                <button
                  className={`skill-category-toggle ${isCategoryCollapsed ? 'collapsed' : ''}`}
                  type="button"
                >
                  <DownOutlined />
                </button>
              </div>
              
              <div className={`skill-subcategories ${isCategoryCollapsed ? 'collapsed' : ''}`}>
                {category.subcategories.map((subcategory) => {
                  const isSubcategoryCollapsed = collapsedSections[`subcategory-${subcategory.id}`];
                  
                  return (
                    <div key={subcategory.id} className="skill-subcategory">
                      <div 
                        className="skill-subcategory-header"
                        onClick={() => onToggleSection(`subcategory-${subcategory.id}`)}
                      >
                        <div className="skill-subcategory-header-left">
                          <h6 className="skill-subcategory-title">{subcategory.label}</h6>
                          <span className="skill-subcategory-count">({subcategory.skills.length})</span>
                        </div>
                        <button
                          className={`skill-subcategory-toggle ${isSubcategoryCollapsed ? 'collapsed' : ''}`}
                          type="button"
                        >
                          <DownOutlined />
                        </button>
                      </div>
                      
                      <div className={`skill-subcategory-options ${isSubcategoryCollapsed ? 'collapsed' : ''}`}>
                        {subcategory.skills.map((skill) => (
                          <div key={skill.value} className="filter-option skill-option">
                            <Checkbox
                              checked={selectedFilters.includes(skill.value)}
                              onChange={(e: CheckboxChangeEvent) => 
                                onFilterChange(skill.value, e.target.checked)
                              }
                            >
                              <span className="filter-label">{skill.label}</span>
                            </Checkbox>
                            <span className="filter-count">{skill.count}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default SkillsFilter; 