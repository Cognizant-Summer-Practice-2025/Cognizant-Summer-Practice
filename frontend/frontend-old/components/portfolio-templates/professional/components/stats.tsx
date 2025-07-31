"use client";

import React from 'react';
import { Stat } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';

interface StatsProps {
  stats: Stat[];
}

export function Stats({ stats }: StatsProps) {
  if (!stats || stats.length === 0) {
    return null;
  }

  return (
    <div className="prof-stats">
      <div className="prof-stats-grid">
        {stats.map((stat, index) => (
          <Card key={stat.id || index} className="prof-stat-card">
            <div className="prof-stat-content">
              <div className="prof-stat-icon">
                {stat.icon}
              </div>
              <div className="prof-stat-info">
                <div className="prof-stat-value">{stat.value}</div>
                <div className="prof-stat-label">{stat.label}</div>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
} 