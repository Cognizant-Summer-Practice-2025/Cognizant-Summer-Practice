'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useAuth } from '@/lib/contexts/auth-context';
import { User } from '@/lib/user/interfaces';
import { ServiceUserData } from '@/types/global';
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
      
      // Get user data from injected storage
      const response = await fetch('/api/user/get');
      
      if (!response.ok) {
        if (response.status === 404) {
          setUser(null);
          setError('User not found in this service. Please log in again.');
          return;
        }
        throw new Error(`Failed to get user data: ${response.statusText}`);
      }
      
      const userData: ServiceUserData = await response.json();
      
      // Transform ServiceUserData to User format with isAdmin
      const transformedUser: User & { isAdmin: boolean } = {
        id: userData.id,
        email: userData.email,
        username: userData.username,
        firstName: userData.firstName || '',
        lastName: userData.lastName || '',
        professionalTitle: userData.professionalTitle,
        bio: userData.bio,
        location: userData.location,
        avatarUrl: userData.profileImage,
        isActive: userData.isActive,
        isAdmin: userData.isAdmin,
        lastLoginAt: userData.lastLoginAt,
      };
      
      setUser(transformedUser);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to fetch user data';
      setError(errorMessage);
      Logger.error('Error fetching user data', err);
    } finally {
      setLoading(false);
    }
  };

  const refetchUser = async () => {
    if (isAuthenticated) {
      await fetchUser();
    }
  };

  const updateUserData = async (userData: {
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