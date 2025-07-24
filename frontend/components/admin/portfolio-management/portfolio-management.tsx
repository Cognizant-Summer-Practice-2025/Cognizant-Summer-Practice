'use client';

import React, { useState, useEffect } from 'react';
import { Button, Modal, message } from 'antd';
import { ExportOutlined, EyeOutlined, EditOutlined, DeleteOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { AdminAPI } from '@/lib/admin/api';
import { PortfolioWithOwner } from '@/lib/admin/interfaces';
import './style.css';

const { confirm } = Modal;

const PortfolioManagement: React.FC = () => {
  const [portfolios, setPortfolios] = useState<PortfolioWithOwner[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deletingPortfolioId, setDeletingPortfolioId] = useState<string | null>(null);

  useEffect(() => {
    fetchPortfolios();
  }, []);

  const fetchPortfolios = async () => {
    try {
      setLoading(true);
      const portfoliosData = await AdminAPI.getPortfoliosWithOwners();
      setPortfolios(portfoliosData);
      setError(null);
    } catch (err) {
      console.error('Error fetching portfolios:', err);
      setError('Failed to load portfolios');
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

  const getPortfolioStatus = (portfolio: PortfolioWithOwner): string => {
    if (!portfolio.isPublished) return 'Draft';
    if (portfolio.visibility === 'Private') return 'Private';
    if (portfolio.visibility === 'Unlisted') return 'Unlisted';
    return 'Published';
  };

  const getThumbnail = (portfolio: PortfolioWithOwner): string => {
    // Use owner avatar as placeholder thumbnail, or generate a placeholder
    return portfolio.ownerAvatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(portfolio.title)}&size=40&background=f0f0f0&color=666`;
  };

  const handleViewPortfolio = (portfolioId: string, userId: string) => {
    // Open portfolio in new tab using the portfolio page URL structure
    const portfolioUrl = `/portfolio?portfolio=${portfolioId}`;
    window.open(portfolioUrl, '_blank', 'noopener,noreferrer');
  };

  const handleEditPortfolio = (portfolioId: string) => {
    // TODO: Implement edit portfolio functionality
    console.log('Edit portfolio:', portfolioId);
    message.info('Edit functionality will be implemented soon');
  };

  const handleDeletePortfolio = (portfolio: PortfolioWithOwner) => {
    console.log('🔴 DELETE BUTTON CLICKED - Portfolio:', portfolio.id, portfolio.title);
    console.log('🔴 DELETE BUTTON CLICKED - Full portfolio object:', portfolio);
    
    confirm({
      title: 'Delete Portfolio',
      icon: <ExclamationCircleOutlined />,
      content: (
        <div>
          <p>Are you sure you want to delete the portfolio <strong>"{portfolio.title}"</strong>?</p>
          <p>This action will permanently delete:</p>
          <ul style={{ marginLeft: '20px', marginTop: '10px' }}>
            <li>The portfolio and all its settings</li>
            <li>All associated projects</li>
            <li>All associated experiences</li>
            <li>All associated skills</li>
            <li>All associated blog posts</li>
            <li>All bookmarks pointing to this portfolio</li>
          </ul>
          <p style={{ color: '#ff4d4f', fontWeight: 'bold', marginTop: '15px' }}>
            This action cannot be undone.
          </p>
        </div>
      ),
      okText: 'Delete',
      okType: 'danger',
      cancelText: 'Cancel',
      width: 500,
      onOk: async () => {
        console.log('🔴 CONFIRMATION OK CLICKED - Starting deletion process');
        await performDeletePortfolio(portfolio.id, portfolio.title);
      },
    });
  };

  const performDeletePortfolio = async (portfolioId: string, portfolioTitle: string) => {
    try {
      setDeletingPortfolioId(portfolioId);
      
      await AdminAPI.deletePortfolio(portfolioId);
      
      // Remove the deleted portfolio from the local state
      setPortfolios(prev => prev.filter(p => p.id !== portfolioId));
      
      message.success(`Portfolio "${portfolioTitle}" has been deleted successfully`);
    } catch (error) {
      console.error('Error deleting portfolio:', error);
      message.error(
        error instanceof Error 
          ? `Failed to delete portfolio: ${error.message}`
          : 'Failed to delete portfolio. Please try again.'
      );
    } finally {
      setDeletingPortfolioId(null);
    }
  };

  const handleExportPortfolios = () => {
    // TODO: Implement export functionality
    console.log('Export portfolios');
    message.info('Export functionality will be implemented soon');
  };

  const handleModeratePorfolios = () => {
    // TODO: Implement moderation functionality
    console.log('Moderate portfolios');
    message.info('Moderation functionality will be implemented soon');
  };

  if (loading) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>Portfolio Management</h2>
        </div>
        <div className="loading-container">
          <p>Loading portfolios...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="management-section">
        <div className="section-header">
          <h2>Portfolio Management</h2>
        </div>
        <div className="error-container">
          <p>{error}</p>
          <Button onClick={fetchPortfolios}>Retry</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>Portfolio Management</h2>
        <div className="section-actions">
          <Button icon={<ExportOutlined />} className="export-btn" onClick={handleExportPortfolios}>
            Export
          </Button>
          <Button type="primary" className="moderate-btn" onClick={handleModeratePorfolios}>
            Moderate
          </Button>
        </div>
      </div>
      
      <div className="table-container">
        <div className="table-header portfolio-header">
          <div className="col-portfolio">Portfolio</div>
          <div className="col-owner">Owner</div>
          <div className="col-created">Created</div>
          <div className="col-views">Views</div>
          <div className="col-p-status">Status</div>
          <div className="col-p-actions">Actions</div>
        </div>
        
        <div className="table-body">
          {portfolios.map((portfolio) => (
            <div key={portfolio.id} className="table-row">
              <div className="col-portfolio">
                <div className="portfolio-cell">
                  <img 
                    src={getThumbnail(portfolio)} 
                    alt={portfolio.title} 
                    className="portfolio-thumbnail" 
                  />
                  <div className="portfolio-info">
                    <div className="portfolio-title">{portfolio.title}</div>
                    <div className="portfolio-description">
                      {portfolio.bio || 'No description available'}
                    </div>
                  </div>
                </div>
              </div>
              <div className="col-owner">{portfolio.ownerName}</div>
              <div className="col-created">{formatDate(portfolio.createdAt)}</div>
              <div className="col-views">{portfolio.viewCount.toLocaleString()}</div>
              <div className="col-p-status">
                <span className={`status-badge ${getStatusBadgeClass(getPortfolioStatus(portfolio))}`}>
                  {getPortfolioStatus(portfolio)}
                </span>
              </div>
              <div className="col-p-actions">
                <Button 
                  type="text" 
                  icon={<EyeOutlined />} 
                  size="small"
                  onClick={() => handleViewPortfolio(portfolio.id, portfolio.userId)}
                  title="View portfolio in new tab"
                />
                <Button 
                  type="text" 
                  icon={<EditOutlined />} 
                  size="small"
                  onClick={() => handleEditPortfolio(portfolio.id)}
                  title="Edit portfolio"
                />
                <Button 
                  type="text" 
                  icon={<DeleteOutlined />} 
                  size="small" 
                  className="delete-btn"
                  onClick={(e) => {
                    console.log('🔴 DELETE BUTTON RAW CLICK EVENT:', e);
                    console.log('🔴 DELETE BUTTON RAW CLICK - Portfolio ID:', portfolio.id);
                    e.preventDefault();
                    e.stopPropagation();
                    handleDeletePortfolio(portfolio);
                  }}
                  loading={deletingPortfolioId === portfolio.id}
                  title="Delete portfolio and all related data"
                  style={{ color: '#ff4d4f', border: '1px solid #ff4d4f' }}
                />
                {/* Test button to verify click events work */}
                <button 
                  onClick={() => {
                    console.log('🟢 TEST BUTTON CLICKED - Simple button works!');
                    alert('Test button clicked for portfolio: ' + portfolio.title);
                  }}
                  style={{ 
                    background: 'red', 
                    color: 'white', 
                    border: 'none', 
                    padding: '4px 8px',
                    cursor: 'pointer',
                    fontSize: '10px'
                  }}
                >
                  TEST
                </button>
              </div>
            </div>
          ))}
        </div>

        {portfolios.length === 0 && (
          <div className="empty-state">
            <p>No portfolios found.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default PortfolioManagement;