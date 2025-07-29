"use client";

import React from 'react';
import { Skill } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Award, Star } from 'lucide-react';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data }: SkillsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="prof-skills">
        <div className="prof-empty-state">
          <Award size={48} />
          <h3>No Skills Data</h3>
          <p>Skills and expertise will be displayed here when available.</p>
        </div>
      </div>
    );
  }

  // Group skills by category
  const groupedSkills = data.reduce((acc, skill) => {
    const category = skill.category || 'Other';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  const renderProficiencyLevel = (level: number) => {
    const stars = [];
    const maxStars = 5;
    const filledStars = Math.round((level / 100) * maxStars);
    
    for (let i = 0; i < maxStars; i++) {
      stars.push(
        <Star 
          key={i} 
          size={14} 
          className={`prof-skill-star ${i < filledStars ? 'filled' : ''}`}
        />
      );
    }
    return stars;
  };

  const getProficiencyText = (level: number) => {
    if (level >= 90) return 'Expert';
    if (level >= 75) return 'Advanced';
    if (level >= 60) return 'Intermediate';
    if (level >= 40) return 'Beginner';
    return 'Novice';
  };

  return (
    <div className="prof-skills">
      <div className="prof-skills-container">
        {Object.entries(groupedSkills).map(([category, skills]) => (
          <Card key={category} className="prof-skills-category">
            <div className="prof-skills-header">
              <h3 className="prof-skills-title">{category}</h3>
            </div>
            
            <div className="prof-skills-list">
              {skills.map((skill, index) => (
                <div key={skill.id || index} className="prof-skill-item">
                  <div className="prof-skill-info">
                    <div className="prof-skill-name">{skill.name}</div>
                    <div className="prof-skill-level">
                      <div className="prof-skill-stars">
                        {renderProficiencyLevel(skill.proficiency || 0)}
                      </div>
                      <span className="prof-skill-text">
                        {getProficiencyText(skill.proficiency || 0)}
                      </span>
                    </div>
                  </div>
                  
                  <div className="prof-skill-bar">
                    <div 
                      className="prof-skill-progress"
                      style={{ width: `${skill.proficiency || 0}%` }}
                    ></div>
                  </div>
                  
                  {skill.yearsOfExperience && (
                    <div className="prof-skill-experience">
                      {skill.yearsOfExperience} year{skill.yearsOfExperience !== 1 ? 's' : ''} experience
                    </div>
                  )}
                </div>
              ))}
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
} 