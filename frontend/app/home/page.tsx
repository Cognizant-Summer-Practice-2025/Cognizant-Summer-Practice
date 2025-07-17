import React from 'react';
import Header from '@/components/header';
import FilterSidebar from '@/components/home-page/filter-sidebar';
import PortfolioGrid from '@/components/home-page/portfolio-grid';
import './style.css';

const HomePage: React.FC = () => {
  return (
    <div className="home-page">
      <Header />
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