"use client";

import { useState } from "react";
import { GripVertical } from "lucide-react";
import { ComponentConfig } from "@/lib/interfaces";

interface ComponentOrderingProps {
  components: ComponentConfig[];
  onComponentsChange: (components: ComponentConfig[]) => void;
  className?: string;
}

export function ComponentOrdering({ components, onComponentsChange, className = "" }: ComponentOrderingProps) {
  const moveComponent = (index: number, direction: 'up' | 'down') => {
    const newComponents = [...components];
    const targetIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (targetIndex < 0 || targetIndex >= newComponents.length) return;
    
    // Swap elements
    [newComponents[index], newComponents[targetIndex]] = [newComponents[targetIndex], newComponents[index]];
    
    // Update order values
    newComponents.forEach((component, idx) => {
      component.order = idx + 1;
    });
    
    onComponentsChange(newComponents);
  };

  const toggleComponentVisibility = (id: string) => {
    const updatedComponents = components.map(component => 
      component.id === id 
        ? { ...component, isVisible: !component.isVisible }
        : component
    );
    onComponentsChange(updatedComponents);
  };

  const getComponentDisplayName = (type: string) => {
    const names: Record<string, string> = {
      'about': 'About / Quotes',
      'experience': 'Work Experience',
      'projects': 'Projects',
      'skills': 'Skills',
      'blog_posts': 'Blog Posts',
      'contact': 'Contact Information'
    };
    return names[type] || type;
  };

  return (
    <div className={`border border-[#E2E8F0] rounded-lg p-4 sm:p-6 ${className}`}>
      <h3 className="text-base sm:text-lg font-semibold text-[#020817] leading-tight sm:leading-[28.8px] mb-4">
        Portfolio Sections
      </h3>
      <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px] mb-6">
        Customize the order and visibility of sections in your portfolio
      </p>
      
      <div className="space-y-3">
        {components.map((component, index) => (
          <div key={component.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-100">
            <div className="flex items-center gap-3">
              <GripVertical className="w-4 h-4 text-gray-400 cursor-grab" />
              <span className="text-sm font-medium text-[#020817]">
                {getComponentDisplayName(component.type)}
              </span>
            </div>
            
            <div className="flex items-center gap-2">
              {/* Move Up/Down Buttons */}
              <button
                onClick={() => moveComponent(index, 'up')}
                disabled={index === 0}
                className="p-1 text-gray-600 hover:text-gray-800 disabled:text-gray-300 disabled:cursor-not-allowed"
                title="Move up"
              >
                ↑
              </button>
              <button
                onClick={() => moveComponent(index, 'down')}
                disabled={index === components.length - 1}
                className="p-1 text-gray-600 hover:text-gray-800 disabled:text-gray-300 disabled:cursor-not-allowed"
                title="Move down"
              >
                ↓
              </button>
              
              {/* Visibility Toggle */}
              <button
                onClick={() => toggleComponentVisibility(component.id)}
                className={`w-11 h-6 rounded-full transition-colors ${
                  component.isVisible ? "bg-[#2563EB]" : "bg-gray-300"
                }`}
                title={component.isVisible ? "Hide section" : "Show section"}
              >
                <div
                  className={`w-[18px] h-[18px] bg-white rounded-full transition-transform ${
                    component.isVisible ? "translate-x-[23px]" : "translate-x-[3px]"
                  }`}
                />
              </button>
            </div>
          </div>
        ))}
      </div>
      
      <div className="mt-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
        <p className="text-sm text-blue-800">
          💡 <strong>Tip:</strong> Use the arrow buttons to reorder sections, and toggle switches to show/hide them on your portfolio.
        </p>
      </div>
    </div>
  );
} 