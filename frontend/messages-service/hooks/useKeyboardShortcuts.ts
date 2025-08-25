import { useEffect } from 'react';
import { MobileView } from './useMobileViewManager';

interface UseKeyboardShortcutsProps {
  isMobile: boolean;
  mobileView: MobileView;
  onBackToSidebar: () => void;
}

export const useKeyboardShortcuts = ({
  isMobile,
  mobileView,
  onBackToSidebar,
}: UseKeyboardShortcutsProps) => {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isMobile && mobileView === 'chat') {
        onBackToSidebar();
        return;
      }
      
      if (event.ctrlKey || event.metaKey) {
        switch (event.key) {
          case 'f': 
            if (mobileView === 'chat' || !isMobile) {
              event.preventDefault();
              const messageInput = document.querySelector('.message-input') as HTMLInputElement;
              messageInput?.focus();
            }
            break;
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isMobile, mobileView, onBackToSidebar]);
}; 