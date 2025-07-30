import React, { useState } from 'react';
import { UserProfile, ContactInfo, SocialLink } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Mail, MapPin, Send, Gamepad2, Heart, MessageSquare } from 'lucide-react';

interface ContactProps {
  data: {
    profile: UserProfile;
    contacts: ContactInfo;
    socialLinks: SocialLink[];
  };
}

export function Contact({ data }: ContactProps) {
  const { profile, contacts, socialLinks } = data;
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: ''
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    
    // Simulate form submission
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Reset form
    setFormData({
      name: '',
      email: '',
      subject: '',
      message: ''
    });
    setIsSubmitting(false);
  };

  return (
    <div className="retro-contact" id="contact">
      <div className="section-header">
        <h2 className="section-title">
          <MessageSquare className="pixel-icon" size={24} />
          COMMUNICATION HUB
        </h2>
        <div className="pixel-border"></div>
        <p className="section-subtitle">Initiate Player-to-Player Connection</p>
      </div>

      <div className="contact-grid">
        <Card className="contact-info-card">
          <div className="info-header">
            <Gamepad2 className="pixel-icon" size={24} />
            <h3 className="info-title">Player Profile</h3>
            <div className="status-indicator">
              <div className="status-dot online"></div>
              <span>ONLINE</span>
            </div>
          </div>
          
          <div className="player-card">
            <div className="player-avatar">
              <img
                src={profile.profileImage}
                alt={profile.name}
                className="avatar-image pixel-art"
              />
              <div className="avatar-frame"></div>
            </div>
            <div className="player-info">
              <h4 className="player-name">{profile.name}</h4>
              <p className="player-class">{profile.title}</p>
              <div className="player-level">
                <span>Level 99 Developer</span>
              </div>
            </div>
          </div>

          <div className="contact-details">
            <div className="detail-item">
              <Mail className="detail-icon" size={20} />
              <div className="detail-content">
                <span className="detail-label">Email Channel:</span>
                <a href={`mailto:${profile.email}`} className="detail-value">
                  {profile.email}
                </a>
              </div>
            </div>
            
            {(profile.location || contacts?.location) && (
              <div className="detail-item">
                <MapPin className="detail-icon" size={20} />
                <div className="detail-content">
                  <span className="detail-label">Base Location:</span>
                  <span className="detail-value">
                    {profile.location || contacts?.location}
                  </span>
                </div>
              </div>
            )}
          </div>

          {socialLinks && socialLinks.length > 0 && (
            <div className="social-section">
              <h4 className="social-title">Guild Networks:</h4>
              <div className="social-grid">
                {socialLinks.map((link) => (
                  <a
                    key={link.id}
                    href={link.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="social-link"
                  >
                    <div className="social-platform">
                      {link.platform}
                    </div>
                    <div className="link-glow"></div>
                  </a>
                ))}
              </div>
            </div>
          )}

          <div className="connection-stats">
            <div className="stat-item">
              <span className="stat-label">Response Time:</span>
              <span className="stat-value">Fast</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Availability:</span>
              <span className="stat-value">24/7</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Connection:</span>
              <span className="stat-value">Stable</span>
            </div>
          </div>
        </Card>

        <Card className="message-form-card">
          <div className="form-header">
            <Send className="pixel-icon" size={24} />
            <h3 className="form-title">Send Message</h3>
            <div className="command-prompt">
              <span className="prompt">$</span>
              <span className="command">compose_message --to=player</span>
            </div>
          </div>
          
          <form onSubmit={handleSubmit} className="message-form">
            <div className="form-grid">
              <div className="form-field">
                <label htmlFor="name" className="field-label">
                  Player Name:
                </label>
                <Input
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  className="retro-input"
                  placeholder="Enter your name"
                  required
                />
                <div className="input-glow"></div>
              </div>
              
              <div className="form-field">
                <label htmlFor="email" className="field-label">
                  Email Address:
                </label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  className="retro-input"
                  placeholder="Enter your email"
                  required
                />
                <div className="input-glow"></div>
              </div>
            </div>
            
            <div className="form-field">
              <label htmlFor="subject" className="field-label">
                Message Subject:
              </label>
              <Input
                id="subject"
                name="subject"
                value={formData.subject}
                onChange={handleInputChange}
                className="retro-input"
                placeholder="What's this about?"
                required
              />
              <div className="input-glow"></div>
            </div>
            
            <div className="form-field">
              <label htmlFor="message" className="field-label">
                Message Content:
              </label>
              <Textarea
                id="message"
                name="message"
                value={formData.message}
                onChange={handleInputChange}
                className="retro-textarea"
                placeholder="Type your message here..."
                rows={6}
                required
              />
              <div className="input-glow"></div>
            </div>
            
            <Button 
              type="submit" 
              className="submit-button pixel-button"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <div className="loading-spinner pixel-spinner"></div>
                  <span>TRANSMITTING...</span>
                </>
              ) : (
                <>
                  <Send size={16} />
                  <span>SEND MESSAGE</span>
                </>
              )}
            </Button>
          </form>
        </Card>
      </div>

      <div className="connection-footer">
        <div className="footer-stats">
          <div className="connection-quality">
            <span className="quality-label">Connection Quality:</span>
            <div className="signal-bars">
              <div className="bar active"></div>
              <div className="bar active"></div>
              <div className="bar active"></div>
              <div className="bar active"></div>
              <div className="bar active"></div>
            </div>
            <span className="quality-text">Excellent</span>
          </div>
          
          <div className="encryption-info">
            <span className="encryption-label">Security:</span>
            <span className="encryption-value">256-bit Encrypted</span>
          </div>
        </div>

        <div className="fun-facts">
          <div className="fact-item">
            <Heart className="pixel-icon" size={16} />
            <span>Messages powered by ♥ and ☕</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Contact;