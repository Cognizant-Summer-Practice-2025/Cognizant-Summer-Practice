import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { UserInjectionService } from '@/lib/services/user-injection-service';

/**
 * Custom sign-out endpoint that removes user data from all services
 * and automatically triggers NextAuth signOut
 */
export async function POST(request: NextRequest) {
  try {
    // Get current session
    const session = await getServerSession(authOptions);
    
    if (!session?.user?.email) {
      return NextResponse.json({ error: 'No active session found' }, { status: 401 });
    }

    // Remove user data from all other services
    await UserInjectionService.removeUser(session.user.email);

    // Get the callback URL from the request or use default
    const { searchParams } = new URL(request.url);
    const callbackUrl = searchParams.get('callbackUrl') || process.env.NEXTAUTH_URL || 'http://localhost:3000';

    // Create response with automatic redirect to NextAuth signout
    const response = NextResponse.json({ 
      success: true, 
      message: 'User data removed from all services. Redirecting to signout...',
      redirectUrl: `/api/auth/signout?callbackUrl=${encodeURIComponent(callbackUrl)}`
    });

    // Set immediate redirect headers
    response.headers.set('Location', `/api/auth/signout?callbackUrl=${encodeURIComponent(callbackUrl)}`);
    response.headers.set('Cache-Control', 'no-cache, no-store, must-revalidate');
    response.headers.set('Pragma', 'no-cache');
    response.headers.set('Expires', '0');

    return response;
  } catch (error) {
    console.error('Error in custom sign-out:', error);
    return NextResponse.json(
      { error: 'Failed to sign out from all services' },
      { status: 500 }
    );
  }
}