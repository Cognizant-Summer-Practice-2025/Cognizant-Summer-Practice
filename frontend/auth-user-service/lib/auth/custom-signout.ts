import { signOut } from 'next-auth/react';

/**
 * Custom sign-out function that removes user data from all services
 * before signing out from NextAuth
 */
export async function customSignOut() {
  try {
    // First, remove user data from all other services
    const response = await fetch('/api/auth/signout-all', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      console.error('Failed to remove user data from all services');
      // Continue with sign-out even if service cleanup fails
    }

    // Then sign out from NextAuth
    await signOut({ 
      callbackUrl: '/',
      redirect: true 
    });
  } catch (error) {
    console.error('Error during sign-out:', error);
    // Fall back to regular NextAuth sign-out
    await signOut({ 
      callbackUrl: '/',
      redirect: true 
    });
  }
}