'use client';

import React, { useState } from 'react';
import { Select, Row, Col } from 'antd';
import PortfolioCard from './portfolio-card';
import './style.css';

const { Option } = Select;

interface Portfolio {
  id: string;
  name: string;
  role: string;
  location: string;
  description: string;
  skills: string[];
  views: number;
  likes: number;
  comments: number;
  date: string;
  avatar?: string;
  featured?: boolean;
}

const PortfolioGrid: React.FC = () => {
  const [sortBy, setSortBy] = useState('most-recent');

  // Sample portfolio data
  const portfolios: Portfolio[] = [
    {
      id: '1',
      name: 'Alex Johnson',
      role: 'Full Stack Developer',
      location: 'San Francisco, CA',
      description: 'Crafting digital experiences with modern web technologies',
      skills: ['React', 'Node.js', 'TypeScript', 'GraphQL'],
      views: 546,
      likes: 49,
      comments: 2,
      date: '15/03/2024',
      avatar: 'https://placehold.co/48x48',
      featured: false,
    },
    {
      id: '2',
      name: 'Sarah Chen',
      role: 'UI/UX Designer',
      location: 'New York, NY',
      description: 'Creating beautiful and intuitive user experiences',
      skills: ['Figma', 'Adobe XD', 'Prototyping', 'User Research'],
      views: 832,
      likes: 73,
      comments: 5,
      date: '12/03/2024',
      featured: true,
    },
    {
      id: '3',
      name: 'Mike Rodriguez',
      role: 'Data Scientist',
      location: 'Austin, TX',
      description: 'Turning data into actionable insights and predictive models',
      skills: ['Python', 'Machine Learning', 'TensorFlow', 'SQL'],
      views: 421,
      likes: 34,
      comments: 3,
      date: '10/03/2024',
      avatar: 'https://placehold.co/48x48',
      featured: false,
    },
  ];

  const handleSortChange = (value: string) => {
    setSortBy(value);
    // Implement sorting logic here
  };

  return (
    <div className="portfolio-grid">
      <div className="portfolio-grid-header">
        <div className="portfolio-grid-title">
          <h1>Discover Portfolios</h1>
          <p>Explore creative work from talented professionals</p>
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
              <PortfolioCard {...portfolio} />
            </Col>
          ))}
        </Row>
      </div>
    </div>
  );
};

export default PortfolioGrid; 