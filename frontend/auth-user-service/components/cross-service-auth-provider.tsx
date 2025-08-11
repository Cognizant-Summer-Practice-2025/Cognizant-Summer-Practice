'use client';

import { ReactNode } from 'react';
import { useCrossServiceAuth } from '@/lib/hooks/use-cross-service-auth';

interface CrossServiceAuthProviderProps {
  children: ReactNode;
}

/**
 * Provider that sets up cross-service authentication detection
 * This allows the auth service to respond to logout events from other services
 */
export function CrossServiceAuthProvider({ children }: CrossServiceAuthProviderProps) {
  // Set up cross-service logout detection
  useCrossServiceAuth();

  return <>{children}</>;
}