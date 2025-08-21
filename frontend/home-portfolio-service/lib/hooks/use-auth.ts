'use client';

import { useJWTAuth } from '@/lib/contexts/jwt-auth-context';

/**
 * Simple authentication hook that provides easy access to auth state
 * Use this in components instead of directly accessing the context
 */
export function useAuth() {
  const auth = useJWTAuth();
  
  return {
    isAuthenticated: auth.isAuthenticated,
    user: auth.user,
    loading: auth.loading,
    login: auth.login,
    logout: auth.logout,
    refreshAuth: auth.refreshAuth,
    
    // Convenience getters
    isLoggedIn: auth.isAuthenticated,
    currentUser: auth.user,
    userEmail: auth.user?.email,
    userId: auth.user?.id,
    userName: auth.user?.firstName && auth.user?.lastName 
      ? `${auth.user.firstName} ${auth.user.lastName}` 
      : auth.user?.username || 'User',
    userAvatar: auth.user?.avatarUrl,
  };
}
