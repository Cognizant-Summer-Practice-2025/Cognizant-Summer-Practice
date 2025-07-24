'use client';

import { SessionProvider } from 'next-auth/react';
import { UserProvider } from '@/lib/contexts/user-context';
import { PortfolioProvider } from '@/lib/contexts/portfolio-context';
import { DraftProvider } from '@/lib/contexts/draft-context';
import { BookmarkProvider } from '@/lib/contexts/bookmark-context';
import { HomePageCacheProvider } from '@/lib/contexts/home-page-cache-context';
import { MessagesProvider } from '@/lib/contexts/messages-context';

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <UserProvider>
        <BookmarkProvider>
          <PortfolioProvider>
            <MessagesProvider>
              <DraftProvider>
                  <HomePageCacheProvider>
                {children}
                  </HomePageCacheProvider>
              </DraftProvider>
            </MessagesProvider>
          </PortfolioProvider>
        </BookmarkProvider>
      </UserProvider>
    </SessionProvider>
  );
} 