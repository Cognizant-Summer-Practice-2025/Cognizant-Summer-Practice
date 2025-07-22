'use client';

import React from 'react';
import { Card, Avatar, Tag, Button } from 'antd';
import { EyeOutlined, LikeOutlined, MessageOutlined, StarOutlined, StarFilled } from '@ant-design/icons';
import { useBookmarks } from '@/lib/contexts/bookmark-context';
import { useUser } from '@/lib/contexts/user-context';
import { usePortfolioNavigation } from '@/lib/contexts/use-portfolio-navigation';
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
  bookmarks: number;
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
  bookmarks,
  date,
  avatar,
  featured = false,
}) => {
  const { user } = useUser();
  const { isBookmarked, toggleBookmark, loading } = useBookmarks();
  const { navigateToPortfolio } = usePortfolioNavigation();

  const handleViewPortfolio = () => {
    navigateToPortfolio(id);
  };

  const handleBookmarkClick = async (e: React.MouseEvent) => {
    e.stopPropagation(); 
    
    if (!user) {
      router.push('/login');
      return;
    }

    await toggleBookmark(id, `${name}'s Portfolio`);
  };

  const bookmarked = isBookmarked(id);

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
          <button
            onClick={handleBookmarkClick}
            disabled={loading}
            className="bookmark-button"
            style={{
              background: 'none',
              border: 'none',
              cursor: loading ? 'not-allowed' : 'pointer',
              padding: '4px',
              borderRadius: '4px',
              transition: 'all 0.2s ease',
            }}
            onMouseEnter={(e) => {
              if (!loading) {
                e.currentTarget.style.backgroundColor = '#f0f0f0';
              }
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.backgroundColor = 'transparent';
            }}
          >
            {bookmarked ? (
              <StarFilled 
                style={{ 
                  color: '#1890ff', 
                  fontSize: '20px',
                  opacity: loading ? 0.6 : 1 
                }} 
              />
            ) : (
              <StarOutlined 
                style={{ 
                  color: '#8c8c8c', 
                  fontSize: '20px',
                  opacity: loading ? 0.6 : 1 
                }} 
              />
            )}
          </button>
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