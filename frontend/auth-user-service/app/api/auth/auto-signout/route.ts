import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { clearNextAuthSession } from '@/lib/auth/session-clearer';

/**
 * Automatic sign-out endpoint that clears NextAuth session without any redirects
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

    // Create response that clears NextAuth session cookies
    const response = NextResponse.json({ 
      success: true, 
      message: 'User logged out automatically',
      callbackUrl: callbackUrl
    });

    // Clear NextAuth session cookies using utility
    clearNextAuthSession(response);

    return response;

  } catch (error) {
    console.error('Error in automatic sign-out:', error);
    return NextResponse.json(
      { error: 'Failed to sign out' },
      { status: 500 }
    );
  }
} 