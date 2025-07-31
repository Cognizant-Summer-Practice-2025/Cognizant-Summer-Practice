import React from 'react';
import { Quote, User, Heart } from 'lucide-react';

interface Quote {
  id: number;
  text: string;
  author?: string;
  context?: string;
}

interface AboutProps {
  data: Quote[] | { quotes?: Quote[] } | unknown;
}

export function About({ data }: AboutProps) {
  // Ensure data is an array and handle various data structures
  let quotes: Quote[] = [];
  
  if (Array.isArray(data)) {
    quotes = data;
  } else if (data && typeof data === 'object' && Array.isArray((data as { quotes: Quote[] }).quotes)) {
    quotes = (data as { quotes: Quote[] }).quotes;
  } else {
    // Default quotes if no valid data is provided
    quotes = [
      {
        id: 1,
        text: "Code is poetry in motion, and every line tells a story.",
        author: "Anonymous Developer",
        context: "On the art of programming"
      }
    ];
  }

  return (
    <div className="component-card">
      <div className="component-title">
        <User size={20} />
        About Me
      </div>

      <div className="code-block">
        <div className="code-line">
          <span className="syntax-keyword">class</span>{' '}
          <span className="syntax-highlight">About</span> {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>
          <span className="syntax-keyword">constructor</span>() {'{'}
        </div>
        <div className="code-line" style={{ marginLeft: '40px' }}>
          <span className="syntax-keyword">this</span>.<span className="syntax-highlight">passion</span> = 
          <span className="syntax-string">&quot;Creating digital experiences&quot;</span>;
        </div>
        <div className="code-line" style={{ marginLeft: '40px' }}>
          <span className="syntax-keyword">this</span>.<span className="syntax-highlight">philosophy</span> = 
          <span className="syntax-string">&quot;Always learning, always growing&quot;</span>;
        </div>
        <div className="code-line" style={{ marginLeft: '40px' }}>
          <span className="syntax-keyword">this</span>.<span className="syntax-highlight">goal</span> = 
          <span className="syntax-string">&quot;Building the future, one line at a time&quot;</span>;
        </div>
        <div className="code-line" style={{ marginLeft: '20px' }}>{'}'}</div>
        <div className="code-line">{'}'}</div>
      </div>

      <div style={{ marginTop: '24px' }}>
        <h4 style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '8px', 
          marginBottom: '16px',
          color: 'var(--text-primary)'
        }}>
          <Quote size={16} />
          Inspiration & Quotes
        </h4>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {quotes.map((quote) => (
            <div key={quote.id} className="code-block" style={{ 
              background: 'var(--bg-tertiary)',
              borderLeft: '4px solid var(--bg-accent)',
              position: 'relative'
            }}>
              <div className="code-line">
                <span className="syntax-comment">{`/*`}</span>
              </div>
              <div className="code-line" style={{ marginLeft: '4px' }}>
                <span className="syntax-comment">{`* \"${quote.text}\"`}</span>
              </div>
              {quote.author && (
                <div className="code-line" style={{ marginLeft: '4px' }}>
                  <span className="syntax-comment">* - {quote.author}</span>
                </div>
              )}
              {quote.context && (
                <div className="code-line" style={{ marginLeft: '4px' }}>
                  <span className="syntax-comment">* Context: {quote.context}</span>
                </div>
              )}
              <div className="code-line">
                <span className="syntax-comment">{`*/`}</span>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div style={{ marginTop: '24px' }}>
        <div className="code-block">
          <div className="code-line">
            <span className="syntax-comment">{/* Fun fact generator */}</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">const</span> <span className="syntax-highlight">funFacts</span> = [
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">&quot;I debug with console.log and I&apos;m not ashamed&quot;</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">&quot;My code compiles on the first try... sometimes&quot;</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">&quot;I speak fluent JavaScript and broken English&quot;</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">&quot;Stack Overflow is my second home&quot;</span>
          </div>
          <div className="code-line">];</div>
        </div>
      </div>

      <div style={{ 
        marginTop: '24px', 
        padding: '16px',
        background: 'linear-gradient(135deg, var(--bg-accent) 0%, #6366f1 100%)',
        borderRadius: '8px',
        color: 'white',
        textAlign: 'center'
      }}>
        <Heart size={20} style={{ marginBottom: '8px' }} />
        <div style={{ fontWeight: '600', marginBottom: '4px' }}>
          Let&apos;s create something amazing together!
        </div>
        <div style={{ fontSize: '14px', opacity: 0.9 }}>
          Always open to new opportunities and exciting projects
        </div>
      </div>
    </div>
  );
} 