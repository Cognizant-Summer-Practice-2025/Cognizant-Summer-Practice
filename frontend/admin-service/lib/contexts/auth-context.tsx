'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { verifySSOToken, createLocalSession, getLocalSession, clearLocalSession, redirectToAuth, redirectToLogout } from '@/lib/auth/sso-auth';

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

  const login = () => {
    redirectToAuth();
  };

  const logout = () => {
    clearLocalSession();
    setIsAuthenticated(false);
    setUserEmail(null);
    setUserId(null);
    redirectToLogout();
  };

  const checkAuthentication = async () => {
    try {
      setLoading(true);

      // Check for SSO token in URL
      const urlParams = new URLSearchParams(window.location.search);
      const ssoToken = urlParams.get('ssoToken');

      if (ssoToken) {
        // Verify and create local session from SSO token
        const tokenPayload = await verifySSOToken(ssoToken);
        if (tokenPayload) {
          await createLocalSession(tokenPayload);
          setIsAuthenticated(true);
          setUserEmail(tokenPayload.email);
          setUserId(tokenPayload.userId);
          
          // Remove token from URL
          const newUrl = new URL(window.location.href);
          newUrl.searchParams.delete('ssoToken');
          window.history.replaceState({}, '', newUrl.toString());
        }
      } else {
        // Check existing local session
        const session = getLocalSession();
        if (session) {
          setIsAuthenticated(true);
          setUserEmail(session.email);
          setUserId(session.userId);
        } else {
          setIsAuthenticated(false);
          setUserEmail(null);
          setUserId(null);
        }
      }
    } catch (error) {
      console.error('Authentication check failed:', error);
      setIsAuthenticated(false);
      setUserEmail(null);
      setUserId(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    checkAuthentication();
  }, []);

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