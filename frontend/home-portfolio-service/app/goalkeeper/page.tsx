'use client';

import React from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Users, Star, Award, Eye, Camera, Palette, Code, FileText } from 'lucide-react';
import Header from '@/components/header';
import { motion } from 'framer-motion';
import './goalkeeper.css';

export default function GoalKeeperPage() {
  const router = useRouter();

  const handleGoToHome = () => {
    router.push('/');
  };

  return (
    <div className="goalkeeper-container">
      <Header />

      {/* Main Hero Section */}
      <main className="hero-main">
        <div className="hero-container">
          {/* Left Content */}
          <div className="hero-content">
            <h1 className="hero-title">
              GoalKeeper
              <br />
              <span className="title-highlight">Design your career</span>
              <br />
              with us
            </h1>
            <p className="hero-description">
               Showcase your work, connect with professionals, and build your career
            </p>
                          <Button
                onClick={handleGoToHome}
                className="discover-btn"
              >
              Discover
            </Button>
          </div>

                     {/* Right Illustration */}
           <div className="hero-illustration">
             <div className="illustration-container">
               {/* Portfolio Floating Elements */}
               <motion.div 
                 className="floating-element element-1"
                 initial={{ opacity: 0, y: 20 }}
                 animate={{ opacity: 1, y: 0 }}
                 transition={{ delay: 0.2, duration: 0.6 }}
                 whileHover={{ scale: 1.1, rotate: 5 }}
               >
                 <Camera size={24} className="element-icon" />
                 <span className="element-label">Showcase</span>
               </motion.div>
               
               <motion.div 
                 className="floating-element element-2"
                 initial={{ opacity: 0, y: 20 }}
                 animate={{ opacity: 1, y: 0 }}
                 transition={{ delay: 0.4, duration: 0.6 }}
                 whileHover={{ scale: 1.1, rotate: -5 }}
               >
                 <Users size={24} className="element-icon" />
                 <span className="element-label">Connect</span>
               </motion.div>
               
               <motion.div 
                 className="floating-element element-3"
                 initial={{ opacity: 0, y: 20 }}
                 animate={{ opacity: 1, y: 0 }}
                 transition={{ delay: 0.6, duration: 0.6 }}
                 whileHover={{ scale: 1.1, rotate: 3 }}
               >
                 <Award size={24} className="element-icon" />
                 <span className="element-label">Achieve</span>
               </motion.div>
               
               {/* Portfolio Browser Window */}
               <motion.div 
                 className="portfolio-browser"
                 initial={{ opacity: 0, scale: 0.8 }}
                 animate={{ opacity: 1, scale: 1 }}
                 transition={{ delay: 0.8, duration: 0.8 }}
               >
                 <div className="browser-header">
                   <div className="browser-dots">
                     <span className="dot red"></span>
                     <span className="dot yellow"></span>
                     <span className="dot green"></span>
                   </div>
                   <div className="browser-url">myportfolio.com</div>
                 </div>
                 <div className="browser-content">
                   <motion.div 
                     className="portfolio-preview"
                     whileHover={{ scale: 1.02 }}
                   >
                     <div className="preview-header">
                       <div className="avatar-placeholder"></div>
                       <div className="name-info">
                         <div className="name-bar"></div>
                         <div className="title-bar"></div>
                       </div>
                     </div>
                     <div className="preview-stats">
                       <motion.div 
                         className="stat-item"
                         animate={{ scale: [1, 1.1, 1] }}
                         transition={{ duration: 2, repeat: Infinity, delay: 1 }}
                       >
                         <Eye size={16} />
                         <span>2.5K</span>
                       </motion.div>
                       <motion.div 
                         className="stat-item"
                         animate={{ scale: [1, 1.1, 1] }}
                         transition={{ duration: 2, repeat: Infinity, delay: 1.5 }}
                       >
                         <Star size={16} />
                         <span>124</span>
                       </motion.div>
                     </div>
                     <div className="preview-projects">
                       <motion.div 
                         className="project-card"
                         whileHover={{ y: -5 }}
                       >
                         <Palette size={20} />
                       </motion.div>
                       <motion.div 
                         className="project-card"
                         whileHover={{ y: -5 }}
                       >
                         <Code size={20} />
                       </motion.div>
                       <motion.div 
                         className="project-card"
                         whileHover={{ y: -5 }}
                       >
                         <FileText size={20} />
                       </motion.div>
                     </div>
                   </motion.div>
                 </div>
                               </motion.div>
             </div>
           </div>
        </div>
      </main>
    </div>
  );
} 