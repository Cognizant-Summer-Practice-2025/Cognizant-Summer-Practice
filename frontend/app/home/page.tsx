import React from 'react';
import HomeHeader from '@/components/home-page/home-header';
import FilterSidebar from '@/components/home-page/filter-sidebar';
import PortfolioGrid from '@/components/home-page/portfolio-grid';
import './style.css';

const HomePage: React.FC = () => {
  return (
    <div className="home-page">
      <HomeHeader />
      <div className="home-main">
        <div className="home-container">
          <div className="home-content">
            <FilterSidebar />
            <PortfolioGrid />
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage; 