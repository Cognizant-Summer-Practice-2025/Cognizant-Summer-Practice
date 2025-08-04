interface SSOTokenPayload {
  email: string;
  userId: string;
  timestamp: number;
}

/**
 * Verify SSO token using server-side API
 */
export async function verifySSOToken(token: string): Promise<SSOTokenPayload | null> {
  try {
    const response = await fetch('/api/auth/verify-sso', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ token }),
    });

    if (!response.ok) {
      console.error('Failed to verify SSO token:', response.statusText);
      return null;
    }

    const data = await response.json();
    return data.payload as SSOTokenPayload;
  } catch (error) {
    console.error('Failed to verify SSO token:', error);
    return null;
  }
}

/**
 * Create local session from SSO token
 */
export async function createLocalSession(tokenPayload: SSOTokenPayload): Promise<boolean> {
  try {
    // Store session in localStorage
    const sessionData = {
      email: tokenPayload.email,
      userId: tokenPayload.userId,
      timestamp: Date.now(),
      expires: Date.now() + (24 * 60 * 60 * 1000) // 24 hours
    };
    
    localStorage.setItem('sso_session', JSON.stringify(sessionData));
    return true;
  } catch (error) {
    console.error('Failed to create local session:', error);
    return false;
  }
}

/**
 * Get current local session
 */
export function getLocalSession(): SSOTokenPayload | null {
  try {
    const sessionData = localStorage.getItem('sso_session');
    if (!sessionData) return null;
    
    const session = JSON.parse(sessionData);
    
    // Check if session is expired
    if (Date.now() > session.expires) {
      localStorage.removeItem('sso_session');
      return null;
    }
    
    return {
      email: session.email,
      userId: session.userId,
      timestamp: session.timestamp
    };
  } catch (error) {
    console.error('Failed to get local session:', error);
    return null;
  }
}

/**
 * Clear local session
 */
export function clearLocalSession(): void {
  localStorage.removeItem('sso_session');
}

/**
 * Redirect to auth service for login
 */
export function redirectToAuth(): void {
  const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
  const currentUrl = window.location.href;
  window.location.href = `${authServiceUrl}/api/sso/callback?callbackUrl=${encodeURIComponent(currentUrl)}`;
}

/**
 * Logout from all services and redirect to auth service
 */
export async function logoutFromAllServices(): Promise<void> {
  try {
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
    const currentUrl = window.location.origin;
    
    // Call the auto-signout endpoint that automatically clears NextAuth session
    const response = await fetch(`${authServiceUrl}/api/auth/auto-signout?callbackUrl=${encodeURIComponent(currentUrl)}`, {
      method: 'POST',
      credentials: 'include', // Include cookies for session
    });
    
    if (response.ok) {
      const data = await response.json();
      console.log('User logged out from all services automatically');
      
      // Clear local session
      clearLocalSession();
      
      // Redirect to the callback URL (home page)
      if (data.callbackUrl) {
        window.location.href = data.callbackUrl;
      } else {
        window.location.href = currentUrl;
      }
    } else {
      console.warn('Failed to trigger automatic signout, falling back to manual logout');
      // Fallback: clear local session and redirect to home
      clearLocalSession();
      window.location.href = currentUrl;
    }
  } catch (error) {
    console.error('Error during automatic signout:', error);
    // Fallback: clear local session and redirect to home
    clearLocalSession();
    const currentUrl = window.location.origin;
    window.location.href = currentUrl;
  }
}

/**
 * Redirect to auth service for logout (legacy function, use logoutFromAllServices instead)
 */
export function redirectToLogout(): void {
  logoutFromAllServices();
}