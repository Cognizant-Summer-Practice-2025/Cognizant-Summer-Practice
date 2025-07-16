"use client";

import React from 'react';
import { PortfolioData } from '@/lib/interfaces';

interface ModernTemplateProps {
  data?: PortfolioData;
}

export default function ModernTemplate({ data }: ModernTemplateProps) {
  return (
    <div style={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      color: 'white',
      fontFamily: 'Inter, sans-serif'
    }}>
      <div style={{ textAlign: 'center', padding: '2rem' }}>
        <h1 style={{ fontSize: '3rem', marginBottom: '1rem' }}>Modern Template</h1>
        <p style={{ fontSize: '1.2rem', opacity: 0.9 }}>
          This template is under development. Coming soon!
        </p>
        <p style={{ fontSize: '1rem', marginTop: '1rem', opacity: 0.7 }}>
          For now, please use the Gabriel Bârzu template.
        </p>
      </div>
    </div>
  );
} 