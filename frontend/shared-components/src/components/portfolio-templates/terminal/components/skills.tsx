import React, { useState, useEffect } from 'react';
import { Skill } from '@/lib/portfolio';
import { Terminal, Code, Database, Server, Globe, Monitor } from 'lucide-react';

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data: skills }: SkillsProps) {
  const [animatedLevels, setAnimatedLevels] = useState<Record<string, number>>({});
  const [terminalOutput, setTerminalOutput] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState('all');

  useEffect(() => {
    // Animate skill levels
    skills.forEach(skill => {
      const targetLevel = skill.proficiencyLevel || 80;
      let currentLevel = 0;
      const increment = Math.ceil(targetLevel / 50);
      
      const timer = setInterval(() => {
        if (currentLevel < targetLevel) {
          currentLevel = Math.min(currentLevel + increment, targetLevel);
          setAnimatedLevels(prev => ({
            ...prev,
            [skill.id]: currentLevel
          }));
        } else {
          clearInterval(timer);
        }
      }, 30);
    });
  }, [skills]);

  useEffect(() => {
    // Simulate package manager output
    const packageOutput = [
      '$ npm list --depth=0',
      '',
      'portfolio@1.0.0 /home/user/portfolio',
      ...skills.map(skill => `├── ${skill.name.toLowerCase().replace(/\s+/g, '-')}@${((skill.proficiencyLevel || 80) / 10).toFixed(1)}.0`),
      '',
      '$ composer show --installed',
      '',
      ...skills.filter(skill => getSkillCategory(skill) === 'backend').map(skill => 
        `${skill.name.toLowerCase().replace(/\s+/g, '-')} v${((skill.proficiencyLevel || 80) / 10).toFixed(1)}.0`
      )
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < packageOutput.length) {
        setTerminalOutput(prev => [...prev, packageOutput[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 150);

    return () => clearInterval(interval);
  }, [skills]);

  const getSkillCategory = (skill: { name: string }) => {
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
    return 'tools';
  };

  const getCategoryIcon = (category: string) => {
    switch (category) {
      case 'frontend': return <Monitor className="category-icon" size={16} />;
      case 'backend': return <Server className="category-icon" size={16} />;
      case 'devops': return <Database className="category-icon" size={16} />;
      case 'tools': return <Code className="category-icon" size={16} />;
      default: return <Globe className="category-icon" size={16} />;
    }
  };

  const getSkillLevel = (level: number) => {
    if (level >= 90) return 'Expert';
    if (level >= 75) return 'Advanced';
    if (level >= 50) return 'Intermediate';
    return 'Beginner';
  };

  const getProgressBar = (level: number) => {
    const bars = Math.floor(level / 10);
    return '█'.repeat(bars) + '░'.repeat(10 - bars);
  };

  const categories = [
    { id: 'all', name: 'All', icon: <Globe size={14} /> },
    { id: 'frontend', name: 'Frontend', icon: <Monitor size={14} /> },
    { id: 'backend', name: 'Backend', icon: <Server size={14} /> },
    { id: 'devops', name: 'DevOps', icon: <Database size={14} /> },
    { id: 'tools', name: 'Tools', icon: <Code size={14} /> }
  ];

  const filteredSkills = selectedCategory === 'all' 
    ? skills 
    : skills.filter(skill => getSkillCategory(skill) === selectedCategory);

  return (
    <div className="terminal-skills" id="skills">
      <div className="skills-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">skills_inventory.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="package-output">
            {terminalOutput.map((line, index) => (
              <div key={index} className="output-line">
                {line.startsWith('$') ? (
                  <span className="command-line">
                    <Terminal className="cmd-icon" size={12} />
                    {line}
                  </span>
                ) : line.startsWith('├──') || line.startsWith('└──') ? (
                  <span className="package-line">
                    <Code className="package-icon" size={12} />
                    {line}
                  </span>
                ) : (
                  line
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="skills-interface">
        <div className="interface-header">
          <Terminal className="interface-icon" size={20} />
          <span>Skill Matrix</span>
          
          <div className="category-tabs">
            {categories.map(category => (
              <button
                key={category.id}
                className={`tab ${selectedCategory === category.id ? 'active' : ''}`}
                onClick={() => setSelectedCategory(category.id)}
              >
                {category.icon}
                <span>{category.name}</span>
              </button>
            ))}
          </div>
        </div>
        
        <div className="skills-table">
          <div className="table-header">
            <span className="col-skill">SKILL</span>
            <span className="col-level">LEVEL</span>
            <span className="col-progress">PROGRESS</span>
            <span className="col-category">CATEGORY</span>
            <span className="col-status">STATUS</span>
          </div>
          
          {filteredSkills.map(skill => {
            const level = animatedLevels[skill.id] || 0;
            const category = getSkillCategory(skill);
            
            return (
              <div key={skill.id} className="table-row">
                <span className="col-skill">
                  {getCategoryIcon(category)}
                  <span className="skill-name">{skill.name}</span>
                </span>
                <span className="col-level">
                  <span className="level-value">{level}%</span>
                  <span className="level-label">{getSkillLevel(skill.proficiencyLevel || 80)}</span>
                </span>
                <span className="col-progress">
                  <div className="progress-container">
                    <div className="progress-bar">
                      <div 
                        className="progress-fill"
                        style={{ width: `${level}%` }}
                      ></div>
                    </div>
                    <span className="progress-ascii">[{getProgressBar(level)}]</span>
                  </div>
                </span>
                <span className="col-category">
                  <span className={`category-badge ${category}`}>
                    {category.toUpperCase()}
                  </span>
                </span>
                <span className="col-status">
                  <span className="status-indicator active"></span>
                  <span className="status-text">ACTIVE</span>
                </span>
              </div>
            );
          })}
        </div>
      </div>

      <div className="skills-analytics">
        <div className="analytics-header">
          <Code className="analytics-icon" size={16} />
          <span>Skill Analytics</span>
        </div>
        
        <div className="analytics-grid">
          <div className="analytic-card">
            <div className="card-header">
              <span className="card-title">Total Skills</span>
              <span className="card-value">{skills.length}</span>
            </div>
            <div className="card-content">
              <div className="mini-chart">
                {categories.slice(1).map(category => {
                  const count = skills.filter(skill => getSkillCategory(skill) === category.id).length;
                  const width = (count / skills.length) * 100;
                  return (
                    <div key={category.id} className="chart-bar" style={{ width: `${width}%` }}>
                      <span className="bar-label">{category.name}</span>
                      <span className="bar-value">{count}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
          
          <div className="analytic-card">
            <div className="card-header">
              <span className="card-title">Average Level</span>
              <span className="card-value">
                {Math.round(skills.reduce((sum, skill) => sum + (skill.proficiencyLevel || 80), 0) / skills.length)}%
              </span>
            </div>
            <div className="card-content">
              <div className="level-distribution">
                <div className="dist-item">
                  <span className="dist-label">Expert (90%+)</span>
                  <span className="dist-value">{skills.filter(s => (s.proficiencyLevel || 80) >= 90).length}</span>
                </div>
                <div className="dist-item">
                  <span className="dist-label">Advanced (75%+)</span>
                  <span className="dist-value">{skills.filter(s => (s.proficiencyLevel || 80) >= 75 && (s.proficiencyLevel || 80) < 90).length}</span>
                </div>
                <div className="dist-item">
                  <span className="dist-label">Intermediate (50%+)</span>
                  <span className="dist-value">{skills.filter(s => (s.proficiencyLevel || 80) >= 50 && (s.proficiencyLevel || 80) < 75).length}</span>
                </div>
              </div>
            </div>
          </div>
          
          <div className="analytic-card">
            <div className="card-header">
              <span className="card-title">Top Category</span>
              <span className="card-value">
                {categories.slice(1).reduce((prev, current) => {
                  const prevCount = skills.filter(skill => getSkillCategory(skill) === prev.id).length;
                  const currentCount = skills.filter(skill => getSkillCategory(skill) === current.id).length;
                  return currentCount > prevCount ? current : prev;
                }).name}
              </span>
            </div>
            <div className="card-content">
              <div className="top-skills">
                {skills
                  .sort((a, b) => (b.proficiencyLevel || 80) - (a.proficiencyLevel || 80))
                  .slice(0, 3)
                  .map((skill, index) => (
                    <div key={skill.id} className="top-skill">
                      <span className="skill-rank">#{index + 1}</span>
                      <span className="skill-name">{skill.name}</span>
                      <span className="skill-level">{skill.proficiencyLevel || 80}%</span>
                    </div>
                  ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Skills;