'use client';

import React from 'react';
import { Card, Avatar, Tag, Button } from 'antd';
import { EyeOutlined, LikeOutlined, MessageOutlined, StarOutlined } from '@ant-design/icons';
import { useRouter } from 'next/navigation';
import './style.css';

interface PortfolioCardProps {
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

const PortfolioCard: React.FC<PortfolioCardProps> = ({
  id,
  name,
  role,
  location,
  description,
  skills,
  views,
  likes,
  comments,
  date,
  avatar,
  featured = false,
}) => {
  const router = useRouter();

  const handleViewPortfolio = () => {
    router.push(`/portfolio?portfolio=${id}`);
  };

  return (
    <Card
      className="portfolio-card"
      styles={{ body: { padding: '24px' } }}
      hoverable
    >
      <div className="portfolio-card-header">
        <div className="portfolio-user-info">
          <Avatar 
            src={avatar || "https://placehold.co/48x48"} 
            size={48}
            className="portfolio-avatar"
          />
          <div className="portfolio-user-details">
            <h3 className="portfolio-user-name">{name}</h3>
            <p className="portfolio-user-role">{role}</p>
            <p className="portfolio-user-location">{location}</p>
          </div>
        </div>
        <div className="portfolio-favorite">
          {featured ? (
            <StarOutlined className="portfolio-star-filled" />
          ) : (
            <StarOutlined className="portfolio-star-outline" />
          )}
        </div>
      </div>

      <div className="portfolio-description">
        <p>{description}</p>
      </div>

      <div className="portfolio-skills">
        {skills.map((skill, index) => (
          <Tag key={index} className="portfolio-skill-tag">
            {skill}
          </Tag>
        ))}
      </div>

      <div className="portfolio-stats">
        <div className="portfolio-stats-left">
          <div className="portfolio-stat">
            <EyeOutlined />
            <span>{views}</span>
          </div>
          <div className="portfolio-stat">
            <LikeOutlined />
            <span>{likes}</span>
          </div>
          <div className="portfolio-stat">
            <MessageOutlined />
            <span>{comments}</span>
          </div>
        </div>
        <div className="portfolio-date">
          <span>{date}</span>
        </div>
      </div>

      <div className="portfolio-actions">
        <Button 
          type="default" 
          block 
          onClick={handleViewPortfolio}
          className="portfolio-view-btn"
        >
          View Portfolio
        </Button>
      </div>
    </Card>
  );
};

export default PortfolioCard; 