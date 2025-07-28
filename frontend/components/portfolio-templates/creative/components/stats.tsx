import React, { useEffect, useState } from 'react';
import { StatData } from '@/lib/portfolio/interfaces';

interface StatsProps {
  stats: StatData[];
}

export function Stats({ stats }: StatsProps) {
  const [animatedStats, setAnimatedStats] = useState<Record<string, number>>({});

  useEffect(() => {
    const duration = 2000; // 2 seconds
    const steps = 60;
    const stepDuration = duration / steps;

    // Convert StatData array to targets
    const targets: Record<string, number> = {};
    stats.forEach(stat => {
      const numValue = parseInt(stat.value) || 0;
      targets[stat.id] = numValue;
    });

    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      const progress = currentStep / steps;

      const newAnimatedStats: Record<string, number> = {};
      Object.entries(targets).forEach(([key, target]) => {
        newAnimatedStats[key] = Math.floor(target * progress);
      });
      
      setAnimatedStats(newAnimatedStats);

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
        {stats.map((stat, index) => (
          <div key={stat.id} className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-highlight">{stat.label.toLowerCase().replace(/\s+/g, '')}</span>: 
            <span style={{ color: '#79c0ff', fontWeight: 'bold' }}>{animatedStats[stat.id] || 0}</span>
            {index < stats.length - 1 ? ',' : ''}
        </div>
        ))}
        <div className="code-line">{'}'};</div>
      </div>

      <div style={{ 
        marginTop: '24px', 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
        gap: '16px' 
      }}>
        {stats.map((stat, index) => (
          <div key={stat.id} className={`stat-card gradient-bg-${(index % 4) + 1}`} style={{ 
          padding: '16px', 
          borderRadius: '8px', 
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          gap: '12px'
        }}>
            <span style={{ fontSize: '20px' }}>{stat.icon || '📊'}</span>
          <div>
            <div style={{ fontSize: '24px', fontWeight: 'bold' }}>
                {animatedStats[stat.id] || 0}
            </div>
              <div style={{ fontSize: '12px', opacity: 0.9 }}>{stat.label}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
} 