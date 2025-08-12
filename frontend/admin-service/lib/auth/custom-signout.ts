import { Logger } from '@/lib/logger';

/**
 * Custom sign-out function for admin service that redirects to auth service logout
 */
export async function customSignOut() {
  try {
    // Redirect to the auth service logout endpoint which handles cross-service logout
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    
    // Redirect to auth service logout with callback to home
    window.location.href = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(homeServiceUrl)}`;
  } catch (error) {
    Logger.error('Error during sign-out', error);
    // Fall back to direct redirect to home service
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    window.location.href = homeServiceUrl;
  }
}
