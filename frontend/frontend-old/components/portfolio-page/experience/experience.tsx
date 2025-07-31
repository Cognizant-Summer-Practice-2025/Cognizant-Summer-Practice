import React from 'react';
import './style.css';

interface ExperienceItemProps {
  number: string;
  title: string;
  company: string;
  period: string;
  description: string;
}

const ExperienceItem: React.FC<ExperienceItemProps> = ({ 
  number, 
  title, 
  company, 
  period, 
  description 
}) => {
  return (
    <div className="experience-item">
      <div className="experience-number">
        <div className="experience-number-text">{number}</div>
      </div>
      <div className="experience-details">
        <div className="experience-title-container">
          <div className="experience-title">{title}</div>
        </div>
        <div className="experience-company-container">
          <div className="experience-company">{company}</div>
        </div>
        <div className="experience-period-container">
          <div className="experience-period">{period}</div>
        </div>
        <div className="experience-description-container">
          <div className="experience-description">{description}</div>
        </div>
      </div>
    </div>
  );
};

const ExperienceSection = () => {
  const experiences = [
    {
      number: "1",
      title: "Senior Full Stack Developer",
      company: "TechCorp Solutions",
      period: "2022 - Present",
      description: "Leading development of scalable web applications using React, Node.js, and cloud technologies."
    },
    {
      number: "2",
      title: "Full Stack Developer",
      company: "Digital Innovation Labs",
      period: "2020 - 2022",
      description: "Developed modern web applications with React, Express.js, and PostgreSQL databases."
    }
  ];

  return (
    <div className="experience-section">
      <div className="experience-header">
        <div className="experience-section-title">Professional Experience</div>
      </div>
      <div className="experience-list">
        {experiences.map((experience, index) => (
          <ExperienceItem
            key={index}
            number={experience.number}
            title={experience.title}
            company={experience.company}
            period={experience.period}
            description={experience.description}
          />
        ))}
      </div>
    </div>
  );
};

export default ExperienceSection;