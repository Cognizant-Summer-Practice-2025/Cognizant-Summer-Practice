'use client';

import React, { useState } from 'react';
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
    // TODO: Implement sorting logic here
  };

  return (
    <div className="portfolio-grid">
      <div className="portfolio-grid-header">
        <div className="portfolio-grid-title">
          <h1>Discover Portfolios</h1>
          <p>
            {portfolios.length === 0 && !loading ? 
              'No portfolios match your current filters' : 
              `Showing ${portfolios.length} ${portfolios.length === 1 ? 'portfolio' : 'portfolios'}`
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
        ) : portfolios.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '40px' }}>
            <p>No portfolios found. Be the first to create one!</p>
          </div>
        ) : (
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