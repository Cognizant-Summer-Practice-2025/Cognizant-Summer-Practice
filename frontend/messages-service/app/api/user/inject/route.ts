import { NextRequest, NextResponse } from 'next/server';

interface ServiceUserData {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  isActive: boolean;
  isAdmin: boolean;
  lastLoginAt?: string;
}

// Global storage for user data
declare global {
  var messagesServiceUserStorage: Map<string, ServiceUserData>;
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
 * Inject user data into this service
 */
export async function POST(request: NextRequest) {
  try {
    if (!verifyServiceAuth(request)) {
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    const userData: ServiceUserData = await request.json();

    if (!userData.email || !userData.id) {
      return NextResponse.json({ error: 'User email and ID are required' }, { status: 400 });
    }
    global.messagesServiceUserStorage.set(userData.email, userData);

    return NextResponse.json({ 
      success: true, 
      message: 'User data injected successfully',
      userId: userData.id
    });
  } catch (error) {
    return NextResponse.json(
      { error: `Failed to inject user data: ${error.message}` },
      { status: 500 }
    );
  }
}