import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Trophy, Star, Zap, Target } from 'lucide-react';

interface StatsProps {
  data: PortfolioDataFromDB;
}

export function Stats({ data }: StatsProps) {
  const { stats } = data;
  const [animatedStats, setAnimatedStats] = useState<{ [key: string]: number }>({});

  useEffect(() => {
    // Animate stats counters
    stats.forEach((stat, index) => {
      const targetValue = parseInt(stat.value) || 0;
      const duration = 1500 + (index * 200); // Stagger animations
      const steps = 50;
      const increment = targetValue / steps;
      let currentStep = 0;

      const timer = setInterval(() => {
        currentStep++;
        const currentValue = Math.min(Math.floor(increment * currentStep), targetValue);
        
        setAnimatedStats(prev => ({
          ...prev,
          [stat.id]: currentValue
        }));
        
        if (currentStep >= steps) {
          clearInterval(timer);
        }
      }, duration / steps);

      return () => clearInterval(timer);
    });
  }, [stats]);

  const getStatIcon = (label: string, icon?: string) => {
    if (icon) return icon;
    
    const labelLower = label.toLowerCase();
    if (labelLower.includes('project')) return '🚀';
    if (labelLower.includes('skill')) return '🎯';
    if (labelLower.includes('view') || labelLower.includes('visitor')) return '👁️';
    if (labelLower.includes('like') || labelLower.includes('heart')) return '❤️';
    if (labelLower.includes('star')) return '⭐';
    if (labelLower.includes('follow')) return '👥';
    return '🏆';
  };

  const getPixelColor = (index: number) => {
    const colors = ['var(--retro-blue)', 'var(--retro-pink)', 'var(--retro-green)', 'var(--retro-yellow)'];
    return colors[index % colors.length];
  };

  return (
    <div className="retro-stats">
      <div className="section-header">
        <h2 className="section-title">
          <Trophy className="pixel-icon" size={24} />
          POWER-UPS & STATS
        </h2>
        <div className="pixel-border"></div>
      </div>

      <div className="stats-grid">
        {stats.map((stat, index) => (
          <Card key={stat.id} className="stat-card" style={{ '--accent-color': getPixelColor(index) } as React.CSSProperties}>
            <div className="stat-header">
              <div className="stat-icon-container">
                <span className="stat-icon pixel-emoji">
                  {getStatIcon(stat.label, stat.icon)}
                </span>
                <div className="icon-glow"></div>
              </div>
              <div className="stat-sparkles">
                <Star className="sparkle" size={12} />
                <Star className="sparkle" size={8} />
                <Star className="sparkle" size={10} />
              </div>
            </div>

            <div className="stat-content">
              <div className="stat-value-container">
                <span className="stat-value">
                  {animatedStats[stat.id]?.toLocaleString() || '0'}
                </span>
                <div className="value-shadow">
                  {animatedStats[stat.id]?.toLocaleString() || '0'}
                </div>
              </div>
              
              <h3 className="stat-label">{stat.label}</h3>
              
              <div className="progress-bar">
                <div 
                  className="progress-fill" 
                  style={{ 
                    width: `${Math.min((animatedStats[stat.id] || 0) / 100 * 100, 100)}%`,
                    animationDelay: `${index * 0.2}s`
                  }}
                ></div>
                <div className="progress-shine"></div>
              </div>
            </div>

            <div className="stat-effects">
              <div className="pixel-particle"></div>
              <div className="pixel-particle"></div>
              <div className="pixel-particle"></div>
            </div>

            <div className="card-border">
              <div className="border-corner top-left"></div>
              <div className="border-corner top-right"></div>
              <div className="border-corner bottom-left"></div>
              <div className="border-corner bottom-right"></div>
            </div>
          </Card>
        ))}
      </div>

      <div className="bonus-stats">
        <div className="bonus-header">
          <Zap className="pixel-icon" size={20} />
          <span>BONUS MULTIPLIERS</span>
        </div>
        <div className="multiplier-list">
          <div className="multiplier-item">
            <span className="multiplier-name">Code Quality</span>
            <span className="multiplier-value">x2.5</span>
          </div>
          <div className="multiplier-item">
            <span className="multiplier-name">Team Synergy</span>
            <span className="multiplier-value">x1.8</span>
          </div>
          <div className="multiplier-item">
            <span className="multiplier-name">Innovation</span>
            <span className="multiplier-value">x3.0</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Stats;