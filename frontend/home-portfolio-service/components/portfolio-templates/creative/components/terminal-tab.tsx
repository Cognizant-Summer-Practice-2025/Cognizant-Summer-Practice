import React, { useState, useEffect } from 'react';
import { X, Terminal } from 'lucide-react';

interface TerminalTabProps {
  onClose: () => void;
}

export function TerminalTab({ onClose }: TerminalTabProps) {
  const [currentLine, setCurrentLine] = useState(0);
  const [isTyping, setIsTyping] = useState(true);

  const terminalLines = [
    'portfolio@dev:~$ whoami',
    'creative-developer',
    '',
    'portfolio@dev:~$ ls -la skills/',
    'total 42',
    'drwxr-xr-x  2 dev dev 4096 Jan 15 10:30 .',
    'drwxr-xr-x  5 dev dev 4096 Jan 15 10:30 ..',
    '-rw-r--r--  1 dev dev 1337 Jan 15 10:30 javascript.js',
    '-rw-r--r--  1 dev dev 2048 Jan 15 10:30 react.tsx',
    '-rw-r--r--  1 dev dev 1024 Jan 15 10:30 nodejs.js',
    '-rw-r--r--  1 dev dev  512 Jan 15 10:30 python.py',
    '',
    'portfolio@dev:~$ git status',
    'On branch creative-template',
    'Your branch is up to date with \'origin/creative-template\'.',
    '',
    'Changes to be committed:',
    '  (use "git reset HEAD <file>..." to unstage)',
    '',
    '        new file:   creative-template.tsx',
    '        modified:   portfolio.json',
    '',
    'portfolio@dev:~$ npm run dev',
            'Server running at https://auth-user-service.lemongrass-88207da5.northeurope.azurecontainerapps.io',
    '✨ Portfolio ready to impress!',
    '',
    'portfolio@dev:~$ echo "Ready to create amazing things!"',
    'Ready to create amazing things!',
    '',
    'portfolio@dev:~$ █'
  ];

  useEffect(() => {
    if (currentLine < terminalLines.length - 1) {
      const timer = setTimeout(() => {
        setCurrentLine(prev => prev + 1);
      }, 150);
      return () => clearTimeout(timer);
    } else {
      setIsTyping(false);
    }
  }, [currentLine, terminalLines.length]);

  return (
    <div className="terminal-section">
      <div className="terminal-header">
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Terminal size={14} />
          <span>Terminal</span>
        </div>
        <button
          onClick={onClose}
          style={{
            background: 'none',
            border: 'none',
            color: 'var(--text-secondary)',
            cursor: 'pointer',
            padding: '4px',
            borderRadius: '4px'
          }}
        >
          <X size={14} />
        </button>
      </div>
      <div className="terminal-content">
        {terminalLines.slice(0, currentLine + 1).map((line, index) => (
          <div key={index} style={{ 
            marginBottom: '2px',
            fontFamily: 'Consolas, Monaco, monospace',
            fontSize: '13px',
            whiteSpace: 'pre-wrap'
          }}>
            {line.includes('portfolio@dev:~$') ? (
              <span>
                <span style={{ color: '#00ff00' }}>portfolio@dev</span>
                <span style={{ color: '#ffffff' }}>:</span>
                <span style={{ color: '#0099ff' }}>~</span>
                <span style={{ color: '#ffffff' }}>$ </span>
                <span style={{ color: '#ffff00' }}>{line.split('$ ')[1]}</span>
              </span>
            ) : line.includes('✨') ? (
              <span style={{ color: '#ff69b4' }}>{line}</span>
            ) : line.includes('Changes to be committed') || line.includes('new file') || line.includes('modified') ? (
              <span style={{ color: '#00ff00' }}>{line}</span>
            ) : line.includes('Server running') ? (
              <span style={{ color: '#00ffff' }}>{line}</span>
            ) : line.includes('Ready to create') ? (
              <span style={{ color: '#ffff00' }}>{line}</span>
            ) : (
              <span style={{ color: '#cccccc' }}>{line}</span>
            )}
            {index === currentLine && isTyping && (
              <span style={{ color: '#00ff00', animation: 'pulse 1s infinite' }}>█</span>
            )}
          </div>
        ))}
      </div>
    </div>
  );
} 