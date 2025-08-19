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
          // Server says no user session - try to establish session from global storage
          // This handles cases where server-to-server injection worked but session cookies weren't set
          try {
            const establishResponse = await fetch('/api/user/establish-session', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' }
            });
            
            if (establishResponse.ok) {
              const sessionData = await establishResponse.json();
              if (sessionData.success && sessionData.userEmail) {
                console.log('✅ Session established from global storage for:', sessionData.userEmail);
                await createLocalSession({
                  email: sessionData.userEmail,
                  userId: sessionData.userId,
                  timestamp: Date.now(),
                });
                setIsAuthenticated(true);
                setUserEmail(sessionData.userEmail);
                setUserId(sessionData.userId);
                return;
              }
            }
          } catch (establishError) {
            console.log('No session could be established from global storage:', establishError);
          }
          
          // Clear any stale local session if no server session available
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