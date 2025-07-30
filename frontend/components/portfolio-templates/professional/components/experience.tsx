"use client";

import React from 'react';
import { Experience } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Calendar, Building } from 'lucide-react';

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data }: ExperienceProps) {
  if (!data || data.length === 0) {
    return (
      <div className="prof-experience">
        <div className="prof-empty-state">
          <Building size={48} />
          <h3>No Experience Data</h3>
          <p>Experience information will be displayed here when available.</p>
        </div>
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short'
      });
    } catch {
      return dateString;
    }
  };

  const totalCount = data.length;
  const needsScrolling = data.length > 4;

  return (
    <div className="prof-experience">
      {/* Count indicator */}
      <div className="prof-count-indicator">
        <p className="prof-count-text">
          {totalCount} experience{totalCount !== 1 ? 's' : ''} in my career
        </p>
      </div>

      <div className={`prof-timeline ${needsScrolling ? 'prof-scrollable' : ''}`}>
        {data.map((exp, index) => (
          <div key={exp.id || index} className="prof-timeline-item">
            <div className="prof-timeline-marker">
              <div className="prof-timeline-dot"></div>
              {index < data.length - 1 && <div className="prof-timeline-line"></div>}
            </div>
            
            <Card className="prof-experience-card">
              <div className="prof-experience-header">
                <div className="prof-experience-main">
                  <h3 className="prof-experience-title">{exp.jobTitle}</h3>
                  <h4 className="prof-experience-company">{exp.companyName}</h4>
                </div>
                
                <div className="prof-experience-meta">
                  <div className="prof-experience-date">
                    <Calendar size={14} />
                    <span>
                      {formatDate(exp.startDate)} - {exp.endDate ? formatDate(exp.endDate) : 'Present'}
                    </span>
                  </div>
                  
                </div>
              </div>

              {exp.description && (
                <div className="prof-experience-description">
                  <p>{exp.description}</p>
                </div>
              )}

              {exp.skillsUsed && exp.skillsUsed.length > 0 && (
                <div className="prof-experience-tech">
                  <h5>Technologies Used:</h5>
                  <div className="prof-tech-tags">
                    {exp.skillsUsed.map((tech, techIndex) => (
                      <span key={techIndex} className="prof-tech-tag">
                        {tech}
                      </span>
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