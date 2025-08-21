'use client';

import { JWTAuthProvider } from '@/lib/contexts/jwt-auth-context';
import { UserProvider } from '@/lib/contexts/user-context';
import { PortfolioProvider } from '@/lib/contexts/portfolio-context';
import { DraftProvider } from '@/lib/contexts/draft-context';
import { HomePageCacheProvider } from '@/lib/contexts/home-page-cache-context';
import { WebSocketProvider } from '@/lib/contexts/websocket-context';
import { ToastProvider } from '@/components/ui/toast';


export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <JWTAuthProvider>
      <UserProvider>
        <WebSocketProvider>
          <PortfolioProvider>      
              <DraftProvider>
                  <HomePageCacheProvider>
                    <ToastProvider>
                      {children}
                    </ToastProvider>
                  </HomePageCacheProvider>
              </DraftProvider>
          </PortfolioProvider>
        </WebSocketProvider>
      </UserProvider>
    </JWTAuthProvider>
  );
} 