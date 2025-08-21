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
  const { isAuthenticated, user: authUser, loading: authLoading } = useAuth();
  const [user, setUser] = useState<(User & { isAdmin: boolean }) | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = async () => {
    try {
      setLoading(true);
      setError(null);
      console.log('[admin][user] fetchUser: start', { isAuthenticated, hasAuthUser: !!authUser });
      if (isAuthenticated && authUser) {
        const mapped: User & { isAdmin: boolean } = {
          id: authUser.id || '',
          email: authUser.email,
          username: authUser.username || '',
          firstName: authUser.firstName || '',
          lastName: authUser.lastName || '',
          professionalTitle: '',
          bio: '',
          location: '',
          avatarUrl: authUser.avatarUrl || '',
          isActive: true,
          isAdmin: !!authUser.isAdmin,
          lastLoginAt: null,
        } as unknown as User & { isAdmin: boolean };
        setUser(mapped);
        console.log('[admin][user] fetchUser: mapped from auth user', { isAdmin: mapped.isAdmin });
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
    if (isAuthenticated) await fetchUser();
  };

  const updateUserData = async () => {
    // User updates should be handled by the auth-user-service
    // Redirect to auth service for profile updates
    const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE as string;
    window.location.href = `${authServiceUrl}/profile`;
  };

  useEffect(() => {
    // Reset user state when auth changes
    if (authLoading) {
      setLoading(true);
      console.log('[admin][user] effect: authLoading true');
      return;
    }

    if (!isAuthenticated || !authUser) {
      setUser(null);
      setLoading(false);
      setError(null);
      console.log('[admin][user] effect: unauthenticated or no email');
      return;
    }

    // Fetch user data when authenticated
    if (isAuthenticated && authUser && !user) {
      console.log('[admin][user] effect: fetching user');
      fetchUser();
    }
  }, [isAuthenticated, authUser, authLoading]);

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