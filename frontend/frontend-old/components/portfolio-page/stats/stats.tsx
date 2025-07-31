import React from 'react';
import './style.css';

interface StatCardProps {
  value: string;
  label: string;
}

const StatCard: React.FC<StatCardProps> = ({ value, label }) => {
  return (
    <div className="stat-card">
      <div className="stat-value-container">
        <div className="stat-value">{value}</div>
      </div>
      <div className="stat-label-container">
        <div className="stat-label">{label}</div>
      </div>
    </div>
  );
};

const StatsSection = () => {
  const stats = [
    { value: "47", label: "Projects delivered" },
    { value: "125,000+", label: "Lines of code" },
    { value: "1,247", label: "Coffees ☕" },
    { value: "6", label: "Programming years" }
  ];

  return (
    <div className="stats-container">
      {stats.map((stat, index) => (
        <StatCard key={index} value={stat.value} label={stat.label} />
      ))}
    </div>
  );
};

export default StatsSection;