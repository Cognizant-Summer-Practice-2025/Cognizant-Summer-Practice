import { NextRequest, NextResponse } from 'next/server';

// Reference to the same storage used in inject
declare global {
  var messagesServiceUserStorage: Map<string, any>;
}

if (!global.messagesServiceUserStorage) {
  global.messagesServiceUserStorage = new Map();
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
    if (!verifyServiceAuth(request)) {
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    const { email } = await request.json();

    if (!email) {
      return NextResponse.json({ error: 'User email is required' }, { status: 400 });
    }
    const existed = global.messagesServiceUserStorage.has(email);
    global.messagesServiceUserStorage.delete(email);

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