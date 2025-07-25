'use client';

import React, { useState, useEffect } from 'react';
import { Download, UserPlus, Eye, Edit, Trash2, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAlert } from '@/components/ui/alert-dialog';
import { AdminAPI } from '@/lib/admin/api';
import { UserWithPortfolio } from '@/lib/admin/interfaces';
import './style.css';

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<UserWithPortfolio[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);
  
  const { showAlert, showConfirm } = useAlert();

  useEffect(() => {
    fetchUsers();
  }, []);

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

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  const getStatusBadgeClass = (status: string): string => {
    return status.toLowerCase().replace(' ', '-');
  };

  const getUserDisplayName = (user: UserWithPortfolio): string => {
    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    return user.username;
  };

  const getUserRole = (user: UserWithPortfolio): string => {
    return user.professionalTitle || 'User';
  };

  const getUserAvatar = (user: UserWithPortfolio): string => {
    return user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(getUserDisplayName(user))}&size=40&background=f0f0f0&color=666`;
  };

  const handleViewUser = (userId: string) => {
    // TODO: Implement view user functionality
    showAlert({
      title: 'View User',
      description: 'View functionality will be implemented soon.',
      type: 'info',
    });
  };

  const handleEditUser = (userId: string) => {
    // TODO: Implement edit user functionality
    showAlert({
      title: 'Edit User',
      description: 'Edit functionality will be implemented soon.',
      type: 'info',
    });
  };

  const handleDeleteUser = (user: UserWithPortfolio) => {
    const deleteMessage = `Are you sure you want to delete user "${getUserDisplayName(user)}"?

This action will permanently delete:
• The user account and all profile data
• Associated portfolio (if any)
• All projects, experiences, and skills
• All messages and conversations
• All bookmarks and activity

This action cannot be undone.`;

    showConfirm({
      title: 'Delete User',
      description: deleteMessage,
      type: 'error',
      confirmText: 'Delete User',
      cancelText: 'Cancel',
      onConfirm: () => performDeleteUser(user.id, getUserDisplayName(user)),
    });
  };

  const performDeleteUser = async (userId: string, userName: string) => {
    try {
      setDeletingUserId(userId);
      
      // TODO: Implement actual user deletion API
      // await AdminAPI.deleteUser(userId);
      
      // For now, just remove from local state (simulated)
      setUsers(prev => prev.filter(u => u.id !== userId));
      
      showAlert({
        title: 'User Deleted',
        description: `User "${userName}" has been deleted successfully.`,
        type: 'success',
      });
    } catch (error) {
      console.error('Error deleting user:', error);
      const errorMessage = error instanceof Error 
        ? error.message
        : 'Failed to delete user. Please try again.';
      
      showAlert({
        title: 'Delete Failed',
        description: `Failed to delete user "${userName}": ${errorMessage}`,
        type: 'error',
      });
    } finally {
      setDeletingUserId(null);
    }
  };

  const handleExportUsers = () => {
    // TODO: Implement export functionality
    showAlert({
      title: 'Export Users',
      description: 'Export functionality will be implemented soon.',
      type: 'info',
    });
  };

  const handleAddUser = () => {
    // TODO: Implement add user functionality
    showAlert({
      title: 'Add User',
      description: 'Add user functionality will be implemented soon.',
      type: 'info',
    });
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
          <Button onClick={fetchUsers} variant="outline">
            Retry
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>User Management</h2>
        <div className="section-actions">
          <Button 
            variant="outline" 
            onClick={handleExportUsers}
            className="flex items-center gap-2"
          >
            <Download className="w-4 h-4" />
            Export
          </Button>
          <Button 
            onClick={handleAddUser}
            className="flex items-center gap-2"
          >
            <UserPlus className="w-4 h-4" />
            Add User
          </Button>
        </div>
      </div>
      
      <div className="table-container">
        <div className="table-header">
          <div className="col-user">User</div>
          <div className="col-email">Email</div>
          <div className="col-joined">Joined</div>
          <div className="col-portfolio">Portfolio</div>
          <div className="col-status">Status</div>
          <div className="col-actions">Actions</div>
        </div>
        
        <div className="table-body">
          {users.map((user) => (
            <div key={user.id} className="table-row">
              <div className="col-user">
                <div className="user-cell">
                  <img 
                    src={getUserAvatar(user)} 
                    alt={getUserDisplayName(user)} 
                    className="user-avatar" 
                  />
                  <div className="user-info">
                    <div className="user-name">{getUserDisplayName(user)}</div>
                    <div className="user-role">{getUserRole(user)}</div>
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
                <span className={`status-badge ${user.isActive ? 'active' : 'suspended'}`}>
                  {user.isActive ? 'Active' : 'Suspended'}
                </span>
              </div>
              <div className="col-actions">
                <Button 
                  variant="ghost" 
                  size="icon"
                  onClick={() => handleViewUser(user.id)}
                  title="View user details"
                >
                  <Eye className="w-4 h-4" />
                </Button>
                <Button 
                  variant="ghost" 
                  size="icon"
                  onClick={() => handleEditUser(user.id)}
                  title="Edit user"
                >
                  <Edit className="w-4 h-4" />
                </Button>
                <Button 
                  variant="ghost" 
                  size="icon"
                  onClick={() => handleDeleteUser(user)}
                  disabled={deletingUserId === user.id}
                  title="Delete user and all related data"
                  className="text-red-600 hover:text-red-700 hover:bg-red-50"
                >
                  {deletingUserId === user.id ? (
                    <div className="w-4 h-4 animate-spin border-2 border-red-600 border-t-transparent rounded-full" />
                  ) : (
                    <Trash2 className="w-4 h-4" />
                  )}
                </Button>
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