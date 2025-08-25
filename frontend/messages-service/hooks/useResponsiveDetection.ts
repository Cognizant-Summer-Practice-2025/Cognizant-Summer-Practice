import { useState, useEffect } from 'react';

export interface ResponsiveState {
  isMobile: boolean;
  isSmallMobile: boolean;
  isLargeMobile: boolean;
  isSmallTablet: boolean;
  isMediumTablet: boolean;
  isLargeTablet: boolean;
  isDesktop: boolean;
  isLandscape: boolean;
  isMobileLandscape: boolean;
}

export const useResponsiveDetection = () => {
  const [responsiveState, setResponsiveState] = useState<ResponsiveState>({
    isMobile: false,
    isSmallMobile: false,
    isLargeMobile: false,
    isSmallTablet: false,
    isMediumTablet: false,
    isLargeTablet: false,
    isDesktop: false,
    isLandscape: false,
    isMobileLandscape: false,
  });

  useEffect(() => {
    const checkResponsiveState = () => {
      const width = window.innerWidth;
      const height = window.innerHeight;
      
      // Enhanced mobile detection with device classification
      const isMobile = width <= 745;
      const isSmallMobile = width <= 480;
      const isLargeMobile = width > 480 && width <= 576;
      const isSmallTablet = width > 576 && width <= 745;
      const isMediumTablet = width > 745 && width <= 768;
      const isLargeTablet = width > 768 && width <= 1024;
      const isDesktop = width > 1024;
      
      // Check for landscape orientation on mobile devices
      const isLandscape = width > height;
      const isMobileLandscape = isMobile && isLandscape;
      
      const newState: ResponsiveState = {
        isMobile,
        isSmallMobile,
        isLargeMobile,
        isSmallTablet,
        isMediumTablet,
        isLargeTablet,
        isDesktop,
        isLandscape,
        isMobileLandscape,
      };

      setResponsiveState(newState);
      
      // Store device info for potential use in components
      if (typeof window !== 'undefined') {
        document.documentElement.setAttribute('data-device-type', 
          isSmallMobile ? 'small-mobile' :
          isLargeMobile ? 'large-mobile' :
          isSmallTablet ? 'small-tablet' :
          isMediumTablet ? 'medium-tablet' :
          isLargeTablet ? 'large-tablet' :
          'desktop'
        );
        
        document.documentElement.setAttribute('data-orientation', 
          isLandscape ? 'landscape' : 'portrait'
        );
        
        document.documentElement.setAttribute('data-mobile-landscape', 
          isMobileLandscape ? 'true' : 'false'
        );
      }
    };

    checkResponsiveState();
    
    // Use ResizeObserver for better performance if available
    if (typeof ResizeObserver !== 'undefined') {
      const resizeObserver = new ResizeObserver(checkResponsiveState);
      resizeObserver.observe(document.documentElement);
      
      return () => {
        resizeObserver.disconnect();
      };
    } else {
      // Fallback to resize event with throttling
      let timeoutId: NodeJS.Timeout;
      const throttledCheck = () => {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(checkResponsiveState, 100);
      };
      
      window.addEventListener('resize', throttledCheck);
      window.addEventListener('orientationchange', checkResponsiveState);
      
      return () => {
        window.removeEventListener('resize', throttledCheck);
        window.removeEventListener('orientationchange', checkResponsiveState);
        clearTimeout(timeoutId);
      };
    }
  }, []);

  return responsiveState;
}; 