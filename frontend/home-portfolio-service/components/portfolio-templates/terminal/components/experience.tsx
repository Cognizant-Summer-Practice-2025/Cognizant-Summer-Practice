import React, { useState, useEffect } from 'react';
import { type Experience } from '@/lib/portfolio';
import { Calendar, Building, GitBranch, Terminal } from 'lucide-react';

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data: experience }: ExperienceProps) {
  const [selectedExperience, setSelectedExperience] = useState<string | null>(null);
  const [terminalOutput, setTerminalOutput] = useState<string[]>([]);

  useEffect(() => {
    // Simulate git log command output
    const gitLogOutput = [
      '$ git log --oneline --graph --all',
      '',
      ...experience.map((exp, index) => {
        const startYear = new Date(exp.startDate).getFullYear();
        const endYear = exp.endDate ? new Date(exp.endDate).getFullYear() : 'Present';
        return `* ${(index + 1).toString().padStart(7, '0')} (${startYear}-${endYear}) ${exp.jobTitle} at ${exp.companyName}`;
      }),
      '',
      '$ git show --stat'
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < gitLogOutput.length) {
        setTerminalOutput(prev => [...prev, gitLogOutput[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 300);

    return () => clearInterval(interval);
  }, [experience]);

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short'
    });
  };

  const calculateDuration = (startDate: string, endDate?: string) => {
    const start = new Date(startDate);
    const end = endDate ? new Date(endDate) : new Date();
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    const years = Math.floor(diffDays / 365);
    const months = Math.floor((diffDays % 365) / 30);
    
    if (years > 0) {
      return `${years}y ${months}m`;
    }
    return `${months}m`;
  };

  return (
    <div className="terminal-experience" id="experience">
      <div className="experience-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">work_history.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="git-log">
            {terminalOutput.map((line, index) => (
              <div key={index} className="log-line">
                {line.startsWith('*') ? (
                  <span className="commit-line">
                    <GitBranch className="git-icon" size={12} />
                    {line.substring(1)}
                  </span>
                ) : line.startsWith('$') ? (
                  <span className="command-line">
                    <Terminal className="cmd-icon" size={12} />
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

      <div className="experience-list">
        <div className="list-header">
          <Building className="list-icon" size={20} />
          <span>Work Experience</span>
          <span className="list-count">({experience.length} entries)</span>
        </div>
        
        <div className="experience-entries">
          {experience.map((exp, index) => (
            <div 
              key={exp.id} 
              className={`experience-entry ${selectedExperience === exp.id ? 'selected' : ''}`}
              onClick={() => setSelectedExperience(selectedExperience === exp.id ? null : exp.id)}
            >
              <div className="entry-header">
                <div className="entry-number">
                  <span className="commit-hash">#{(index + 1).toString().padStart(3, '0')}</span>
                </div>
                
                <div className="entry-info">
                  <h3 className="position-title">{exp.jobTitle}</h3>
                  <div className="company-info">
                    <Building className="company-icon" size={14} />
                    <span className="company-name">{exp.companyName}</span>
                  </div>
                </div>
                
                <div className="entry-meta">
                  <div className="date-range">
                    <Calendar className="date-icon" size={14} />
                    <span>{formatDate(exp.startDate)} - {exp.endDate ? formatDate(exp.endDate) : 'Present'}</span>
                  </div>
                  <div className="duration">
                    <span className="duration-badge">{calculateDuration(exp.startDate, exp.endDate)}</span>
                  </div>
                </div>
              </div>

              <div className="entry-description">
                <p>{exp.description}</p>
              </div>

              {exp.skillsUsed && exp.skillsUsed.length > 0 && (
                <div className="entry-technologies">
                  <div className="tech-title">
                    <span className="tech-label">Tech Stack:</span>
                  </div>
                  <div className="tech-tags">
                    {exp.skillsUsed.map((tech, techIndex) => (
                      <span key={techIndex} className="tech-tag">
                        {tech}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              <div className="entry-footer">
                <div className="commit-stats">
                  <span className="stat-item">
                    <span className="stat-value">{exp.skillsUsed?.length || 0}</span>
                    <span className="stat-label">technologies</span>
                  </span>
                  <span className="stat-item">
                    <span className="stat-value">{calculateDuration(exp.startDate, exp.endDate)}</span>
                    <span className="stat-label">duration</span>
                  </span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="experience-summary">
        <div className="summary-terminal">
          <div className="summary-header">
            <Terminal className="summary-icon" size={16} />
            <span>Career Summary</span>
          </div>
          
          <div className="summary-content">
            <div className="summary-line">
              <span className="prompt">$</span>
              <span className="command">wc -l career.log</span>
            </div>
            <div className="summary-output">
              <span className="output-value">{experience.length}</span>
              <span className="output-label">positions</span>
            </div>
            
            <div className="summary-line">
              <span className="prompt">$</span>
              <span className="command">sort -u technologies.txt | wc -l</span>
            </div>
            <div className="summary-output">
              <span className="output-value">{new Set(experience.flatMap(exp => exp.skillsUsed || [])).size}</span>
              <span className="output-label">unique technologies</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Experience;