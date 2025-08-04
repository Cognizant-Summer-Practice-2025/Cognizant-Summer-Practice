import { NextResponse } from 'next/server';

/**
 * Utility to clear NextAuth session cookies and session data
 */
export function clearNextAuthSession(response: NextResponse): NextResponse {
  const cookieOptions = {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax' as const,
    path: '/',
    maxAge: 0 // Expire immediately
  };

  // Clear all possible NextAuth session cookies
  response.cookies.set('next-auth.session-token', '', cookieOptions);
  response.cookies.set('next-auth.csrf-token', '', cookieOptions);
  response.cookies.set('next-auth.callback-url', '', cookieOptions);
  response.cookies.set('__Secure-next-auth.session-token', '', cookieOptions);
  response.cookies.set('__Secure-next-auth.csrf-token', '', cookieOptions);
  response.cookies.set('__Secure-next-auth.callback-url', '', cookieOptions);
  
  // Clear any custom session cookies
  response.cookies.set('auth_session', '', cookieOptions);
  response.cookies.set('session', '', cookieOptions);
  response.cookies.set('__Host-next-auth.csrf-token', '', cookieOptions);
  response.cookies.set('__Secure-next-auth.pkce.verifier', '', cookieOptions);

  // Set cache control headers to prevent caching
  response.headers.set('Cache-Control', 'no-cache, no-store, must-revalidate');
  response.headers.set('Pragma', 'no-cache');
  response.headers.set('Expires', '0');

  return response;
} 