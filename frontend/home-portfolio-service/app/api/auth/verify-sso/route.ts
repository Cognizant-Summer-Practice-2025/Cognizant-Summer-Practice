import { NextRequest, NextResponse } from 'next/server';
import { jwtVerify } from 'jose';

interface SSOTokenPayload {
  email: string;
  userId: string;
  timestamp: number;
}

/**
 * Server-side JWT verification API
 */
export async function POST(request: NextRequest) {
  try {
    const { token } = await request.json();

    if (!token) {
      return NextResponse.json({ error: 'Token is required' }, { status: 400 });
    }

    // Verify JWT token on server side where AUTH_SECRET is available
    const secret = new TextEncoder().encode(process.env.AUTH_SECRET || 'fallback-secret');
    const { payload } = await jwtVerify(token, secret);
    
    const ssoPayload: SSOTokenPayload = {
      email: payload.email as string,
      userId: payload.userId as string,
      timestamp: payload.timestamp as number
    };

    return NextResponse.json({ 
      success: true, 
      payload: ssoPayload 
    });
  } catch (error) {
    console.error('Failed to verify SSO token:', error);
    return NextResponse.json({ 
      error: 'Invalid token' 
    }, { status: 401 });
  }
}