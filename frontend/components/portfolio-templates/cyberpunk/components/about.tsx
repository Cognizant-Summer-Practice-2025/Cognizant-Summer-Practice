import React from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Terminal, Code, User } from 'lucide-react';

interface AboutProps {
  data: PortfolioDataFromDB;
}

export function About({ data }: AboutProps) {
  const { profile, quotes } = data;

  return (
    <div className="cyberpunk-about">
      <div className="terminal-window">
        <div className="terminal-header">
          <span className="terminal-prompt">user@neural-net:~$ </span>
          <span className="command">whoami --verbose</span>
        </div>
        
        <div className="terminal-content">
          <div className="output-line">
            <span className="output-label">NAME:</span>
            <span className="output-value">{profile.name}</span>
          </div>
          <div className="output-line">
            <span className="output-label">ROLE:</span>
            <span className="output-value">{profile.title}</span>
          </div>
          <div className="output-line">
            <span className="output-label">STATUS:</span>
            <span className="output-value status-active">ACTIVE</span>
          </div>
          <div className="output-line">
            <span className="output-label">BIO:</span>
            <span className="output-value">{profile.bio}</span>
          </div>
        </div>
      </div>

      {quotes && quotes.length > 0 && (
        <div className="quotes-section">
          <h3 className="section-title">
            <Terminal size={20} />
            Neural Fragments
          </h3>
          <div className="quotes-grid">
            {quotes.map((quote) => (
              <Card key={quote.id} className="quote-card">
                <div className="quote-content">
                  <blockquote className="quote-text">
                    "{quote.text}"
                  </blockquote>
                  {quote.author && (
                    <footer className="quote-author">
                      — {quote.author}
                      {quote.position && (
                        <span className="author-position">, {quote.position}</span>
                      )}
                    </footer>
                  )}
                </div>
              </Card>
            ))}
          </div>
        </div>
      )}

      <div className="system-info">
        <h3 className="section-title">
          <Code size={20} />
          System Information
        </h3>
        <div className="info-grid">
          <div className="info-item">
            <span className="info-label">OS:</span>
            <span className="info-value">Neural_Linux 3.14.159</span>
          </div>
          <div className="info-item">
            <span className="info-label">Kernel:</span>
            <span className="info-value">Consciousness-6.2.0</span>
          </div>
          <div className="info-item">
            <span className="info-label">Shell:</span>
            <span className="info-value">/bin/creativity</span>
          </div>
          <div className="info-item">
            <span className="info-label">Uptime:</span>
            <span className="info-value">∞ years, ∞ days</span>
          </div>
        </div>
      </div>
    </div>
  );
}