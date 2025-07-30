import React, { useState, useEffect } from 'react';
import { Skill } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Gamepad2, Star, Zap, Target, Shield, Sword } from 'lucide-react';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data: skills }: SkillsProps) {
  const [animatedLevels, setAnimatedLevels] = useState<{ [key: string]: number }>({});
  const [selectedCategory, setSelectedCategory] = useState<string>('all');

  useEffect(() => {
    // Animate skill levels
    skills.forEach((skill, index) => {
      const targetLevel = skill.level || 80;
      const duration = 1000 + (index * 100);
      const steps = 50;
      const increment = targetLevel / steps;
      let currentStep = 0;

      const timer = setInterval(() => {
        currentStep++;
        const currentLevel = Math.min(Math.floor(increment * currentStep), targetLevel);
        
        setAnimatedLevels(prev => ({
          ...prev,
          [skill.id]: currentLevel
        }));
        
        if (currentStep >= steps) {
          clearInterval(timer);
        }
      }, duration / steps);

      return () => clearInterval(timer);
    });
  }, [skills]);

  const getSkillCategory = (skill: any) => {
    const name = skill.name.toLowerCase();
    if (name.includes('react') || name.includes('vue') || name.includes('angular') || name.includes('javascript') || name.includes('typescript') || name.includes('html') || name.includes('css')) {
      return 'frontend';
    }
    if (name.includes('node') || name.includes('python') || name.includes('java') || name.includes('c#') || name.includes('sql') || name.includes('database')) {
      return 'backend';
    }
    if (name.includes('git') || name.includes('docker') || name.includes('aws') || name.includes('linux') || name.includes('ci/cd')) {
      return 'devops';
    }
    return 'other';
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'frontend': return <Star className="category-icon" size={20} />;
      case 'backend': return <Shield className="category-icon" size={20} />;
      case 'devops': return <Zap className="category-icon" size={20} />;
      default: return <Target className="category-icon" size={20} />;
    }
  };

  const getSkillIcon = (level: number) => {
    if (level >= 90) return <Sword className="skill-icon legendary" size={16} />;
    if (level >= 70) return <Shield className="skill-icon epic" size={16} />;
    if (level >= 50) return <Target className="skill-icon rare" size={16} />;
    return <Zap className="skill-icon common" size={16} />;
  };

  const getSkillRarity = (level: number) => {
    if (level >= 90) return 'legendary';
    if (level >= 70) return 'epic';
    if (level >= 50) return 'rare';
    return 'common';
  };

  const categories = [
    { id: 'all', name: 'All Skills', icon: <Gamepad2 size={16} /> },
    { id: 'frontend', name: 'Frontend', icon: <Star size={16} /> },
    { id: 'backend', name: 'Backend', icon: <Shield size={16} /> },
    { id: 'devops', name: 'DevOps', icon: <Zap size={16} /> },
    { id: 'other', name: 'Other', icon: <Target size={16} /> }
  ];

  const filteredSkills = selectedCategory === 'all' 
    ? skills 
    : skills.filter(skill => getSkillCategory(skill) === selectedCategory);

  return (
    <div className="retro-skills" id="skills">
      <div className="section-header">
        <h2 className="section-title">
          <Gamepad2 className="pixel-icon" size={24} />
          SKILL TREE
        </h2>
        <div className="pixel-border"></div>
        <p className="section-subtitle">Abilities & Mastery Levels</p>
      </div>

      <div className="skill-categories">
        {categories.map(category => (
          <button
            key={category.id}
            className={`category-btn ${selectedCategory === category.id ? 'active' : ''}`}
            onClick={() => setSelectedCategory(category.id)}
          >
            {category.icon}
            <span>{category.name}</span>
          </button>
        ))}
      </div>

      <div className="skills-grid">
        {filteredSkills.map((skill, index) => {
          const level = animatedLevels[skill.id] || 0;
          const rarity = getSkillRarity(skill.level || 80);
          const category = getSkillCategory(skill);
          
          return (
            <Card key={skill.id} className={`skill-card ${rarity}`}>
              <div className="skill-header">
                <div className="skill-icon-container">
                  {getSkillIcon(skill.level || 80)}
                  <div className="icon-glow"></div>
                </div>
                <div className="skill-category">
                  {getCategoryIcon(category)}
                </div>
              </div>

              <div className="skill-content">
                <h3 className="skill-name">{skill.name}</h3>
                
                <div className="skill-level-display">
                  <span className="level-text">LVL {level}</span>
                  <span className="max-level">/100</span>
                </div>

                <div className="skill-progress">
                  <div className="progress-track">
                    <div 
                      className="progress-fill"
                      style={{ 
                        width: `${level}%`,
                        animationDelay: `${index * 0.1}s`
                      }}
                    ></div>
                    <div className="progress-segments">
                      {[...Array(10)].map((_, i) => (
                        <div 
                          key={i} 
                          className={`segment ${level > i * 10 ? 'filled' : ''}`}
                        ></div>
                      ))}
                    </div>
                  </div>
                  <div className="progress-shine"></div>
                </div>

                <div className="skill-description">
                  {skill.description && (
                    <p className="description-text">{skill.description}</p>
                  )}
                </div>

                <div className="skill-stats">
                  <div className="stat-item">
                    <span className="stat-label">EXP:</span>
                    <span className="stat-value">{level * 100}</span>
                  </div>
                  <div className="stat-item">
                    <span className="stat-label">Rank:</span>
                    <span className={`stat-value ${rarity}`}>
                      {rarity.toUpperCase()}
                    </span>
                  </div>
                </div>
              </div>

              <div className="skill-effects">
                <div className="pixel-particles">
                  <div className="particle"></div>
                  <div className="particle"></div>
                  <div className="particle"></div>
                </div>
              </div>

              <div className={`skill-border ${rarity}`}>
                <div className="corner-glow tl"></div>
                <div className="corner-glow tr"></div>
                <div className="corner-glow bl"></div>
                <div className="corner-glow br"></div>
              </div>
            </Card>
          );
        })}
      </div>

      <div className="skill-summary">
        <Card className="summary-card">
          <div className="summary-header">
            <Star className="pixel-icon" size={20} />
            <h3>MASTERY OVERVIEW</h3>
          </div>
          
          <div className="mastery-stats">
            <div className="mastery-item">
              <span className="mastery-label">Total Skills:</span>
              <span className="mastery-value">{skills.length}</span>
            </div>
            <div className="mastery-item">
              <span className="mastery-label">Average Level:</span>
              <span className="mastery-value">
                {Math.round(skills.reduce((sum, skill) => sum + (skill.level || 80), 0) / skills.length)}
              </span>
            </div>
            <div className="mastery-item">
              <span className="mastery-label">Legendary Skills:</span>
              <span className="mastery-value">
                {skills.filter(skill => (skill.level || 80) >= 90).length}
              </span>
            </div>
          </div>

          <div className="overall-progress">
            <span className="progress-label">Overall Mastery</span>
            <div className="overall-bar">
              <div 
                className="overall-fill"
                style={{ 
                  width: `${skills.reduce((sum, skill) => sum + (skill.level || 80), 0) / skills.length}%`
                }}
              ></div>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}

export default Skills;