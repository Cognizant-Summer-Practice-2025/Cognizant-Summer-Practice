import { NextRequest, NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import '@/types/global';
import { randomBytes } from 'crypto';

// Session-based storage for user data
if (!global.messagesServiceSessionStorage) {
  global.messagesServiceSessionStorage = new Map();
}

// Generate a secure session ID
function generateSessionId(): string {
  return randomBytes(32).toString('hex');
}

interface SSOTokenPayload {
  email: string;
  userId: string;
  timestamp: number;
}

/**
 * Client-facing inject endpoint that sets session cookies
 * Called by browsers after SSO verification
 */
export async function POST(request: NextRequest) {
  try {
    const { ssoToken } = await request.json();

    if (!ssoToken) {
      return NextResponse.json({ error: 'SSO token is required' }, { status: 400 });
    }

    // Verify the SSO token
    try {
      const secret = new TextEncoder().encode(process.env.AUTH_SECRET || 'fallback-secret');
      const { payload } = await jwtVerify(ssoToken, secret);
      
      const ssoPayload: SSOTokenPayload = {
        email: payload.email as string,
        userId: payload.userId as string,
        timestamp: payload.timestamp as number
      };

      // Check if we have user data from server-to-server injection
      const globalUserData = global.messagesServiceUserStorage?.get(ssoPayload.email);
      
      if (!globalUserData) {
        return NextResponse.json({ error: 'User data not found. Please try logging in again.' }, { status: 404 });
      }

      // Generate a session ID for this device/browser
      const sessionId = generateSessionId();

      // Store user data with session ID
      global.messagesServiceSessionStorage.set(sessionId, globalUserData);

      // Create response with session cookie
      const response = NextResponse.json({
        success: true,
        message: 'User session established',
        userId: globalUserData.id
      });

      // Set session cookie (ms_sid = messages service session id)
      response.cookies.set('ms_sid', sessionId, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        sameSite: 'lax',
        maxAge: 60 * 60 * 24 * 7, // 7 days
        path: '/'
      });

      return response;

    } catch (verifyError) {
      console.error('SSO token verification failed:', verifyError);
      return NextResponse.json({ error: 'Invalid SSO token' }, { status: 401 });
    }

  } catch (error) {
    console.error('Error in client inject:', error);
    return NextResponse.json(
      { error: 'Failed to establish user session' },
      { status: 500 }
    );
  }
}
