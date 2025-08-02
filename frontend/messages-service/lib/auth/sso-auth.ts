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
 * Redirect to auth service for logout
 */
export function redirectToLogout(): void {
  const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
  const currentUrl = window.location.origin;
  
  // Clear local session first
  clearLocalSession();
  
  // Redirect to auth service logout
  window.location.href = `${authServiceUrl}/api/auth/signout?callbackUrl=${encodeURIComponent(currentUrl)}`;
}