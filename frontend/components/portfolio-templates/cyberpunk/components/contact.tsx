import React, { useState } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Mail, MapPin, Send, Terminal, Zap } from 'lucide-react';

interface ContactProps {
  data: PortfolioDataFromDB;
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
            )}\n          </div>\n        </Card>\n\n        <Card className=\"contact-form-card\">\n          <div className=\"form-header\">\n            <h3 className=\"form-title\">Send Message</h3>\n            <div className=\"terminal-prompt\">\n              <span className=\"prompt-symbol\">$</span>\n              <span className=\"prompt-text\">compose_message --priority=high</span>\n            </div>\n          </div>\n          \n          <form onSubmit={handleSubmit} className=\"contact-form\">\n            <div className=\"form-grid\">\n              <div className=\"form-field\">\n                <label htmlFor=\"name\" className=\"field-label\">\n                  Name:\n                </label>\n                <Input\n                  id=\"name\"\n                  name=\"name\"\n                  value={formData.name}\n                  onChange={handleInputChange}\n                  className=\"cyber-input\"\n                  placeholder=\"Enter your name\"\n                  required\n                />\n              </div>\n              \n              <div className=\"form-field\">\n                <label htmlFor=\"email\" className=\"field-label\">\n                  Email:\n                </label>\n                <Input\n                  id=\"email\"\n                  name=\"email\"\n                  type=\"email\"\n                  value={formData.email}\n                  onChange={handleInputChange}\n                  className=\"cyber-input\"\n                  placeholder=\"Enter your email\"\n                  required\n                />\n              </div>\n            </div>\n            \n            <div className=\"form-field\">\n              <label htmlFor=\"subject\" className=\"field-label\">\n                Subject:\n              </label>\n              <Input\n                id=\"subject\"\n                name=\"subject\"\n                value={formData.subject}\n                onChange={handleInputChange}\n                className=\"cyber-input\"\n                placeholder=\"Message subject\"\n                required\n              />\n            </div>\n            \n            <div className=\"form-field\">\n              <label htmlFor=\"message\" className=\"field-label\">\n                Message:\n              </label>\n              <Textarea\n                id=\"message\"\n                name=\"message\"\n                value={formData.message}\n                onChange={handleInputChange}\n                className=\"cyber-textarea\"\n                placeholder=\"Enter your message...\"\n                rows={6}\n                required\n              />\n            </div>\n            \n            <Button \n              type=\"submit\" \n              className=\"submit-button\"\n              disabled={isSubmitting}\n            >\n              {isSubmitting ? (\n                <>\n                  <div className=\"loading-spinner\"></div>\n                  Transmitting...\n                </>\n              ) : (\n                <>\n                  <Send size={16} />\n                  Send Message\n                </>\n              )}\n            </Button>\n          </form>\n        </Card>\n      </div>\n\n      <div className=\"connection-footer\">\n        <div className=\"protocol-info\">\n          <span className=\"protocol-label\">Protocol:</span>\n          <span className=\"protocol-value\">HTTPS/2.0 + TLS 1.3</span>\n        </div>\n        <div className=\"encryption-info\">\n          <span className=\"encryption-label\">Encryption:</span>\n          <span className=\"encryption-value\">AES-256-GCM</span>\n        </div>\n      </div>\n    </div>\n  );\n}