'use client';

import React, { useState, useMemo, useEffect } from 'react';
import { Checkbox, Divider, Input } from 'antd';
import { FilterOutlined, UserOutlined, CodeOutlined, DownOutlined, SearchOutlined, CloseOutlined } from '@ant-design/icons';
import type { CheckboxChangeEvent } from 'antd/es/checkbox';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import skillsConfig from '../../lib/skills-config.json';
import './style.css';

interface FilterOption {
  value: string;
  label: string;
  count: number;
}

interface FilterSidebarProps {
  portfolios: PortfolioCardDto[];
  onFiltersChange: (filters: string[]) => void;
  activeFilters: string[];
}

const FilterSidebar: React.FC<FilterSidebarProps> = ({ portfolios, onFiltersChange, activeFilters }) => {
  const [selectedFilters, setSelectedFilters] = useState<string[]>(activeFilters);
  const [isMobileFilterOpen, setIsMobileFilterOpen] = useState(false);
  const [skillsSearchTerm, setSkillsSearchTerm] = useState('');
  const [collapsedSections, setCollapsedSections] = useState<Record<string, boolean>>({});

  useEffect(() => {
    setSelectedFilters(activeFilters);
  }, [activeFilters]);

  const calculatePortfolioFilters = (): FilterOption[] => {
    const totalCount = portfolios.length;
    const featuredCount = portfolios.filter(p => p.featured).length;
    const oneWeekAgo = new Date();
    oneWeekAgo.setDate(oneWeekAgo.getDate() - 7);
    const newThisWeekCount = portfolios.filter(p => {
      const portfolioDate = new Date(p.date);
      return portfolioDate >= oneWeekAgo;
    }).length;

    return [
      { value: 'all-portfolios', label: 'All Portfolios', count: totalCount },
      { value: 'featured', label: 'Featured', count: featuredCount },
      { value: 'new-this-week', label: 'New This Week', count: newThisWeekCount },
    ];
  };

  // Calculate role filter counts from actual data
  const calculateRoleFilters = (): FilterOption[] => {
    const roleCounts = new Map<string, number>();
    
    portfolios.forEach(portfolio => {
      const role = portfolio.role.toLowerCase().trim();
      roleCounts.set(role, (roleCounts.get(role) || 0) + 1);
    });

    const roleFilters: FilterOption[] = [];
    
    // Add predefined common roles if they exist in data
    const commonRoles = [
      { key: 'developer', labels: ['developer', 'software developer', 'web developer', 'frontend developer', 'backend developer', 'full stack developer'] },
      { key: 'designer', labels: ['designer', 'ui designer', 'ux designer', 'graphic designer', 'web designer', 'product designer'] },
      { key: 'product-manager', labels: ['product manager', 'pm', 'project manager'] },
      { key: 'engineer', labels: ['engineer', 'software engineer', 'data engineer', 'devops engineer'] },
      { key: 'analyst', labels: ['analyst', 'data analyst', 'business analyst'] },
    ];

    commonRoles.forEach(roleGroup => {
      let count = 0;
      roleGroup.labels.forEach(label => {
        count += roleCounts.get(label) || 0;
      });
      
      if (count > 0) {
        roleFilters.push({
          value: roleGroup.key,
          label: roleGroup.key.split('-').map(word => 
            word.charAt(0).toUpperCase() + word.slice(1)
          ).join(' '),
          count: count
        });
      }
    });

    roleCounts.forEach((count, role) => {
      const isAlreadyIncluded = commonRoles.some(roleGroup => 
        roleGroup.labels.includes(role)
      );
      
      if (!isAlreadyIncluded && count > 0) {
        const displayLabel = role.split(' ').map(word => 
          word.charAt(0).toUpperCase() + word.slice(1)
        ).join(' ');
        
        roleFilters.push({
          value: role.replace(/[^a-z0-9]/g, '-'),
          label: displayLabel,
          count: count
        });
      }
    });

    return roleFilters.sort((a, b) => b.count - a.count);
  };

  const portfolioFilters: FilterOption[] = calculatePortfolioFilters();
  const roleFilters: FilterOption[] = calculateRoleFilters();

  const calculateSkillCounts = (): Map<string, number> => {
    const skillCounts = new Map<string, number>();
    
    portfolios.forEach(portfolio => {
      portfolio.skills.forEach(skill => {
        const normalizedSkill = skill.toLowerCase().trim();
        skillCounts.set(normalizedSkill, (skillCounts.get(normalizedSkill) || 0) + 1);
      });
    });
    
    return skillCounts;
  };

  // Enhanced skill filter structure with categories
  interface SkillCategory {
    id: string;
    label: string;
    type: 'hard' | 'soft';
    subcategories: SkillSubcategory[];
  }

  interface SkillSubcategory {
    id: string;
    label: string;
    skills: FilterOption[];
  }

  // Generate categorized skill filters from skills-config.json with real counts
  const generateCategorizedSkillFilters = (): SkillCategory[] => {
    const skillCounts = calculateSkillCounts();
    const categories: SkillCategory[] = [];
    
    // Get all skills from both hard and soft skills categories
    const { hard_skills, soft_skills } = skillsConfig.skillCategories;
    
    // Function to normalize skill names for matching
    const normalizeSkillName = (skill: string) => skill.toLowerCase().trim();
    
    // Function to find count for a skill (case-insensitive matching)
    const getSkillCount = (skill: string): number => {
      const normalizedSkill = normalizeSkillName(skill);
      
      // Direct match
      if (skillCounts.has(normalizedSkill)) {
        return skillCounts.get(normalizedSkill)!;
      }
      
      let totalCount = 0;
      
      // Try to find partial matches for common variations
      for (const [portfolioSkill, count] of skillCounts.entries()) {
        let isMatch = false;
        
        // Handle common variations (e.g., "JavaScript" vs "JS")
        const commonVariations = new Map([
          ['javascript', ['js', 'javascript', 'java script']],
          ['js', ['javascript', 'js', 'java script']],
          ['react.js', ['react', 'reactjs', 'react.js', 'react js']],
          ['react', ['react', 'reactjs', 'react.js', 'react js']],
          ['reactjs', ['react', 'reactjs', 'react.js', 'react js']],
          ['node.js', ['nodejs', 'node.js', 'node js', 'node']],
          ['nodejs', ['nodejs', 'node.js', 'node js', 'node']],
          ['node', ['nodejs', 'node.js', 'node js', 'node']],
          ['vue.js', ['vue', 'vuejs', 'vue.js', 'vue js']],
          ['vue', ['vue', 'vuejs', 'vue.js', 'vue js']],
          ['vuejs', ['vue', 'vuejs', 'vue.js', 'vue js']],
          ['angular.js', ['angular', 'angularjs', 'angular.js', 'angular js']],
          ['angular', ['angular', 'angularjs', 'angular.js', 'angular js']],
          ['angularjs', ['angular', 'angularjs', 'angular.js', 'angular js']],
          ['c++', ['cpp', 'c++', 'c plus plus']],
          ['cpp', ['cpp', 'c++', 'c plus plus']],
          ['c#', ['csharp', 'c#', 'c sharp']],
          ['csharp', ['csharp', 'c#', 'c sharp']],
          ['python', ['python', 'py']],
          ['py', ['python', 'py']],
          ['typescript', ['typescript', 'ts']],
          ['ts', ['typescript', 'ts']],
          ['html5', ['html', 'html5']],
          ['html', ['html', 'html5']],
          ['css3', ['css', 'css3']],
          ['css', ['css', 'css3']],
        ]);
        
        const variations = commonVariations.get(normalizedSkill);
        if (variations && variations.includes(portfolioSkill)) {
          isMatch = true;
        }
        
        // Check if the config skill is contained in the portfolio skill or vice versa
        // But only for longer skill names to avoid false matches
        if (!isMatch && normalizedSkill.length > 2 && portfolioSkill.length > 2) {
          if (portfolioSkill.includes(normalizedSkill) || normalizedSkill.includes(portfolioSkill)) {
            isMatch = true;
          }
        }
        
        if (isMatch) {
          totalCount += count;
        }
      }
      
      return totalCount;
    };
    
    // Process hard skills
    const hardSkillsCategory: SkillCategory = {
      id: 'hard-skills',
      label: 'Hard Skills',
      type: 'hard',
      subcategories: []
    };

    Object.entries(hard_skills.subcategories).forEach(([subKey, subcategory]) => {
      const skillsInSubcategory: FilterOption[] = subcategory.skills.map(skill => ({
        value: skill.toLowerCase().replace(/[^a-z0-9]/g, '-'),
        label: skill,
        count: getSkillCount(skill)
      })).filter(skill => skill.count > 0) // Only show skills that exist in portfolios
      .sort((a, b) => {
        if (a.count !== b.count) {
          return b.count - a.count; // Higher counts first
        }
        return a.label.localeCompare(b.label); // Then alphabetical
      });

      // Only add subcategory if it has skills that exist in portfolios
      if (skillsInSubcategory.length > 0) {
        hardSkillsCategory.subcategories.push({
          id: `hard-${subKey}`,
          label: subcategory.label,
          skills: skillsInSubcategory
        });
      }
    });

    // Only add hard skills category if it has subcategories with skills
    if (hardSkillsCategory.subcategories.length > 0) {
      categories.push(hardSkillsCategory);
    }
    
    // Process soft skills
    const softSkillsCategory: SkillCategory = {
      id: 'soft-skills',
      label: 'Soft Skills',
      type: 'soft',
      subcategories: []
    };

    Object.entries(soft_skills.subcategories).forEach(([subKey, subcategory]) => {
      const skillsInSubcategory: FilterOption[] = subcategory.skills.map(skill => ({
        value: skill.toLowerCase().replace(/[^a-z0-9]/g, '-'),
        label: skill,
        count: getSkillCount(skill)
      })).filter(skill => skill.count > 0) // Only show skills that exist in portfolios
      .sort((a, b) => {
        if (a.count !== b.count) {
          return b.count - a.count; 
        }
        return a.label.localeCompare(b.label);
      });

      // Only add subcategory if it has skills that exist in portfolios
      if (skillsInSubcategory.length > 0) {
        softSkillsCategory.subcategories.push({
          id: `soft-${subKey}`,
          label: subcategory.label,
          skills: skillsInSubcategory
        });
      }
    });

    // Only add soft skills category if it has subcategories with skills
    if (softSkillsCategory.subcategories.length > 0) {
      categories.push(softSkillsCategory);
    }
    
    // Handle uncategorized skills (portfolio skills not in config)
    const uncategorizedSkills: FilterOption[] = [];
    skillCounts.forEach((count, portfolioSkill) => {
      const normalizedPortfolioSkill = normalizeSkillName(portfolioSkill);
      
      const isAlreadyInConfig = categories.some(category =>
        category.subcategories.some(subcategory =>
          subcategory.skills.some(skill =>
            normalizeSkillName(skill.label) === normalizedPortfolioSkill
          )
        )
      );
      
      if (!isAlreadyInConfig) {
        // Capitalize first letter for display
        const displayLabel = portfolioSkill.charAt(0).toUpperCase() + portfolioSkill.slice(1);
        uncategorizedSkills.push({
          value: portfolioSkill.replace(/[^a-z0-9]/g, '-'),
          label: displayLabel,
          count: count
        });
      }
    });

    // Add uncategorized skills as a separate category if any exist
    if (uncategorizedSkills.length > 0) {
      uncategorizedSkills.sort((a, b) => {
        if (a.count !== b.count) {
          return b.count - a.count;
        }
        return a.label.localeCompare(b.label);
      });

      categories.push({
        id: 'other-skills',
        label: 'Other Skills',
        type: 'hard', 
        subcategories: [{
          id: 'uncategorized',
          label: 'Uncategorized',
          skills: uncategorizedSkills
        }]
      });
    }
    
    return categories;
  };

  const skillCategories: SkillCategory[] = generateCategorizedSkillFilters();

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

  // Get total count of skills for display
  const getTotalSkillsCount = (categories: SkillCategory[]): number => {
    return categories.reduce((total, category) =>
      total + category.subcategories.reduce((subTotal, subcategory) =>
        subTotal + subcategory.skills.length, 0
      ), 0
    );
  };

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
    groupKey: string,
    isSkillsGroup: boolean = false
  ) => {
    const isCollapsed = collapsedSections[groupKey];
    const selectedCount = filters.filter(filter => selectedFilters.includes(filter.value)).length;

    return (
      <div className={`filter-group ${isSkillsGroup ? 'skills-filter' : ''}`}>
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
            onClick={() => toggleSection(groupKey)}
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
  };

  const renderCategorizedSkillsGroup = () => {
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
            onClick={() => toggleSection('skills-main')}
          >
            <DownOutlined />
          </button>
        </div>
        
        {!isMainCollapsed && (
          <div className="skills-search">
            <Input
              placeholder="Search skills..."
              prefix={<SearchOutlined />}
              value={skillsSearchTerm}
              onChange={(e) => setSkillsSearchTerm(e.target.value)}
              allowClear
            />
          </div>
        )}
        
        <div className={`skills-categories ${isMainCollapsed ? 'collapsed' : ''}`}>
          {filteredSkillCategories.map((category) => {
            const isCategoryCollapsed = collapsedSections[`category-${category.id}`];
            
            return (
              <div key={category.id} className={`skill-category ${category.type}-skills`}>
                <div 
                  className="skill-category-header"
                  onClick={() => toggleSection(`category-${category.id}`)}
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
                          onClick={() => toggleSection(`subcategory-${subcategory.id}`)}
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
                                  handleFilterChange(skill.value, e.target.checked)
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

  // Get selected filters for display (excluding 'all-portfolios')
  const displaySelectedFilters = selectedFilters.filter(filter => filter !== 'all-portfolios');
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

  // The complete filter sidebar content
  const filterSidebarContent = (
    <div className="filter-sidebar-content">
      {displaySelectedFilters.length > 0 && (
        <div className="selected-filters-summary">
          <div className="selected-filters-title">Active Filters ({displaySelectedFilters.length})</div>
          <div className="selected-filters-tags">
            {displaySelectedFilters.map((filterValue) => (
              <div key={filterValue} className="selected-filter-tag">
                {getFilterLabel(filterValue)}
                <button
                  className="remove-filter"
                  onClick={() => removeFilter(filterValue)}
                >
                  <CloseOutlined />
                </button>
              </div>
            ))}
          </div>
          <button className="clear-all-filters" onClick={clearAllFilters}>
            Clear All
          </button>
        </div>
      )}

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
      
      {renderCategorizedSkillsGroup()}
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