import React from 'react';
import { Button } from 'antd';
import { ExportOutlined, PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import './style.css';

const UserManagement: React.FC = () => {
  const users = [
    {
      id: 1,
      name: 'John Doe',
      role: 'Full Stack Developer',
      email: 'john@example.com',
      joined: 'Jan 15, 2024',
      portfolioStatus: 'Published',
      status: 'Active',
      avatar: 'https://placehold.co/40x40'
    },
    {
      id: 2,
      name: 'Sarah Wilson',
      role: 'UI/UX Designer',
      email: 'sarah@example.com',
      joined: 'Feb 3, 2024',
      portfolioStatus: 'Draft',
      status: 'Active',
      avatar: null
    },
    {
      id: 3,
      name: 'Mike Chen',
      role: 'Data Scientist',
      email: 'mike@example.com',
      joined: 'Jan 28, 2024',
      portfolioStatus: 'Published',
      status: 'Suspended',
      avatar: 'https://placehold.co/40x40'
    }
  ];

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>User Management</h2>
        <div className="section-actions">
          <Button icon={<ExportOutlined />} className="export-btn">
            Export
          </Button>
          <Button type="primary" icon={<PlusOutlined />} className="add-btn">
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
                  {user.avatar ? (
                    <img src={user.avatar} alt={user.name} className="user-avatar" />
                  ) : (
                    <div className="user-avatar-placeholder" />
                  )}
                  <div className="user-info">
                    <div className="user-name">{user.name}</div>
                    <div className="user-role">{user.role}</div>
                  </div>
                </div>
              </div>
              <div className="col-email">{user.email}</div>
              <div className="col-joined">{user.joined}</div>
              <div className="col-portfolio">
                <span className={`status-badge ${user.portfolioStatus.toLowerCase()}`}>
                  {user.portfolioStatus}
                </span>
              </div>
              <div className="col-status">
                <span className={`status-badge ${user.status.toLowerCase()}`}>
                  {user.status}
                </span>
              </div>
              <div className="col-actions">
                <Button type="text" icon={<EditOutlined />} size="small" />
                <Button type="text" icon={<DeleteOutlined />} size="small" className="delete-btn" />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default UserManagement;