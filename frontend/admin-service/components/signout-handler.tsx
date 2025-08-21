'use client';

import { useEffect } from 'react';

export function SignoutHandler() {
  useEffect(() => {
    try {
      const url = new URL(window.location.href);
      if (url.searchParams.get('signout') === '1') {
        localStorage.removeItem('jwt_auth_token');
        sessionStorage.removeItem('jwt_auth_token');
        url.searchParams.delete('signout');
        window.history.replaceState({}, '', url.toString());
      }
    } catch {}
  }, []);

  return null;
}


