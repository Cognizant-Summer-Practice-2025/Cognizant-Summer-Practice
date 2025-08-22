'use client';

import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Brain, Sparkles, Loader2, Newspaper } from 'lucide-react';
import PortfolioCard from '@/components/home-page/portfolio-card';
import { generateBestPortfolios, convertAIPortfoliosToCards, getLatestTechNews } from '@/lib/ai/api';
import { PortfolioCardDto } from '@/lib/portfolio/api';
import '@/components/home-page/style.css'; // Import home page styles for consistent portfolio card styling

interface AIPageState {
  portfolios: PortfolioCardDto[];
  loading: boolean;
  error: string | null;
  hasGenerated: boolean;
  techNews?: { summary?: string } | null;
  techLoading?: boolean;
}

export default function AIPage() {
  const [state, setState] = useState<AIPageState>({
    portfolios: [],
    loading: false,
    error: null,
    hasGenerated: false,
    techNews: null,
    techLoading: false
  });

  const [visibleSections, setVisibleSections] = useState<Set<number>>(new Set());

  // Parse custom-tagged summary into sections and items
  const parsedSummary = React.useMemo(() => {
    const summary = state.techNews?.summary || '';
    if (!summary) return null as null | { sections: { name: string; items: { title?: string; text: string }[] }[] };

    const sections: { name: string; items: { title?: string; text: string }[] }[] = [];

    // Split by <category> tags and collect text blocks
    const catRegex = /<category>(.*?)<\/category>/gi;
    let lastIndex = 0;
    let match: RegExpExecArray | null;
    const parts: Array<{ name: string; start: number }> = [];
    while ((match = catRegex.exec(summary)) !== null) {
      parts.push({ name: match[1].trim(), start: match.index + match[0].length });
    }
    // Build ranges
    for (let i = 0; i < parts.length; i++) {
      const start = parts[i].start;
      const end = i + 1 < parts.length ? parts[i + 1].start - ("".length) - 0 : summary.length;
      const block = summary.slice(start, end).trim();
      const name = parts[i].name;

      const lines = block.split(/\r?\n/).filter(l => l.trim().length > 0);
      const items: { title?: string; text: string }[] = [];
      let idx = 0;
      while (idx < lines.length) {
        const line = lines[idx];
        if (line.trim().startsWith('-')) {
          // bullet item
          const bullet = line.replace(/^\s*-\s*/, '');
          // extract optional <title><bold>Title</bold></title>
          let title: string | undefined;
          let text = '';
          const titleMatch = bullet.match(/<title>\s*<bold>(.*?)<\/bold>\s*<\/title>/i);
          if (titleMatch) {
            title = titleMatch[1].trim();
            text = bullet.replace(titleMatch[0], '').trim();
          } else {
            // plain bullet
            text = bullet.trim();
          }
          // If next line(s) are indented description, append until next bullet or end
          let j = idx + 1;
          const descLines: string[] = [];
          while (j < lines.length && !lines[j].trim().startsWith('-') && !/<category>/i.test(lines[j])) {
            descLines.push(lines[j].trim());
            j++;
          }
          if (descLines.length > 0) {
            text = [text, descLines.join(' ')].filter(Boolean).join(' ').trim();
          }
          // Cleanup only title wrappers; keep <bold> markers for rendering
          text = text.replace(/<title>|<\/title>/gi, '');
          items.push({ title, text });
          idx = j;
        } else {
          idx++;
        }
      }
      sections.push({ name, items });
    }
    return { sections };
  }, [state.techNews?.summary]);

  // Animate sections when tech news changes
  useEffect(() => {
    if (state.techNews?.summary) {
      setVisibleSections(new Set());
      // Stagger the reveal of each section
      const timer = setTimeout(() => {
        setVisibleSections(new Set([0]));
      }, 100);
      
      return () => clearTimeout(timer);
    }
  }, [state.techNews?.summary]);

  // Reveal next section after delay
  useEffect(() => {
    if (visibleSections.size === 0) return;
    
    const maxSections = parsedSummary?.sections.length || 0;
    if (visibleSections.size < maxSections) {
      const timer = setTimeout(() => {
        setVisibleSections(prev => new Set([...prev, prev.size]));
      }, 300);
      
      return () => clearTimeout(timer);
    }
  }, [visibleSections, parsedSummary?.sections.length]);

  const handleGeneratePortfolios = async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      // Call AI service to generate best portfolios
      const aiPortfolios = await generateBestPortfolios();
      
      console.log('Raw AI Response:', aiPortfolios); // Debug log
      
      // Check if we have enough portfolios for AI generation
      if (!aiPortfolios || aiPortfolios.length === 0) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'Insufficient portfolio data. The AI requires at least 10 portfolios in the database to generate recommendations. Please add more portfolios and try again.'
        }));
        return;
      }
      
      // Convert to portfolio card format
      const portfolioCards = await convertAIPortfoliosToCards(aiPortfolios);
      
      // Additional check after conversion
      if (portfolioCards.length === 0) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'No valid portfolios could be processed. The AI requires at least 10 portfolios in the database to generate recommendations.'
        }));
        return;
      }
      
      setState({
        portfolios: portfolioCards,
        loading: false,
        error: null,
        hasGenerated: true
      });
    } catch (error) {
      console.error('Error generating portfolios:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to generate portfolios. Please try again.';
      
      // Check if the error is related to insufficient data
      if (errorMessage.toLowerCase().includes('insufficient') || errorMessage.toLowerCase().includes('not enough')) {
        setState(prev => ({
          ...prev,
          loading: false,
          error: 'Insufficient portfolio data. The AI requires at least 10 portfolios in the database to generate recommendations. Please add more portfolios and try again.'
        }));
      } else {
        setState(prev => ({
          ...prev,
          loading: false,
          error: errorMessage
        }));
      }
    }
  };

  const handleShowTechNews = async () => {
    setState(prev => ({ ...prev, techLoading: true }));
    try {
      const news = await getLatestTechNews();
      setState(prev => ({ ...prev, techNews: news, techLoading: false }));
    } catch (e) {
      console.error('Failed to load tech news', e);
      setState(prev => ({ ...prev, techNews: null, techLoading: false }));
    }
  };

  // Helper to render custom <bold> tags as strong
  const renderWithBold = (text: string) => {
    const html = text.replace(/<bold>(.*?)<\/bold>/gi, '<strong>$1</strong>');
    return <span dangerouslySetInnerHTML={{ __html: html }} />;
  };

  // Get emoji for each category
  const getCategoryEmoji = (categoryName: string): string => {
    const emojiMap: Record<string, string> = {
      'TOP STORIES': '🚨',
      'MARKET & BUSINESS MOVES': '📈',
      'TECHNOLOGY & INNOVATION': '🔬',
      'REGULATORY & LEGAL': '⚖️',
      'TREND ANALYSIS': '🔮',
      'KEY TAKEAWAYS': '💡'
    };
    return emojiMap[categoryName] || '📋';
  };

  // Raw mode: no markdown or table parsing

  return (
    <div className="min-h-screen bg-gray-50 pt-16">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Generate Button Section - Top Left */}
        <div className="p-6 bg-white border border-gray-300 rounded-lg shadow-sm flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-4">
          <Button
            onClick={handleGeneratePortfolios}
            disabled={state.loading}
            className="px-8 py-4 bg-blue-500 hover:bg-blue-600 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
            size="lg"
          >
            {state.loading ? (
              <>
                <Loader2 className="w-6 h-6 animate-spin" />
                Generating Portfolios...
              </>
            ) : (
              <>
                <Sparkles className="w-6 h-6" />
                Generate Portfolio
              </>
            )}
          </Button>

          <Button
            onClick={handleShowTechNews}
            disabled={state.techLoading}
            className="px-8 py-4 bg-emerald-600 hover:bg-emerald-700 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
            size="lg"
          >
            {state.techLoading ? (
              <>
                <Loader2 className="w-6 h-6 animate-spin" />
                Loading Tech News...
              </>
            ) : (
              <>
                <Newspaper className="w-6 h-6" />
                Tech News
              </>
            )}
          </Button>
        </div>

        {/* Error Message */}
        {state.error && (
          <div className="mb-8">
            <div className={`rounded-lg p-4 ${
              state.error.toLowerCase().includes('insufficient') 
                ? 'bg-amber-50 border border-amber-200' 
                : 'bg-red-50 border border-red-200'
            }`}>
              <div className="flex items-start gap-3">
                {state.error.toLowerCase().includes('insufficient') ? (
                  <Brain className="w-5 h-5 text-amber-600 mt-0.5 flex-shrink-0" />
                ) : (
                  <div className="w-5 h-5 bg-red-600 rounded-full flex items-center justify-center mt-0.5">
                    <span className="text-white text-xs font-bold">!</span>
                  </div>
                )}
                <div className="flex-1">
                  <p className={`font-medium mb-1 ${
                    state.error.toLowerCase().includes('insufficient')
                      ? 'text-amber-800'
                      : 'text-red-700'
                  }`}>
                    {state.error.toLowerCase().includes('insufficient') 
                      ? 'Not Enough Portfolio Data' 
                      : 'Error'
                    }
                  </p>
                  <p className={`text-sm ${
                    state.error.toLowerCase().includes('insufficient')
                      ? 'text-amber-700'
                      : 'text-red-700'
                  }`}>
                    {state.error}
                  </p>
                  {!state.error.toLowerCase().includes('insufficient') && (
                    <Button
                      onClick={handleGeneratePortfolios}
                      variant="outline"
                      className="mt-3 border-red-200 text-red-700 hover:bg-red-50"
                    >
                      Try Again
                    </Button>
                  )}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Loading State */}
        {state.loading && (
          <div className="py-12 text-center">
            <div className="flex items-center gap-3 text-gray-600 justify-center">
              <Loader2 className="w-6 h-6 animate-spin" />
              <span className="text-lg">AI is analyzing portfolios and selecting the best ones...</span>
            </div>
            <div className="mt-4 text-sm text-gray-500">
              This may take a few moments while we process the data
            </div>
          </div>
        )}

        {/* Generated Portfolios Grid */}
        {state.portfolios.length > 0 && !state.loading && (
          <div className="bg-white rounded-lg shadow-sm overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200">
              <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                <Sparkles className="w-5 h-5 text-blue-600" />
                AI-Generated Top Portfolios
              </h2>
              <p className="text-gray-600 mt-1">
                {state.portfolios.length} portfolios selected based on content quality, skills, and engagement
              </p>
            </div>
            
            <div className="p-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {state.portfolios.map((portfolio) => (
                  <PortfolioCard
                    key={portfolio.id}
                    id={portfolio.id}
                    name={portfolio.name}
                    role={portfolio.role}
                    location={portfolio.location}
                    description={portfolio.description}
                    skills={portfolio.skills}
                    views={portfolio.views}
                    likes={portfolio.likes}
                    comments={portfolio.comments}
                    bookmarks={portfolio.bookmarks}
                    date={portfolio.date}
                    avatar={portfolio.avatar}
                    featured={portfolio.featured}
                  />
                ))}
              </div>
            </div>
          </div>
        )}

        {state.techNews?.summary && (
          <div className="bg-white rounded-lg shadow-sm overflow-hidden mt-8">
            <div className="px-6 py-4 border-b border-gray-200 flex items-center gap-2">
              <Newspaper className="w-5 h-5 text-emerald-600" />
              <h2 className="text-xl font-semibold text-gray-900">Tech News Summary</h2>
              {/* no timestamp */}
            </div>
            {!parsedSummary && (
              <div className="p-6">
                <pre className="whitespace-pre-wrap leading-7 text-sm text-gray-900">{state.techNews.summary}</pre>
              </div>
            )}
            {parsedSummary && (
              <div className="p-6 space-y-10">
                {parsedSummary.sections.map((section, si) => (
                  <div 
                    key={si} 
                    className={`space-y-4 transition-all duration-700 ease-out transform ${
                      visibleSections.has(si) 
                        ? 'opacity-100 translate-y-0' 
                        : 'opacity-0 translate-y-8'
                    }`}
                  >
                    <h3 className="text-lg font-semibold tracking-wide text-gray-900">
                      {getCategoryEmoji(section.name)} {section.name}
                    </h3>
                    <ul className="space-y-4">
                      {section.items.map((item, ii) => (
                        <li key={ii} className="bg-gray-50 border border-gray-200 rounded-lg p-4">
                          {item.title && (
                            <div className="font-semibold text-gray-900 mb-1">{renderWithBold(item.title)}</div>
                          )}
                          <div className="text-gray-700 leading-7 text-sm">{renderWithBold(item.text)}</div>
                        </li>
                      ))}
                    </ul>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}



        {/* Empty State - Show only if user hasn't generated yet */}
        {!state.hasGenerated && !state.loading && !state.error && (
          <div className="py-12 text-center">
            <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mb-4 flex items-center justify-center mx-auto">
              <Brain className="w-8 h-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Ready to Generate</h3>
            <p className="text-gray-600 max-w-md mx-auto">
              Click the generate button above to let our AI curate the best portfolios for you.
            </p>
          </div>
        )}
      </div>
    </div>
  );
} 