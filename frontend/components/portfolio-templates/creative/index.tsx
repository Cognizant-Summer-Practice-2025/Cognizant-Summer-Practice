"use client";

import React from 'react';
import { PortfolioData } from '@/lib/portfolio';

interface CreativeTemplateProps {
  data?: PortfolioData;
}

export default function CreativeTemplate({ data }: CreativeTemplateProps) {
  return (
    <div style={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      background: 'linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1, #96ceb4)',
      backgroundSize: '400% 400%',
      animation: 'gradient 15s ease infinite',
      color: 'white',
      fontFamily: 'Inter, sans-serif'
    }}>
      <style jsx>{`
        @keyframes gradient {
          0% { background-position: 0% 50%; }
          50% { background-position: 100% 50%; }
          100% { background-position: 0% 50%; }
        }
      `}</style>
      <div style={{ textAlign: 'center', padding: '2rem' }}>
        <h1 style={{ fontSize: '3rem', marginBottom: '1rem' }}>Creative Template</h1>
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