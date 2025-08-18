'use client';

import React from 'react';
import Image from 'next/image';
import { Target, Users, Award } from 'lucide-react';
import Header from '@/components/header';
import { motion } from 'framer-motion';
import './aboutus.css';

export default function AboutUsPage() {
  const handleGitHubRedirect = (url: string) => {
    if (typeof window !== 'undefined') {
      window.open(url, '_blank');
    }
  };

  return (
    <div className="aboutus-container">
      <Header />

            {/* Main Content */}
      <main className="creative-main">
        <div className="creative-container">
          {/* Header Section */}
          <motion.div 
            className="header-section"
            initial={{ opacity: 0, y: -30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <h1 className="main-heading">
              Best way to <br />
              <span className="highlight-text">Design your career</span>
            </h1>
            <p className="main-description">
              Empowering programmers by providing a collaborative platform for skill development, 
              professional networking, and showcasing their work, fostering growth and innovation 
              in the programming community.
            </p>
          </motion.div>



          {/* Features Section */}
          <motion.div 
            className="features-section"
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.8, duration: 0.6 }}
          >
            <div className="features-grid">
              <motion.div 
                className="feature-item"
                whileHover={{ y: -5 }}
                transition={{ duration: 0.3 }}
              >
                <div className="feature-icon">
                  <Target size={24} />
                </div>
                <h3 className="feature-title">Quality Matters</h3>
                <p className="feature-description">
                  We ensure high-quality portfolios that showcase your best work and professional achievements.
                </p>
              </motion.div>

              <motion.div 
                className="feature-item"
                whileHover={{ y: -5 }}
                transition={{ duration: 0.3 }}
              >
                <div className="feature-icon">
                  <Users size={24} />
                </div>
                <h3 className="feature-title">Collaborative Design</h3>
                <p className="feature-description">
                  Connect with other developers and get feedback to improve your projects and skills.
                </p>
              </motion.div>

              <motion.div 
                className="feature-item"
                whileHover={{ y: -5 }}
                transition={{ duration: 0.3 }}
              >
                <div className="feature-icon">
                  <Award size={24} />
                </div>
                <h3 className="feature-title">Fast Growth</h3>
                <p className="feature-description">
                  Accelerate your career growth through our platform&apos;s networking and showcasing opportunities.
                </p>
              </motion.div>
            </div>
          </motion.div>

        </div>
      </main>

      {/* Footer - Creators Section */}
      <footer className="creators-footer">
        <motion.div 
          className="creators-section"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 1.0, duration: 0.6 }}
        >
          <div className="creators-container">
            <h4 className="creators-title">Created by</h4>
            <div className="creators-list">
              <div 
                className="creator-item clickable"
                onClick={() => handleGitHubRedirect('https://github.com/Theo3883')}
              >
                <div className="creator-avatar">
                  <Image src="/images/theodor-avatar.svg" alt="Theodor Sandu" width={50} height={50} />
                </div>
                <span className="creator-name">Theodor Sandu</span>
              </div>

              <div 
                className="creator-item clickable"
                onClick={() => handleGitHubRedirect('https://github.com/biancamilea04')}
              >
                <div className="creator-avatar">
                  <Image src="/images/bianca-avatar.svg" alt="Milea Bianca" width={50} height={50} />
                </div>
                <span className="creator-name">Milea Bianca</span>
              </div>
            </div>
          </div>
        </motion.div>
      </footer>
    </div>
  );
} 