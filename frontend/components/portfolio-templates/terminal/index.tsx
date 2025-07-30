"use client";

import React, { useState, useEffect, useRef } from 'react';
import { PortfolioDataFromDB } from '@/lib/portfolio';
import { TemplateManager, ComponentMap } from '@/lib/template-manager';
import { Header } from './components/header';
import { Stats } from './components/stats';
import { Contact } from './components/contact';
import { About } from './components/about';
import { Experience } from './components/experience';
import { Projects } from './components/projects';
import { Skills } from './components/skills';
import { BlogPosts } from './components/blog-posts';
import { Terminal, Folder, File, User, Code, Database, Mail } from 'lucide-react';
import './styles/main.css';

interface TerminalTemplateProps {
  data: PortfolioDataFromDB;
}

// Component mapping for the template manager
const componentMap: ComponentMap = {
  experience: Experience as React.ComponentType<{ data: unknown }>,
  projects: Projects as React.ComponentType<{ data: unknown }>,
  skills: Skills as React.ComponentType<{ data: unknown }>,
  blog_posts: BlogPosts as React.ComponentType<{ data: unknown }>,
  contact: Contact as React.ComponentType<{ data: unknown }>,
  about: About as React.ComponentType<{ data: unknown }>
};

// Command aliases for different sections
const commands: Record<string, string> = {
  'about': 'about',
  'whoami': 'about',
  'experience': 'experience',
  'work': 'experience',
  'projects': 'projects',
  'ls': 'projects',
  'skills': 'skills',
  'tech': 'skills',
  'blog': 'blog_posts',
  'posts': 'blog_posts',
  'contact': 'contact',
  'email': 'contact',
  'stats': 'stats',
  'info': 'stats'
};

const helpText = `
Available commands:
  about, whoami     - Show personal information
  experience, work  - Display work experience
  projects, ls      - List projects
  skills, tech      - Show technical skills
  blog, posts       - Display blog posts
  contact, email    - Contact information
  stats, info       - Portfolio statistics
  help              - Show this help
  clear             - Clear terminal
  history           - Show command history
`;

export default function TerminalTemplate({ data }: TerminalTemplateProps) {
  const [activeSection, setActiveSection] = useState('about');
  const [terminalHistory, setTerminalHistory] = useState<string[]>([]);
  const [currentCommand, setCurrentCommand] = useState('');
  const [commandHistory, setCommandHistory] = useState<string[]>([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const [isTyping, setIsTyping] = useState(false);
  const terminalRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  
  // Initialize template manager
  const templateManager = new TemplateManager(componentMap);
  
  // Validate and transform the data before use
  const validatedData = TemplateManager.validateAndTransformPortfolioData(data);
  
  // Get dynamic components based on portfolio configuration
  const dynamicComponents = templateManager.renderComponents(validatedData);

  // Filter components for navigation
  const navComponents = dynamicComponents.filter(comp => 
    comp && ['about', 'experience', 'projects', 'skills', 'blog_posts', 'contact', 'stats'].includes(comp.type)
  );

  // Auto-focus input and scroll to bottom
  useEffect(() => {
    if (inputRef.current) {
      inputRef.current.focus();
    }
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight;
    }
  }, [terminalHistory]);

  // Initialize with welcome message
  useEffect(() => {
    const welcomeMessages = [
      `Welcome to ${validatedData.profile.name}'s Portfolio Terminal`,
      `Portfolio OS v2.1.0 (${new Date().toDateString()})`,
      '',
      'Type "help" for available commands.',
      ''
    ];
    setTerminalHistory(welcomeMessages);
  }, [validatedData.profile.name]);

  const executeCommand = (cmd: string) => {
    const trimmedCmd = cmd.trim().toLowerCase();
    const newHistory = [...terminalHistory, `$ ${cmd}`];
    
    if (!trimmedCmd) {
      setTerminalHistory([...newHistory, '']);
      return;
    }

    // Add to command history
    if (trimmedCmd !== commandHistory[commandHistory.length - 1]) {
      setCommandHistory(prev => [...prev, trimmedCmd]);
    }
    setHistoryIndex(-1);

    switch (trimmedCmd) {
      case 'help':
        setTerminalHistory([...newHistory, helpText]);
        break;
      
      case 'clear':
        setTerminalHistory([]);
        break;
      
      case 'history':
        const historyOutput = commandHistory.map((cmd, index) => 
          `  ${index + 1}  ${cmd}`
        ).join('\n');
        setTerminalHistory([...newHistory, historyOutput || 'No command history']);
        break;
      
      default:
        if (commands[trimmedCmd]) {
          setActiveSection(commands[trimmedCmd]);
          setTerminalHistory([...newHistory, `Loading ${commands[trimmedCmd]}...`]);
        } else {
          setTerminalHistory([...newHistory, `Command not found: ${trimmedCmd}. Type 'help' for available commands.`]);
        }
        break;
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      executeCommand(currentCommand);
      setCurrentCommand('');
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      if (commandHistory.length > 0) {
        const newIndex = historyIndex === -1 ? commandHistory.length - 1 : Math.max(0, historyIndex - 1);
        setHistoryIndex(newIndex);
        setCurrentCommand(commandHistory[newIndex] || '');
      }
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (historyIndex >= 0) {
        const newIndex = historyIndex + 1;
        if (newIndex >= commandHistory.length) {
          setHistoryIndex(-1);
          setCurrentCommand('');
        } else {
          setHistoryIndex(newIndex);
          setCurrentCommand(commandHistory[newIndex]);
        }
      }
    } else if (e.key === 'Tab') {
      e.preventDefault();
      // Simple autocomplete
      const availableCommands = Object.keys(commands);
      const matches = availableCommands.filter(cmd => cmd.startsWith(currentCommand.toLowerCase()));
      if (matches.length === 1) {
        setCurrentCommand(matches[0]);
      }
    }
  };

  const renderActiveComponent = () => {
    const activeComponent = navComponents.find(comp => comp && comp.type === activeSection);
    if (!activeComponent) return null;

    return activeComponent.component;
  };

  const getCurrentDir = () => {
    switch (activeSection) {
      case 'about': return '~/about';
      case 'experience': return '~/experience';
      case 'projects': return '~/projects';
      case 'skills': return '~/skills';
      case 'blog_posts': return '~/blog';
      case 'contact': return '~/contact';
      case 'stats': return '~/stats';
      default: return '~';
    }
  };

  return (
    <div className="terminal-template">
      <div className="terminal-window">
        <div className="terminal-header">
          <div className="window-controls">
            <div className="control close"></div>
            <div className="control minimize"></div>
            <div className="control maximize"></div>
          </div>
          <div className="terminal-title">
            {validatedData.profile.name} - Portfolio Terminal
          </div>
          <div className="terminal-status">
            <span className="status-indicator online"></span>
            <span>Connected</span>
          </div>
        </div>

        <div className="terminal-body">
          {/* Terminal Output */}
          <div className="terminal-output" ref={terminalRef}>
            {terminalHistory.map((line, index) => (
              <div key={index} className="terminal-line">
                {line}
              </div>
            ))}
          </div>

          {/* Command Input */}
          <div className="terminal-input-line">
            <span className="prompt">
              <span className="prompt-user">{validatedData.profile.name.toLowerCase().replace(/\s+/g, '')}</span>
              <span className="prompt-separator">@</span>
              <span className="prompt-host">portfolio</span>
              <span className="prompt-separator">:</span>
              <span className="prompt-path">{getCurrentDir()}</span>
              <span className="prompt-symbol">$</span>
            </span>
            <input
              ref={inputRef}
              type="text"
              value={currentCommand}
              onChange={(e) => setCurrentCommand(e.target.value)}
              onKeyDown={handleKeyDown}
              className="terminal-input"
              autoComplete="off"
              autoFocus
            />
            <span className="cursor">_</span>
          </div>
        </div>
      </div>

      {/* Content Display */}
      <div className="content-display">
        <div className="content-header">
          <div className="breadcrumb">
            <Folder size={16} />
            <span>{getCurrentDir()}</span>
          </div>
          <div className="file-info">
            <File size={16} />
            <span>{activeSection}.md</span>
          </div>
        </div>
        
        <div className="content-body">
          {renderActiveComponent()}
        </div>
      </div>

      {/* Command Suggestions */}
      <div className="command-suggestions">
        <div className="suggestions-header">
          <Terminal size={16} />
          <span>Quick Commands</span>
        </div>
        <div className="suggestions-grid">
          {Object.entries(commands).slice(0, 8).map(([cmd, section]) => (
            <button
              key={cmd}
              onClick={() => {
                setCurrentCommand(cmd);
                executeCommand(cmd);
              }}
              className={`suggestion-btn ${activeSection === section ? 'active' : ''}`}
            >
              {cmd}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}