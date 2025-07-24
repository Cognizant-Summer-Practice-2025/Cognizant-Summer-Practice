'use client';

import React, { useState, useEffect } from 'react';
import { Button } from 'antd';
import { ExportOutlined, PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { AdminAPI } from '@/lib/admin/api';
import { UserWithPortfolio } from '@/lib/admin/interfaces';
import './style.css';

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<UserWithPortfolio[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        setLoading(true);
        const usersData = await AdminAPI.getUsersWithPortfolios();
        setUsers(usersData);
        setError(null);
      } catch (err) {
        console.error('Error fetching users:', err);
        setError('Failed to load users');
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, []);

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  const getDisplayName = (user: UserWithPortfolio): string => {
    const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
    return fullName || user.username;
  };

  const getStatusBadgeClass = (status: string): string => {
    return status.toLowerCase().replace(' ', '-');
  };

  const handleEditUser = (userId: string) => {
    // TODO: Implement edit user functionality
    console.log('Edit user:', userId);
  };

  const handleDeleteUser = (userId: string) => {
    // TODO: Implement delete user functionality
    console.log('Delete user:', userId);
  };

  const handleExportUsers = () => {
    // TODO: Implement export functionality
    console.log('Export users');
  };

  const handleAddUser = () => {
    // TODO: Implement add user functionality
    console.log('Add new user');
  };

  if (loading) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>User Management</h2>
        </div>
        <div className="loading-container">
          <p>Loading users...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>User Management</h2>
        </div>
        <div className="error-container">
          <p>{error}</p>
          <Button onClick={() => window.location.reload()}>Retry</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>User Management</h2>
        <div className="section-actions">
          <Button icon={<ExportOutlined />} className="export-btn" onClick={handleExportUsers}>
            Export
          </Button>
          <Button type="primary" icon={<PlusOutlined />} className="add-btn" onClick={handleAddUser}>
            Add User
          </Button>
        </div>
      </div>
      
      <div className="table-container">
        <div className="table-header">
          <div className="col-user">User</div>
          <div className="col-email">Email</div>
          <div className="col-joined">Joined</div>
          <div className="col-portfolio">Portfolio Status</div>
          <div className="col-status">Status</div>
          <div className="col-actions">Actions</div>
        </div>
        
        <div className="table-body">
          {users.map((user) => (
            <div key={user.id} className="table-row">
              <div className="col-user">
                <div className="user-cell">
                  {user.avatarUrl ? (
                    <img src={user.avatarUrl} alt={getDisplayName(user)} className="user-avatar" />
                  ) : (
                    <div className="user-avatar-placeholder">
                      {getDisplayName(user).charAt(0).toUpperCase()}
                    </div>
                  )}
                  <div className="user-info">
                    <div className="user-name">{getDisplayName(user)}</div>
                    <div className="user-role">{user.professionalTitle || 'No title'}</div>
                  </div>
                </div>
              </div>
              <div className="col-email">{user.email}</div>
              <div className="col-joined">{formatDate(user.joinedDate)}</div>
              <div className="col-portfolio">
                <span className={`status-badge ${getStatusBadgeClass(user.portfolioStatus)}`}>
                  {user.portfolioStatus}
                </span>
              </div>
              <div className="col-status">
                <span className={`status-badge ${user.isActive ? 'active' : 'inactive'}`}>
                  {user.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>
              <div className="col-actions">
                <Button 
                  type="text" 
                  icon={<EditOutlined />} 
                  size="small" 
                  onClick={() => handleEditUser(user.id)}
                />
                <Button 
                  type="text" 
                  icon={<DeleteOutlined />} 
                  size="small" 
                  className="delete-btn"
                  onClick={() => handleDeleteUser(user.id)}
                />
              </div>
            </div>
          ))}
        </div>
        
        {users.length === 0 && (
          <div className="empty-state">
            <p>No users found.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default UserManagement;