import { NextRequest, NextResponse } from 'next/server';

/**
 * Silent SSO endpoint - attempts to get an SSO token from auth service
 * without user interaction. This allows automatic user identification.
 */
export async function POST(request: NextRequest) {
  try {
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE;
    if (!authServiceUrl) {
      return NextResponse.json({ 
        error: 'Auth service URL not configured' 
      }, { status: 500 });
    }

    // Get the current URL for callback
    const currentUrl = request.headers.get('referer') || request.headers.get('origin') || process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
    
    // Forward all cookies from the original request to the auth service
    // This includes any session cookies that might identify the user
    const forwardHeaders: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    // Forward cookies to auth service so it can identify the user
    const cookieHeader = request.headers.get('cookie');
    if (cookieHeader) {
      forwardHeaders['Cookie'] = cookieHeader;
    }

    // Call auth service SSO callback endpoint with silent flag
    const ssoUrl = `${authServiceUrl}/api/sso/callback?callbackUrl=${encodeURIComponent(currentUrl)}&silent=true`;
    
    console.log('🔄 Attempting silent SSO authentication...');
    console.log('SSO URL:', ssoUrl);
    console.log('Forward headers:', JSON.stringify(forwardHeaders));
    
    const authResponse = await fetch(ssoUrl, {
      method: 'GET',
      headers: forwardHeaders,
      redirect: 'manual' // Don't follow redirects, handle them ourselves
    });

    console.log('Auth service response status:', authResponse.status);
    
    // Check if auth service returned a redirect with SSO token (user is logged in)
    // Handle both 302 (Found) and 307 (Temporary Redirect)
    if (authResponse.status === 302 || authResponse.status === 307) {
      const location = authResponse.headers.get('location');
      console.log('Redirect location:', location);
      
      if (location) {
        const redirectUrl = new URL(location);
        const ssoToken = redirectUrl.searchParams.get('ssoToken');
        
        if (ssoToken) {
          console.log('✅ Silent SSO successful - got token');
          return NextResponse.json({
            success: true,
            ssoToken,
            message: 'Silent authentication successful'
          });
        } else {
          console.log('⚠️ Redirect found but no ssoToken in URL:', redirectUrl.toString());
        }
      }
    }

    // Check if auth service returned 401 (no active session - expected for silent mode)
    if (authResponse.status === 401) {
      console.log('🔒 Silent SSO: No active session in auth service');
      return NextResponse.json({
        success: false,
        error: 'No active session',
        requiresLogin: true
      }, { status: 401 });
    }

    // Other responses (errors, etc.)
    const responseText = await authResponse.text().catch(() => 'Unknown response');
    console.log('🔒 Silent SSO failed - auth service response:', authResponse.status, responseText);
    
    // If we get a redirect but couldn't extract token, log more details
    if (authResponse.status === 302 || authResponse.status === 307) {
      const location = authResponse.headers.get('location');
      console.log('Failed redirect location:', location);
    }
    
    return NextResponse.json({
      success: false,
      error: 'Silent authentication failed',
      requiresLogin: true,
      authServiceStatus: authResponse.status,
      authServiceResponse: responseText
    }, { status: 401 });

  } catch (error) {
    console.error('Error in silent SSO:', error);
    return NextResponse.json({
      success: false,
      error: 'Silent authentication error',
      requiresLogin: true
    }, { status: 500 });
  }
}
