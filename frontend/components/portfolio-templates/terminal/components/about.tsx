import React, { useState, useEffect } from 'react';
import { UserProfile, Quote } from '@/lib/portfolio';
import { User, Terminal, ChevronRight, Folder, File } from 'lucide-react';

interface AboutProps {
  data: {
    profile: UserProfile;
    quotes: Quote[];
  };
}

export function About({ data }: AboutProps) {
  const { profile, quotes } = data;
  const [typedText, setTypedText] = useState('');
  const [currentLine, setCurrentLine] = useState(0);
  
  const aboutLines = [
    `Name: ${profile.name}`,
    `Title: ${profile.title}`,
    `Location: ${profile.location || 'Unknown'}`,
    `Email: ${profile.email}`,
    '',
    'Bio:',
    ...(profile.bio ? profile.bio.split('\n') : ['No bio available']),
    '',
    quotes && quotes.length > 0 ? `Quote: "${quotes[0].text}" - ${quotes[0].author}` : ''
  ].filter(line => line !== undefined);

  useEffect(() => {
    if (currentLine < aboutLines.length) {
      const line = aboutLines[currentLine];
      let charIndex = 0;
      
      const typeInterval = setInterval(() => {
        if (charIndex <= line.length) {
          setTypedText(prev => prev + line[charIndex]);
          charIndex++;
        } else {
          clearInterval(typeInterval);
          setTimeout(() => {
            setTypedText(prev => prev + '\n');
            setCurrentLine(prev => prev + 1);
          }, 100);
        }
      }, 30);

      return () => clearInterval(typeInterval);
    }
  }, [currentLine, aboutLines]);

  return (
    <div className="terminal-about">
      <div className="terminal-window">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">about.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="command-line">
            <span className="prompt">$</span>
            <span className="command">cat about.txt</span>
          </div>
          
          <div className="file-output">
            <pre className="terminal-text">
{typedText}
{currentLine < aboutLines.length && <span className="cursor-blink">_</span>}
            </pre>
          </div>
        </div>
      </div>

      <div className="file-system">
        <div className="fs-header">
          <Folder className="fs-icon" size={16} />
          <span>~/about</span>
        </div>
        
        <div className="fs-tree">
          <div className="fs-item">
            <File className="fs-icon" size={14} />
            <span>personal_info.json</span>
          </div>
          <div className="fs-item">
            <File className="fs-icon" size={14} />
            <span>bio.txt</span>
          </div>
          <div className="fs-item">
            <File className="fs-icon" size={14} />
            <span>contact.md</span>
          </div>
          {quotes && quotes.length > 0 && (
            <div className="fs-item">
              <File className="fs-icon" size={14} />
              <span>quotes.txt</span>
            </div>
          )}
        </div>
      </div>

      <div className="system-info">
        <div className="info-block">
          <div className="info-header">
            <Terminal className="info-icon" size={16} />
            <span>System Information</span>
          </div>
          <div className="info-content">
            <div className="info-line">
              <span className="info-key">OS:</span>
              <span className="info-value">PortfolioOS 2.1.0</span>
            </div>
            <div className="info-line">
              <span className="info-key">Shell:</span>
              <span className="info-value">/bin/portfolio-shell</span>
            </div>
            <div className="info-line">
              <span className="info-key">User:</span>
              <span className="info-value">{profile.name.toLowerCase().replace(/\s+/g, '')}</span>
            </div>
            <div className="info-line">
              <span className="info-key">Home:</span>
              <span className="info-value">/home/{profile.name.toLowerCase().replace(/\s+/g, '')}</span>
            </div>
          </div>
        </div>

        <div className="profile-display">
          <div className="ascii-art">
            <pre className="ascii-text">{`
    ╭─────────────────╮
    │   PROFILE.EXE   │
    ╰─────────────────╯
         ┌───────┐
         │  IMG  │
         │   📷  │
         └───────┘
    `}</pre>
          </div>
          
          <div className="profile-stats">
            <div className="stat-line">
              <ChevronRight size={12} />
              <span>Status: Online</span>
            </div>
            <div className="stat-line">
              <ChevronRight size={12} />
              <span>Last Login: {new Date().toLocaleString()}</span>
            </div>
            <div className="stat-line">
              <ChevronRight size={12} />
              <span>Session: Active</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default About;