'use client';

import { SessionProvider } from 'next-auth/react';
import { UserProvider } from '@/lib/contexts/user-context';
import { PortfolioProvider } from '@/lib/contexts/portfolio-context';
import { DraftProvider } from '@/lib/contexts/draft-context';
import { BookmarkProvider } from '@/lib/contexts/bookmark-context';
import { HomePageCacheProvider } from '@/lib/contexts/home-page-cache-context';
import { WebSocketProvider } from '@/lib/contexts/websocket-context';
import { ToastProvider } from '@/components/ui/toast';


export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <UserProvider>
        <WebSocketProvider>
          <BookmarkProvider>
            <PortfolioProvider>      
                <DraftProvider>
                    <HomePageCacheProvider>
                      <ToastProvider>
                        {children}
                      </ToastProvider>
                    </HomePageCacheProvider>
                </DraftProvider>
            </PortfolioProvider>
          </BookmarkProvider>
        </WebSocketProvider>
      </UserProvider>
    </SessionProvider>
  );
} 