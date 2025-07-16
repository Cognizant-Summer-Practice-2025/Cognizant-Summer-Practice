import React from 'react';
import { Button } from 'antd';
import { ExportOutlined, EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import './style.css';

const PortfolioManagement: React.FC = () => {
  const portfolios = [
    {
      id: 1,
      title: 'Full Stack Developer Portfolio',
      description: 'Modern web applications',
      owner: 'John Doe',
      created: 'Jan 15, 2024',
      views: 1234,
      status: 'Published',
      thumbnail: 'https://placehold.co/40x40'
    },
    {
      id: 2,
      title: 'Design Portfolio',
      description: 'UI/UX case studies',
      owner: 'Sarah Wilson',
      created: 'Feb 3, 2024',
      views: 892,
      status: 'Under Review',
      thumbnail: 'https://placehold.co/40x40'
    }
  ];

  return (
    <div className="management-section">
      <div className="section-header">
        <h2>Portfolio Management</h2>
        <div className="section-actions">
          <Button icon={<ExportOutlined />} className="export-btn">
            Export
          </Button>
          <Button type="primary" className="moderate-btn">
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
                  <img src={portfolio.thumbnail} alt={portfolio.title} className="portfolio-thumbnail" />
                  <div className="portfolio-info">
                    <div className="portfolio-title">{portfolio.title}</div>
                    <div className="portfolio-description">{portfolio.description}</div>
                  </div>
                </div>
              </div>
              <div className="col-owner">{portfolio.owner}</div>
              <div className="col-created">{portfolio.created}</div>
              <div className="col-views">{portfolio.views.toLocaleString()}</div>
              <div className="col-p-status">
                <span className={`status-badge ${portfolio.status.toLowerCase().replace(' ', '-')}`}>
                  {portfolio.status}
                </span>
              </div>
              <div className="col-p-actions">
                <Button type="text" icon={<EyeOutlined />} size="small" />
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

export default PortfolioManagement;