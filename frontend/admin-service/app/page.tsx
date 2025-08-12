'use client';

import React, { useState } from 'react';
import AdminHeader from '@/components/admin/header/header';
import StatsCards from '@/components/admin/stats-cards/stats-cards';
import ChartsSection from '@/components/admin/charts/charts-section';
import UserManagement from '@/components/admin/user-management/user-management';
import PortfolioManagement from '@/components/admin/portfolio-management/portfolio-management';
import AdminExport from '@/components/admin/export/admin-export';
import { AlertProvider } from '@/components/ui/alert-dialog';
import { AdminGuard } from '@/components/auth/admin-guard';
import './style.css';

type TabType = 'statistics' | 'management' | 'export';

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

  const renderExportSection = () => (
    <div className="admin-section">
      <AdminExport />
    </div>
  );

  return (
    <AlertProvider>
      <AdminGuard>
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
            {activeTab === 'export' && renderExportSection()}
          </div>
          </div>
        </div>
      </AdminGuard>
    </AlertProvider>
  );
};

export default AdminDashboard;