import React from 'react';
import { StatData } from '@/lib/portfolio';

interface StatsProps {
  stats: StatData[];
}

export function Stats({ stats }: StatsProps) {
  return (
    <section className="gb-stats">
      <div className="stats-grid">
        {stats.map((stat) => (
          <div key={stat.id} className="stat-card">
            {stat.icon && (
              <div className="stat-icon">{stat.icon}</div>
            )}
            <div className="stat-content">
              <div className="stat-value">{stat.value}</div>
              <div className="stat-label">{stat.label}</div>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
} 