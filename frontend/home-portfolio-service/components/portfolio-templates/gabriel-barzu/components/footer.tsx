import React from 'react';
import { SocialLink } from '@/lib/portfolio';

interface FooterProps {
  socialLinks: SocialLink[];
}

export function Footer({ socialLinks }: FooterProps) {

  const getSocialIcon = (platform: string) => {
    const iconMap: { [key: string]: string } = {
      github: '🔗',
      linkedin: '💼',
      twitter: '🐦',
      instagram: '📷',
      facebook: '📘',
      youtube: '📺',
      email: '📧',
      website: '🌐'
    };
    return iconMap[platform.toLowerCase()] || '🔗';
  };

  return (
    <footer className="gb-footer">
      <div className="footer-content">
        {socialLinks && socialLinks.length > 0 && (
          <div className="social-links">
            <h4 className="social-title">Connect with me</h4>
            <div className="social-grid">
              {socialLinks.map((link) => (
                <a
                  key={link.id}
                  href={link.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="social-link"
                  aria-label={`Visit ${link.platform}`}
                >
                  <span className="social-icon">{getSocialIcon(link.platform)}</span>
                  <span className="social-name">{link.platform}</span>
                </a>
              ))}
            </div>
          </div>
        )}
        
      </div>
    </footer>
  );
} 