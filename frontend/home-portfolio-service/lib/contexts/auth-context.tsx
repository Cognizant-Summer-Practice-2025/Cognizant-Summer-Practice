'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode, useCallback } from 'react';
import { verifySSOToken, createLocalSession, clearLocalSession, redirectToAuth, logoutFromAllServices } from '@/lib/auth/sso-auth';

interface AuthContextType {
  isAuthenticated: boolean;
  userEmail: string | null;
  userId: string | null;
  loading: boolean;
  login: () => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [userEmail, setUserEmail] = useState<string | null>(null);
  const [userId, setUserId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const login = () => {
    redirectToAuth();
  };

  const logout = async () => {
    setIsLoggingOut(true);
    try {
      // Clear local state immediately for better UX
      setIsAuthenticated(false);
      setUserEmail(null);
      setUserId(null);
      
      // Call the logout function that removes user from all services
      await logoutFromAllServices();
    } finally {
      setIsLoggingOut(false);
    }
  };

  const checkAuthentication = useCallback(async () => {
    try {
      // If we are in the middle of logging out, skip auth checks to avoid races
      if (isLoggingOut) {
        return;
      }
      setLoading(true);

      // Check for SSO token in URL
      const urlParams = new URLSearchParams(window.location.search);
      const ssoToken = urlParams.get('ssoToken');

      if (ssoToken) {
        // Verify and create local session from SSO token
        const tokenPayload = await verifySSOToken(ssoToken);
        if (tokenPayload) {
          // Establish session cookies by calling inject-client endpoint
          try {
            const injectResponse = await fetch('/api/user/inject-client', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
              },
              body: JSON.stringify({ ssoToken }),
            });

            if (!injectResponse.ok) {
              console.error('Failed to establish session cookies:', injectResponse.statusText);
              throw new Error('Failed to establish session');
            }

            await createLocalSession(tokenPayload);
            setIsAuthenticated(true);
            setUserEmail(tokenPayload.email);
            setUserId(tokenPayload.userId);

            // Remove token from URL
            const newUrl = new URL(window.location.href);
            newUrl.searchParams.delete('ssoToken');
            window.history.replaceState({}, '', newUrl.toString());
            return;
          } catch (error) {
            console.error('Error establishing session:', error);
            // Fall through to handle as unauthenticated
          }
        }
      }

      // Always validate against server-injected data first to avoid stale local sessions
      try {
        const response = await fetch('/api/user/get');
        if (response.ok) {
          const userData = await response.json();
          if (userData && userData.email) {
            await createLocalSession({
              email: userData.email,
              userId: userData.id,
              timestamp: Date.now(),
            });
            setIsAuthenticated(true);
            setUserEmail(userData.email);
            setUserId(userData.id);
            return;
          }
        } else if (response.status === 404 || response.status === 401) {
          // Server says no user session - try silent authentication
          // This automatically identifies the user through background SSO
          try {
            console.log('🔄 Attempting silent authentication...');
            const silentResponse = await fetch('/api/auth/silent-sso', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              credentials: 'include' // Include cookies for user identification
            });
            
            if (silentResponse.ok) {
              const silentData = await silentResponse.json();
              if (silentData.success && silentData.ssoToken) {
                console.log('✅ Silent authentication successful');
                
                // Verify the SSO token and establish session
                const tokenPayload = await verifySSOToken(silentData.ssoToken);
                if (tokenPayload) {
                  // Establish session cookies by calling inject-client endpoint
                  const injectResponse = await fetch('/api/user/inject-client', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ ssoToken: silentData.ssoToken }),
                  });

                  if (injectResponse.ok) {
                    await createLocalSession(tokenPayload);
                    setIsAuthenticated(true);
                    setUserEmail(tokenPayload.email);
                    setUserId(tokenPayload.userId);
                    console.log('✅ Auto-authentication successful for user:', tokenPayload.email);
                    return;
                  }
                }
              }
            } else {
              // Log detailed error information for debugging
              const errorData = await silentResponse.json().catch(() => ({}));
              if (silentResponse.status === 401) {
                console.log('🔒 Silent authentication: No active session in auth service (expected if user not logged in)');
              } else {
                console.log('🔒 Silent authentication failed with status:', silentResponse.status, errorData);
              }
            }
          } catch (silentError) {
            console.log('🔒 Silent authentication error:', silentError);
          }
          
          // Clear local session if silent auth failed
          clearLocalSession();
        }
      } catch {
        // Network errors: do not auto-login from local session to avoid stale auth
      }

      // Default to unauthenticated when server has no user or on error
      setIsAuthenticated(false);
      setUserEmail(null);
      setUserId(null);
    } catch (error) {
      console.error('Authentication check failed:', error);
      setIsAuthenticated(false);
      setUserEmail(null);
      setUserId(null);
    } finally {
      setLoading(false);
    }
  }, [isLoggingOut]);

  useEffect(() => {
    checkAuthentication();

    // Periodic check regardless of current auth state to detect cross-service logout
    const intervalId = setInterval(() => {
      checkAuthentication();
    }, 15000);

    // Listen for storage events (when user logs out from another tab)
    const handleStorageChange = (event: StorageEvent) => {
      if (event.key === 'sso_session' && event.newValue === null) {
        // User logged out from another tab
        setIsAuthenticated(false);
        setUserEmail(null);
        setUserId(null);
      }
    };

    window.addEventListener('storage', handleStorageChange);

    // Detect when tab becomes active to refresh auth state (helps cross-service logout)
    const handleVisibilityOrFocus = () => {
      checkAuthentication();
    };
    window.addEventListener('focus', handleVisibilityOrFocus);
    document.addEventListener('visibilitychange', handleVisibilityOrFocus);

    return () => {
      clearInterval(intervalId);
      window.removeEventListener('storage', handleStorageChange);
      window.removeEventListener('focus', handleVisibilityOrFocus);
      document.removeEventListener('visibilitychange', handleVisibilityOrFocus);
    };
  }, [isAuthenticated, isLoggingOut, checkAuthentication]);

  const value: AuthContextType = {
    isAuthenticated,
    userEmail,
    userId,
    loading,
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}