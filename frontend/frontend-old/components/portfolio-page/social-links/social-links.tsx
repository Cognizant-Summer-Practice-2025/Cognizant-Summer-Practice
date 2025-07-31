import React from 'react';
import './style.css';

interface SocialLinkProps {
  icon: string;
  label: string;
  href?: string;
}

const SocialLink: React.FC<SocialLinkProps> = ({ icon, label }) => {
  return (
    <div className="social-link">
      <div className="social-icon">
        <div className="social-icon-text">{icon}</div>
      </div>
      <div className="social-label-container">
        <div className="social-label">{label}</div>
      </div>
    </div>
  );
};

const SocialLinksSection = () => {
  const socialLinks = [
    { icon: "💼", label: "LinkedIn", href: "#" },
    { icon: "🐙", label: "GitHub", href: "#" },
    { icon: "🐦", label: "Twitter", href: "#" }
  ];

  return (
    <div className="social-links-container">
      {socialLinks.map((social, index) => (
        <SocialLink 
          key={index} 
          icon={social.icon} 
          label={social.label} 
          href={social.href} 
        />
      ))}
    </div>
  );
};

export default SocialLinksSection;