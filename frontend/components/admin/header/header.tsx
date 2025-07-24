'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import { Button } from 'antd';
import { EyeOutlined, BarChartOutlined, TeamOutlined } from '@ant-design/icons';
import './style.css';

type TabType = 'statistics' | 'management';

interface AdminHeaderProps {
  activeTab?: TabType;
  onTabChange?: (tab: TabType) => void;
}

const AdminHeader: React.FC<AdminHeaderProps> = ({ 
  activeTab = 'statistics', 
  onTabChange 
}) => {
  const router = useRouter();

  const handleViewSite = () => {
    router.push('/');
  };

  return (
    <div className="admin-header">
      <div className="admin-header-content">
        <div className="admin-logo">
          <h2>GoalKeeper</h2>
        </div>
        
        {/* Navigation Tabs in the middle */}
        <div className="admin-nav-tabs">
          <div className="nav-tabs">
            <button
              className={`nav-tab ${activeTab === 'statistics' ? 'active' : ''}`}
              onClick={() => onTabChange?.('statistics')}
            >
              <BarChartOutlined className="nav-tab-icon" />
              <span className="nav-tab-text">Statistics</span>
            </button>
            <button
              className={`nav-tab ${activeTab === 'management' ? 'active' : ''}`}
              onClick={() => onTabChange?.('management')}
            >
              <TeamOutlined className="nav-tab-icon" />
              <span className="nav-tab-text">Management</span>
            </button>
          </div>
        </div>
      
        <div className="admin-header-actions">
          <Button icon={<EyeOutlined />} className="view-site-btn" onClick={handleViewSite}>
            View Site
          </Button>
          <img className="admin-avatar" src="https://placehold.co/32x32" alt="Admin" />
        </div>
      </div>
    </div>
  );
};

export default AdminHeader;