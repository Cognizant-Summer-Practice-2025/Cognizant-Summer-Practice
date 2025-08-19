import { NextRequest, NextResponse } from 'next/server';
import '@/types/global';

// Reference to the same storage used in inject - using global declaration from types/global.ts

if (!global.messagesServiceUserStorage) {
  global.messagesServiceUserStorage = new Map();
}

// Session-based storage for user data
if (!global.messagesServiceSessionStorage) {
  global.messagesServiceSessionStorage = new Map();
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
 * Remove user data from this service
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

    // Remove user data from storage
    const existed = global.messagesServiceUserStorage.has(email);
    global.messagesServiceUserStorage.delete(email);

    // Also remove all sessions for this user
    const sessionsToRemove: string[] = [];
    for (const [sessionId, userData] of global.messagesServiceSessionStorage.entries()) {
      if (userData.email === email) {
        sessionsToRemove.push(sessionId);
      }
    }
    sessionsToRemove.forEach(sessionId => {
      global.messagesServiceSessionStorage.delete(sessionId);
    });

    console.log(`User ${email} and ${sessionsToRemove.length} sessions removed from messages-service`);

    return NextResponse.json({ 
      success: true, 
      message: 'User data removed successfully',
      existed
    });
  } catch (error) {
    console.error('Error removing user data:', error);
    return NextResponse.json(
      { error: 'Failed to remove user data' },
      { status: 500 }
    );
  }
}