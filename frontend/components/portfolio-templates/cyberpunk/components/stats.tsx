import React from 'react';
import { StatData } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { TrendingUp, Activity, Zap, Database } from 'lucide-react';

interface StatsProps {
  data: StatData[];
}

export function Stats({ data }: StatsProps) {
  if (!data || data.length === 0) {
    return (
      <div className="cyberpunk-stats empty">
        <div className="empty-state">
          <Activity size={48} className="empty-icon" />
          <h3>No statistics available</h3>
          <p>System metrics not initialized</p>
        </div>
      </div>
    );
  }

  const getStatIcon = (label: string) => {
    const lowerLabel = label.toLowerCase();
    if (lowerLabel.includes('project')) return <Database size={20} />;
    if (lowerLabel.includes('experience') || lowerLabel.includes('year')) return <TrendingUp size={20} />;
    if (lowerLabel.includes('client') || lowerLabel.includes('customer')) return <Activity size={20} />;
    return <Zap size={20} />;
  };

  return (
    <div className="cyberpunk-stats">
      <div className="stats-header">
        <h2 className="section-title">
          <Activity size={24} />
          System Metrics
        </h2>
        <div className="metrics-info">
          <span className="metrics-text">Real-time performance data</span>
          <span className="metrics-count">{data.length} indicators</span>
        </div>
      </div>

      <div className="stats-grid">
        {data.map((stat) => (
          <Card key={stat.id} className="stat-card">
            <div className="stat-header">
              <div className="stat-icon">
                {getStatIcon(stat.label)}
              </div>
              <div className="stat-indicator">
                <div className="indicator-dot active"></div>
              </div>
            </div>
            
            <div className="stat-content">
              <div className="stat-value">{stat.value}</div>
              <div className="stat-label">{stat.label}</div>
            </div>
            
            <div className="stat-footer">
              <div className="stat-pulse"></div>
            </div>
          </Card>
        ))}
      </div>

      <div className="system-overview">
        <Card className="overview-card">
          <h3 className="overview-title">System Overview</h3>
          <div className="overview-grid">
            <div className="overview-item">
              <span className="overview-label">Status:</span>
              <span className="overview-value status-optimal">OPTIMAL</span>
            </div>
            <div className="overview-item">
              <span className="overview-label">Uptime:</span>
              <span className="overview-value">99.9%</span>
            </div>
            <div className="overview-item">
              <span className="overview-label">Response Time:</span>
              <span className="overview-value">< 50ms</span>
            </div>
            <div className="overview-item">
              <span className="overview-label">Load:</span>
              <span className="overview-value">Minimal</span>
            </div>
          </div>
        </Card>
      </div>
    </div>
  );
}