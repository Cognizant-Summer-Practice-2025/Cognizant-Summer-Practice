import React from 'react';
import { Code2, Star, TrendingUp, Zap } from 'lucide-react';
import { Skill as PortfolioSkill } from '@/lib/portfolio/interfaces';
import { AnimatedNumber, AnimatedProgressBar } from '@/components/ui/animated-number';

type Skill = PortfolioSkill;

interface SkillsProps {
  data: Skill[];
}

export function Skills({ data }: SkillsProps) {
  const skills = React.useMemo(() => data || [], [data]);

  const getSkillColor = (proficiencyLevel: number) => {
    if (proficiencyLevel >= 90) return '#43e97b';
    if (proficiencyLevel >= 70) return '#4facfe';
    if (proficiencyLevel >= 50) return '#ffbd2e';
    return '#ff5f57';
  };

  const getSkillIcon = (category?: string) => {
    switch (category?.toLowerCase()) {
      case 'frontend':
      case 'frontend development':
        return '🎨';
      case 'backend':
      case 'backend development':
        return '⚙️';
      case 'database':
        return '🗄️';
      case 'devops':
        return '🚀';
      case 'mobile':
        return '📱';
      case 'design':
        return '🎯';
      default:
        return '💻';
    }
  };

  const groupedSkills = skills.reduce((acc, skill) => {
    const category = skill.category || 'General';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(skill);
    return acc;
  }, {} as Record<string, Skill[]>);

  return (
    <div className="component-card">
      <div className="component-title">
        <Code2 size={20} />
        Technical Skills
      </div>

      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">skillSet</span> = {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">totalSkills</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>
            <AnimatedNumber value={skills.length} />
          </span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">averageLevel</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>
            <AnimatedNumber 
              value={skills.length > 0 ? Math.round(skills.reduce((acc, skill) => acc + (skill.proficiencyLevel || 0), 0) / skills.length) : 0} 
            />%
          </span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">categories</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>
            <AnimatedNumber value={Object.keys(groupedSkills).length} />
          </span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">learning</span>: 
          <span className="syntax-string">&quot;Always expanding...&quot;</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      {skills.length === 0 ? (
        <div style={{ 
          textAlign: 'center', 
          padding: '40px',
          color: 'var(--text-secondary)',
          fontStyle: 'italic'
        }}>
          <Code2 size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
          <div>No skills data available.</div>
          <div style={{ fontSize: '14px', marginTop: '8px' }}>
            Skills will be displayed here once added.
          </div>
        </div>
      ) : (
        <div style={{ marginTop: '24px' }}>
          {Object.entries(groupedSkills).map(([category, categorySkills]) => (
            <div key={category} style={{ marginBottom: '32px' }}>
              <h4 style={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: '8px', 
                marginBottom: '16px',
                color: 'var(--text-primary)',
                fontSize: '16px',
                fontWeight: '600'
              }}>
                <span style={{ fontSize: '20px' }}>{getSkillIcon(category)}</span>
                {category}
                <span style={{ 
                  fontSize: '12px', 
                  color: 'var(--text-secondary)',
                  fontWeight: 'normal',
                  marginLeft: '8px'
                }}>
                  ({categorySkills.length} skills)
                </span>
              </h4>

              <div style={{ 
                display: 'grid', 
                gap: '16px',
                gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))'
              }}>
                {categorySkills.map((skill) => (
                  <div key={skill.id} style={{
                    padding: '16px',
                    background: 'var(--bg-tertiary)',
                    borderRadius: '8px',
                    border: '1px solid var(--border-color)',
                    transition: 'all 0.3s ease'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-2px)';
                    e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
                  }}>
                    <div style={{ 
                      display: 'flex', 
                      justifyContent: 'space-between', 
                      alignItems: 'center',
                      marginBottom: '8px'
                    }}>
                      <h5 style={{ 
                        margin: 0,
                        fontSize: '14px',
                        fontWeight: '600',
                        color: 'var(--text-primary)'
                      }}>
                        {skill.name}
                      </h5>
                      <div style={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: '6px'
                      }}>
                        <span style={{ 
                          fontSize: '12px',
                          fontWeight: 'bold',
                          color: getSkillColor(skill.proficiencyLevel || 0)
                        }}>
                          <AnimatedNumber value={skill.proficiencyLevel || 0} />%
                        </span>

                      </div>
                    </div>

                    <div className="skill-bar">
                      <AnimatedProgressBar 
                        percentage={skill.proficiencyLevel || 0}
                        className="skill-progress"
                        style={{
                          background: `linear-gradient(90deg, ${getSkillColor(skill.proficiencyLevel || 0)}, ${getSkillColor(skill.proficiencyLevel || 0)}aa)`
                        }}
                      />
                    </div>


                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      <div style={{ marginTop: '32px' }}>
        <div className="code-block">
          <div className="code-line">
          {/* Continuous learning loop */}
          </div>
          <div className="code-line">
            <span className="syntax-keyword">while</span> (alive) {'{'}
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">learnNewSkill</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">practiceDaily</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">shareKnowledge</span>();
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">stayUpdated</span>();
          </div>
          <div className="code-line">{'}'}</div>
        </div>
      </div>

      <div style={{ 
        marginTop: '24px',
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
        gap: '12px'
      }}>
        <div style={{ 
          padding: '12px',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          borderRadius: '8px',
          color: 'white',
          textAlign: 'center'
        }}>
          <Star size={20} style={{ marginBottom: '4px' }} />
          <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
            {skills.filter(s => (s.proficiencyLevel || 0) >= 90).length}
          </div>
          <div style={{ fontSize: '11px', opacity: 0.9 }}>Expert Level</div>
        </div>

        <div style={{ 
          padding: '12px',
          background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
          borderRadius: '8px',
          color: 'white',
          textAlign: 'center'
        }}>
          <TrendingUp size={20} style={{ marginBottom: '4px' }} />
          <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
            {skills.filter(s => {
              const lvl = s.proficiencyLevel || 0;
              return lvl >= 70 && lvl < 90;
            }).length}
          </div>
          <div style={{ fontSize: '11px', opacity: 0.9 }}>Advanced</div>
        </div>

        <div style={{ 
          padding: '12px',
          background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
          borderRadius: '8px',
          color: 'white',
          textAlign: 'center'
        }}>
          <Zap size={20} style={{ marginBottom: '4px' }} />
          <div style={{ fontSize: '20px', fontWeight: 'bold' }}>
            {skills.filter(s => {
              const lvl = s.proficiencyLevel || 0;
              return lvl >= 50 && lvl < 70;
            }).length}
          </div>
          <div style={{ fontSize: '11px', opacity: 0.9 }}>Intermediate</div>
        </div>
      </div>
    </div>
  );
} 