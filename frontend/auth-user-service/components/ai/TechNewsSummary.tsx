import React, { useState, useEffect } from 'react';

interface TechNewsSummaryProps {
  summary: string;
}

export function TechNewsSummary({ summary }: TechNewsSummaryProps) {
  const [visibleSections, setVisibleSections] = useState<Set<number>>(new Set());

  // Parse custom-tagged summary into sections and items
  const parsedSummary = React.useMemo(() => {
    if (!summary) return null as null | { sections: { name: string; items: { title?: string; text: string }[] }[] };

    const sections: { name: string; items: { title?: string; text: string }[] }[] = [];

    // Split by <category> tags and collect text blocks
    const catRegex = /<category>(.*?)<\/category>/gi;
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
  }, [summary]);

  // Animate sections when summary changes
  useEffect(() => {
    if (summary) {
      setVisibleSections(new Set());
      // Stagger the reveal of each section
      const timer = setTimeout(() => {
        setVisibleSections(new Set([0]));
      }, 100);
      
      return () => clearTimeout(timer);
    }
  }, [summary]);

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

  if (!parsedSummary) {
    return (
      <div className="p-6">
        <pre className="whitespace-pre-wrap leading-7 text-sm text-gray-900">{summary}</pre>
      </div>
    );
  }

  return (
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
  );
}

