import React from 'react';
import { User, Mail, MapPin, Code2 } from 'lucide-react';

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
          <span className="syntax-string">&quot;{basicInfo.name || 'Developer'}&quot;</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">role</span>: 
          <span className="syntax-string">&quot;{basicInfo.title || 'Full Stack Developer'}&quot;</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">email</span>: 
          <span className="syntax-string">&quot;<a
            href={`mailto:${basicInfo.email || 'developer@example.com'}`}
            style={{
              color: 'inherit',
              textDecoration: 'none',
              transition: 'color 0.2s ease',
              fontFamily: 'inherit'
            }}
            onMouseEnter={e => {
              e.currentTarget.style.color = 'var(--primary-color)';
            }}
            onMouseLeave={e => {
              e.currentTarget.style.color = 'inherit';
            }}
          >{basicInfo.email || 'developer@example.com'}</a>&quot;</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">location</span>: 
          <span className="syntax-string">&quot;{basicInfo.location || 'Remote'}&quot;</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">passion</span>: 
          <span className="syntax-string">&quot;Building amazing user experiences&quot;</span>,
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-highlight">status</span>: 
          <span className="syntax-string">&quot;Available for opportunities&quot;</span>
        </div>
        <div className="code-line">{'}'};</div>
      </div>

      {basicInfo.bio && (
        <div style={{ marginTop: '16px' }}>
          <div className="code-line">{/* About me */}</div>
          <div className="code-line">
            <span className="syntax-keyword">console</span>.log(
            <span className="syntax-string">&quot;{basicInfo.bio}&quot;</span>);
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
          <a 
            href={`mailto:${basicInfo.email || 'developer@example.com'}`}
            style={{ 
              color: 'inherit', 
              textDecoration: 'none',
              transition: 'color 0.2s ease'
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.color = 'var(--primary-color)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.color = 'inherit';
            }}
          >
            {basicInfo.email || 'developer@example.com'}
          </a>
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