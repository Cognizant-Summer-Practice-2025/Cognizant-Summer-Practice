import React, { useState, useEffect } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { Terminal, Download, Wifi, Battery, Clock, User, Mail, FileText, Code, Briefcase } from 'lucide-react';

interface HeaderProps {
  data: PortfolioDataFromDB;
}

export function Header({ data }: HeaderProps) {
  const { profile } = data;
  const [currentTime, setCurrentTime] = useState(new Date());
  const [typedName, setTypedName] = useState('');
  const [showCursor, setShowCursor] = useState(true);

  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    // Cursor blinking effect
    const cursorInterval = setInterval(() => {
      setShowCursor(prev => !prev);
    }, 500);
    return () => clearInterval(cursorInterval);
  }, []);

  useEffect(() => {
    // Typing effect for name
    const name = profile.name;
    let index = 0;
    const typingInterval = setInterval(() => {
      if (index < name.length) {
        setTypedName(name.substring(0, index + 1));
        index++;
      } else {
        clearInterval(typingInterval);
      }
    }, 100);
    return () => clearInterval(typingInterval);
  }, [profile.name]);

  const scrollToSection = (sectionId: string) => {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  };

  return (
    <header className="terminal-header-section">
      {/* Terminal Window Header */}
      <div className="terminal-window">
        <div className="window-header">
          <div className="window-controls">
            <span className="control close"></span>
            <span className="control minimize"></span>
            <span className="control maximize"></span>
          </div>
          <div className="window-title">
            <Terminal size={16} />
            <span>{profile.name} - Portfolio Terminal</span>
          </div>
          <div className="window-status">
            <Battery className="status-icon" size={14} />
            <Wifi className="status-icon" size={14} />
            <span className="status-time">{currentTime.toLocaleTimeString()}</span>
          </div>
        </div>

        <div className="terminal-body">
          <div className="startup-sequence">
            <div className="boot-line">Portfolio OS v2.1.0 booting...</div>
            <div className="boot-line">Loading user profile...</div>
            <div className="boot-line">Initializing portfolio modules...</div>
            <div className="boot-line">Ready.</div>
            <div className="boot-line"></div>
            <div className="welcome-line">
              <span className="prompt">$</span>
              <span className="command">whoami</span>
            </div>
            <div className="output-line">
              {typedName}
              {showCursor && <span className="terminal-cursor">_</span>}
            </div>
            <div className="output-line">{profile.title}</div>
            <div className="boot-line"></div>
            <div className="help-line">
              Type &apos;help&apos; for available commands or use the navigation below.
            </div>
          </div>
        </div>
      </div>

      {/* Navigation Menu */}
      <nav className="terminal-nav">
        <div className="nav-header">
          <Terminal className="nav-icon" size={20} />
          <span>Navigation</span>
        </div>
        
        <div className="nav-commands">
          <button
            className="nav-command"
            onClick={() => scrollToSection('about')}
          >
            <User className="command-icon" size={16} />
            <span className="command-text">./about</span>
            <span className="command-desc">Personal information</span>
          </button>
          
          <button
            className="nav-command"
            onClick={() => scrollToSection('experience')}
          >
            <Briefcase className="command-icon" size={16} />
            <span className="command-text">./experience</span>
            <span className="command-desc">Work history</span>
          </button>
          
          <button
            className="nav-command"
            onClick={() => scrollToSection('projects')}
          >
            <Code className="command-icon" size={16} />
            <span className="command-text">./projects</span>
            <span className="command-desc">Portfolio items</span>
          </button>
          
          <button
            className="nav-command"
            onClick={() => scrollToSection('skills')}
          >
            <FileText className="command-icon" size={16} />
            <span className="command-text">./skills</span>
            <span className="command-desc">Technical abilities</span>
          </button>
          
          <button
            className="nav-command"
            onClick={() => scrollToSection('contact')}
          >
            <Mail className="command-icon" size={16} />
            <span className="command-text">./contact</span>
            <span className="command-desc">Get in touch</span>
          </button>
        </div>
      </nav>

      {/* System Status Bar */}
      <div className="status-bar">
        <div className="status-left">
          <div className="status-item">
            <span className="status-label">USER:</span>
            <span className="status-value">{profile.name.toLowerCase().replace(/\s+/g, '')}</span>
          </div>
          <div className="status-item">
            <span className="status-label">HOST:</span>
            <span className="status-value">portfolio.local</span>
          </div>
          <div className="status-item">
            <span className="status-label">PWD:</span>
            <span className="status-value">~/portfolio</span>
          </div>
        </div>
        
        <div className="status-right">
          <Button className="download-btn terminal-button">
            <Download size={14} />
            <span>Download CV</span>
          </Button>
          
          <div className="connection-status">
            <div className="status-dot online"></div>
            <span>Connected</span>
          </div>
          
          <div className="system-time">
            <Clock size={14} />
            <span>{currentTime.toLocaleString()}</span>
          </div>
        </div>
      </div>

      {/* ASCII Art Banner */}
      <div className="ascii-banner">
        <pre className="ascii-art">{`
 ____            _    __       _ _       
|  _ \\ ___  _ __| |_ / _| ___ | (_) ___  
| |_) / _ \\| '__| __| |_ / _ \\| | |/ _ \\ 
|  __/ (_) | |  | |_|  _| (_) | | | (_) |
|_|   \\___/|_|   \\__|_|  \\___/|_|_|\\___/ 
                                        
    Welcome to the ${profile.name} Portfolio Terminal
        `}</pre>
      </div>
    </header>
  );
}

export default Header;