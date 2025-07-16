import React from 'react';
import AdminHeader from '@/components/admin/header/header';
import StatsCards from '@/components/admin/stats-cards/stats-cards';
import UserManagement from '@/components/admin/user-management/user-management';
import PortfolioManagement from '@/components/admin/portfolio-management/portfolio-management';
import './style.css';

const AdminDashboard: React.FC = () => {
  return (
    <div className="admin-page">
      <AdminHeader />
      <div className="admin-main">
        <div className="admin-container">
          <div className="admin-header-section">
            <h1 className="admin-title">Admin Dashboard</h1>
            <p className="admin-subtitle">Manage users, portfolios, and platform settings</p>
          </div>
          
          <StatsCards />
          <UserManagement />
          <PortfolioManagement />
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;