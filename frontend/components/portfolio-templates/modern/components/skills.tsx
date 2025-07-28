import React from 'react';
import { Skill } from '@/lib/portfolio';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data: skills }: SkillsProps) {
  if (!skills || skills.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No skills data available.
      </div>
    );
  }

  // Group skills by category
  const groupedSkills = skills.reduce((acc, skill) => {
    const category = skill.categoryType || skill.category || 'Other';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  // Sort skills within each category by proficiency level (if available) or display order
  Object.keys(groupedSkills).forEach(category => {
    groupedSkills[category].sort((a, b) => {
      if (a.proficiencyLevel && b.proficiencyLevel) {
        return b.proficiencyLevel - a.proficiencyLevel;
      }
      if (a.displayOrder && b.displayOrder) {
        return a.displayOrder - b.displayOrder;
      }
      return a.name.localeCompare(b.name);
    });
  });

  const categoryLabels: Record<string, string> = {
    'hard_skills': 'Technical Skills',
    'soft_skills': 'Soft Skills',
    'frontend': 'Frontend',
    'backend': 'Backend',
    'devops': 'DevOps',
    'design': 'Design',
    'other': 'Other Skills'
  };

  const getCategoryLabel = (category: string) => {
    return categoryLabels[category.toLowerCase()] || category;
  };

  const totalCount = skills.length;

  return (
    <div className="modern-component-container">
      {/* Count indicator */}
      <div className="mb-4 pb-2 border-b border-border">
        <p className="text-sm text-muted-foreground">
          {totalCount} skill{totalCount !== 1 ? 's' : ''}
        </p>
      </div>

      <div className="max-h-[800px] overflow-y-auto pr-2">
        <div className="modern-grid">
          {Object.entries(groupedSkills).map(([category, categorySkills]) => (
            <Card key={category} className="modern-card">
              <CardContent className="p-6">
                <div className="modern-skill-category">
                  <h3 className="modern-skill-category-title">
                    {getCategoryLabel(category)}
                  </h3>
                  
                  <div className="modern-skills-list">
                    {categorySkills.map((skill) => (
                      <Badge 
                        key={skill.id} 
                        variant="secondary" 
                        className="modern-skill-tag"
                        title={skill.proficiencyLevel ? `Proficiency: ${skill.proficiencyLevel}%` : undefined}
                      >
                        {skill.name}
                        {skill.proficiencyLevel && skill.proficiencyLevel > 0 && (
                          <span className="ml-1 text-xs opacity-70">
                            {skill.proficiencyLevel}%
                          </span>
                        )}
                      </Badge>
                    ))}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
} 