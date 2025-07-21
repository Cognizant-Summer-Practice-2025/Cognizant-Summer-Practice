'use client';

import React, { useState, useMemo } from 'react';
import { Select, Row, Col } from 'antd';
import PortfolioCard from './portfolio-card';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import './style.css';

const { Option } = Select;

interface PortfolioGridProps {
  portfolios: PortfolioCardDto[];
  loading: boolean;
  error: string | null;
}

const PortfolioGrid: React.FC<PortfolioGridProps> = ({ portfolios, loading, error }) => {
  const [sortBy, setSortBy] = useState('most-recent');

  const handleSortChange = (value: string) => {
    setSortBy(value);
  };

  // 🔄 SORTING LOGIC - Sort portfolios based on selected criteria
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
    <div className="portfolio-grid">
      <div className="portfolio-grid-header">
        <div className="portfolio-grid-title">
          <h1>Discover Portfolios</h1>
          <p>
            {sortedPortfolios.length === 0 && !loading ? 
              'No portfolios match your current filters' : 
              `Showing ${sortedPortfolios.length} ${sortedPortfolios.length === 1 ? 'portfolio' : 'portfolios'}`
            }
          </p>
        </div>
        <div className="portfolio-sort">
          <Select
            value={sortBy}
            onChange={handleSortChange}
            className="portfolio-sort-select"
            size="middle"
          >
            <Option value="most-recent">Most Recent</Option>
            <Option value="most-popular">Most Popular</Option>
            <Option value="most-liked">Most Liked</Option>
            <Option value="most-bookmarked">Most Bookmarked</Option>
            <Option value="featured">Featured</Option>
          </Select>
        </div>
      </div>

      <div className="portfolio-grid-content">
        {loading ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <p>Loading portfolios...</p>
          </div>
        ) : error ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <p style={{ color: 'red' }}>{error}</p>
          </div>
        ) : sortedPortfolios.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <p>No portfolios found. Be the first to create one!</p>
          </div>
        ) : (
          <Row gutter={[24, 24]} className="portfolio-row">
            {sortedPortfolios.map((portfolio) => (
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
        )}
      </div>
    </div>
  );
};

export default PortfolioGrid; 