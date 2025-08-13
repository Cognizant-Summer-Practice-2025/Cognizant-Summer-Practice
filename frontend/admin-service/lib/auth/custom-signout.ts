import { Logger } from '@/lib/logger';

/**
 * Custom sign-out function for admin service that redirects to auth service logout
 */
export async function customSignOut() {
  try {
    // First call the auth service's signout-all endpoint to ensure user removal
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    
    try {
      // Call the signout-all endpoint which handles user removal and then redirects to NextAuth signout
      const response = await fetch(`${authServiceUrl}/api/auth/signout-all?callbackUrl=${encodeURIComponent(homeServiceUrl)}`, {
        method: 'POST',
        credentials: 'include', // Include cookies for session
      });
      
      if (response.ok) {
        // The signout-all endpoint will redirect, but in case it doesn't, handle it
        const data = await response.json();
        if (data.redirectUrl) {
          window.location.href = data.redirectUrl;
        } else {
          // Fallback to direct NextAuth signout
          window.location.href = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(homeServiceUrl)}`;
        }
      } else {
        Logger.warn('signout-all endpoint failed, falling back to direct NextAuth signout');
        // Fallback to direct NextAuth signout
        window.location.href = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(homeServiceUrl)}`;
      }
    } catch (fetchError) {
      Logger.warn('Failed to call signout-all endpoint, falling back to direct NextAuth signout', fetchError);
      // Fallback to direct NextAuth signout
      window.location.href = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(homeServiceUrl)}`;
    }
  } catch (error) {
    Logger.error('Error during sign-out', error);
    // Fall back to direct redirect to home service
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    window.location.href = homeServiceUrl;
  }
}
