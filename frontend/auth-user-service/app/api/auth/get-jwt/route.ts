import { NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { SignJWT } from 'jose';

function buildCorsHeaders(request: Request): HeadersInit {
  const origin = request.headers.get('origin') || '*';
  return {
    'Access-Control-Allow-Origin': origin,
    'Vary': 'Origin',
    'Access-Control-Allow-Methods': 'GET, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    'Access-Control-Allow-Credentials': 'true',
  };
}

export async function OPTIONS(request: Request) {
  return new NextResponse(null, { headers: buildCorsHeaders(request) });
}

/**
 * Get JWT token endpoint for microservices
 * Other services can call this to get a valid JWT token for the current user
 */
export async function GET(request: Request) {
  try {
    // Get current NextAuth session
    const session = await getServerSession(authOptions);
    
    if (!session?.user?.email) {
      return NextResponse.json({ 
        error: 'No active session',
        requiresLogin: true
      }, { status: 401, headers: buildCorsHeaders(request) });
    }

    // Create a JWT token for the calling service
    const secret = new TextEncoder().encode(process.env.AUTH_SECRET || 'fallback-secret');
    const token = await new SignJWT({ 
      email: session.user.email,
      userId: session.userId,
      accessToken: session.accessToken || '',
      timestamp: new Date().getTime()
    })
      .setProtectedHeader({ alg: 'HS256' })
      .setExpirationTime('1h') // Token expires in 1 hour
      .sign(secret);

    // Return the JWT token
    return NextResponse.json({
      success: true,
      token,
      user: {
        email: session.user.email,
        userId: session.userId,
        accessToken: session.accessToken || ''
      },
      message: 'JWT token generated successfully'
    }, { headers: buildCorsHeaders(request) });

  } catch (error) {
    console.error('Error generating JWT token:', error);
    return NextResponse.json({ 
      error: 'Failed to generate JWT token' 
    }, { status: 500, headers: buildCorsHeaders(request) });
  }
}
