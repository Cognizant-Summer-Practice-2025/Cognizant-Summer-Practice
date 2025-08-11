import { NextRequest, NextResponse } from 'next/server';
import type { ServiceUserData } from '@/types/global';

// Ensure global storage is initialized
if (typeof global !== 'undefined' && !global.homePortfolioServiceUserStorage) {
  global.homePortfolioServiceUserStorage = new Map<string, ServiceUserData>();
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
    const existing = global.homePortfolioServiceUserStorage.get(userData.email);
    const merged: ServiceUserData = {
      ...(existing || {}),
      ...userData,
      accessToken: userData.accessToken || existing?.accessToken,
    } as ServiceUserData;

    // Store user data in global storage
    global.homePortfolioServiceUserStorage.set(userData.email, merged);

    

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