import React from 'react';
import { Quote } from '@/lib/portfolio';

interface AboutProps {
  data: Quote[];
}

export function About({ data: quotes }: AboutProps) {
  if (!quotes || quotes.length === 0) {
    return null;
  }

  return (
    <section className="gb-about">
      <h3 className="section-title">Quotes</h3>
      <div className="quotes-container">
        {quotes.map((quote) => (
          <blockquote key={quote.id} className="quote-item">
            <div className="quote-text">"{quote.text}"</div>
            {quote.author && (
              <cite className="quote-author">
                — {quote.author}
                {quote.position && <span className="quote-position">, {quote.position}</span>}
              </cite>
            )}
          </blockquote>
        ))}
      </div>
    </section>
  );
} 