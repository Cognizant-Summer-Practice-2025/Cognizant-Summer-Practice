import React from 'react';
import { Button } from 'antd';
import Link from 'next/link';
import './style.css';

const LandingPage: React.FC = () => {
  return (
    <div className="landing-page">
      <div className="landing-container">
        <div className="landing-content">
          <h1>Welcome to GoalKeeper</h1>
          <p>Discover amazing portfolios from talented professionals</p>
          <div className="landing-actions">
            <Link href="/">
              <Button type="primary" size="large">
                Explore Portfolios
              </Button>
            </Link>
            <Link href="/admin">
              <Button size="large">
                Admin Dashboard
              </Button>
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LandingPage; 