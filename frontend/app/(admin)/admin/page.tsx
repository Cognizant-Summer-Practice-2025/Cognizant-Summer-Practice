'use client';

import React, { useState } from 'react';
import AdminHeader from '@/components/admin/header/header';
import StatsCards from '@/components/admin/stats-cards/stats-cards';
import ChartsSection from '@/components/admin/charts/charts-section';
import UserManagement from '@/components/admin/user-management/user-management';
import PortfolioManagement from '@/components/admin/portfolio-management/portfolio-management';
import './style.css';

type TabType = 'statistics' | 'management';

const AdminDashboard: React.FC = () => {
  const [activeTab, setActiveTab] = useState<TabType>('statistics');

  const renderStatisticsSection = () => (
    <div className="admin-section">
      <StatsCards />
      <ChartsSection />
    </div>
  );

  const renderManagementSection = () => (
    <div className="admin-section">
      <UserManagement />
      <PortfolioManagement />
    </div>
  );

  return (
    <div className="admin-page">
      <AdminHeader 
        activeTab={activeTab} 
        onTabChange={setActiveTab} 
      />

      {/* Content Sections */}
      <div className="admin-main">
        <div className="admin-container">
          {activeTab === 'statistics' && renderStatisticsSection()}
          {activeTab === 'management' && renderManagementSection()}
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;