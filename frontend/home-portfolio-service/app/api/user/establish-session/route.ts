import { NextRequest, NextResponse } from 'next/server';
import type { ServiceUserData } from '@/types/global';
import { randomBytes } from 'crypto';

// Session-based storage for user data
if (typeof global !== 'undefined' && !global.homePortfolioServiceSessionStorage) {
  global.homePortfolioServiceSessionStorage = new Map<string, ServiceUserData>();
}

// Generate a secure session ID
function generateSessionId(): string {
  return randomBytes(32).toString('hex');
}

/**
 * Establish session cookies when user data exists in global storage but no session cookies set
 * This handles cases where server-to-server injection worked but client-side session wasn't established
 */
export async function POST(request: NextRequest) {
  try {
    // Check if session cookie already exists
    const existingSessionId = request.cookies.get('hp_sid')?.value;
    if (existingSessionId && global.homePortfolioServiceSessionStorage.has(existingSessionId)) {
      // Session already exists and is valid
      const userData = global.homePortfolioServiceSessionStorage.get(existingSessionId);
      return NextResponse.json({
        success: true,
        message: 'Session already established',
        userId: userData?.id
      });
    }

    // Check if there's any user data in global storage from server-to-server injection
    if (!global.homePortfolioServiceUserStorage || global.homePortfolioServiceUserStorage.size === 0) {
      return NextResponse.json({ error: 'No user data available. Please log in.' }, { status: 404 });
    }

    // Take the first (and should be only) user from global storage
    // In most cases, there will be only one user due to server-to-server injection
    const globalUserData = Array.from(global.homePortfolioServiceUserStorage.values())[0];
    
    if (!globalUserData) {
      return NextResponse.json({ error: 'No valid user data found.' }, { status: 404 });
    }

    // Generate a session ID for this device/browser
    const sessionId = generateSessionId();

    // Store user data with session ID
    global.homePortfolioServiceSessionStorage.set(sessionId, globalUserData);

    // Create response with session cookie
    const response = NextResponse.json({
      success: true,
      message: 'Session established from global storage',
      userId: globalUserData.id,
      userEmail: globalUserData.email
    });

    // Set session cookie (hp_sid = home portfolio session id)
    response.cookies.set('hp_sid', sessionId, {
      httpOnly: true,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      maxAge: 60 * 60 * 24 * 7, // 7 days
      path: '/'
    });

    return response;

  } catch (error) {
    console.error('Error establishing session from global storage:', error);
    return NextResponse.json(
      { error: 'Failed to establish session' },
      { status: 500 }
    );
  }
}
