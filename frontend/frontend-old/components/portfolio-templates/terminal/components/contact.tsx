import React, { useState, useEffect } from 'react';
import { UserProfile, ContactInfo, SocialLink } from '@/lib/portfolio';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Mail, MapPin, Send, Terminal, User, Globe, MessageSquare } from 'lucide-react';

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
  const [commandHistory, setCommandHistory] = useState<string[]>([]);

  useEffect(() => {
    // Simulate contact info loading commands
    const commands = [
      '$ whois portfolio.local',
      `Registrant Name: ${profile.name}`,
      `Registrant Email: ${profile.email}`,
      `Registrant Location: ${profile.location || 'Unknown'}`,
      '',
      '$ ping portfolio.local',
      'PING portfolio.local (127.0.0.1): 56 data bytes',
      '64 bytes from 127.0.0.1: icmp_seq=1 ttl=64 time=0.1ms',
      '64 bytes from 127.0.0.1: icmp_seq=2 ttl=64 time=0.1ms',
      '--- portfolio.local ping statistics ---',
      '2 packets transmitted, 2 received, 0% packet loss',
      '',
      '$ netstat -an | grep :80',
      'tcp4  0  0  127.0.0.1.80  *.*  LISTEN'
    ];
    
    let index = 0;
    const interval = setInterval(() => {
      if (index < commands.length) {
        setCommandHistory(prev => [...prev, commands[index]]);
        index++;
      } else {
        clearInterval(interval);
      }
    }, 200);

    return () => clearInterval(interval);
  }, [profile]);

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
    <div className="terminal-contact" id="contact">
      <div className="contact-terminal">
        <div className="terminal-header">
          <div className="window-controls">
            <span className="control red"></span>
            <span className="control yellow"></span>
            <span className="control green"></span>
          </div>
          <span className="window-title">contact_info.sh</span>
        </div>
        
        <div className="terminal-content">
          <div className="command-output">
            {commandHistory.map((line, index) => (
              <div key={index} className="output-line">
                {line.startsWith('$') ? (
                  <span className="command-line">
                    <Terminal className="cmd-icon" size={12} />
                    {line}
                  </span>
                ) : line.startsWith('PING') || line.startsWith('64 bytes') ? (
                  <span className="ping-line">
                    <Globe className="ping-icon" size={12} />
                    {line}
                  </span>
                ) : (
                  line
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="contact-interface">
        <div className="contact-info-section">
          <div className="info-header">
            <User className="info-icon" size={20} />
            <span>Contact Information</span>
            <div className="status-indicator">
              <span className="status-dot online"></span>
              <span className="status-text">Online</span>
            </div>
          </div>
          
          <div className="info-grid">
            <div className="info-card">
              <div className="card-header">
                <Mail className="card-icon" size={16} />
                <span className="card-title">Email</span>
              </div>
              <div className="card-content">
                <a href={`mailto:${profile.email}`} className="contact-link">
                  {profile.email}
                </a>
                <div className="command-hint">
                  <span className="prompt">$</span>
                  <span className="command">mail {profile.email}</span>
                </div>
              </div>
            </div>

            {(profile.location || contacts?.location) && (
              <div className="info-card">
                <div className="card-header">
                  <MapPin className="card-icon" size={16} />
                  <span className="card-title">Location</span>
                </div>
                <div className="card-content">
                  <span className="location-text">
                    {profile.location || contacts?.location}
                  </span>
                  <div className="command-hint">
                    <span className="prompt">$</span>
                    <span className="command">geoip {profile.location || contacts?.location}</span>
                  </div>
                </div>
              </div>
            )}


          </div>

          {socialLinks && socialLinks.length > 0 && (
            <div className="social-links-section">
              <div className="social-header">
                <Globe className="social-icon" size={16} />
                <span>Social Networks</span>
              </div>
              <div className="social-grid">
                {socialLinks.map((link) => (
                  <div key={link.id} className="social-card">
                    <div className="social-platform">{link.platform}</div>
                    <a 
                      href={link.url} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="social-link"
                    >
                      Visit Profile
                    </a>
                    <div className="command-hint">
                      <span className="prompt">$</span>
                      <span className="command">open {link.platform.toLowerCase()}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        <div className="message-form-section">
          <div className="form-terminal">
            <div className="terminal-header">
              <div className="window-controls">
                <span className="control red"></span>
                <span className="control yellow"></span>
                <span className="control green"></span>
              </div>
              <span className="window-title">send_message.sh</span>
            </div>
            
            <div className="form-content">
              <div className="form-header">
                <MessageSquare className="form-icon" size={20} />
                <span className="form-title">Send Message</span>
              </div>
              
              <div className="command-prompt">
                <span className="prompt">$</span>
                <span className="command">compose --to={profile.email}</span>
              </div>
              
              <form onSubmit={handleSubmit} className="contact-form">
                <div className="form-row">
                  <div className="form-field">
                    <label htmlFor="name" className="field-label">
                      --from-name=
                    </label>
                    <Input
                      id="name"
                      name="name"
                      value={formData.name}
                      onChange={handleInputChange}
                      className="terminal-input"
                      placeholder="Your name"
                      required
                    />
                  </div>
                  
                  <div className="form-field">
                    <label htmlFor="email" className="field-label">
                      --from-email=
                    </label>
                    <Input
                      id="email"
                      name="email"
                      type="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      className="terminal-input"
                      placeholder="your@email.com"
                      required
                    />
                  </div>
                </div>
                
                <div className="form-field">
                  <label htmlFor="subject" className="field-label">
                    --subject=
                  </label>
                  <Input
                    id="subject"
                    name="subject"
                    value={formData.subject}
                    onChange={handleInputChange}
                    className="terminal-input"
                    placeholder="Message subject"
                    required
                  />
                </div>
                
                <div className="form-field">
                  <label htmlFor="message" className="field-label">
                    --message=
                  </label>
                  <Textarea
                    id="message"
                    name="message"
                    value={formData.message}
                    onChange={handleInputChange}
                    className="terminal-textarea"
                    placeholder="Type your message here..."
                    rows={6}
                    required
                  />
                </div>
                
                <div className="form-actions">
                  <Button 
                    type="submit" 
                    className="submit-button terminal-button"
                    disabled={isSubmitting}
                  >
                    {isSubmitting ? (
                      <>
                        <div className="loading-dots">
                          <span>.</span><span>.</span><span>.</span>
                        </div>
                        <span>Sending...</span>
                      </>
                    ) : (
                      <>
                        <Send size={16} />
                        <span>$ send</span>
                      </>
                    )}
                  </Button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>

      <div className="connection-status">
        <div className="status-header">
          <Terminal className="status-icon" size={16} />
          <span>Connection Status</span>
        </div>
        
        <div className="status-grid">
          <div className="status-item">
            <span className="status-label">Server:</span>
            <span className="status-value">portfolio.local</span>
            <span className="status-indicator online"></span>
          </div>
          <div className="status-item">
            <span className="status-label">Port:</span>
            <span className="status-value">443 (HTTPS)</span>
            <span className="status-indicator online"></span>
          </div>
          <div className="status-item">
            <span className="status-label">Latency:</span>
            <span className="status-value">&lt; 1ms</span>
            <span className="status-indicator online"></span>
          </div>
          <div className="status-item">
            <span className="status-label">Encryption:</span>
            <span className="status-value">TLS 1.3</span>
            <span className="status-indicator online"></span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Contact;