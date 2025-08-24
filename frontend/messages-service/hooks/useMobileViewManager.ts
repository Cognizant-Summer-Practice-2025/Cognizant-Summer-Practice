import { useState, useEffect } from 'react';
import { Contact } from '@/lib/utils/contact-utils';

export type MobileView = 'sidebar' | 'chat';

export const useMobileViewManager = (isMobile: boolean, selectedContact: Contact | null) => {
  const [mobileView, setMobileView] = useState<MobileView>('sidebar');

  useEffect(() => {
    if (!isMobile) {
      setMobileView('sidebar');
    } else {
      setMobileView(selectedContact ? 'chat' : 'sidebar');
    }
  }, [isMobile, selectedContact]);

  const showChat = () => {
    if (isMobile) {
      setMobileView('chat');
    }
  };

  const showSidebar = () => {
    if (isMobile) {
      setMobileView('sidebar');
    }
  };

  const isViewVisible = (view: MobileView): boolean => {
    return !isMobile || mobileView === view;
  };

  return {
    mobileView,
    showChat,
    showSidebar,
    isViewVisible,
  };
}; 