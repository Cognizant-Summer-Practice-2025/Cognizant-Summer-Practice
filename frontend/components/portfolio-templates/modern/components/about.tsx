import React from 'react';
import { Quote } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Quote as QuoteIcon } from 'lucide-react';

interface AboutProps {
  data: Quote[];
}

export function About({ data: quotes }: AboutProps) {
  if (!quotes || quotes.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No quotes or testimonials available.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {quotes.map((quote) => (
        <Card key={quote.id} className="modern-card">
          <CardContent className="p-6">
            <div className="relative">
              <QuoteIcon 
                size={24} 
                className="absolute -top-2 -left-2 text-primary/20 transform rotate-180" 
              />
              
              <blockquote className="text-lg leading-relaxed text-foreground italic pl-6">
                "{quote.text}"
              </blockquote>
              
              {(quote.author || quote.position) && (
                <footer className="mt-4 text-right">
                  {quote.author && (
                    <cite className="text-sm font-medium text-foreground not-italic">
                      — {quote.author}
                    </cite>
                  )}
                  {quote.position && (
                    <div className="text-xs text-muted-foreground mt-1">
                      {quote.position}
                    </div>
                  )}
                </footer>
              )}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
} 