'use client';

import React, { useState, useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import { Input } from '@/components/ui/input';
import { MapPin, Loader2 } from 'lucide-react';

interface LocationSuggestion {
  display_name: string;
  name: string;
  country: string;
  type: string;
}

interface PlacesAutocompleteProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
  disabled?: boolean;
}

export default function PlacesAutocomplete({
  value,
  onChange,
  placeholder = "Enter city...",
  className,
  disabled
}: PlacesAutocompleteProps) {
  const [suggestions, setSuggestions] = useState<LocationSuggestion[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const [dropdownPosition, setDropdownPosition] = useState({ top: 0, left: 0, width: 0 });
  const inputRef = useRef<HTMLInputElement>(null);
  const suggestionsRef = useRef<HTMLDivElement>(null);

  // Debounced search function
  useEffect(() => {
    if (!value.trim() || value.length < 2) {
      setSuggestions([]);
      setShowSuggestions(false);
      return;
    }

    const timeoutId = setTimeout(async () => {
      await searchLocations(value);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [value]);

  // Update dropdown position on scroll/resize
  useEffect(() => {
    const handleScroll = () => {
      if (showSuggestions) {
        updateDropdownPosition();
      }
    };

    const handleResize = () => {
      if (showSuggestions) {
        updateDropdownPosition();
      }
    };

    window.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleResize);
    };
  }, [showSuggestions]);

  const searchLocations = async (query: string) => {
    setIsLoading(true);
    try {
      // Using Nominatim API (OpenStreetMap) - completely free
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?` +
        `q=${encodeURIComponent(query)}&` +
        `format=json&` +
        `addressdetails=1&` +
        `limit=5&` +
        `featuretype=city,country&` +
        `extratags=1`
      );

      if (!response.ok) throw new Error('Search failed');

      const data = await response.json();
      
      // Filter and format results to show cities and countries
      const filteredResults: LocationSuggestion[] = data
        .filter((item: any) => {
          const type = item.type || item.class;
          return (
            type === 'city' || 
            type === 'town' || 
            type === 'village' || 
            type === 'country' ||
            item.addresstype === 'city' ||
            item.addresstype === 'country'
          );
        })
        .map((item: any) => ({
          display_name: item.display_name,
          name: item.name || item.display_name.split(',')[0],
          country: item.address?.country || item.display_name.split(',').pop()?.trim() || '',
          type: item.type || item.class || 'location'
        }))
        .slice(0, 5);

      setSuggestions(filteredResults);
      if (filteredResults.length > 0) {
        updateDropdownPosition();
        setShowSuggestions(true);
      } else {
        setShowSuggestions(false);
      }
      setSelectedIndex(-1);
    } catch (error) {
      console.error('Location search error:', error);
      setSuggestions([]);
      setShowSuggestions(false);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSuggestionClick = (suggestion: LocationSuggestion) => {
    // Format the selection nicely
    const formattedLocation = suggestion.type === 'country' 
      ? suggestion.name 
      : `${suggestion.name}, ${suggestion.country}`;
    
    onChange(formattedLocation);
    setShowSuggestions(false);
    setSuggestions([]);
    setSelectedIndex(-1);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!showSuggestions || suggestions.length === 0) return;

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex(prev => 
          prev < suggestions.length - 1 ? prev + 1 : 0
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex(prev => 
          prev > 0 ? prev - 1 : suggestions.length - 1
        );
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < suggestions.length) {
          handleSuggestionClick(suggestions[selectedIndex]);
        }
        break;
      case 'Escape':
        setShowSuggestions(false);
        setSelectedIndex(-1);
        break;
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value);
  };

  const handleInputBlur = () => {
    // Delay hiding suggestions to allow for clicks
    setTimeout(() => {
      setShowSuggestions(false);
      setSelectedIndex(-1);
    }, 200);
  };

  const updateDropdownPosition = () => {
    if (inputRef.current) {
      const rect = inputRef.current.getBoundingClientRect();
      setDropdownPosition({
        top: rect.bottom + window.scrollY,
        left: rect.left + window.scrollX,
        width: rect.width
      });
    }
  };

  const handleInputFocus = () => {
    updateDropdownPosition();
    if (suggestions.length > 0) {
      setShowSuggestions(true);
    }
  };

  return (
    <div className="relative">
      <div className="relative">
        <Input
          ref={inputRef}
          type="text"
          value={value}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onBlur={handleInputBlur}
          onFocus={handleInputFocus}
          placeholder={placeholder}
          className={`${className} pr-8`}
          disabled={disabled}
          autoComplete="off"
        />
        <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
          {isLoading ? (
            <Loader2 className="h-4 w-4 animate-spin text-gray-400" />
          ) : (
            <MapPin className="h-4 w-4 text-gray-400" />
          )}
        </div>
      </div>

      {showSuggestions && suggestions.length > 0 && typeof window !== 'undefined' && 
        createPortal(
          <div
            ref={suggestionsRef}
            className="fixed z-[9999] bg-white border border-gray-200 rounded-md shadow-lg max-h-60 overflow-y-auto"
            style={{
              top: `${dropdownPosition.top}px`,
              left: `${dropdownPosition.left}px`,
              width: `${dropdownPosition.width}px`
            }}
          >
            {suggestions.map((suggestion, index) => {
              // Format display text as "City, Country"
              const displayText = suggestion.type === 'country' 
                ? suggestion.name 
                : `${suggestion.name}, ${suggestion.country}`;
                
              return (
                <div
                  key={index}
                  className={`px-4 py-2 cursor-pointer hover:bg-gray-50 border-b border-gray-100 last:border-b-0 ${
                    index === selectedIndex ? 'bg-blue-50 text-blue-700' : 'text-gray-700'
                  }`}
                  onClick={() => handleSuggestionClick(suggestion)}
                >
                  <div className="flex items-center space-x-2">
                    <MapPin className="h-3 w-3 text-gray-400 flex-shrink-0" />
                    <div className="flex-1 min-w-0">
                      <div className="font-medium truncate">
                        {displayText}
                      </div>
                    </div>
                    <div className="text-xs text-gray-400 capitalize">
                      {suggestion.type === 'country' ? 'Country' : 'City'}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>,
          document.body
        )
      }
    </div>
  );
} 