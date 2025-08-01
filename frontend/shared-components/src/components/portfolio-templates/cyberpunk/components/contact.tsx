import React, { useState } from 'react';
import { UserProfile, ContactInfo, SocialLink } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Mail, MapPin, Send, Terminal, Zap } from 'lucide-react';

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
    <div className="cyberpunk-contact">
      <div className="contact-header">
        <h2 className="section-title">
          <Terminal size={24} />
          Establish Connection
        </h2>
        <div className="connection-info">
          <span className="connection-text">Initialize communication protocol</span>
          <div className="signal-strength">
            <Zap size={16} />
            <span>Signal: Strong</span>
          </div>
        </div>
      </div>

      <div className="contact-grid">
        <Card className="contact-info-card">
          <div className="info-header">
            <h3 className="info-title">Contact Information</h3>
            <div className="status-indicator">
              <div className="status-dot online"></div>
              <span>ONLINE</span>
            </div>
          </div>
          
          <div className="contact-details">
            <div className="detail-item">
              <Mail size={20} className="detail-icon" />
              <div className="detail-content">
                <span className="detail-label">Email:</span>
                <a href={`mailto:${profile.email}`} className="detail-value">
                  {profile.email}
                </a>
              </div>
            </div>
            
            {(profile.location || contacts?.location) && (
              <div className="detail-item">
                <MapPin size={20} className="detail-icon" />
                <div className="detail-content">
                  <span className="detail-label">Location:</span>
                  <span className="detail-value">
                    {profile.location || contacts?.location}
                  </span>
                </div>
              </div>
            )}
            
            {socialLinks && socialLinks.length > 0 && (
              <div className="social-section">
                <h4 className="social-title">Neural Links:</h4>
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
                    </a>
                  ))}
                </div>
              </div>
            )}
          </div>
        </Card>

        <Card className="contact-form-card">
          <div className="form-header">
            <h3 className="form-title">Send Message</h3>
            <div className="terminal-prompt">
              <span className="prompt-symbol">$</span>
              <span className="prompt-text">compose_message --priority=high</span>
            </div>
          </div>
          
          <form onSubmit={handleSubmit} className="contact-form">
            <div className="form-grid">
              <div className="form-field">
                <label htmlFor="name" className="field-label">
                  Name:
                </label>
                <Input
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  className="cyber-input"
                  placeholder="Enter your name"
                  required
                />
              </div>
              
              <div className="form-field">
                <label htmlFor="email" className="field-label">
                  Email:
                </label>
                <Input
                  id="email"
                  name="email"
                  type="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  className="cyber-input"
                  placeholder="Enter your email"
                  required
                />
              </div>
            </div>
            
            <div className="form-field">
              <label htmlFor="subject" className="field-label">
                Subject:
              </label>
              <Input
                id="subject"
                name="subject"
                value={formData.subject}
                onChange={handleInputChange}
                className="cyber-input"
                placeholder="Message subject"
                required
              />
            </div>
            
            <div className="form-field">
              <label htmlFor="message" className="field-label">
                Message:
              </label>
              <Textarea
                id="message"
                name="message"
                value={formData.message}
                onChange={handleInputChange}
                className="cyber-textarea"
                placeholder="Enter your message..."
                rows={6}
                required
              />
            </div>
            
            <Button 
              type="submit" 
              className="submit-button"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <div className="loading-spinner"></div>
                  Transmitting...
                </>
              ) : (
                <>
                  <Send size={16} />
                  Send Message
                </>
              )}
            </Button>
          </form>
        </Card>
      </div>

      <div className="connection-footer">
        <div className="protocol-info">
          <span className="protocol-label">Protocol:</span>
          <span className="protocol-value">HTTPS/2.0 + TLS 1.3</span>
        </div>
        <div className="encryption-info">
          <span className="encryption-label">Encryption:</span>
          <span className="encryption-value">AES-256-GCM</span>
        </div>
      </div>
    </div>
  );
}

export default Contact;