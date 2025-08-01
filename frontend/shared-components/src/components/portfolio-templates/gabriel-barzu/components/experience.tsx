import React from 'react';
import { Experience as ExperienceType } from '@/lib/portfolio';

interface ExperienceProps {
  data: ExperienceType[];
}

export function Experience({ data: experience }: ExperienceProps) {
  if (!experience || experience.length === 0) {
    return null;
  }

  const formatDuration = (startDate: string, endDate?: string, isCurrent?: boolean) => {
    const start = new Date(startDate).getFullYear();
    if (isCurrent) {
      return `${start} - Present`;
    }
    const end = endDate ? new Date(endDate).getFullYear() : 'Present';
    return `${start} - ${end}`;
  };

  // Check if we need scrolling (more than 5 experiences)
  const needsScrolling = experience.length > 5;

  return (
    <section className="gb-experience-modern">
      <h3 className="experience-modern-title">Work experience</h3>
      <p className="experience-count-text">
        {experience.length} {experience.length === 1 ? 'position' : 'positions'} in my career
      </p>
      <div className={`experience-modern-timeline ${needsScrolling ? 'scrollable-timeline' : ''}`}>
        {experience.map((item, index) => (
          <div 
            key={item.id} 
            className={`experience-modern-item ${index % 2 === 1 ? 'experience-item-right' : 'experience-item-left'}`}
          >
            <div className="experience-modern-marker">
              <div className="marker-modern-number">{index + 1}</div>
              {index < experience.length - 1 && <div className="marker-modern-line"></div>}
            </div>
            <div className="experience-modern-content">
              <h4 className="experience-modern-title-text">{item.jobTitle}</h4>
              <div className="experience-modern-company">{item.companyName}</div>
              <div className="experience-modern-duration">{formatDuration(item.startDate, item.endDate, item.isCurrent)}</div>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
} 