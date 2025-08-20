import { signOut } from 'next-auth/react';
import { triggerCrossServiceLogout } from './sso-auth';

/**
 * Custom sign-out function that cascades logout to all services
 * and clears NextAuth cookies first.
 */
export async function customSignOut() {
  try {
    // Signal (no-op for JWT flow but kept for compatibility)
    triggerCrossServiceLogout();

    // Determine services and return URL
    const homeService = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    const messagesService = process.env.NEXT_PUBLIC_MESSAGES_SERVICE || 'http://localhost:3002';
    const services = [homeService, messagesService].filter(Boolean);

    // Clear NextAuth cookies on auth domain
    const authOrigin = window.location.origin; // e.g., http://localhost:3000
    const returnUrl = homeService || authOrigin;
    try {
      await fetch(`/api/auth/auto-signout?callbackUrl=${encodeURIComponent(returnUrl)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });
    } catch {
      // ignore and continue to cascade
    }

    // Always navigate to cascade-signout so each service can clear its own local storage via ?signout=1
    const cascade = new URL(`${authOrigin}/api/auth/cascade-signout`);
    cascade.searchParams.set('services', services.join(','));
    cascade.searchParams.set('return', returnUrl);
    window.location.href = cascade.toString();
  } catch (error) {
    console.error('Error during sign-out:', error);
    // Fallback to NextAuth signOut to at least clear auth cookies and return to Home
    const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    await signOut({ callbackUrl: homeServiceUrl, redirect: true });
  }
}