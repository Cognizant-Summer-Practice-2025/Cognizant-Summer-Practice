'use client';

import React, { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import { SERVICES } from '@/lib/config';

interface JWTUser {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  username?: string;
  professionalTitle?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  isActive?: boolean;
  isAdmin?: boolean;
  lastLoginAt?: string;
}

interface JWTAuthContextType {
  isAuthenticated: boolean;
  user: JWTUser | null;
  loading: boolean;
  login: (token: string) => Promise<boolean>;
  logout: () => void;
  refreshAuth: () => Promise<void>;
}

const JWTAuthContext = createContext<JWTAuthContextType | undefined>(undefined);

export function JWTAuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<JWTUser | null>(null);
  const [loading, setLoading] = useState(true);

  // Get JWT token from localStorage or sessionStorage
  const getStoredToken = useCallback((): string | null => {
    if (typeof window === 'undefined') return null;
    
    // Try to get from localStorage first (persistent)
    let token = localStorage.getItem('jwt_auth_token');
    
    // If not in localStorage, try sessionStorage (session-only)
    if (!token) {
      token = sessionStorage.getItem('jwt_auth_token');
    }
    
    return token;
  }, []);

  // Store JWT token
  const storeToken = useCallback((token: string, persistent: boolean = true) => {
    if (typeof window === 'undefined') return;
    
    if (persistent) {
      localStorage.setItem('jwt_auth_token', token);
    } else {
      sessionStorage.setItem('jwt_auth_token', token);
    }
  }, []);

  // Clear stored token
  const clearStoredToken = useCallback(() => {
    if (typeof window === 'undefined') return;
    
    localStorage.removeItem('jwt_auth_token');
    sessionStorage.removeItem('jwt_auth_token');
  }, []);

  // Verify JWT token with auth service
  const verifyToken = useCallback(async (token: string): Promise<JWTUser | null> => {
    try {
      const authServiceUrl = SERVICES.AUTH_USER_SERVICE;

      // Validate the URL format
      try {
        new URL(authServiceUrl);
      } catch {
        console.error('Invalid auth service URL:', authServiceUrl);
        return null;
      }

      const response = await fetch(`${authServiceUrl}/api/auth/verify-jwt`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ token }),
      });

      if (!response.ok) {
        if (response.status === 401) {
          // Token is invalid or expired
          clearStoredToken();
          return null;
        }
        throw new Error(`Auth service error: ${response.status}`);
      }

      const data = await response.json();
      if (data.success && data.user) {
        return data.user;
      }
      
      return null;
    } catch (error) {
      console.error('Error verifying JWT token:', error);
      return null;
    }
  }, [clearStoredToken]);

  // Try to obtain a new JWT from auth service if user has an active NextAuth session
  const attemptAutoLogin = useCallback(async (): Promise<boolean> => {
    try {
      const authServiceUrl = SERVICES.AUTH_USER_SERVICE;

      const resp = await fetch(`${authServiceUrl}/api/auth/get-jwt`, {
        method: 'GET',
        credentials: 'include', // send cookies to auth domain
        headers: { 'Content-Type': 'application/json' },
      });

      if (!resp.ok) return false;
      const data = await resp.json();
      if (data?.success && data?.token) {
        const userData = await verifyToken(data.token);
        if (userData) {
          storeToken(data.token, true);
          setUser(userData);
          setIsAuthenticated(true);
          return true;
        }
      }
      return false;
    } catch {
      return false;
    }
  }, [storeToken, verifyToken]);

  // Login with JWT token
  const login = useCallback(async (token: string): Promise<boolean> => {
    try {
      setLoading(true);
      
      const userData = await verifyToken(token);
      if (userData) {
        setUser(userData);
        setIsAuthenticated(true);
        storeToken(token, true); // Store persistently
        console.log('✅ JWT authentication successful');
        return true;
      }
      
      return false;
    } catch (error) {
      console.error('Login failed:', error);
      return false;
    } finally {
      setLoading(false);
    }
  }, [verifyToken, storeToken]);

  // Logout
  const logout = useCallback(async () => {
    setUser(null);
    setIsAuthenticated(false);
    clearStoredToken();
    try {
      if (typeof window !== 'undefined') {
        const authServiceUrl = SERVICES.AUTH_USER_SERVICE;
        const callbackUrl = window.location.origin;
        const services = [
          SERVICES.HOME_PORTFOLIO_SERVICE,
          SERVICES.MESSAGES_SERVICE,
        ].filter(Boolean).join(',');
        // First clear NextAuth cookies, then cascade signout across services, end at current origin
        await fetch(`${authServiceUrl}/api/auth/auto-signout?callbackUrl=${encodeURIComponent(callbackUrl)}`, {
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
        });
        const cascadeUrl = `${authServiceUrl}/api/auth/cascade-signout?services=${encodeURIComponent(services)}&return=${encodeURIComponent(callbackUrl)}`;
        window.location.href = cascadeUrl;
      }
    } catch {
      // no-op
    }
    console.log('🔒 User logged out');
  }, [clearStoredToken]);

  // Refresh authentication from stored token; if none, try auto-login via auth service
  const refreshAuth = useCallback(async () => {
    try {
      setLoading(true);
      
      const token = getStoredToken();
      if (!token) {
        // Attempt seamless login if user has active session on auth service
        const autoOk = await attemptAutoLogin();
        if (!autoOk) {
          setIsAuthenticated(false);
          setUser(null);
        }
        return;
      }

      const userData = await verifyToken(token);
      if (userData) {
        setUser(userData);
        setIsAuthenticated(true);
        console.log('✅ JWT authentication refreshed');
      } else {
        setIsAuthenticated(false);
        setUser(null);
        clearStoredToken();
      }
    } catch (error) {
      console.error('Error refreshing authentication:', error);
      setIsAuthenticated(false);
      setUser(null);
      clearStoredToken();
    } finally {
      setLoading(false);
    }
  }, [getStoredToken, verifyToken, clearStoredToken, attemptAutoLogin]);

  // Check for SSO token in URL on mount
  useEffect(() => {
    // Only run on client side
    if (typeof window === 'undefined') return;
    
    const checkForSSOToken = async () => {
      const urlParams = new URLSearchParams(window.location.search);
      const ssoToken = urlParams.get('ssoToken');
      
      if (ssoToken) {
        console.log('🔄 SSO token found in URL, processing...');
        const success = await login(ssoToken);
        
        if (success) {
          // Remove SSO token from URL
          const newUrl = new URL(window.location.href);
          newUrl.searchParams.delete('ssoToken');
          window.history.replaceState({}, '', newUrl.toString());
          return;
        }
      }

      // No SSO token or login failed: attempt auto-login, then fallback to token refresh logic
      const autoLogged = await attemptAutoLogin();
      if (!autoLogged) {
        await refreshAuth();
      }
    };

    checkForSSOToken();
  }, [login, refreshAuth, attemptAutoLogin]);

  // Periodic token refresh (every 5 minutes)
  useEffect(() => {
    // Only run on client side
    if (typeof window === 'undefined') return;
    if (!isAuthenticated) return;

    const intervalId = setInterval(() => {
      refreshAuth();
    }, 5 * 60 * 1000); // 5 minutes

    return () => clearInterval(intervalId);
  }, [isAuthenticated, refreshAuth]);

  const value: JWTAuthContextType = {
    isAuthenticated,
    user,
    loading,
    login,
    logout,
    refreshAuth,
  };

  return (
    <JWTAuthContext.Provider value={value}>
      {children}
    </JWTAuthContext.Provider>
  );
}

export function useJWTAuth(): JWTAuthContextType {
  const context = useContext(JWTAuthContext);
  if (context === undefined) {
    throw new Error('useJWTAuth must be used within a JWTAuthProvider');
  }
  return context;
}
