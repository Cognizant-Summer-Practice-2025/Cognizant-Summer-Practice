import React from 'react';
import './style.css';

interface QuoteCardProps {
  quote: string;
  author: string;
}

const QuoteCard: React.FC<QuoteCardProps> = ({ quote, author }) => {
  return (
    <div className="quote-card">
      <div className="quote-text-container">
        <div className="quote-text">&ldquo;{quote}&rdquo;</div>
      </div>
      <div className="quote-author">- {author}</div>
    </div>
  );
};

const QuotesSection = () => {
  const quotes = [
    {
      quote: "The only way to do great work is to love what you do.",
      author: "Steve Jobs, co-founder of Apple"
    },
    {
      quote: "Code is like humor. When you have to explain it, it's bad.",
      author: "Cory House, software developer"
    }
  ];

  return (
    <div className="quotes-section">
      {quotes.map((quote, index) => (
        <QuoteCard 
          key={index} 
          quote={quote.quote} 
          author={quote.author} 
        />
      ))}
    </div>
  );
};

export default QuotesSection;