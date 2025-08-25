import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';

/**
 * Verify service-to-service authentication
 */
function verifyServiceAuth(request: NextRequest): boolean {
  const authHeader = request.headers.get('X-Service-Auth');
  const expectedSecret = process.env.SERVICE_AUTH_SECRET || 'default-secret';
  return authHeader === expectedSecret;
}

/**
 * External signout endpoint that can be called by other services
 * This triggers a NextAuth signout when other services initiate logout
 */
export async function POST(request: NextRequest) {
  try {
    // Verify service authentication
    if (!verifyServiceAuth(request)) {
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    const { userEmail } = await request.json();

    if (!userEmail) {
      return NextResponse.json({ error: 'User email is required' }, { status: 400 });
    }

    // Get current session to verify the user
    const session = await getServerSession(authOptions);
    
    if (session?.user?.email === userEmail) {
      // User matches current session, we should sign them out
      
      // Clear the auth service user storage
      if (global.authServiceUserStorage) {
        global.authServiceUserStorage.delete(userEmail);
      }
      
      // Return response indicating that client-side signout is needed
      return NextResponse.json({ 
        success: true,
        requiresClientSignout: true,
        message: 'User should be signed out from NextAuth'
      });
    } else {
      // Different user or no session, just clear storage
      
      if (global.authServiceUserStorage) {
        global.authServiceUserStorage.delete(userEmail);
      }
      
      return NextResponse.json({ 
        success: true,
        requiresClientSignout: false,
        message: 'User data cleared'
      });
    }
  } catch (error) {
    console.error('Error in external signout:', error);
    return NextResponse.json(
      { error: 'Failed to process external signout' },
      { status: 500 }
    );
  }
}