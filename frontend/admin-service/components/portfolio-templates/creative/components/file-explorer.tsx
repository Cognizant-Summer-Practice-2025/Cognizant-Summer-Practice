import React from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';

interface FileItem {
  name: string;
  icon: React.ComponentType<{ size?: number }>;
  component: string;
}

interface FileExplorerProps {
  files: FileItem[];
  onFileClick: (fileName: string, componentType: string) => void;
  activeFile: string;
}

export function FileExplorer({ files, onFileClick, activeFile }: FileExplorerProps) {
  return (
    <div className="file-explorer">
      <h3>Portfolio Explorer</h3>
      
      <div style={{ marginBottom: '16px' }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '4px',
          fontSize: '12px',
          color: 'var(--text-secondary)',
          marginBottom: '8px'
        }}>
          <ChevronDown size={12} />
          <span>src</span>
        </div>
        
        <ul className="file-list" style={{ marginLeft: '16px' }}>
          {files.map((file) => {
            const IconComponent = file.icon;
            return (
              <li
                key={file.name}
                className={`file-item ${activeFile === file.name ? 'active' : ''}`}
                onClick={() => onFileClick(file.name, file.component)}
              >
                <IconComponent size={16} />
                                  <span className="file-name-mobile">
                    {file.name === 'about.md' ? 'About' :
                     file.name === 'experience.json' ? 'Work' :
                     file.name === 'projects/' ? 'Projects' :
                     file.name === 'skills.js' ? 'Skills' :
                     file.name === 'blog/' ? 'Blog' : file.name}
                  </span>
              </li>
            );
          })}
        </ul>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '4px',
          fontSize: '12px',
          color: 'var(--text-secondary)',
          marginBottom: '8px'
        }}>
          <ChevronRight size={12} />
          <span>node_modules</span>
        </div>
      </div>

      <div style={{ marginBottom: '16px' }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '4px',
          fontSize: '12px',
          color: 'var(--text-secondary)',
          marginBottom: '8px'
        }}>
          <ChevronRight size={12} />
          <span>.git</span>
        </div>
      </div>

      <div style={{ 
        fontSize: '11px', 
        color: 'var(--text-secondary)',
        marginTop: '24px',
        padding: '8px',
        background: 'var(--bg-tertiary)',
        borderRadius: '4px'
      }}>
        <div>📁 Portfolio Structure</div>
        <div style={{ marginTop: '4px', opacity: 0.7 }}>
          Click on files to explore different sections of the portfolio
        </div>
      </div>
    </div>
  );
} 