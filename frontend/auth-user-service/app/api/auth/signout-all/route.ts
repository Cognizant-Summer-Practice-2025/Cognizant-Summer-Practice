import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';

/**
 * Custom sign-out endpoint that triggers NextAuth signOut
 */
export async function POST(request: NextRequest) {
  try {
    // Get current session
    const session = await getServerSession(authOptions);
    
    if (!session?.user?.email) {
      return NextResponse.json({ error: 'No active session found' }, { status: 401 });
    }

    // Get the callback URL from the request or use default
    const { searchParams } = new URL(request.url);
    const callbackUrl = searchParams.get('callbackUrl') || process.env.NEXTAUTH_URL || 'http://localhost:3000';
    
    // Get the current auth service URL to create absolute URLs
    const authServiceUrl = process.env.NEXTAUTH_URL || 'http://localhost:3000';
    const signoutUrl = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(callbackUrl)}`;

    // Create response with automatic redirect to NextAuth signout
    const response = NextResponse.json({ 
      success: true, 
      message: 'Redirecting to signout...',
      redirectUrl: signoutUrl
    });

    // Set immediate redirect headers
    response.headers.set('Location', signoutUrl);
    response.headers.set('Cache-Control', 'no-cache, no-store, must-revalidate');
    response.headers.set('Pragma', 'no-cache');
    response.headers.set('Expires', '0');

    return response;
  } catch (error) {
    console.error('Error in custom sign-out:', error);
    return NextResponse.json(
      { error: 'Failed to sign out' },
      { status: 500 }
    );
  }
}