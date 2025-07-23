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

  const sortedPortfolios = useMemo(() => {
    if (!portfolios || portfolios.length === 0) {
      return portfolios;
    }

    const sorted = [...portfolios]; // Create a copy to avoid mutating original array

    switch (sortBy) {
      case 'most-recent':
        return sorted.sort((a, b) => {
          const dateA = new Date(a.date);
          const dateB = new Date(b.date);
          return dateB.getTime() - dateA.getTime(); // Newest first
        });

      case 'most-popular':
        return sorted.sort((a, b) => {
          // Combine views and likes for popularity score
          const popularityA = a.views + (a.likes * 2); // Weight likes more than views
          const popularityB = b.views + (b.likes * 2);
          return popularityB - popularityA; // Highest popularity first
        });

      case 'most-liked':
        return sorted.sort((a, b) => {
          if (a.likes !== b.likes) {
            return b.likes - a.likes; // Most liked first
          }
          // If likes are equal, use views as tiebreaker
          return b.views - a.views;
        });

      case 'most-bookmarked':
        return sorted.sort((a, b) => {
          if (a.bookmarks !== b.bookmarks) {
            return b.bookmarks - a.bookmarks; // Most bookmarked first
          }
          // If bookmarks are equal, use likes as tiebreaker
          if (a.likes !== b.likes) {
            return b.likes - a.likes;
          }
          // If both are equal, use views as final tiebreaker
          return b.views - a.views;
        });

      case 'featured':
        return sorted.sort((a, b) => {
          // Featured portfolios first, then by most recent
          if (a.featured !== b.featured) {
            return a.featured ? -1 : 1; // Featured portfolios first
          }
          // If both are featured or both are not, sort by date
          const dateA = new Date(a.date);
          const dateB = new Date(b.date);
          return dateB.getTime() - dateA.getTime();
        });

      default:
        return sorted;
    }
  }, [portfolios, sortBy]);

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
