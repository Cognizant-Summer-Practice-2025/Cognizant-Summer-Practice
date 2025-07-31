import React, { useState, useEffect } from 'react';
import Image from 'next/image';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { Download, Gamepad2, Mail, User, Briefcase, Star } from 'lucide-react';

interface HeaderProps {
  data: PortfolioDataFromDB;
}

export function Header({ data }: HeaderProps) {
  const { profile } = data;
  const [score, setScore] = useState(0);
  const [showMenu, setShowMenu] = useState(false);

  useEffect(() => {
    // Animated score counter
    const targetScore = 99999;
    const duration = 2000;
    const steps = 60;
    const increment = targetScore / steps;
    let currentStep = 0;

    const timer = setInterval(() => {
      currentStep++;
      setScore(Math.min(Math.floor(increment * currentStep), targetScore));
      
      if (currentStep >= steps) {
        clearInterval(timer);
      }
    }, duration / steps);

    return () => clearInterval(timer);
  }, []);

  const scrollToSection = (sectionId: string) => {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
    setShowMenu(false);
  };

  return (
    <header className="retro-header">
      <div className="game-ui">
        <div className="ui-top">
          <div className="score-display">
            <span className="score-label">SCORE</span>
            <span className="score-value">{score.toLocaleString()}</span>
          </div>
          
          <div className="game-title">
            <Gamepad2 className="game-icon pixel-icon" size={32} />
            <h1 className="title-text">PORTFOLIO QUEST</h1>
          </div>
          
          <div className="lives-display">
            <span className="lives-label">LIVES</span>
            <div className="hearts">
              <span className="heart">♥</span>
              <span className="heart">♥</span>
              <span className="heart">♥</span>
            </div>
          </div>
        </div>

        <div className="player-info">
          <div className="player-card">
            <div className="player-avatar">
              <Image
                src={profile.profileImage}
                alt={profile.name}
                className="avatar-image pixel-art"
                width={120}
                height={120}
              />
              <div className="avatar-border"></div>
            </div>
            <div className="player-details">
              <h2 className="player-name">{profile.name}</h2>
              <p className="player-class">{profile.title}</p>
              <div className="level-info">
                <Star className="pixel-icon" size={16} />
                <span>LEVEL 99 DEVELOPER</span>
              </div>
            </div>
          </div>

          <nav className="game-menu">
            <button
              className="menu-toggle"
              onClick={() => setShowMenu(!showMenu)}
            >
              <span className="menu-lines"></span>
              <span className="menu-text">MENU</span>
            </button>
            
            <div className={`menu-items ${showMenu ? 'active' : ''}`}>
              <button
                className="menu-item"
                onClick={() => scrollToSection('about')}
              >
                <User className="menu-icon" size={16} />
                <span>ABOUT</span>
              </button>
              <button
                className="menu-item"
                onClick={() => scrollToSection('experience')}
              >
                <Briefcase className="menu-icon" size={16} />
                <span>QUESTS</span>
              </button>
              <button
                className="menu-item"
                onClick={() => scrollToSection('projects')}
              >
                <Star className="menu-icon" size={16} />
                <span>ITEMS</span>
              </button>
              <button
                className="menu-item"
                onClick={() => scrollToSection('skills')}
              >
                <Gamepad2 className="menu-icon" size={16} />
                <span>SKILLS</span>
              </button>
              <button
                className="menu-item"
                onClick={() => scrollToSection('contact')}
              >
                <Mail className="menu-icon" size={16} />
                <span>CONTACT</span>
              </button>
            </div>
          </nav>

          <div className="action-buttons">
            <Button className="download-btn pixel-button">
              <Download size={16} />
              <span>DOWNLOAD CV</span>
            </Button>
          </div>
        </div>
      </div>

      <div className="background-pattern">
        <div className="pixel-clouds"></div>
        <div className="floating-pixels"></div>
      </div>
    </header>
  );
}

export default Header;