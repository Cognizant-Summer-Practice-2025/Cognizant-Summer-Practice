"use client";

import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { TemplateManager, ComponentMap } from '@/lib/template-manager';
import { Header } from './components/header';
import { Stats } from './components/stats';
import { Contact } from './components/contact';
import { About } from './components/about';
import { Experience } from './components/experience';
import { Projects } from './components/projects';
import { Skills } from './components/skills';
import { BlogPosts } from './components/blog-posts';
import { Gamepad2, Star, Trophy, Zap } from 'lucide-react';
import './styles/main.css';

interface RetroGamingTemplateProps {
  data: PortfolioDataFromDB;
}

// Component mapping for the template manager
const componentMap: ComponentMap = {
  experience: Experience as React.ComponentType<{ data: unknown }>,
  projects: Projects as React.ComponentType<{ data: unknown }>,
  skills: Skills as React.ComponentType<{ data: unknown }>,
  blog_posts: BlogPosts as React.ComponentType<{ data: unknown }>,
  contact: Contact as React.ComponentType<{ data: unknown }>,
  about: About as React.ComponentType<{ data: unknown }>
};

// Tab labels mapping
const tabLabels: Record<string, string> = {
  experience: 'CAREER',
  projects: 'PROJECTS', 
  skills: 'SKILLS',
  blog_posts: 'BLOG',
  contact: 'CONTACT',
  about: 'PLAYER'
};

export default function RetroGamingTemplate({ data }: RetroGamingTemplateProps) {
  const [activeSection, setActiveSection] = useState('about');
  const [score, setScore] = useState(0);
  const [level, setLevel] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  
  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Validate and transform the data before use
  const validatedData = TemplateManager.validateAndTransformPortfolioData(data);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(validatedData);

  // Filter components for navigation
  const navComponents = dynamicComponents.filter(comp => 
    comp && ['about', 'experience', 'projects', 'skills', 'blog_posts', 'contact'].includes(comp.type)
  );

  // Loading animation
  useEffect(() => {
    const timer = setTimeout(() => {
      setIsLoading(false);
    }, 2000);
    return () => clearTimeout(timer);
  }, []);

  // Score calculation based on data
  useEffect(() => {
    let calculatedScore = 0;
    calculatedScore += (validatedData.projects?.length || 0) * 100;
    calculatedScore += (validatedData.experience?.length || 0) * 150;
    calculatedScore += (validatedData.skills?.length || 0) * 50;
    calculatedScore += (validatedData.blogPosts?.length || 0) * 75;
    
    setScore(calculatedScore);
    setLevel(Math.floor(calculatedScore / 500) + 1);
  }, [validatedData]);

  const renderActiveComponent = () => {
    const activeComponent = navComponents.find(comp => comp && comp.type === activeSection);
    if (!activeComponent) return null;

    return activeComponent.component;
  };

  if (isLoading) {
    return (
      <div className="retro-loading">
        <div className="loading-screen">
          <div className="game-title">PORTFOLIO GAME</div>
          <div className="loading-bar">
            <div className="loading-fill"></div>
          </div>
          <div className="loading-text">LOADING...</div>
          <div className="pixel-art">
            <div className="pixel-row">
              <span>████████████</span>
            </div>
            <div className="pixel-row">
              <span>██ ████ ██ ██</span>
            </div>
            <div className="pixel-row">
              <span>██ ████ ██ ██</span>
            </div>
            <div className="pixel-row">
              <span>██████████ ██</span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="retro-gaming-template">
      {/* Game HUD */}
      <div className="game-hud">
        <div className="hud-left">
          <div className="player-info">
            <span className="label">PLAYER:</span>
            <span className="value">{validatedData.profile.name.toUpperCase()}</span>
          </div>
          <div className="score-info">
            <span className="label">SCORE:</span>
            <span className="value">{score.toLocaleString()}</span>
          </div>
        </div>
        
        <div className="hud-center">
          <div className="game-title">PORTFOLIO QUEST</div>
        </div>
        
        <div className="hud-right">
          <div className="level-info">
            <span className="label">LEVEL:</span>
            <span className="value">{level}</span>
          </div>
          <div className="lives-info">
            <span className="label">LIVES:</span>
            <div className="hearts">
              {[...Array(3)].map((_, i) => (
                <span key={i} className="heart">♥</span>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* Main Game Area */}
      <div className="game-area">
        {/* Menu Selection */}
        <div className="game-menu">
          <div className="menu-title">SELECT LEVEL</div>
          <div className="menu-items">
            {navComponents.map((component, index) => {
              if (!component) return null;
              
              return (
                <button
                  key={component.type}
                  onClick={() => setActiveSection(component.type)}
                  className={`menu-item ${activeSection === component.type ? 'active' : ''}`}
                >
                  <span className="menu-icon">
                    {index + 1}
                  </span>
                  <span className="menu-label">
                    {tabLabels[component.type] || component.type.toUpperCase()}
                  </span>
                  <span className="menu-indicator">
                    {activeSection === component.type ? '▶' : ''}
                  </span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Content Display */}
        <div className="game-content">
          <div className="content-window">
            <div className="window-title">
              LEVEL {navComponents.findIndex(c => c?.type === activeSection) + 1}: {tabLabels[activeSection]?.toUpperCase()}
            </div>
            <div className="window-body">
              {renderActiveComponent()}
            </div>
          </div>
        </div>
      </div>

      {/* Footer */}
      <div className="game-footer">
        <div className="controls-info">
          <span>USE MOUSE TO NAVIGATE • CLICK TO SELECT</span>
        </div>
        <div className="credits">
          <span>PORTFOLIO GAME v1.0 • CREATED BY {validatedData.profile.name.toUpperCase()}</span>
        </div>
      </div>
    </div>
  );
}