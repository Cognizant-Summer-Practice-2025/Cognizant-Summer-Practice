import React from 'react';
import { Experience } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Building2, Calendar, MapPin } from 'lucide-react';

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data }: ExperienceProps) {
  if (!data || data.length === 0) {
    return (
      <div className="cyberpunk-experience empty">
        <div className="empty-state">
          <Building2 size={48} className="empty-icon" />
          <h3>No experience records found</h3>
          <p>Career log appears to be empty</p>
        </div>
      </div>
    );
  }

  const sortedExperience = [...data].sort((a, b) => {
    if (a.isCurrent && !b.isCurrent) return -1;
    if (!a.isCurrent && b.isCurrent) return 1;
    return new Date(b.startDate).getTime() - new Date(a.startDate).getTime();
  });

  return (
    <div className="cyberpunk-experience">
      <div className="experience-header">
        <h2 className="section-title">
          <Building2 size={24} />
          Career Log
        </h2>
        <div className="log-info">
          <span className="log-text">Chronological work history</span>
          <span className="record-count">{data.length} entries</span>
        </div>
      </div>

      <div className="timeline">
        {sortedExperience.map((exp, index) => (
          <div key={exp.id} className="timeline-item">
            <div className="timeline-marker">
              <div className={`marker-dot ${exp.isCurrent ? 'current' : 'completed'}`}>
                <div className="dot-pulse"></div>
              </div>
              {index < sortedExperience.length - 1 && (
                <div className="timeline-line"></div>
              )}
            </div>
            
            <Card className="experience-card">
              <div className="card-header">
                <div className="job-info">
                  <h3 className="job-title">{exp.jobTitle}</h3>
                  <div className="company-info">
                    <Building2 size={16} />
                    <span className="company-name">{exp.companyName}</span>
                  </div>
                </div>
                
                <div className="status-info">
                  {exp.isCurrent && (
                    <Badge className="current-badge">
                      ACTIVE
                    </Badge>
                  )}
                  <div className="date-range">
                    <Calendar size={14} />
                    <span>
                      {new Date(exp.startDate).toLocaleDateString('en-US', { 
                        month: 'short', 
                        year: 'numeric' 
                      })}
                      {' - '}
                      {exp.isCurrent ? 'Present' : 
                        new Date(exp.endDate!).toLocaleDateString('en-US', { 
                          month: 'short', 
                          year: 'numeric' 
                        })
                      }
                    </span>
                  </div>
                </div>
              </div>
              
              {exp.description && (
                <div className="job-description">
                  <p>{exp.description}</p>
                </div>
              )}
              
              {exp.skillsUsed && exp.skillsUsed.length > 0 && (
                <div className="skills-section">
                  <span className="skills-label">Technologies Used:</span>
                  <div className="skills-list">
                    {exp.skillsUsed.map((skill, skillIndex) => (
                      <Badge key={skillIndex} variant="outline" className="skill-tag">
                        {skill}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}
            </Card>
          </div>
        ))}
      </div>
    </div>
  );
}