'use client';

import React, { useState, useEffect } from 'react';
import { Trash2, User, MessageCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { Table, Column } from '@/components/ui/table';
import { useAlert } from '@/components/ui/alert-dialog';
import { message } from '@/components/ui/toast';
import { AdminAPI } from '@/lib/admin';
import { Logger } from '@/lib/logger';
import './style.css';

interface UserReport {
  id: string;
  userId: string;
  reportedByUserId: string;
  reason: string;
  createdAt: string;
  user?: {
    id: string;
    username: string;
    email: string;
    firstName?: string;
    lastName?: string;
  };
}

interface MessageReport {
  id: string;
  messageId: string;
  reportedByUserId: string;
  reason: string;
  createdAt: string;
  message?: {
    id: string;
    content: string;
    senderId: string;
    receiverId: string;
    createdAt: string;
  };
}

const ReportsManagement: React.FC = () => {
  const [userReports, setUserReports] = useState<UserReport[]>([]);
  const [messageReports, setMessageReports] = useState<MessageReport[]>([]);
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('user-reports');
  const { showConfirm } = useAlert();

  useEffect(() => {
    fetchReports();
  }, []);

  const fetchReports = async () => {
    setLoading(true);
    try {
      // Use AdminAPI instead of direct fetch calls
      const [userReportsData, messageReportsData] = await Promise.all([
        AdminAPI.getAllUserReports().catch(error => {
          Logger.error('Error fetching user reports:', error);
          message.error('Failed to fetch user reports');
          return [];
        }),
        AdminAPI.getAllMessageReports().catch(error => {
          Logger.error('Error fetching message reports:', error);
          message.error('Failed to fetch message reports');
          return [];
        })
      ]);

      setUserReports(userReportsData);
      setMessageReports(messageReportsData);
    } catch (error) {
      Logger.error('Error fetching reports:', error);
      message.error('Failed to fetch reports');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteUser = (userId: string, username: string) =>
    showConfirm({
      title: 'Delete User - Cascade Deletion',
      description: `Are you sure you want to permanently delete the account for "${username}"?

This will delete ALL user data across all services:
• User account and profile data
• All portfolios, projects, and experiences  
• All messages and conversations
• All reports and bookmarks

This action cannot be undone and will remove ALL user data from the entire system.`,
      type: 'error',
      confirmText: 'Delete All User Data',
      cancelText: 'Cancel',
      onConfirm: async () => {
        try {
          await AdminAPI.deleteUser(userId);
          message.success('User account and all associated data deleted successfully');
          // Remove reports for deleted user
          setUserReports(prev => prev.filter(report => report.userId !== userId));
          fetchReports(); // Refresh to get updated data
        } catch (error) {
          Logger.error('Error deleting user:', error);
          message.error('Failed to delete user account');
        }
      }
    });

  const userReportsColumns: Column<UserReport>[] = [
    {
      title: 'Reported User',
      key: 'reportedUser',
      render: (_, record) => (
        <div>
          <div className="font-medium">
            {record.user?.firstName && record.user?.lastName 
              ? `${record.user.firstName} ${record.user.lastName}`
              : record.user?.username || 'Unknown User'
            }
          </div>
          <div className="text-sm text-muted-foreground">{record.user?.email}</div>
        </div>
      ),
    },
    {
      title: 'Reason',
      key: 'reason',
      dataIndex: 'reason',
      render: (reason: string) => (
        <Badge variant="secondary" className="badge-orange">
          {reason}
        </Badge>
      ),
    },
    {
      title: 'Reported At',
      key: 'createdAt',
      dataIndex: 'createdAt',
      render: (date: string) => new Date(date).toLocaleString(),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Button
          variant="destructive"
          size="sm"
          onClick={() => handleDeleteUser(record.userId, record.user?.username || 'Unknown')}
          className="gap-2"
        >
          <Trash2 className="h-4 w-4" />
          Delete Account
        </Button>
      ),
    },
  ];

  const messageReportsColumns: Column<MessageReport>[] = [
    {
      title: 'Message Content',
      key: 'messageContent',
      render: (_, record) => (
        <div className="max-w-xs">
          <div className="truncate font-medium">
            {record.message?.content || 'Message content unavailable'}
          </div>
          <div className="text-sm text-muted-foreground">
            Sent: {record.message?.createdAt ? new Date(record.message.createdAt).toLocaleString() : 'Unknown'}
          </div>
        </div>
      ),
    },
    {
      title: 'Reason',
      key: 'reason',
      dataIndex: 'reason',
      render: (reason: string) => (
        <Badge variant="secondary" className="badge-red">
          {reason}
        </Badge>
      ),
    },
    {
      title: 'Reported At',
      key: 'createdAt',
      dataIndex: 'createdAt',
      render: (date: string) => new Date(date).toLocaleString(),
    },
    {
      title: 'Message ID',
      key: 'messageId',
      dataIndex: 'messageId',
      render: (messageId: string) => (
        <code className="text-xs bg-muted px-2 py-1 rounded font-mono">
          {messageId.substring(0, 8)}...
        </code>
      ),
    },
  ];

  return (
    <div className="reports-management">
      <div className="reports-header">
        <h2>Reports Management</h2>
        <p>Review and manage reported users and messages</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="reports-tabs">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="user-reports" className="flex items-center gap-2">
            <User className="h-4 w-4" />
            User Reports ({userReports.length})
          </TabsTrigger>
          <TabsTrigger value="message-reports" className="flex items-center gap-2">
            <MessageCircle className="h-4 w-4" />
            Message Reports ({messageReports.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="user-reports" className="mt-6">
          <div className="reports-table-container">
            <Table
              columns={userReportsColumns}
              dataSource={userReports}
              rowKey="id"
              loading={loading}
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} reports`,
              }}
              scroll={{ x: 800 }}
            />
          </div>
        </TabsContent>

        <TabsContent value="message-reports" className="mt-6">
          <div className="reports-table-container">
            <Table
              columns={messageReportsColumns}
              dataSource={messageReports}
              rowKey="id"
              loading={loading}
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} reports`,
              }}
              scroll={{ x: 800 }}
            />
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default ReportsManagement; 