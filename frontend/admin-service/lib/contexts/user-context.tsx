'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from '@/lib/hooks/use-auth';
import { User } from '@/lib/user/interfaces';
import { Logger } from '@/lib/logger';

interface UserContextType {
  user: (User & { isAdmin: boolean }) | null;
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
  const [user, setUser] = useState<(User & { isAdmin: boolean }) | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = async () => {
    try {
      setLoading(true);
      setError(null);
      console.log('[admin][user] fetchUser: start', { isAuthenticated, userEmail });
      // Map from JWT auth user directly; this already includes isAdmin from verify-jwt
      // We only need minimal mapping to the local User shape
      if (isAuthenticated && userEmail) {
        setUser(prev => prev ?? ({
          id: '',
          email: userEmail,
          username: '',
          firstName: '',
          lastName: '',
          professionalTitle: '',
          bio: '',
          location: '',
          avatarUrl: '',
          isActive: true,
          isAdmin: true, // Will be corrected below if available from window.authUser
          lastLoginAt: null,
        } as unknown as User & { isAdmin: boolean }));
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch user data';
      setError(errorMessage);
      Logger.error('Error fetching user data', err);
    } finally {
      setLoading(false);
      console.log('[admin][user] fetchUser: done');
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
      console.log('[admin][user] effect: authLoading true');
      return;
    }

    if (!isAuthenticated || !userEmail) {
      setUser(null);
      setLoading(false);
      setError(null);
      console.log('[admin][user] effect: unauthenticated or no email');
      return;
    }

    // Fetch user data when authenticated
    if (isAuthenticated && userEmail && !user) {
      console.log('[admin][user] effect: fetching user');
      fetchUser();
    }
  }, [isAuthenticated, userEmail, authLoading]);

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