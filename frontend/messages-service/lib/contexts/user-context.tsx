'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from '@/lib/contexts/auth-context';
import { User } from '@/lib/user/interfaces';
import { getLocalSession } from '@/lib/auth/sso-auth';

interface UserContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  refetchUser: () => Promise<void>;
  updateUserData: (userData: {
    firstName?: string;
    lastName?: string;
    professionalTitle?: string;
    bio?: string;
    location?: string;
    profileImage?: string;
  }) => Promise<void>;
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
      
      // Get user ID from SSO session for session-based identification
      const session = getLocalSession();
      let url = '/api/user/get';
      
      // Add user identification parameters for session-based lookup
      const params = new URLSearchParams();
      
      if (session?.userId) {
        params.append('userId', session.userId);
        console.log('Fetching user data for userId:', session.userId);
      }
      
      if (session?.email) {
        params.append('userEmail', session.email);
        console.log('Fetching user data for email:', session.email);
      }
      
      // Fallback to userEmail from auth context if no session
      if (!session && userEmail) {
        params.append('userEmail', userEmail);
        console.log('Fallback: Fetching user data for auth email:', userEmail);
      }
      
      if (params.toString()) {
        url += `?${params.toString()}`;
      }
      
      // Get user data from injected storage with session-based identification
      const response = await fetch(url);
      
      if (!response.ok) {
        if (response.status === 404) {
          setUser(null);
          setError('User not found in this service. Please log in again.');
          return;
        }
        throw new Error(`Failed to get user data: ${response.statusText}`);
      }
      
      const userData = await response.json();
      setUser(userData);
      console.log('Successfully loaded user data:', { email: userData.email, id: userData.id });
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

  const updateUserData = async (_userData: {
    firstName?: string;
    lastName?: string;
    professionalTitle?: string;
    bio?: string;
    location?: string;
    profileImage?: string;
  }) => {
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

    // Fetch user data when authenticated
    if (isAuthenticated && userEmail && !user) {
      fetchUser();
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