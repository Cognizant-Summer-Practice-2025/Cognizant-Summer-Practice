import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { BarChart3, Activity, TrendingUp, Database } from 'lucide-react';

interface StatsProps {
  data: PortfolioDataFromDB;
}

export function Stats({ data }: StatsProps) {
  const { stats } = data;
  const [animatedValues, setAnimatedValues] = useState<Record<string, number>>({});
  const [commandOutput, setCommandOutput] = useState<string[]>([]);

  useEffect(() => {
    // Animate counter values
    stats.forEach(stat => {
      const targetValue = parseInt(stat.value) || 0;
      let currentValue = 0;
      const increment = Math.ceil(targetValue / 50);
      
      const timer = setInterval(() => {
        if (currentValue < targetValue) {
          currentValue = Math.min(currentValue + increment, targetValue);
          setAnimatedValues(prev => ({
            ...prev,
            [stat.id]: currentValue
          }));
        } else {
          clearInterval(timer);
        }
      }, 30);
    });
  }, [stats]);

  useEffect(() => {
    // Simulate command execution for stats
    const commands = [
      '$ ps aux | grep portfolio',
      '$ df -h /portfolio',
      '$ uptime',
      '$ cat /proc/stats',
      ''
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < commands.length) {
        setCommandOutput(prev => [...prev, commands[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 500);

    return () => clearInterval(interval);
  }, []);

  const getProgressBarWidth = (value: string) => {
    const num = parseInt(value) || 0;
    const max = Math.max(...stats.map(s => parseInt(s.value) || 0));
    return (num / max) * 100;
  };

  return (
    <div className="terminal-stats">
      <div className="stats-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">portfolio-stats.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="command-output">
            {commandOutput.map((line, index) => (
              <div key={index} className="output-line">
                {line}
              </div>
            ))}
          </div>
          
          <div className="stats-display">
            <div className="stats-header">
              <BarChart3 className="stats-icon" size={16} />
              <span>Portfolio Statistics</span>
            </div>
            
            <div className="stats-table">
              <div className="table-header">
                <span className="col-metric">METRIC</span>
                <span className="col-value">VALUE</span>
                <span className="col-graph">GRAPH</span>
                <span className="col-status">STATUS</span>
              </div>
              
              {stats.map(stat => (
                <div key={stat.id} className="table-row">
                  <span className="col-metric">
                    <span className="metric-icon">{stat.icon || '📊'}</span>
                    {stat.label}
                  </span>
                  <span className="col-value">
                    {animatedValues[stat.id]?.toLocaleString() || '0'}
                  </span>
                  <span className="col-graph">
                    <div className="progress-bar">
                      <div 
                        className="progress-fill"
                        style={{ width: `${getProgressBarWidth(stat.value)}%` }}
                      ></div>
                    </div>
                  </span>
                  <span className="col-status">
                    <span className="status-indicator online"></span>
                    ACTIVE
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="system-monitor">
        <div className="monitor-header">
          <Activity className="monitor-icon" size={16} />
          <span>System Monitor</span>
        </div>
        
        <div className="monitor-grid">
          <div className="monitor-item">
            <div className="monitor-label">CPU Usage</div>
            <div className="monitor-bar">
              <div className="bar-fill cpu" style={{ width: '67%' }}></div>
            </div>
            <div className="monitor-value">67%</div>
          </div>
          
          <div className="monitor-item">
            <div className="monitor-label">Memory</div>
            <div className="monitor-bar">
              <div className="bar-fill memory" style={{ width: '42%' }}></div>
            </div>
            <div className="monitor-value">42%</div>
          </div>
          
          <div className="monitor-item">
            <div className="monitor-label">Storage</div>
            <div className="monitor-bar">
              <div className="bar-fill storage" style={{ width: '23%' }}></div>
            </div>
            <div className="monitor-value">23%</div>
          </div>
          
          <div className="monitor-item">
            <div className="monitor-label">Network</div>
            <div className="monitor-bar">
              <div className="bar-fill network" style={{ width: '89%' }}></div>
            </div>
            <div className="monitor-value">89%</div>
          </div>
        </div>
      </div>

      <div className="performance-metrics">
        <div className="metrics-header">
          <TrendingUp className="metrics-icon" size={16} />
          <span>Performance Metrics</span>
        </div>
        
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-title">Portfolio Uptime</div>
            <div className="metric-value">99.9%</div>
            <div className="metric-trend up">↗ +0.1%</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Response Time</div>
            <div className="metric-value">0.23s</div>
            <div className="metric-trend down">↘ -0.05s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Success Rate</div>
            <div className="metric-value">98.7%</div>
            <div className="metric-trend up">↗ +1.2%</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Total Requests</div>
            <div className="metric-value">{stats.reduce((sum, stat) => sum + (parseInt(stat.value) || 0), 0).toLocaleString()}</div>
            <div className="metric-trend up">↗ +{stats.length * 100}</div>
          </div>
        </div>
      </div>

      <div className="log-viewer">
        <div className="log-header">
          <Database className="log-icon" size={16} />
          <span>Activity Log</span>
        </div>
        
        <div className="log-content">
          <div className="log-line">
            <span className="log-timestamp">[{new Date().toISOString()}]</span>
            <span className="log-level info">INFO</span>
            <span className="log-message">Portfolio statistics loaded successfully</span>
          </div>
          <div className="log-line">
            <span className="log-timestamp">[{new Date(Date.now() - 60000).toISOString()}]</span>
            <span className="log-level success">SUCCESS</span>
            <span className="log-message">Database connection established</span>
          </div>
          <div className="log-line">
            <span className="log-timestamp">[{new Date(Date.now() - 120000).toISOString()}]</span>
            <span className="log-level info">INFO</span>
            <span className="log-message">User session initiated</span>
          </div>
          <div className="log-line">
            <span className="log-timestamp">[{new Date(Date.now() - 180000).toISOString()}]</span>
            <span className="log-level success">SUCCESS</span>
            <span className="log-message">Portfolio system online</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Stats;