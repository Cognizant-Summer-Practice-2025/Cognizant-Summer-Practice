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
  accessToken?: string;
}

// Global storage for user data (in production, use Redis or database)
declare global {
  // Use any to avoid redeclaration type mismatch across sibling routes
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  var adminServiceUserStorage: Map<string, any>;
}

// Initialize global storage if it doesn't exist
if (!global.adminServiceUserStorage) {
  global.adminServiceUserStorage = new Map();
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

    // Preserve existing accessToken if not provided
    const existing = global.adminServiceUserStorage.get(userData.email);
    const merged: ServiceUserData = {
      ...(existing || {}),
      ...userData,
      accessToken: userData.accessToken || existing?.accessToken,
    } as ServiceUserData;

    // Store user data in global memory
    global.adminServiceUserStorage.set(userData.email, merged);

    

    return NextResponse.json({ 
      success: true, 
      message: 'User data injected successfully',
      userId: merged.id
    });
  } catch (error) {
    console.error('Error injecting user data:', error);
    return NextResponse.json(
      { error: 'Failed to inject user data' },
      { status: 500 }
    );
  }
}