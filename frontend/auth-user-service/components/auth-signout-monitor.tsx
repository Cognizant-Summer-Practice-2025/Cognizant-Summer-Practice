'use client';

import { useEffect } from 'react';
import { useSession, signOut } from 'next-auth/react';

/**
 * Component that monitors for signout signals and triggers NextAuth signOut
 * This handles cases where the server indicates that the current user should be signed out
 */
export function AuthSignoutMonitor() {
  const { data: session } = useSession();

  useEffect(() => {
    const checkSignoutSignal = async () => {
      if (!session?.user?.email) return;

      try {
        // Check if there's a signout signal for the current user
        const response = await fetch('/api/auth/check-signout-signal', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email: session.user.email }),
        });

        if (response.ok) {
          const data = await response.json();
          if (data.shouldSignOut) {
            console.log('Signout signal detected, signing out from NextAuth...');
            const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
            await signOut({ 
              callbackUrl: homeServiceUrl,
              redirect: true 
            });
          }
        }
      } catch (error) {
        console.error('Error checking signout signal:', error);
      }
    };

    // Check for signout signal every 5 seconds
    const interval = setInterval(checkSignoutSignal, 5000);

    // Check immediately on mount
    checkSignoutSignal();

    return () => clearInterval(interval);
  }, [session?.user?.email]);

  return null; // This component doesn't render anything
}