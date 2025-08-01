import React from 'react';
import { Skill } from '@/lib/portfolio';
import { AnimatedNumber, AnimatedProgressBar } from '@/components/ui/animated-number';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data: skills }: SkillsProps) {
  if (!skills || skills.length === 0) {
    return null;
  }

  // Group skills by category type, then by subcategory
  const skillsByHierarchy = skills.reduce((acc, skill) => {
    // Use the new hierarchical structure or fallback to legacy category
    const categoryType = skill.categoryType || 'hard_skills';
    const subcategory = skill.subcategory || 'other';
    
    if (!acc[categoryType]) {
      acc[categoryType] = {};
    }
    if (!acc[categoryType][subcategory]) {
      acc[categoryType][subcategory] = [];
    }
    acc[categoryType][subcategory].push(skill);
    return acc;
  }, {} as Record<string, Record<string, Skill[]>>);

  // Helper function to get display name for category type
  const getCategoryTypeDisplayName = (categoryType: string) => {
    switch (categoryType) {
      case 'hard_skills':
        return 'Hard Skills';
      case 'soft_skills':
        return 'Soft Skills';
      default:
        return 'Other Skills';
    }
  };

  // Helper function to get display name for subcategory
  const getSubcategoryDisplayName = (subcategory: string) => {
    const subcategoryMap: Record<string, string> = {
      frontend: 'Frontend',
      backend: 'Backend',
      database: 'Database',
      devops: 'DevOps & Cloud',
      mobile: 'Mobile Development',
      tools: 'Tools & Technologies',
      data_science: 'Data Science & AI',
      security: 'Cybersecurity',
      communication: 'Communication',
      leadership: 'Leadership',
      problem_solving: 'Problem Solving',
      collaboration: 'Collaboration',
      adaptability: 'Adaptability',
      professional: 'Professional Skills',
      other: 'Other'
    };
    return subcategoryMap[subcategory] || subcategory.charAt(0).toUpperCase() + subcategory.slice(1);
  };

  // Helper function to get proficiency color based on level
  const getProficiencyColor = (level: number = 0) => {
    if (level >= 80) return 'linear-gradient(90deg, #10b981, #059669)'; // Green
    if (level >= 60) return 'linear-gradient(90deg, #3b82f6, #2563eb)'; // Blue
    if (level >= 40) return 'linear-gradient(90deg, #f59e0b, #d97706)'; // Orange
    return 'linear-gradient(90deg, #ef4444, #dc2626)'; // Red
  };

  return (
    <section className="gb-skills">
      <h3 className="section-title">Skills</h3>
      <div className="skills-container">
        {Object.entries(skillsByHierarchy).map(([categoryType, subcategories]) => (
          <div key={categoryType} className="skills-category-type">
            <h4 className="skills-category-type-title">{getCategoryTypeDisplayName(categoryType)}</h4>
            <div className="skills-subcategories">
              {Object.entries(subcategories).map(([subcategory, categorySkills]) => (
                <div key={subcategory} className="skills-subcategory">
                  <h5 className="skills-subcategory-title">{getSubcategoryDisplayName(subcategory)}</h5>
                  <div className="skills-list">
                    {categorySkills.map((skill) => (
                      <div key={skill.id} className="skill-item">
                        <div className="skill-header">
                          <span className="skill-name">{skill.name}</span>
                          <span className="skill-percentage">
                            <AnimatedNumber value={skill.proficiencyLevel || 0} />%
                          </span>
                        </div>
                        <div className="skill-bar">
                          <AnimatedProgressBar 
                            percentage={skill.proficiencyLevel || 0}
                            className="skill-progress"
                            style={{
                              background: getProficiencyColor(skill.proficiencyLevel || 0)
                            }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </section>
  );
} 