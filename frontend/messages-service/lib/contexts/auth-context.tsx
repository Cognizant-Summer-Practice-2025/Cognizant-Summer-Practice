'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { verifySSOToken, createLocalSession, getLocalSession, clearLocalSession, redirectToAuth, logoutFromAllServices } from '@/lib/auth/sso-auth';

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

  const logout = async () => {
    // Clear local state immediately for better UX
    setIsAuthenticated(false);
    setUserEmail(null);
    setUserId(null);
    
    // Call the logout function that removes user from all services
    await logoutFromAllServices();
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
        // Check existing local session first
        const session = getLocalSession();
        if (session) {
          setIsAuthenticated(true);
          setUserEmail(session.email);
          setUserId(session.userId);
        } else {
          // Check if user data has been injected by auth service
          try {
            const response = await fetch('/api/user/get');
            if (response.ok) {
              const userData = await response.json();
              if (userData && userData.email) {
                // Create local session from injected data
                await createLocalSession({
                  email: userData.email,
                  userId: userData.id,
                  timestamp: Date.now()
                });
                setIsAuthenticated(true);
                setUserEmail(userData.email);
                setUserId(userData.id);
              } else {
                setIsAuthenticated(false);
                setUserEmail(null);
                setUserId(null);
              }
            } else {
              setIsAuthenticated(false);
              setUserEmail(null);
              setUserId(null);
            }
          } catch (injectionError) {
            console.log('No injected user data found, user not authenticated');
            setIsAuthenticated(false);
            setUserEmail(null);
            setUserId(null);
          }
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

    // Set up periodic check for user injection (every 15 seconds)
    const intervalId = setInterval(() => {
      if (!isAuthenticated) {
        checkAuthentication();
      }
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

    return () => {
      clearInterval(intervalId);
      window.removeEventListener('storage', handleStorageChange);
    };
  }, [isAuthenticated]);

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