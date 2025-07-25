'use client';

import React, { useState, useEffect, useMemo } from 'react';
import { Download, UserPlus, Eye, Trash2, Search, ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useAlert } from '@/components/ui/alert-dialog';
import { AdminAPI, UserWithPortfolio } from '@/lib/admin';
import { Loading } from '@/components/loader';
import UserDetailsDialog from './user-details-dialog';
import './style.css';

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<UserWithPortfolio[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingUserId, setDeletingUserId] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 15;
  const [selectedUser, setSelectedUser] = useState<UserWithPortfolio | null>(null);
  const [isUserDetailsOpen, setIsUserDetailsOpen] = useState(false);
  
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

  // Filter and paginate users
  const filteredUsers = useMemo(() => {
    return users.filter(user => {
      const searchLower = searchTerm.toLowerCase();
      const name = getUserDisplayName(user).toLowerCase();
      const email = user.email.toLowerCase();
      const role = getUserRole(user).toLowerCase();
      
      return name.includes(searchLower) || 
             email.includes(searchLower) || 
             role.includes(searchLower);
    });
  }, [users, searchTerm]);

  const totalPages = Math.ceil(filteredUsers.length / itemsPerPage);
  const paginatedUsers = useMemo(() => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    return filteredUsers.slice(startIndex, startIndex + itemsPerPage);
  }, [filteredUsers, currentPage, itemsPerPage]);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleViewUser = (userId: string) => {
    const user = users.find(u => u.id === userId);
    if (user) {
      setSelectedUser(user);
      setIsUserDetailsOpen(true);
    }
  };

  const handleCloseUserDetails = () => {
    setIsUserDetailsOpen(false);
    setSelectedUser(null);
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
          <Loading className="scale-50" backgroundColor="white" />
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

      {/* Search Bar */}
      <div className="search-container">
        <div className="search-input-wrapper">
          <Search className="search-icon" />
          <Input
            type="text"
            placeholder="Search users by name, email, or role..."
            value={searchTerm}
            onChange={(e) => handleSearchChange(e.target.value)}
            className="search-input"
          />
        </div>
        <div className="search-results-info">
          {searchTerm && (
            <span className="text-sm text-gray-600">
              Found {filteredUsers.length} users
            </span>
          )}
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
        
        <div className="table-body scrollable-table-body">
          {paginatedUsers.map((user) => (
            <div key={user.id} className="table-row">
              <div className="col-user">
                <div className="user-cell">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
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

        {/* Pagination Controls */}
        {filteredUsers.length > 0 && (
          <div className="pagination-container">
            <div className="pagination-info">
              Showing {Math.min((currentPage - 1) * itemsPerPage + 1, filteredUsers.length)} to{' '}
              {Math.min(currentPage * itemsPerPage, filteredUsers.length)} of {filteredUsers.length} users
            </div>
            <div className="pagination-controls">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage - 1)}
                disabled={currentPage === 1}
                className="flex items-center gap-1"
              >
                <ChevronLeft className="w-4 h-4" />
                Previous
              </Button>
              
              <div className="pagination-numbers">
                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                  let pageNum;
                  if (totalPages <= 5) {
                    pageNum = i + 1;
                  } else if (currentPage <= 3) {
                    pageNum = i + 1;
                  } else if (currentPage >= totalPages - 2) {
                    pageNum = totalPages - 4 + i;
                  } else {
                    pageNum = currentPage - 2 + i;
                  }
                  
                  return (
                    <Button
                      key={pageNum}
                      variant={currentPage === pageNum ? "default" : "outline"}
                      size="sm"
                      onClick={() => handlePageChange(pageNum)}
                      className="pagination-number"
                    >
                      {pageNum}
                    </Button>
                  );
                })}
              </div>
              
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="flex items-center gap-1"
              >
                Next
                <ChevronRight className="w-4 h-4" />
              </Button>
            </div>
          </div>
        )}

        {paginatedUsers.length === 0 && !loading && (
          <div className="empty-state">
            {searchTerm ? (
              <>
                <p>No users found matching &ldquo;{searchTerm}&rdquo;</p>
                <Button 
                  variant="outline" 
                  onClick={() => handleSearchChange('')}
                  className="mt-2"
                >
                  Clear search
                </Button>
              </>
            ) : (
              <p>No users found.</p>
            )}
          </div>
        )}
      </div>

      {/* User Details Dialog */}
      <UserDetailsDialog
        user={selectedUser}
        isOpen={isUserDetailsOpen}
        onClose={handleCloseUserDetails}
      />
    </div>
  );
};

export default UserManagement;