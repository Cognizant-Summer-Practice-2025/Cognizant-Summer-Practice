import React from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { MapPin, Mail, Terminal, Zap } from 'lucide-react';

interface HeaderProps {
  data: PortfolioDataFromDB;
}

export function Header({ data }: HeaderProps) {
  const { profile, socialLinks } = data;

  return (
    <header className="cyberpunk-header">
      <div className="header-grid">
        <div className="header-info">
          <div className="profile-section">
            <div className="avatar-container">
              <Avatar className="avatar-large">
                <AvatarImage src={profile.profileImage} alt={profile.name} />
                <AvatarFallback className="avatar-fallback">
                  {profile.name.split(' ').map(n => n[0]).join('').slice(0, 2)}
                </AvatarFallback>
              </Avatar>
              <div className="avatar-glow"></div>
            </div>
            
            <div className="profile-details">
              <h1 className="profile-name">
                <span className="name-bracket">[</span>
                {profile.name}
                <span className="name-bracket">]</span>
              </h1>
              <p className="profile-title">
                <Terminal className="title-icon" size={16} />
                {profile.title}
              </p>
              
              <div className="profile-meta">
                {profile.location && (
                  <div className="meta-item">
                    <MapPin size={14} />
                    <span>{profile.location}</span>
                  </div>
                )}
                <div className="meta-item">
                  <Mail size={14} />
                  <span>{profile.email}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div className="status-section">
          <div className="status-indicators">
            <div className="status-indicator online">
              <div className="status-dot"></div>
              <span>ONLINE</span>
            </div>
            <div className="connection-status">
              <Zap size={16} />
              <span>NEURAL LINK ACTIVE</span>
            </div>
          </div>
          
          {socialLinks && socialLinks.length > 0 && (
            <div className="social-links">
              {socialLinks.map((link) => (
                <a
                  key={link.id}
                  href={link.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="social-link"
                >
                  <div className="social-icon">
                    {link.platform.toLowerCase()}
                  </div>
                </a>
              ))}
            </div>
          )}
        </div>
      </div>
      
      <div className="bio-section">
        <div className="bio-container">
          <div className="bio-prompt">
            <span className="prompt-symbol">$</span>
            <span className="prompt-command">cat profile.bio</span>
          </div>
          <p className="bio-text">{profile.bio}</p>
        </div>
      </div>
    </header>
  );
}

