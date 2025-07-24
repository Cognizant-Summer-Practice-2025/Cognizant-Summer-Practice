'use client';

import React, { useState, useEffect } from 'react';
import { UserOutlined, FileTextOutlined, ProjectOutlined, PlusOutlined } from '@ant-design/icons';
import { AdminAPI } from '@/lib/admin/api';
import { AdminStats } from '@/lib/admin/interfaces';
import './style.css';

interface StatCardProps {
  title: string;
  value: string;
  icon: React.ReactNode;
  loading?: boolean;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, loading }) => (
  <div className="stat-card">
    <div className="stat-info">
      <div className="stat-value">{loading ? '...' : value}</div>
      <div className="stat-title">{title}</div>
    </div>
    <div className="stat-icon">
      {icon}
    </div>
  </div>
);

const StatsCards: React.FC = () => {
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoading(true);
        const adminStats = await AdminAPI.getAdminStats();
        setStats(adminStats);
        setError(null);
      } catch (err) {
        console.error('Error fetching admin stats:', err);
        setError('Failed to load statistics');
        // Fallback to mock data if API fails
        setStats({
          totalUsers: 0,
          activePortfolios: 0,
          totalProjects: 0,
          newThisMonth: 0,
          totalBlogPosts: 0,
          publishedPortfolios: 0,
          draftPortfolios: 0,
          totalViews: 0,
        });
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  const formatNumber = (num: number): string => {
    if (num >= 1000) {
      return (num / 1000).toFixed(1).replace(/\.0$/, '') + 'K';
    }
    return num.toLocaleString();
  };

  const statCards = [
    { 
      title: 'Total Users', 
      value: stats ? formatNumber(stats.totalUsers) : '0', 
      icon: <UserOutlined /> 
    },
    { 
      title: 'Active Portfolios', 
      value: stats ? formatNumber(stats.activePortfolios) : '0', 
      icon: <FileTextOutlined /> 
    },
    { 
      title: 'Total Views', 
      value: stats ? formatNumber(stats.totalViews) : '0', 
      icon: <ProjectOutlined /> 
    },
    { 
      title: 'New This Month', 
      value: stats ? formatNumber(stats.newThisMonth) : '0', 
      icon: <PlusOutlined /> 
    },
  ];

  if (error && !stats) {
    return (
      <div className="stats-section">
        <div className="error-message">
          <p>Failed to load statistics. Please try again later.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="stats-section">
      {statCards.map((stat, index) => (
        <StatCard key={index} {...stat} loading={loading} />
      ))}
    </div>
  );
};

export default StatsCards;