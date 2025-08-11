'use client';

import { SessionProvider } from 'next-auth/react';
import { UserProvider } from '@/lib/contexts/user-context';
import { PortfolioProvider } from '@/lib/contexts/portfolio-context';
import { DraftProvider } from '@/lib/contexts/draft-context';
import { BookmarkProvider } from '@/lib/contexts/bookmark-context';
import { HomePageCacheProvider } from '@/lib/contexts/home-page-cache-context';
import { WebSocketProvider } from '@/lib/contexts/websocket-context';
import { ToastProvider } from '@/components/ui/toast';
import { CrossServiceAuthProvider } from './cross-service-auth-provider';
import { AuthSignoutMonitor } from './auth-signout-monitor';


export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <CrossServiceAuthProvider>
        <UserProvider>
          <WebSocketProvider>
            <BookmarkProvider>
              <PortfolioProvider>      
                  <DraftProvider>
                      <HomePageCacheProvider>
                        <ToastProvider>
                          <AuthSignoutMonitor />
                          {children}
                        </ToastProvider>
                      </HomePageCacheProvider>
                  </DraftProvider>
              </PortfolioProvider>
            </BookmarkProvider>
          </WebSocketProvider>
        </UserProvider>
      </CrossServiceAuthProvider>
    </SessionProvider>
  );
} 