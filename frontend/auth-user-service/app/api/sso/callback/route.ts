import { NextRequest, NextResponse } from 'next/server';
import { getServerSession } from 'next-auth';
import { authOptions } from '@/lib/auth/auth-options';
import { getUserByEmail } from '@/lib/user';
import { SignJWT } from 'jose';

/**
 * SSO callback endpoint that handles authentication and redirects back to calling service
 */
export async function GET(request: NextRequest) {
  try {
    const { searchParams } = new URL(request.url);
    const callbackUrl = searchParams.get('callbackUrl');
    const silent = searchParams.get('silent') === 'true';
    
    if (!callbackUrl) {
      return NextResponse.redirect(new URL('/', request.url));
    }

    // Get current session
    const session = await getServerSession(authOptions);
    
    if (!session?.user?.email) {
      // Handle silent authentication failure
      if (silent) {
        return NextResponse.json({
          error: 'No active session',
          requiresLogin: true
        }, { status: 401 });
      }
      
      // Normal flow: redirect to login with callback
      const loginUrl = new URL('/login', request.url);
      loginUrl.searchParams.set('callbackUrl', callbackUrl);
      return NextResponse.redirect(loginUrl);
    }

    // User is authenticated, get full user data
    const userData = await getUserByEmail(session.user.email);
    
    if (!userData) {
      return NextResponse.redirect(new URL('/login?error=UserNotFound', request.url));
    }

    // Create a temporary SSO token for the calling service
    const secret = new TextEncoder().encode(process.env.AUTH_SECRET || 'fallback-secret');
    const token = await new SignJWT({ 
      email: userData.email,
      userId: userData.id,
      timestamp: new Date().getTime()
    })
      .setProtectedHeader({ alg: 'HS256' })
      .setExpirationTime('5m') // Token expires in 5 minutes
      .sign(secret);

    // Redirect back to the calling service with the token
    const redirectUrl = new URL(callbackUrl);
    redirectUrl.searchParams.set('ssoToken', token);
    
    return NextResponse.redirect(redirectUrl);
    
  } catch (error) {
    console.error('Error in SSO callback:', error);
    return NextResponse.redirect(new URL('/login?error=SSOError', request.url));
  }
}