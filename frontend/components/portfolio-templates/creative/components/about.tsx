import React from 'react';
import { Quote, User, Heart } from 'lucide-react';

interface Quote {
  id: number;
  text: string;
  author?: string;
  context?: string;
}

interface AboutProps {
  data: Quote[];
}

export function About({ data }: AboutProps) {
  const quotes = data || [
    {
      id: 1,
      text: "Code is poetry in motion, and every line tells a story.",
      author: "Anonymous Developer",
      context: "On the art of programming"
    }
  ];

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
          <span className="syntax-string">"Creating digital experiences"</span>;
        </div>
        <div className="code-line" style={{ marginLeft: '40px' }}>
          <span className="syntax-keyword">this</span>.<span className="syntax-highlight">philosophy</span> = 
          <span className="syntax-string">"Always learning, always growing"</span>;
        </div>
        <div className="code-line" style={{ marginLeft: '40px' }}>
          <span className="syntax-keyword">this</span>.<span className="syntax-highlight">goal</span> = 
          <span className="syntax-string">"Building the future, one line at a time"</span>;
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
                <span className="syntax-comment">/*</span>
              </div>
              <div className="code-line" style={{ marginLeft: '4px' }}>
                <span className="syntax-comment">* "{quote.text}"</span>
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
                <span className="syntax-comment">*/</span>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div style={{ marginTop: '24px' }}>
        <div className="code-block">
          <div className="code-line">
            <span className="syntax-comment">// Fun fact generator</span>
          </div>
          <div className="code-line">
            <span className="syntax-keyword">const</span> <span className="syntax-highlight">funFacts</span> = [
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">"I debug with console.log and I'm not ashamed"</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">"My code compiles on the first try... sometimes"</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">"I speak fluent JavaScript and broken English"</span>,
          </div>
          <div className="code-line" style={{ marginLeft: '20px' }}>
            <span className="syntax-string">"Stack Overflow is my second home"</span>
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
          Let's create something amazing together!
        </div>
        <div style={{ fontSize: '14px', opacity: 0.9 }}>
          Always open to new opportunities and exciting projects
        </div>
      </div>
    </div>
  );
} 