import React from 'react';
import { UserProfile, Quote } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Gamepad2, Heart, Star, Trophy } from 'lucide-react';

interface AboutProps {
  data: {
    profile: UserProfile;
    quotes: Quote[];
  };
}

export function About({ data }: AboutProps) {
  const { profile, quotes } = data;
  const quote = quotes && quotes.length > 0 ? quotes[0] : null;

  return (
    <div className="retro-about">
      <div className="section-header">
        <h2 className="section-title">
          <Star className="pixel-icon" size={24} />
          PLAYER INFO
        </h2>
        <div className="pixel-border"></div>
      </div>

      <div className="about-grid">
        <Card className="profile-card">
          <div className="profile-header">
            <div className="profile-avatar-container">
              <img
                src={profile.profileImage}
                alt={profile.name}
                className="profile-avatar pixel-art"
              />
              <div className="avatar-frame"></div>
            </div>
            <div className="profile-info">
              <h3 className="profile-name">{profile.name}</h3>
              <p className="profile-title">{profile.title}</p>
              <div className="level-indicator">
                <Trophy className="pixel-icon" size={16} />
                <span>LVL 99</span>
              </div>
            </div>
          </div>

          <div className="profile-stats">
            <div className="stat-bar">
              <span className="stat-label">STR</span>
              <div className="stat-progress">
                <div className="stat-fill" style={{ width: '85%' }}></div>
              </div>
              <span className="stat-value">85</span>
            </div>
            <div className="stat-bar">
              <span className="stat-label">INT</span>
              <div className="stat-progress">
                <div className="stat-fill" style={{ width: '92%' }}></div>
              </div>
              <span className="stat-value">92</span>
            </div>
            <div className="stat-bar">
              <span className="stat-label">DEX</span>
              <div className="stat-progress">
                <div className="stat-fill" style={{ width: '78%' }}></div>
              </div>
              <span className="stat-value">78</span>
            </div>
            <div className="stat-bar">
              <span className="stat-label">CHA</span>
              <div className="stat-progress">
                <div className="stat-fill" style={{ width: '88%' }}></div>
              </div>
              <span className="stat-value">88</span>
            </div>
          </div>
        </Card>

        {quote && (
          <Card className="quote-card">
            <div className="quote-header">
              <Gamepad2 className="pixel-icon" size={20} />
              <h4 className="quote-title">SPEECH BUBBLE</h4>
            </div>
            <blockquote className="quote-text">
              "{quote.text}"
            </blockquote>
            <footer className="quote-author">
              - {quote.author}
            </footer>
            <div className="speech-tail"></div>
          </Card>
        )}

        <Card className="abilities-card">
          <div className="abilities-header">
            <h4 className="abilities-title">SPECIAL ABILITIES</h4>
            <div className="abilities-border"></div>
          </div>
          <div className="abilities-list">
            <div className="ability-item">
              <div className="ability-icon">⚡</div>
              <div className="ability-info">
                <span className="ability-name">Code Mastery</span>
                <span className="ability-desc">Deal 2x damage with programming</span>
              </div>
            </div>
            <div className="ability-item">
              <div className="ability-icon">🛡️</div>
              <div className="ability-info">
                <span className="ability-name">Bug Shield</span>
                <span className="ability-desc">Immune to critical errors</span>
              </div>
            </div>
            <div className="ability-item">
              <div className="ability-icon">🚀</div>
              <div className="ability-info">
                <span className="ability-name">Deploy Rush</span>
                <span className="ability-desc">Instant project deployment</span>
              </div>
            </div>
          </div>
        </Card>
      </div>

      <div className="achievement-section">
        <h4 className="achievement-title">RECENT ACHIEVEMENTS</h4>
        <div className="achievement-list">
          <div className="achievement-item unlocked">
            <Trophy className="achievement-icon" size={16} />
            <span className="achievement-name">Full Stack Master</span>
            <span className="achievement-points">+1000 XP</span>
          </div>
          <div className="achievement-item unlocked">
            <Star className="achievement-icon" size={16} />
            <span className="achievement-name">Bug Crusher</span>
            <span className="achievement-points">+500 XP</span>
          </div>
          <div className="achievement-item locked">
            <Heart className="achievement-icon" size={16} />
            <span className="achievement-name">Team Player</span>
            <span className="achievement-points">+750 XP</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default About;