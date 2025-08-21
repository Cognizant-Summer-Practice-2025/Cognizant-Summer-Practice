import { useJWTAuth } from '@/lib/contexts/jwt-auth-context';

export function useAuth() {
  const { isAuthenticated, user, loading, login, logout, refreshAuth } = useJWTAuth();
  return {
    isAuthenticated,
    user,
    loading,
    login,
    logout,
    refreshAuth,
    userEmail: user?.email ?? null,
  };
}


