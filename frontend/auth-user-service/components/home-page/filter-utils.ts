import { PortfolioCardDto } from '@/lib/portfolio/api';
import { FilterOption, SkillCategory } from './filter-types';
import skillsConfig from '../../lib/skills-config.json';

export const calculatePortfolioFilters = (portfolios: PortfolioCardDto[]): FilterOption[] => {
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

export const calculateRoleFilters = (portfolios: PortfolioCardDto[]): FilterOption[] => {
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

const calculateSkillCounts = (portfolios: PortfolioCardDto[]): Map<string, number> => {
  const skillCounts = new Map<string, number>();
  
  portfolios.forEach(portfolio => {
    portfolio.skills.forEach(skill => {
      const normalizedSkill = skill.toLowerCase().trim();
      skillCounts.set(normalizedSkill, (skillCounts.get(normalizedSkill) || 0) + 1);
    });
  });
  
  return skillCounts;
};

const normalizeSkillName = (skill: string) => skill.toLowerCase().trim();

const getSkillCount = (skill: string, skillCounts: Map<string, number>): number => {
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

export const generateCategorizedSkillFilters = (portfolios: PortfolioCardDto[]): SkillCategory[] => {
  const skillCounts = calculateSkillCounts(portfolios);
  const categories: SkillCategory[] = [];
  
  // Get all skills from both hard and soft skills categories
  const { hard_skills, soft_skills } = skillsConfig.skillCategories;
  
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
      count: getSkillCount(skill, skillCounts)
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
      count: getSkillCount(skill, skillCounts)
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

export const getTotalSkillsCount = (categories: SkillCategory[]): number => {
  return categories.reduce((total, category) =>
    total + category.subcategories.reduce((subTotal, subcategory) =>
      subTotal + subcategory.skills.length, 0
    ), 0
  );
}; 