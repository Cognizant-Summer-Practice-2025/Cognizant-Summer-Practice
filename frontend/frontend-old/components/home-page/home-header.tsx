'use client';

import React from 'react';
import { Input, Button, Avatar } from 'antd';
import { SearchOutlined, PlusOutlined, BellOutlined } from '@ant-design/icons';
import './style.css';

const { Search } = Input;

const HomeHeader: React.FC = () => {
  return (
    <div className="home-header">
      <div className="home-header-content">
        <div className="home-logo">
          <h2>GoalKeeper</h2>
        </div>
        
        <div className="home-search">
          <Search
            placeholder="Search portfolios, skills, or names..."
            allowClear
            prefix={<SearchOutlined />}
            className="home-search-input"
            size="middle"
          />
        </div>
        
        <div className="home-header-actions">
          <Button
            icon={<BellOutlined />}
            type="text"
            className="home-notification-btn"
          />
          <Button
            type="primary"
            icon={<PlusOutlined />}
            className="home-publish-btn"
          >
            Publish
          </Button>
          <Avatar 
            src="https://placehold.co/32x32" 
            size={32}
            className="home-avatar"
          />
        </div>
      </div>
    </div>
  );
};

export default HomeHeader; 