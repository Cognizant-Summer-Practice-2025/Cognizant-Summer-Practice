'use client';

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useAuth } from '@/lib/hooks/use-auth';
import { getUserByEmail } from '@/lib/user';
import type { User } from '@/lib/user/interfaces';

interface UserContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  fetchUser: (email: string) => Promise<void>;
  refetchUser: () => Promise<void>;
  updateUserData: (userData: {
    firstName?: string;
    lastName?: string;
    username?: string;
    professionalTitle?: string;
    bio?: string;
    location?: string;
    profileImage?: string;
  }) => Promise<void>;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export function UserProvider({ children }: { children: ReactNode }) {
  const { userEmail, loading: authLoading, currentUser } = useAuth();
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = async (email: string) => {
    try {
      setLoading(true);
      setError(null);
      const userData = await getUserByEmail(email);
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
    if (userEmail) {
      await fetchUser(userEmail);
    }
  };

  const updateUserData = async (userData: {
    firstName?: string;
    lastName?: string;
    username?: string;
    professionalTitle?: string;
    bio?: string;
    location?: string;
    profileImage?: string;
  }) => {
    if (!user) return;
    
    setUser(prev => prev ? { ...prev, ...userData } : null);
  };

  // Immediately hydrate from JWT auth user to avoid UI waiting
  useEffect(() => {
    if (currentUser) {
      setUser(prev => prev ?? {
        id: currentUser.id,
        email: currentUser.email,
        username: currentUser.username || currentUser.email.split('@')[0],
        firstName: currentUser.firstName,
        lastName: currentUser.lastName,
        professionalTitle: currentUser.professionalTitle,
        bio: currentUser.bio,
        location: currentUser.location,
        avatarUrl: currentUser.avatarUrl,
        isActive: currentUser.isActive ?? true,
        isAdmin: currentUser.isAdmin ?? false,
        lastLoginAt: currentUser.lastLoginAt,
      });
    }
  }, [currentUser]);

  useEffect(() => {
    if (userEmail && !authLoading) {
      fetchUser(userEmail);
    }
  }, [userEmail, authLoading]);

  const value: UserContextType = {
    user,
    loading: loading || authLoading,
    error,
    fetchUser,
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