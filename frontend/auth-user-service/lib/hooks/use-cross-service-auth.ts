import { useEffect } from 'react';
import { setupCrossServiceLogoutDetection } from '@/lib/auth/sso-auth';

/**
 * Hook to set up cross-service authentication detection in the auth service
 * This allows the auth service to detect when users logout from other services
 */
export function useCrossServiceAuth() {
  useEffect(() => {
    // Set up logout detection from other services
    const cleanup = setupCrossServiceLogoutDetection();

    // Cleanup on unmount
    return cleanup;
  }, []);
}