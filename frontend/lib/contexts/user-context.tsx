'use client';

import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useSession } from 'next-auth/react';
import { User } from '@/lib/user/interfaces';
import { getUserByEmail, updateUser } from '@/lib/user/api';

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
  const { data: session, status } = useSession();
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
    if (session?.user?.email) {
      await fetchUser(session.user.email);
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
    if (!user?.id) {
      throw new Error('No user ID available');
    }

    try {
      setLoading(true);
      setError(null);
      const updatedUser = await updateUser(user.id, userData);
      setUser(updatedUser);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to update user data';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Reset user state when session changes
    if (status === 'loading') {
      setLoading(true);
      return;
    }

    if (status === 'unauthenticated' || !session?.user?.email) {
      setUser(null);
      setLoading(false);
      setError(null);
      return;
    }

    // Fetch user data when session is available
    if (status === 'authenticated' && session?.user?.email && !user) {
      fetchUser(session.user.email);
    }
  }, [session, status, user]);

  const value: UserContextType = {
    user,
    loading: loading || status === 'loading',
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