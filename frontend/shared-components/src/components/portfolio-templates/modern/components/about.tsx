import React from 'react';
import { Quote } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Quote as QuoteIcon } from 'lucide-react';

interface AboutProps {
  data: unknown;
}

export function About({ data }: AboutProps) {
  // Handle different data structures
  let quotes: Quote[] = [];
  
  if (Array.isArray(data)) {
    quotes = data;
  } else if (data && typeof data === 'object' && Array.isArray((data as { quotes: Quote[] }).quotes)) {
    quotes = (data as { quotes: Quote[] }).quotes;
  } else {
    // Default quotes if no valid data is provided
    quotes = [
      {
        id: "1",
        text: "This portfolio showcases exceptional talent and dedication.",
        author: "Default Testimonial"
      }
    ];
  }

  if (!quotes || quotes.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No quotes or testimonials available.
      </div>
    );
  }

  return (
    <div className="modern-component-container">
      <div className="max-h-[800px] overflow-y-auto pr-2">
        <div className="modern-grid">
          {quotes.map((quote) => (
            <Card key={quote.id} className="modern-card">
              <CardContent className="p-6">
                <div className="relative">
                  <QuoteIcon 
                    size={24} 
                    className="absolute -top-2 -left-2 text-primary/20 transform rotate-180" 
                  />
                  
                  <blockquote className="text-lg leading-relaxed text-foreground italic pl-6">
                    &ldquo;{quote.text}&rdquo;
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
      </div>
    </div>
  );
} 