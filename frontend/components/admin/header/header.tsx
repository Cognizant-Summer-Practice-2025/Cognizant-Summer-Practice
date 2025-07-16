import React from 'react';
import { Button } from 'antd';
import { EyeOutlined } from '@ant-design/icons';
import './style.css';

const AdminHeader: React.FC = () => {
  return (
    <div className="admin-header">
      <div className="admin-header-content">
        <div className="admin-logo">
          <h2>GoalKeeper</h2>
        </div>
      
        
        <div className="admin-header-actions">
          <Button icon={<EyeOutlined />} className="view-site-btn">
            View Site
          </Button>
          <img className="admin-avatar" src="https://placehold.co/32x32" alt="Admin" />
        </div>
      </div>
    </div>
  );
};

export default AdminHeader;