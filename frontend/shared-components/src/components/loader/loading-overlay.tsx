"use client";

import { useEffect, useState } from "react";
import { Loading } from "./loading";

interface LoadingOverlayProps {
  isOpen: boolean;
  title?: string;
  message?: string;
  showBackdrop?: boolean;
  preventBodyScroll?: boolean;
  onClose?: () => void;
  className?: string;
  textColor?: string;
}

export function LoadingOverlay({ 
  isOpen, 
  title,
  message,
  showBackdrop = true,
  preventBodyScroll = true,
  onClose,
  className = "",
  textColor = "white"
}: LoadingOverlayProps) {
  const [isVisible, setIsVisible] = useState(false);
  const [isAnimatingOut, setIsAnimatingOut] = useState(false);

  // Handle opening/closing animation
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

  // Prevent body scroll when overlay is open
  useEffect(() => {
    if (isVisible && preventBodyScroll) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isVisible, preventBodyScroll]);

  const handleBackdropClick = () => {
    if (onClose && !isAnimatingOut) {
      onClose();
    }
  };

  if (!isVisible) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      {showBackdrop && (
        <div 
          className={`absolute inset-0 bg-transparent backdrop-blur-sm transition-opacity duration-300 ${
            isAnimatingOut ? 'opacity-0' : 'opacity-100'
          }`}
          onClick={handleBackdropClick}
        />
      )}
      
      {/* Loading Content */}
      <div className={`relative flex flex-col items-center justify-center transition-all duration-300 ${
        isAnimatingOut 
          ? 'animate-out fade-out zoom-out-75 slide-out-to-top-2' 
          : 'animate-in fade-in zoom-in-95 slide-in-from-top-2'
      } ${className}`}>
        {/* 3D Loading Animation */}
        <Loading backgroundColor="white" />
        
        {/* Optional Text Content */}
        {(title || message) && (
          <div className="mt-8 text-center max-w-md animate-in fade-in-50 slide-in-from-bottom-2 duration-500">
            {title && (
              <h3 className="text-lg font-semibold mb-2 drop-shadow-lg" style={{ color: textColor }}>
                {title}
              </h3>
            )}
            
            {message && (
              <p className="text-sm drop-shadow-lg" style={{ color: textColor, opacity: 0.8 }}>
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
