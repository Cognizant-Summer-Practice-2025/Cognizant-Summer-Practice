import React from 'react';
import { User, Mail, MapPin, Calendar, Code2 } from 'lucide-react';

interface BasicInfo {
  name?: string;
  title?: string;
  email?: string;
  location?: string;
  bio?: string;
  avatar?: string;
}

interface HeaderProps {
  basicInfo: BasicInfo;
}

export function Header({ basicInfo }: HeaderProps) {
  return (
    <div className="creative-header">
      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">const</span>{' '}
          <span className="syntax-highlight">developer</span> = {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">name</span>: 
          <span className="syntax-string">"{basicInfo.name || 'Developer'}"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">role</span>: 
          <span className="syntax-string">"{basicInfo.title || 'Full Stack Developer'}"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">email</span>: 
          <span className="syntax-string">"{basicInfo.email || 'developer@example.com'}"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">location</span>: 
          <span className="syntax-string">"{basicInfo.location || 'Remote'}"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">passion</span>: 
          <span className="syntax-string">"Building amazing user experiences"</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">status</span>: 
          <span className="syntax-string">"Available for opportunities"</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      {basicInfo.bio && (
        <div style={{ marginTop: '16px' }}>
          <div className="code-line">
            <span className="syntax-comment">// About me</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">console</span>.log(
            <span className="syntax-string">"{basicInfo.bio}"</span>);
          </div>
        </div>
      )}

      <div style={{ marginTop: '24px', display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <User size={16} />
          <span>{basicInfo.name || 'Developer'}</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Mail size={16} />
          <span>{basicInfo.email || 'developer@example.com'}</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <MapPin size={16} />
          <span>{basicInfo.location || 'Remote'}</span>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Code2 size={16} />
          <span>{basicInfo.title || 'Full Stack Developer'}</span>
        </div>
      </div>
    </div>
  );
} 