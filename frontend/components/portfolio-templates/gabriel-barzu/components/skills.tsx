import React from 'react';
import { Skill } from '@/lib/portfolio';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data: skills }: SkillsProps) {
  if (!skills || skills.length === 0) {
    return null;
  }

  // Group skills by category
  const skillsByCategory = skills.reduce((acc, skill) => {
    if (!acc[skill.category]) {
      acc[skill.category] = [];
    }
    acc[skill.category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  return (
    <section className="gb-skills">
      <h3 className="section-title">Skills</h3>
      <div className="skills-container">
        {Object.entries(skillsByCategory).map(([category, categorySkills]) => (
          <div key={category} className="skills-category">
            <h4 className="skills-category-title">{category}</h4>
            <div className="skills-list">
              {categorySkills.map((skill) => (
                <div key={skill.id} className="skill-item">
                  <div className="skill-header">
                    <span className="skill-name">{skill.name}</span>
                    <span className="skill-percentage">{skill.proficiencyLevel}%</span>
                  </div>
                  <div className="skill-bar">
                    <div 
                      className="skill-progress" 
                      style={{ width: `${skill.proficiencyLevel}%` }}
                    ></div>
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