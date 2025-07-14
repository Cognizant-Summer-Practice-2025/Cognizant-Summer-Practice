"use client";

import React from 'react';

import ProfileSection from '@/components/portfolio-page/profile/profile';
import StatsSection from '@/components/portfolio-page/stats/stats';
import ContactSection from '@/components/portfolio-page/contacts/contacts';
import QuotesSection from '@/components/portfolio-page/quotes/quotes';
import ExperienceSection from '@/components/portfolio-page/experience/experience';
import TabsSection from '@/components/portfolio-page/tabs/tabs';
import SocialLinksSection from '@/components/portfolio-page/social-links/social-links';
import './style.css';


const PortfolioPage = () => {
  return (
    <div className="portfolio-container">
      <div className="portfolio-content">
        <div className="portfolio-wrapper">
          <div className="portfolio-main">
            <ProfileSection />
            <StatsSection />
            <ContactSection />
            <QuotesSection />
            <ExperienceSection />
            <TabsSection />
            <SocialLinksSection />
          </div>
        </div>
      </div>
    </div>
  );
};

export default PortfolioPage;