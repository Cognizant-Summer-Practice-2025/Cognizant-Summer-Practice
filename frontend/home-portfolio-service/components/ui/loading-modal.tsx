"use client";

import { useEffect, useState } from "react";
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
  const [isVisible, setIsVisible] = useState(false);
  const [isAnimatingOut, setIsAnimatingOut] = useState(false);

  // Handle opening animation
  useEffect(() => {
    if (isOpen) {
      setIsVisible(true);
      setIsAnimatingOut(false);
    } else if (isVisible) {
      // Start closing animation
      setIsAnimatingOut(true);
      const timer = setTimeout(() => {
        setIsVisible(false);
        setIsAnimatingOut(false);
      }, 300); // Match animation duration
      return () => clearTimeout(timer);
    }
  }, [isOpen, isVisible]);

  // Prevent body scroll when modal is open
  useEffect(() => {
    if (isVisible) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isVisible]);

  const handleBackdropClick = () => {
    if (onClose && !isAnimatingOut) {
      onClose();
    }
  };

  if (!isVisible) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className={`absolute inset-0 bg-transparent backdrop-blur-sm transition-opacity duration-300 ${
          isAnimatingOut ? 'opacity-0' : 'opacity-100'
        }`}
        onClick={handleBackdropClick}
      />
      
      {/* Modal */}
      <div className={`relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4 p-6 transition-all duration-300 ${
        isAnimatingOut 
          ? 'animate-out fade-out zoom-out-75 slide-out-to-top-2' 
          : 'animate-in fade-in zoom-in-95 slide-in-from-top-2'
      }`}>
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