import React from 'react';
import { StatData } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { AnimatedNumber } from '@/components/ui/animated-number';

interface StatsProps {
  stats: StatData[];
}

export function Stats({ stats }: StatsProps) {
  if (!stats || stats.length === 0) {
    return null;
  }

  return (
    <div className="modern-stats">
      {stats.map((stat) => (
        <Card key={stat.id} className="modern-stat-card">
          <CardContent className="p-0">
            <div className="text-center">
              <h3 className="modern-stat-value">
                <AnimatedNumber value={stat.value} />
              </h3>
              <p className="modern-stat-label">{stat.label}</p>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
} 