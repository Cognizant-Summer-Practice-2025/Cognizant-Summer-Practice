export interface FilterOption {
  value: string;
  label: string;
  count: number;
}

export interface SkillCategory {
  id: string;
  label: string;
  type: 'hard' | 'soft';
  subcategories: SkillSubcategory[];
}

export interface SkillSubcategory {
  id: string;
  label: string;
  skills: FilterOption[];
}

export interface FilterSidebarProps {
  portfolios: import('@/lib/portfolio/api').PortfolioCardDto[];
  onFiltersChange: (filters: string[]) => void;
  activeFilters: string[];
}

export interface FilterGroupProps {
  title: string;
  icon: React.ReactNode;
  filters: FilterOption[];
  groupKey: string;
  selectedFilters: string[];
  collapsedSections: Record<string, boolean>;
  onFilterChange: (value: string, checked: boolean) => void;
  onToggleSection: (sectionKey: string) => void;
}

export interface SelectedFiltersProps {
  selectedFilters: string[];
  onRemoveFilter: (filterValue: string) => void;
  onClearAll: () => void;
  getFilterLabel: (filterValue: string) => string;
}

export interface SkillsFilterProps {
  skillCategories: SkillCategory[];
  selectedFilters: string[];
  collapsedSections: Record<string, boolean>;
  onFilterChange: (value: string, checked: boolean) => void;
  onToggleSection: (sectionKey: string) => void;
}

export interface MobileFilterProps {
  isOpen: boolean;
  selectedFiltersCount: number;
  onToggle: () => void;
  children: React.ReactNode;
} 