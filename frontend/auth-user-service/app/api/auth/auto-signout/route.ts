import { NextRequest, NextResponse } from 'next/server';
import { clearNextAuthSession } from '@/lib/auth/session-clearer';

/**
 * Automatic sign-out endpoint that clears NextAuth session and cascades logout to all services
 */
export async function POST(request: NextRequest) {
  try {
    // Get the callback URL from the request or use current origin
    const { searchParams, origin } = new URL(request.url);
    const callbackUrl = searchParams.get('callbackUrl') || origin;

    const homeService = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    const messagesService = process.env.NEXT_PUBLIC_MESSAGES_SERVICE || 'http://localhost:3002';
    const services = [homeService, messagesService].filter(Boolean);

    // If logout initiated directly on auth (callback equals this origin), return HTML that cascades
    const isDirectFromAuth = (() => {
      try { return new URL(callbackUrl).origin === origin; } catch { return true; }
    })();

    if (isDirectFromAuth) {
      // Redirect to the client signing out page that shows the Loader and cascades
      const page = new URL('/signing-out', origin);
      page.searchParams.set('services', services.join(','));
      page.searchParams.set('return', callbackUrl);
      const res = NextResponse.redirect(page.toString(), { status: 302 });
      clearNextAuthSession(res);
      return res;
    }

    // Otherwise, return JSON with cascade URL for frontends to follow
    const cascade = new URL(`${origin}/api/auth/cascade-signout`);
    cascade.searchParams.set('services', services.join(','));
    cascade.searchParams.set('return', callbackUrl);

    const response = NextResponse.json({ success: true, cascadeUrl: cascade.toString(), callbackUrl });
    clearNextAuthSession(response);
    return response;
  } catch (error) {
    console.error('Error in automatic sign-out:', error);
    return NextResponse.json({ error: 'Failed to sign out' }, { status: 500 });
  }
} 