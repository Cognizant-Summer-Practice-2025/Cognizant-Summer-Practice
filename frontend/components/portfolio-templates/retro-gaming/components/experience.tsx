import React from 'react';
import { Experience } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { MapPin, Calendar, Sword, Shield, Wand2 } from 'lucide-react';

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data: experience }: ExperienceProps) {

  const getQuestIcon = (title: string, index: number) => {
    const titleLower = title.toLowerCase();
    if (titleLower.includes('senior') || titleLower.includes('lead')) return <Sword className="quest-icon legendary" size={24} />;
    if (titleLower.includes('developer') || titleLower.includes('engineer')) return <Wand2 className="quest-icon epic" size={24} />;
    if (titleLower.includes('intern') || titleLower.includes('junior')) return <Shield className="quest-icon common" size={24} />;
    
    const icons = [
      <Sword className="quest-icon legendary" size={24} />,
      <Wand2 className="quest-icon epic" size={24} />,
      <Shield className="quest-icon rare" size={24} />
    ];
    return icons[index % icons.length];
  };

  const getQuestRarity = (title: string) => {
    const titleLower = title.toLowerCase();
    if (titleLower.includes('senior') || titleLower.includes('lead')) return 'legendary';
    if (titleLower.includes('developer') || titleLower.includes('engineer')) return 'epic';
    if (titleLower.includes('intern') || titleLower.includes('junior')) return 'common';
    return 'rare';
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short'
    });
  };

  return (
    <div className="retro-experience" id="experience">
      <div className="section-header">
        <h2 className="section-title">
          <Sword className="pixel-icon" size={24} />
          QUEST LOG
        </h2>
        <div className="pixel-border"></div>
        <p className="section-subtitle">Completed Adventures & Campaigns</p>
      </div>

      <div className="quest-timeline">
        {experience.map((exp, index) => (
          <Card key={exp.id} className={`quest-card ${getQuestRarity(exp.position)}`}>
            <div className="quest-header">
              <div className="quest-icon-container">
                {getQuestIcon(exp.position, index)}
                <div className="icon-glow"></div>
              </div>
              
              <div className="quest-info">
                <h3 className="quest-title">{exp.position}</h3>
                <div className="quest-guild">
                  <span className="guild-name">{exp.company}</span>
                  <div className="guild-badge"></div>
                </div>
              </div>

              <div className="quest-duration">
                <Calendar className="date-icon" size={16} />
                <span className="date-range">
                  {formatDate(exp.startDate)} - {exp.endDate ? formatDate(exp.endDate) : 'Present'}
                </span>
              </div>
            </div>

            {exp.location && (
              <div className="quest-location">
                <MapPin className="location-icon" size={16} />
                <span>{exp.location}</span>
              </div>
            )}

            <div className="quest-description">
              <p>{exp.description}</p>
            </div>

            {exp.achievements && exp.achievements.length > 0 && (
              <div className="quest-achievements">
                <h4 className="achievements-title">Quest Rewards:</h4>
                <ul className="achievement-list">
                  {exp.achievements.map((achievement, achIndex) => (
                    <li key={achIndex} className="achievement-item">
                      <span className="achievement-bullet">→</span>
                      <span className="achievement-text">{achievement}</span>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {exp.technologies && exp.technologies.length > 0 && (
              <div className="quest-tech">
                <h4 className="tech-title">Equipment Used:</h4>
                <div className="tech-list">
                  {exp.technologies.map((tech, techIndex) => (
                    <span key={techIndex} className="tech-badge">
                      {tech}
                    </span>
                  ))}
                </div>
              </div>
            )}

            <div className="quest-xp">
              <span className="xp-label">XP GAINED:</span>
              <span className="xp-value">+{(index + 1) * 1000}</span>
            </div>

            <div className="card-effects">
              <div className="pixel-sparkle"></div>
              <div className="pixel-sparkle"></div>
              <div className="pixel-sparkle"></div>
            </div>

            <div className={`rarity-border ${getQuestRarity(exp.position)}`}></div>
          </Card>
        ))}
      </div>

      <div className="experience-summary">
        <div className="summary-stats">
          <div className="summary-item">
            <span className="summary-label">Total Quests:</span>
            <span className="summary-value">{experience.length}</span>
          </div>
          <div className="summary-item">
            <span className="summary-label">Total XP:</span>
            <span className="summary-value">{experience.length * 1500}</span>
          </div>
          <div className="summary-item">
            <span className="summary-label">Rank:</span>
            <span className="summary-value">Legend</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Experience;