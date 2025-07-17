import React from 'react';
import { UserOutlined, FileTextOutlined, ProjectOutlined, PlusOutlined } from '@ant-design/icons';
import './style.css';

interface StatCardProps {
  title: string;
  value: string;
  icon: React.ReactNode;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon }) => (
  <div className="stat-card">
    <div className="stat-info">
      <div className="stat-value">{value}</div>
      <div className="stat-title">{title}</div>
    </div>
    <div className="stat-icon">
      {icon}
    </div>
  </div>
);

const StatsCards: React.FC = () => {
  const stats = [
    { title: 'Total Users', value: '1,247', icon: <UserOutlined /> },
    { title: 'Active Portfolios', value: '892', icon: <FileTextOutlined /> },
    { title: 'Total Projects', value: '3,456', icon: <ProjectOutlined /> },
    { title: 'New This Month', value: '156', icon: <PlusOutlined /> },
  ];

  return (
    <div className="stats-section">
      {stats.map((stat, index) => (
        <StatCard key={index} {...stat} />
      ))}
    </div>
  );
};

export default StatsCards;