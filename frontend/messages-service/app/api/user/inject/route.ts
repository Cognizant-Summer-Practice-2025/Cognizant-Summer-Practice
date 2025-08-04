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
    console.log('Messages service: Received user injection request');
    
    // Verify service authentication
    if (!verifyServiceAuth(request)) {
      console.log('Messages service: Auth verification failed');
      return NextResponse.json({ error: 'Unauthorized service request' }, { status: 401 });
    }

    console.log('Messages service: Auth verification passed');

    const userData: ServiceUserData = await request.json();
    console.log('Messages service: Received user data:', { email: userData.email, id: userData.id });

    if (!userData.email || !userData.id) {
      console.log('Messages service: Missing required user data');
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
    console.error('Error injecting user data in messages service:', error);
    console.error('Error stack:', error.stack);
    return NextResponse.json(
      { error: `Failed to inject user data: ${error.message}` },
      { status: 500 }
    );
  }
}