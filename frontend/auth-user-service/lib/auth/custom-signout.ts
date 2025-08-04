import { signOut } from 'next-auth/react';
import { triggerCrossServiceLogout } from './sso-auth';

/**
 * Custom sign-out function that removes user data from all services
 * before signing out from NextAuth
 */
export async function customSignOut() {
  try {
    // First, trigger cross-service logout signal
    triggerCrossServiceLogout();

    // Then, remove user data from all other services
    const response = await fetch('/api/auth/signout-all', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      console.error('Failed to remove user data from all services');
      // Continue with sign-out even if service cleanup fails
    } else {
      console.log('Successfully removed user data from all services');
    }

    // Finally, sign out from NextAuth
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    await signOut({ 
      callbackUrl: homeServiceUrl,
      redirect: true 
    });
  } catch (error) {
    console.error('Error during sign-out:', error);
    // Fall back to regular NextAuth sign-out
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    await signOut({ 
      callbackUrl: homeServiceUrl,
      redirect: true 
    });
  }
}