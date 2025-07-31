import React from 'react';
import { Skill } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { Cpu, Code, Brain, Zap } from 'lucide-react';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data }: SkillsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="cyberpunk-skills empty">
        <div className="empty-state">
          <Cpu size={48} className="empty-icon" />
          <h3>No skills data found</h3>
          <p>Skill matrix appears to be uninitialized</p>
        </div>
      </div>
    );
  }

  // Group skills by category
  const groupedSkills = data.reduce((acc, skill) => {
    const category = skill.categoryType || 'uncategorized';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  const getCategoryIcon = (category: string) => {
    switch (category.toLowerCase()) {
      case 'hard_skills':
        return <Code size={20} />;
      case 'soft_skills':
        return <Brain size={20} />;
      default:
        return <Zap size={20} />;
    }
  };

  const getCategoryTitle = (category: string) => {
    switch (category.toLowerCase()) {
      case 'hard_skills':
        return 'Technical Skills';
      case 'soft_skills':
        return 'Soft Skills';
      default:
        return 'Other Skills';
    }
  };

  return (
    <div className="cyberpunk-skills">
      <div className="skills-header">
        <h2 className="section-title">
          <Cpu size={24} />
          Skill Matrix
        </h2>
        <div className="matrix-info">
          <span className="matrix-text">Neural network capabilities</span>
          <span className="skill-count">{data.length} skills loaded</span>
        </div>
      </div>

      <div className="skills-categories">
        {Object.entries(groupedSkills).map(([category, skills]) => (
          <Card key={category} className="skill-category">
            <div className="category-header">
              <div className="category-title">
                {getCategoryIcon(category)}
                <h3>{getCategoryTitle(category)}</h3>
              </div>
              <Badge variant="outline" className="skill-count-badge">
                {skills.length} skills
              </Badge>
            </div>
            
            <div className="skills-grid">
              {skills.map((skill) => (
                <div key={skill.id} className="skill-item">
                  <div className="skill-header">
                    <span className="skill-name">{skill.name}</span>
                    {skill.proficiencyLevel && (
                      <span className="skill-level">
                        {skill.proficiencyLevel}%
                      </span>
                    )}
                  </div>
                  
                  {skill.proficiencyLevel && (
                    <div className="skill-progress">
                      <Progress 
                        value={skill.proficiencyLevel} 
                        className="progress-bar"
                      />
                      <div className="progress-glow" 
                           style={{ width: `${skill.proficiencyLevel}%` }}>
                      </div>
                    </div>
                  )}
                  
                  {skill.subcategory && (
                    <Badge variant="secondary" className="subcategory-badge">
                      {skill.subcategory}
                    </Badge>
                  )}
                </div>
              ))}
            </div>
          </Card>
        ))}
      </div>

      <div className="skill-stats">
        <Card className="stats-card">
          <h4 className="stats-title">System Statistics</h4>
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-label">Total Skills:</span>
              <span className="stat-value">{data.length}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Categories:</span>
              <span className="stat-value">{Object.keys(groupedSkills).length}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Avg Proficiency:</span>
              <span className="stat-value">
                {Math.round(
                  data
                    .filter(s => s.proficiencyLevel)
                    .reduce((sum, s) => sum + (s.proficiencyLevel || 0), 0) /
                  data.filter(s => s.proficiencyLevel).length
                )}%
              </span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Status:</span>
              <span className="stat-value status-active">OPTIMIZED</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}