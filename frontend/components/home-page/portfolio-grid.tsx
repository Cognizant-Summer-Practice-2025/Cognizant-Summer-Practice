'use client';

import React from 'react';
import { Select, Row, Col, Spin, Empty } from 'antd';
import PortfolioCard from './portfolio-card';
import PaginationControls from './pagination-controls';
import { useHomePageCache } from '@/lib/contexts/home-page-cache-context';
import './style.css';

const { Option } = Select;

interface PortfolioGridProps {
  className?: string;
}

const PortfolioGrid: React.FC<PortfolioGridProps> = ({ className = '' }) => {
  const {
    portfolios,
    loading,
    error,
    sortBy,
    setSort,
    pagination
  } = useHomePageCache();

  const handleSortChange = (value: string) => {
    const [sortBy, sortDirection] = value.split(':');
    setSort(sortBy, sortDirection || 'desc');
  };

  const getSortValue = () => {
    return `${sortBy}:desc`; // Default direction for UI
  };

  return (
    <div className={`portfolio-grid ${className}`}>
      <div className="portfolio-grid-header">
        <div className="portfolio-grid-title">
          <h1>Discover Portfolios</h1>
          <p>
            {pagination ? (
              loading ? 
                'Loading portfolios...' : 
                portfolios.length === 0 ? 
                  'No portfolios match your current filters' : 
                  `Showing ${portfolios.length} of ${pagination.totalCount} ${pagination.totalCount === 1 ? 'portfolio' : 'portfolios'}`
            ) : 'Discover amazing portfolios from talented professionals'}
          </p>
        </div>
        <div className="portfolio-sort">
          <Select
            value={getSortValue()}
            onChange={handleSortChange}
            style={{ width: 200 }}
            disabled={loading}
          >
            <Option value="most-recent:desc">Most Recent</Option>
            <Option value="most-popular:desc">Most Popular</Option>
            <Option value="most-liked:desc">Most Liked</Option>
            <Option value="most-bookmarked:desc">Most Bookmarked</Option>
            <Option value="featured:desc">Featured First</Option>
            <Option value="name:asc">Name (A-Z)</Option>
            <Option value="name:desc">Name (Z-A)</Option>
          </Select>
        </div>
      </div>

      <div className="portfolio-grid-content">
        {loading && portfolios.length === 0 ? (
          <div className="portfolio-grid-loading">
            <Spin size="large" />
            <p>Loading amazing portfolios...</p>
          </div>
        ) : error ? (
          <div className="portfolio-grid-error">
            <Empty 
              description={
                <div>
                  <p style={{ color: 'red', marginBottom: 8 }}>{error}</p>
                  <p style={{ color: '#666' }}>Please try again later or contact support if the problem persists.</p>
                </div>
              } 
            />
          </div>
        ) : portfolios.length === 0 ? (
          <div className="portfolio-grid-empty">
            <Empty 
              description="No portfolios found. Try adjusting your filters or be the first to create one!"
            />
          </div>
        ) : (
          <>
            <Row gutter={[24, 24]} className="portfolio-row">
              {portfolios.map((portfolio) => (
                <Col 
                  key={portfolio.id} 
                  xs={24} 
                  sm={24} 
                  md={12} 
                  lg={8} 
                  xl={8}
                  className="portfolio-col"
                >
                  <PortfolioCard 
                    id={portfolio.id}
                    name={portfolio.name}
                    role={portfolio.role}
                    location={portfolio.location}
                    description={portfolio.description}
                    skills={portfolio.skills}
                    views={portfolio.views}
                    likes={portfolio.likes}
                    comments={portfolio.comments}
                    bookmarks={portfolio.bookmarks}
                    date={portfolio.date}
                    avatar={portfolio.avatar}
                    featured={portfolio.featured}
                  />
                </Col>
              ))}
            </Row>
            
            {/* Loading overlay for pagination changes */}
            {loading && portfolios.length > 0 && (
              <div className="portfolio-grid-loading-overlay">
                <Spin size="small" />
              </div>
            )}
          </>
        )}
      </div>

      {/* Pagination Controls */}
      {pagination && portfolios.length > 0 && (
        <PaginationControls 
          showQuickJumper
          showTotal
          showCacheStats={false}
          className="portfolio-pagination"
        />
      )}
    </div>
  );
};

export default PortfolioGrid;
