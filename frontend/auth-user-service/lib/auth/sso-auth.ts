import { signOut } from 'next-auth/react';

/**
 * SSO authentication utilities for the auth service
 */

/**
 * Detect logout from other services via localStorage events
 */
export function setupCrossServiceLogoutDetection(): () => void {
  const handleStorageChange = async (event: StorageEvent) => {
    if (event.key === 'sso_session' && event.newValue === null) {
      // Another service has cleared the SSO session, sign out from auth service too
      console.log('Logout detected from another service, signing out from NextAuth...');
      
      try {
        // Sign out from NextAuth to clear the session
        const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
        await signOut({ 
          callbackUrl: homeServiceUrl,
          redirect: true 
        });
      } catch (error) {
        console.error('Error signing out from NextAuth after cross-service logout:', error);
        // Force redirect to home service if signOut fails
        const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
        window.location.href = homeServiceUrl;
      }
    }
  };

  // Listen for storage events
  window.addEventListener('storage', handleStorageChange);

  // Return cleanup function
  return () => {
    window.removeEventListener('storage', handleStorageChange);
  };
}

/**
 * Trigger cross-service logout by clearing SSO session
 * This will cause other services to detect the logout
 */
export function triggerCrossServiceLogout(): void {
  try {
    // Check if there's an existing SSO session to clear
    const existingSession = localStorage.getItem('sso_session');
    if (existingSession) {
      localStorage.removeItem('sso_session');
      console.log('SSO session cleared, other services will detect logout');
    }
  } catch (error) {
    console.error('Failed to trigger cross-service logout:', error);
  }
}

/**
 * Enhanced custom sign-out that also triggers cross-service logout
 */
export async function enhancedSignOut(): Promise<void> {
  try {
    // Trigger cross-service logout first
    triggerCrossServiceLogout();

    // Then call the existing custom signout
    const { customSignOut } = await import('./custom-signout');
    await customSignOut();
  } catch (error) {
    console.error('Error during enhanced sign-out:', error);
    // Fall back to regular NextAuth sign-out
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    await signOut({ 
      callbackUrl: homeServiceUrl,
      redirect: true 
    });
  }
}