'use client';

import { useEffect } from 'react';

export function SignoutHandler() {
  useEffect(() => {
    // Only run on client side
    if (typeof window === 'undefined') return;

    const urlParams = new URLSearchParams(window.location.search);
    const signout = urlParams.get('signout');

    if (signout === '1') {
      // Clear any local storage or session storage if needed
      // For auth service, we mainly need to clear NextAuth cookies
      // The auto-signout endpoint already cleared them, but we can clear any remaining ones
      document.cookie = 'next-auth.session-token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
      document.cookie = 'next-auth.csrf-token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
      document.cookie = 'next-auth.callback-url=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
      
      // Remove the signout parameter from URL
      const newUrl = new URL(window.location.href);
      newUrl.searchParams.delete('signout');
      window.history.replaceState({}, '', newUrl.toString());
      
      // Optionally redirect to login page or home
      // window.location.href = '/auth/signin';
    }
  }, []);

  return null;
}
