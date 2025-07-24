"use client";

import { useEffect } from "react";
import { Loading } from "./loading";

interface LoadingOverlayProps {
  isOpen: boolean;
  title?: string;
  message?: string;
  showBackdrop?: boolean;
  preventBodyScroll?: boolean;
  onClose?: () => void;
  className?: string;
}

export function LoadingOverlay({ 
  isOpen, 
  title,
  message,
  showBackdrop = true,
  preventBodyScroll = true,
  onClose,
  className = ""
}: LoadingOverlayProps) {
  // Prevent body scroll when overlay is open
  useEffect(() => {
    if (isOpen && preventBodyScroll) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, preventBodyScroll]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      {showBackdrop && (
        <div 
          className="absolute inset-0 bg-transparent backdrop-blur-sm" 
          onClick={onClose}
        />
      )}
      
      {/* Loading Content */}
      <div className={`relative flex flex-col items-center justify-center ${className}`}>
        {/* 3D Loading Animation */}
        <Loading backgroundColor="white" />
        
        {/* Optional Text Content */}
        {(title || message) && (
          <div className="mt-8 text-center max-w-md">
            {title && (
              <h3 className="text-lg font-semibold text-white mb-2 drop-shadow-lg">
                {title}
              </h3>
            )}
            
            {message && (
              <p className="text-sm text-white/80 drop-shadow-lg">
                {message}
              </p>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default LoadingOverlay;
