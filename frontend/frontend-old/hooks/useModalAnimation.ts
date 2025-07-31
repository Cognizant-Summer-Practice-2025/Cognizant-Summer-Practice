"use client";

import { useState, useEffect } from "react";

interface UseModalAnimationProps {
  isOpen: boolean;
  onClose: () => void;
  duration?: number;
}

export function useModalAnimation({ isOpen, onClose, duration = 300 }: UseModalAnimationProps) {
  const [isVisible, setIsVisible] = useState(false);
  const [isAnimatingOut, setIsAnimatingOut] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setIsVisible(true);
      setIsAnimatingOut(false);
    } else if (isVisible) {
      setIsAnimatingOut(true);
      const timer = setTimeout(() => {
        setIsVisible(false);
        setIsAnimatingOut(false);
      }, duration);
      return () => clearTimeout(timer);
    }
  }, [isOpen, isVisible, duration]);

  const handleClose = () => {
    if (!isAnimatingOut) {
      onClose();
    }
  };

  return {
    isVisible,
    isAnimatingOut,
    handleClose,
  };
}

export default useModalAnimation;
