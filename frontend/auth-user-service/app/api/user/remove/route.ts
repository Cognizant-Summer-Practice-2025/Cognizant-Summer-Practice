import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { User } from '@/lib/user/interfaces';

// Global storage for user data (similar to other services)
declare global {
  var authServiceUserStorage: Map<string, User>;
  var authServiceSignoutSignals: Set<string>;
}

// Initialize global storage if it doesn't exist
if (!global.authServiceUserStorage) {
  global.authServiceUserStorage = new Map();
}

// Initialize signout signals storage
if (!global.authServiceSignoutSignals) {
  global.authServiceSignoutSignals = new Set();
}

/**
 * Verify service-to-service authentication
 */
function verifyServiceAuth(request: NextRequest): boolean {
  const authHeader = request.headers.get('X-Service-Auth');
  const expectedSecret = process.env.SERVICE_AUTH_SECRET || 'default-secret';
  return authHeader === expectedSecret;
}

/**
 * Remove user data from this service (auth service)
 * This is called when other services trigger a logout
 */
export async function DELETE(request: NextRequest) {
  try {
    // Verify service authentication
    if (!verifyServiceAuth(request)) {
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    const { email } = await request.json();

    if (!email) {
      return NextResponse.json({ error: 'User email is required' }, { status: 400 });
    }

    // Check if this is the current session user
    const session = await getServerSession(authOptions);
    const isCurrentUser = session?.user?.email === email;

    // Remove user data from storage
    const existed = global.authServiceUserStorage.has(email);
    global.authServiceUserStorage.delete(email);

    // If this is the current user, set a signout signal
    if (isCurrentUser) {
      global.authServiceSignoutSignals.add(email);
      console.log(`User ${email} removed from auth-service (current user - signout signal set)`);
    } else {
      console.log(`User ${email} removed from auth-service`);
    }

    return NextResponse.json({ 
      success: true, 
      message: 'User data removed successfully',
      existed,
      isCurrentUser,
      signoutSignalSet: isCurrentUser
    });
  } catch (error) {
    console.error('Error removing user data:', error);
    return NextResponse.json(
      { error: 'Failed to remove user data' },
      { status: 500 }
    );
  }
}