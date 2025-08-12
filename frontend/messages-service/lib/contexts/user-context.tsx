'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from '@/lib/contexts/auth-context';
import { User } from '@/lib/user/interfaces';

interface UserContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  refetchUser: () => Promise<void>;
  updateUserData: () => Promise<void>;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export function UserProvider({ children }: { children: ReactNode }) {
  const { isAuthenticated, userEmail, loading: authLoading } = useAuth();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Get user data from injected storage
      const response = await fetch('/api/user/get');
      
      if (!response.ok) {
        if (response.status === 404) {
          setUser(null);
          // Avoid showing a scary error on initial render; natural empty state is fine
          setError(null);
          return;
        }
        throw new Error(`Failed to get user data: ${response.statusText}`);
      }
      
      const userData = await response.json();
      setUser(userData);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch user data';
      setError(errorMessage);
      console.error('Error fetching user data:', err);
    } finally {
      setLoading(false);
    }
  };

  const refetchUser = async () => {
    if (isAuthenticated) {
      await fetchUser();
    }
  };

  const updateUserData = async () => {
    // User updates should be handled by the auth-user-service
    // Redirect to auth service for profile updates
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
    window.location.href = `${authServiceUrl}/profile`;
  };

  useEffect(() => {
    // Reset user state when auth changes
    if (authLoading) {
      setLoading(true);
      return;
    }

    if (!isAuthenticated || !userEmail) {
      setUser(null);
      setLoading(false);
      setError(null);
      return;
    }

    // Fetch user data when authenticated, wait a short moment for injection to land
    if (isAuthenticated && userEmail && !user) {
      const timer = setTimeout(() => {
        fetchUser();
      }, 500);
      return () => clearTimeout(timer);
    }
  }, [isAuthenticated, userEmail, authLoading, user]);

  const value: UserContextType = {
    user,
    loading: loading || authLoading,
    error,
    refetchUser,
    updateUserData,
  };

  return (
    <UserContext.Provider value={value}>
      {children}
    </UserContext.Provider>
  );
}

export function useUser(): UserContextType {
  const context = useContext(UserContext);
  if (context === undefined) {
    throw new Error('useUser must be used within a UserProvider');
  }
  return context;
}