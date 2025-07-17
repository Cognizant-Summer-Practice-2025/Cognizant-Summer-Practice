import React from 'react';
import { UserGrowthChart, ProjectTypesChart, DailyActivityChart, TrendChart } from './admin-charts';
import './style.css';

const ChartsSection: React.FC = () => {
  return (
    <div className="charts-section">
      <div className="charts-header">
        <h2>Analytics Dashboard</h2>
        <p>Comprehensive overview of platform statistics</p>
      </div>
      
      <div className="charts-grid">
        {/* Main trend chart takes full width */}
        <div className="chart-full-width">
          <TrendChart />
        </div>
        
        {/* User growth and activity charts */}
        <div className="chart-half-width">
          <UserGrowthChart />
        </div>
        <div className="chart-half-width">
          <DailyActivityChart />
        </div>
        
        {/* Project distribution chart */}
        <div className="chart-full-width">
          <ProjectTypesChart />
        </div>
      </div>
    </div>
  );
};

export default ChartsSection; 