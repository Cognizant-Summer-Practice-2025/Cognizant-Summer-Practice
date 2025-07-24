"use client";

import { useEffect } from "react";
import { Loading } from "../loader";

interface LoadingModalProps {
  isOpen: boolean;
  title?: string;
  message?: string;
  onClose?: () => void;
}

export function LoadingModal({ 
  isOpen, 
  title = "Publishing...", 
  message = "Please wait while we publish your portfolio",
  onClose 
}: LoadingModalProps) {
  // Prevent body scroll when modal is open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-transparent backdrop-blur-sm" 
        onClick={onClose}
      />
      
      {/* Modal */}
      <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6 animate-in fade-in duration-200">
        <div className="text-center">
          {/* Loading Animation */}
          <div className="mx-auto mb-6 flex justify-center">
            <Loading className="scale-50" backgroundColor="white" />
          </div>
          
          {/* Title */}
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            {title}
          </h3>
          
          {/* Message */}
          <p className="text-sm text-gray-600 mb-4">
            {message}
          </p>
          
          {/* Progress Dots */}
          <div className="flex justify-center space-x-1">
                          <div className="w-2 h-2 bg-app-blue rounded-full animate-pulse"></div>
              <div className="w-2 h-2 bg-app-blue rounded-full animate-pulse" style={{ animationDelay: '0.2s' }}></div>
              <div className="w-2 h-2 bg-app-blue rounded-full animate-pulse" style={{ animationDelay: '0.4s' }}></div>
          </div>
        </div>
      </div>
    </div>
  );
} 