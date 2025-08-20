import { NextRequest, NextResponse } from 'next/server';
import { jwtVerify } from 'jose';
import { getUserByEmail } from '@/lib/user';

function buildCorsHeaders(request: NextRequest): HeadersInit {
  const origin = request.headers.get('origin') || '*';
  // Echo back specific origin to support credentials if needed in future
  return {
    'Access-Control-Allow-Origin': origin,
    'Vary': 'Origin',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    'Access-Control-Allow-Credentials': 'true',
  };
}

export async function OPTIONS(request: NextRequest) {
  return new NextResponse(null, { headers: buildCorsHeaders(request) });
}

/**
 * JWT verification endpoint for microservices
 * Other services can call this to validate NextAuth JWT tokens
 */
export async function POST(request: NextRequest) {
  try {
    const { token } = await request.json();
    
    if (!token) {
      return NextResponse.json({ 
        error: 'JWT token is required' 
      }, { status: 400, headers: buildCorsHeaders(request) });
    }

    // Verify the JWT token using a single shared secret
    const secret = process.env.AUTH_SECRET || process.env.NEXTAUTH_SECRET;
    if (!secret) {
      console.error('AUTH_SECRET (or NEXTAUTH_SECRET) not configured');
      return NextResponse.json({ 
        error: 'Server configuration error' 
      }, { status: 500, headers: buildCorsHeaders(request) });
    }

    try {
      // Decode and verify the JWT token
      const secretKey = new TextEncoder().encode(secret);
      const { payload } = await jwtVerify(token, secretKey);

      if (!payload || !payload.email) {
        return NextResponse.json({ 
          error: 'Invalid or expired token',
          requiresLogin: true
        }, { status: 401, headers: buildCorsHeaders(request) });
      }

      // Get full user data
      const userData = await getUserByEmail(payload.email as string);
      if (!userData) {
        return NextResponse.json({ 
          error: 'User not found',
          requiresLogin: true
        }, { status: 404, headers: buildCorsHeaders(request) });
      }

      // Return user data and authentication status
      return NextResponse.json({
        success: true,
        user: {
          id: userData.id,
          email: userData.email,
          firstName: userData.firstName,
          lastName: userData.lastName,
          username: userData.username,
          professionalTitle: userData.professionalTitle,
          bio: userData.bio,
          location: userData.location,
          avatarUrl: userData.avatarUrl,
          isActive: userData.isActive,
          isAdmin: userData.isAdmin,
          lastLoginAt: userData.lastLoginAt,
        },
        message: 'Authentication successful'
      }, { headers: buildCorsHeaders(request) });

    } catch (jwtError) {
      console.error('JWT verification failed:', jwtError);
      return NextResponse.json({ 
        error: 'Invalid token format',
        requiresLogin: true
      }, { status: 401, headers: buildCorsHeaders(request) });
    }

  } catch (error) {
    console.error('Error in JWT verification:', error);
    return NextResponse.json({ 
      error: 'Server error during verification' 
    }, { status: 500, headers: buildCorsHeaders(request) });
  }
}
