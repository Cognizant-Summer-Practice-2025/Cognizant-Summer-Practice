import React, { useEffect, useState } from 'react';
import { TrendingUp, Users, Award, Coffee } from 'lucide-react';

interface StatsData {
  experience?: number;
  projects?: number;
  clients?: number;
  [key: string]: any;
}

interface StatsProps {
  stats: StatsData;
}

export function Stats({ stats }: StatsProps) {
  const [animatedStats, setAnimatedStats] = useState({
    experience: 0,
    projects: 0,
    clients: 0
  });

  useEffect(() => {
    const duration = 2000; // 2 seconds
    const steps = 60;
    const stepDuration = duration / steps;

    const targets = {
      experience: stats.experience || 3,
      projects: stats.projects || 50,
      clients: stats.clients || 100
    };

    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const progress = currentStep / steps;

      setAnimatedStats({
        experience: Math.floor(targets.experience * progress),
        projects: Math.floor(targets.projects * progress),
        clients: Math.floor(targets.clients * progress)
      });

      if (currentStep >= steps) {
        clearInterval(timer);
        setAnimatedStats(targets);
      }
    }, stepDuration);

    return () => clearInterval(timer);
  }, [stats]);

  return (
    <div className="creative-stats">
      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">achievements</span> = {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">yearsOfExperience</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{animatedStats.experience}+</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">projectsCompleted</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{animatedStats.projects}+</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">happyClients</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{animatedStats.clients}+</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">coffeeConsumed</span>: 
          <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>∞</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      <div style={{ 
        marginTop: '24px', 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
        gap: '16px' 
      }}>
        <div className="stat-card gradient-bg-1" style={{ 
          padding: '16px', 
          borderRadius: '8px', 
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
          <TrendingUp size={24} />
          <div>
            <div style={{ fontSize: '24px', fontWeight: 'bold' }}>
              {animatedStats.experience}+
            </div>
            <div style={{ fontSize: '12px', opacity: 0.9 }}>Years Experience</div>
          </div>
        </div>

        <div className="stat-card gradient-bg-2" style={{ 
          padding: '16px', 
          borderRadius: '8px', 
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
          <Award size={24} />
          <div>
            <div style={{ fontSize: '24px', fontWeight: 'bold' }}>
              {animatedStats.projects}+
            </div>
            <div style={{ fontSize: '12px', opacity: 0.9 }}>Projects Completed</div>
          </div>
        </div>

        <div className="stat-card gradient-bg-3" style={{ 
          padding: '16px', 
          borderRadius: '8px', 
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
          <Users size={24} />
          <div>
            <div style={{ fontSize: '24px', fontWeight: 'bold' }}>
              {animatedStats.clients}+
            </div>
            <div style={{ fontSize: '12px', opacity: 0.9 }}>Happy Clients</div>
          </div>
        </div>

        <div className="stat-card gradient-bg-4" style={{ 
          padding: '16px', 
          borderRadius: '8px', 
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
          <Coffee size={24} />
          <div>
            <div style={{ fontSize: '24px', fontWeight: 'bold' }}>∞</div>
            <div style={{ fontSize: '12px', opacity: 0.9 }}>Coffee Cups</div>
          </div>
        </div>
      </div>
    </div>
  );
} 