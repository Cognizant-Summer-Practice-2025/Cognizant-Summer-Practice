import React from 'react';
import { Briefcase, Calendar, MapPin, ExternalLink, Building } from 'lucide-react';

interface Experience {
  id: number;
  title: string;
  company: string;
  description: string;
  start_date: string;
  end_date?: string;
  location?: string;
  technologies?: string;
  type?: string;
  website?: string;
}

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data }: ExperienceProps) {
  const experiences = data || [];

  const formatDate = (dateString: string) => {
    if (!dateString) return '';
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
    
    if (years > 0 && months > 0) {
      return `${years}y ${months}m`;
    } else if (years > 0) {
      return `${years} year${years > 1 ? 's' : ''}`;
    } else if (months > 0) {
      return `${months} month${months > 1 ? 's' : ''}`;
    } else {
      return '< 1 month';
    }
  };

  const getTechArray = (technologies: string | null | undefined = '') => {
    if (!technologies || typeof technologies !== 'string') {
      return [];
    }
    return technologies.split(',').map(tech => tech.trim()).filter(Boolean);
  };

  const getTypeColor = (type?: string) => {
    switch (type?.toLowerCase()) {
      case 'full-time':
        return '#43e97b';
      case 'part-time':
        return '#4facfe';
      case 'contract':
        return '#ffbd2e';
      case 'freelance':
        return '#f093fb';
      case 'internship':
        return '#ff7b72';
      default:
        return 'var(--bg-accent)';
    }
  };

  // Sort experiences by start date (most recent first)
  const sortedExperiences = [...experiences].sort((a, b) => 
    new Date(b.start_date).getTime() - new Date(a.start_date).getTime()
  );

  return (
    <div className="component-card">
      <div className="component-title">
        <Briefcase size={20} />
        Professional Experience
      </div>

      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">careerJourney</span> = {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">totalExperience</span>: 
          <span className="syntax-string">"{experiences.length} positions"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">currentStatus</span>: 
          <span className="syntax-string">"Building the future"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">philosophy</span>: 
          <span className="syntax-string">"Code with purpose, learn with passion"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">nextGoal</span>: 
          <span className="syntax-string">"Creating impact through technology"</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      {experiences.length === 0 ? (
        <div style={{ 
          textAlign: 'center', 
          padding: '40px',
          color: 'var(--text-secondary)',
          fontStyle: 'italic'
        }}>
          <Briefcase size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
          <div>No experience data available.</div>
          <div style={{ fontSize: '14px', marginTop: '8px' }}>
            Professional experience will be displayed here.
          </div>
        </div>
      ) : (
        <div style={{ marginTop: '24px', position: 'relative' }}>
          {/* Timeline line */}
          <div style={{
            position: 'absolute',
            left: '20px',
            top: '20px',
            bottom: '20px',
            width: '2px',
            background: 'linear-gradient(180deg, var(--bg-accent), transparent)',
            zIndex: 1
          }} />

          {sortedExperiences.map((exp, index) => (
            <div key={exp.id} style={{ 
              position: 'relative',
              marginBottom: '32px',
              paddingLeft: '60px'
            }}>
              {/* Timeline dot */}
              <div style={{
                position: 'absolute',
                left: '12px',
                top: '20px',
                width: '16px',
                height: '16px',
                background: 'var(--bg-accent)',
                borderRadius: '50%',
                border: '3px solid var(--bg-primary)',
                zIndex: 2,
                boxShadow: '0 0 0 3px var(--border-color)'
              }} />

              <div style={{
                background: 'var(--bg-tertiary)',
                borderRadius: '12px',
                padding: '20px',
                border: '1px solid var(--border-color)',
                transition: 'all 0.3s ease',
                position: 'relative',
                overflow: 'hidden'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = 'translateX(8px)';
                e.currentTarget.style.boxShadow = '0 8px 25px rgba(0, 0, 0, 0.1)';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateX(0)';
                e.currentTarget.style.boxShadow = 'none';
              }}>
                {/* Header */}
                <div style={{ 
                  display: 'flex', 
                  justifyContent: 'space-between', 
                  alignItems: 'flex-start',
                  marginBottom: '12px',
                  flexWrap: 'wrap',
                  gap: '8px'
                }}>
                  <div>
                    <h3 style={{ 
                      margin: 0,
                      fontSize: '18px',
                      fontWeight: '600',
                      color: 'var(--text-primary)',
                      marginBottom: '4px'
                    }}>
                      {exp.title}
                    </h3>
                    <div style={{ 
                      display: 'flex', 
                      alignItems: 'center', 
                      gap: '8px',
                      flexWrap: 'wrap'
                    }}>
                      <div style={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: '6px',
                        fontSize: '14px',
                        color: 'var(--text-secondary)'
                      }}>
                        <Building size={14} />
                        {exp.website ? (
                          <a 
                            href={exp.website}
                            target="_blank"
                            rel="noopener noreferrer"
                            style={{ 
                              color: 'var(--bg-accent)',
                              textDecoration: 'none',
                              fontWeight: '500'
                            }}
                          >
                            {exp.company}
                            <ExternalLink size={12} style={{ marginLeft: '4px' }} />
                          </a>
                        ) : (
                          <span style={{ fontWeight: '500' }}>{exp.company}</span>
                        )}
                      </div>
                      {exp.location && (
                        <div style={{ 
                          display: 'flex', 
                          alignItems: 'center', 
                          gap: '4px',
                          fontSize: '12px',
                          color: 'var(--text-secondary)'
                        }}>
                          <MapPin size={12} />
                          {exp.location}
                        </div>
                      )}
                    </div>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', gap: '6px' }}>
                    {exp.type && (
                      <span style={{
                        fontSize: '10px',
                        padding: '4px 8px',
                        borderRadius: '12px',
                        background: getTypeColor(exp.type),
                        color: 'white',
                        fontWeight: '500',
                        textTransform: 'uppercase',
                        letterSpacing: '0.5px'
                      }}>
                        {exp.type}
                      </span>
                    )}
                  </div>
                </div>

                {/* Date and Duration */}
                <div style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  gap: '12px',
                  marginBottom: '16px',
                  fontSize: '13px',
                  color: 'var(--text-secondary)'
                }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                    <Calendar size={14} />
                    <span>
                      {formatDate(exp.start_date)} - {exp.end_date ? formatDate(exp.end_date) : 'Present'}
                    </span>
                  </div>
                  <span style={{
                    background: 'var(--bg-secondary)',
                    padding: '2px 8px',
                    borderRadius: '10px',
                    fontSize: '11px',
                    fontWeight: '500'
                  }}>
                    {calculateDuration(exp.start_date, exp.end_date)}
                  </span>
                </div>

                {/* Description */}
                <div style={{
                  fontSize: '14px',
                  color: 'var(--text-primary)',
                  lineHeight: '1.6',
                  marginBottom: '16px'
                }}>
                  {exp.description}
                </div>

                {/* Technologies */}
                {exp.technologies && (
                  <div>
                    <div style={{ 
                      fontSize: '12px', 
                      color: 'var(--text-secondary)',
                      marginBottom: '8px',
                      fontWeight: '500'
                    }}>
                      Technologies & Tools:
                    </div>
                    <div style={{ 
                      display: 'flex', 
                      flexWrap: 'wrap', 
                      gap: '6px'
                    }}>
                      {getTechArray(exp.technologies).map((tech, index) => (
                        <span
                          key={index}
                          style={{
                            fontSize: '11px',
                            padding: '4px 8px',
                            background: 'var(--bg-secondary)',
                            color: 'var(--text-primary)',
                            borderRadius: '4px',
                            border: '1px solid var(--border-color)',
                            fontFamily: 'Consolas, Monaco, monospace'
                          }}
                        >
                          {tech}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      <div style={{ marginTop: '32px' }}>
        <div className="code-block">
          <div className="code-line">
            <span className="syntax-comment">// Career progression algorithm</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">function</span> <span className="syntax-highlight">buildCareer</span>() {'{'}
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-keyword">return</span> experiences
          </div>
          <div className="code-line" style={{ marginLeft: '40px' }}>
            .<span className="syntax-highlight">map</span>(role => role.<span className="syntax-highlight">learnAndGrow</span>())
          </div>
          <div className="code-line" style={{ marginLeft: '40px' }}>
            .<span className="syntax-highlight">filter</span>(exp => exp.<span className="syntax-highlight">addedValue</span>)
          </div>
          <div className="code-line" style={{ marginLeft: '40px' }}>
            .<span className="syntax-highlight">reduce</span>(<span className="syntax-string">'wisdom'</span>, <span className="syntax-string">'expertise'</span>);
          </div>
          <div className="code-line">{'}'}</div>
        </div>
      </div>
    </div>
  );
} 