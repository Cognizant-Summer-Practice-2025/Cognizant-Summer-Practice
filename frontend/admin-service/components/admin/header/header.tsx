'use client';

import React from 'react';
import Image from 'next/image';
import { useRouter } from 'next/navigation';
import { Button } from 'antd';
import { EyeOutlined, BarChartOutlined, TeamOutlined, DownloadOutlined, LogoutOutlined } from '@ant-design/icons';
import { useUser } from '@/lib/contexts/user-context';
import { customSignOut } from '@/lib/auth/custom-signout';
import { Logger } from '@/lib/logger';
import { getSafeImageUrl } from '@/lib/image/utils';
import './style.css';

type TabType = 'statistics' | 'management' | 'export';

interface AdminHeaderProps {
  activeTab?: TabType;
  onTabChange?: (tab: TabType) => void;
}

const AdminHeader: React.FC<AdminHeaderProps> = ({ 
  activeTab = 'statistics', 
  onTabChange 
}) => {
  const router = useRouter();
  const { user } = useUser();

  const handleViewSite = () => {
    // Redirect to home portfolio service
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    window.location.href = homeServiceUrl;
  };

  const handleSignOut = async () => {
    try {
      await customSignOut();
    } catch (error) {
      Logger.error('Error during sign out', error);
      // Fallback redirect if logout fails
      const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
      window.location.href = homeServiceUrl;
    }
  };

  const getUserDisplayName = (): string => {
    if (!user) return 'Admin';
    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    return user.username || 'Admin';
  };

  const getUserAvatar = (): string => {
    // Debug log user data
    Logger.debug('User data in header:', { 
      hasUser: !!user, 
      avatarUrl: user?.avatarUrl,
      username: user?.username,
      firstName: user?.firstName,
      lastName: user?.lastName 
    });
    
    // Check if user has avatar URL
    if (user?.avatarUrl && user.avatarUrl.trim() !== '') {
      Logger.debug('Using user avatar URL:', user.avatarUrl);
      return getSafeImageUrl(user.avatarUrl);
    }
    
    // Generate avatar based on user name (same as other services)
    const userName = user ? 
      `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.username || 'User' :
      'User';
    const initials = userName
      .split(' ')
      .map(name => name.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
    
    const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&size=32&background=3b82f6&color=ffffff&bold=true`;
    Logger.debug('Generated avatar URL:', avatarUrl);
    return avatarUrl;
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
            <button
              className={`nav-tab ${activeTab === 'export' ? 'active' : ''}`}
              onClick={() => onTabChange?.('export')}
            >
              <DownloadOutlined className="nav-tab-icon" />
              <span className="nav-tab-text">Export</span>
            </button>
          </div>
        </div>
      
        <div className="admin-header-actions">
          <Button icon={<EyeOutlined />} className="view-site-btn" onClick={handleViewSite}>
            View Site
          </Button>
          <div className="flex items-center gap-3">
            <Image 
              className="admin-avatar rounded-full" 
              src={getUserAvatar()} 
              alt={getUserDisplayName()} 
              width={32}
              height={32}
            />
            <Button 
              icon={<LogoutOutlined />} 
              className="disconnect-btn text-red-600 hover:text-red-700 hover:bg-red-50" 
              onClick={handleSignOut}
              type="text"
              title="Disconnect"
              size="small"
            >
              Disconnect
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminHeader;