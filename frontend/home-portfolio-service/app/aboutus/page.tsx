'use client';

import React from 'react';
import { Target, Users, Award } from 'lucide-react';
import Header from '@/components/header';
import { motion } from 'framer-motion';
import './aboutus.css';

export default function AboutUsPage() {

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
              <motion.div 
                className="creator-item"
                whileHover={{ scale: 1.05 }}
                transition={{ duration: 0.2 }}
              >
                <div className="creator-avatar">
                  <img src="/images/theodor-avatar.svg" alt="Theodor Sandu" />
                </div>
                <span className="creator-name">Theodor Sandu</span>
              </motion.div>

              <motion.div 
                className="creator-item"
                whileHover={{ scale: 1.05 }}
                transition={{ duration: 0.2 }}
              >
                <div className="creator-avatar">
                  <img src="/images/bianca-avatar.svg" alt="Milea Bianca" />
                </div>
                <span className="creator-name">Milea Bianca</span>
              </motion.div>
            </div>
          </div>
        </motion.div>
      </footer>
    </div>
  );
} 