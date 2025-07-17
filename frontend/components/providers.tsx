'use client';

import { SessionProvider } from 'next-auth/react';
import { UserProvider } from '@/lib/contexts/user-context';
import { PortfolioProvider } from '@/lib/contexts/portfolio-context';
import { DraftProvider } from '@/lib/contexts/draft-context';

export function Providers({ children }: { children: React.ReactNode }) {
  return (
    <SessionProvider>
      <UserProvider>
        <PortfolioProvider>
          <DraftProvider>
            {children}
          </DraftProvider>
        </PortfolioProvider>
      </UserProvider>
    </SessionProvider>
  );
} 