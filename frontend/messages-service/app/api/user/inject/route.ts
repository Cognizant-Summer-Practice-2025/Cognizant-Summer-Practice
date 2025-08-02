import { NextRequest, NextResponse } from 'next/server';

interface ServiceUserData {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  profileImage?: string;
  isAdmin: boolean;
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
    // Verify service authentication
    if (!verifyServiceAuth(request)) {
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    const userData: ServiceUserData = await request.json();

    if (!userData.email || !userData.id) {
      return NextResponse.json({ error: 'User email and ID are required' }, { status: 400 });
    }

    // Store user data in global storage
    global.messagesServiceUserStorage.set(userData.email, userData);

    console.log(`User ${userData.email} injected into messages-service`);

    return NextResponse.json({ 
      success: true, 
      message: 'User data injected successfully',
      userId: userData.id
    });
  } catch (error) {
    console.error('Error injecting user data:', error);
    return NextResponse.json(
      { error: 'Failed to inject user data' },
      { status: 500 }
    );
  }
}