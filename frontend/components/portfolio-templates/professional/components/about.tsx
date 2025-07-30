"use client";

import React from 'react';
import { Quote } from '@/lib/portfolio';
import { Card } from '@/components/ui/card';
import { Quote as QuoteIcon, User } from 'lucide-react';

interface AboutProps {
  data: Quote[] | { quotes?: Quote[] } | unknown;
}

export function About({ data }: AboutProps) {
  // Ensure data is an array and handle various data structures
  let quotes: Quote[] = [];
  
  if (Array.isArray(data)) {
    quotes = data;
  } else if (data && typeof data === 'object' && Array.isArray((data as { quotes?: Quote[] }).quotes)) {
    quotes = (data as { quotes?: Quote[] }).quotes || [];
  }
  
  if (!quotes || quotes.length === 0) {
    return (
      <div className="prof-about">
        <Card className="prof-about-card">
          <div className="prof-about-content">
            <div className="prof-about-icon">
              <User size={24} />
            </div>
            <h3 className="prof-about-title">About Me</h3>
            <p className="prof-about-text">
              Welcome to my professional portfolio. I'm passionate about delivering exceptional results and creating value through innovative solutions.
            </p>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="prof-about">
      <div className="prof-about-grid">
        {quotes.map((quote, index) => (
          <Card key={quote.id || index} className="prof-about-card">
            <div className="prof-about-content">
              <div className="prof-quote-icon">
                <QuoteIcon size={20} />
              </div>
              <blockquote className="prof-quote-text">
                "{quote.text}"
              </blockquote>
              <div className="prof-quote-meta">
                <cite className="prof-quote-author">{quote.author}</cite>
                {quote.context && (
                  <span className="prof-quote-context">{quote.context}</span>
                )}
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
} 