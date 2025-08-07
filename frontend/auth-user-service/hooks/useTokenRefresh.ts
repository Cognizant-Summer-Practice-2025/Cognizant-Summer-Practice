"use client";

import { useSession, signIn } from "next-auth/react";
import { useEffect } from "react";

/**
 * Hook to handle automatic token refresh and re-authentication when tokens expire
 */
export function useTokenRefresh() {
  const { data: session, status } = useSession();

  useEffect(() => {
    if (status === "authenticated" && session?.error === "RefreshAccessTokenError") {
      console.warn("Token refresh failed, prompting for re-authentication");
      // Force re-authentication by signing in again
      // This will trigger the OAuth flow and get fresh tokens
      signIn();
    }
  }, [session?.error, status]);

  return {
    session,
    status,
    isTokenExpired: session?.error === "RefreshAccessTokenError",
    needsReauth: status === "authenticated" && session?.error === "RefreshAccessTokenError"
  };
}

/**
 * Hook to check token expiration status from backend
 */
export function useTokenStatus() {
  const { data: session } = useSession();

  const checkTokenStatus = async () => {
    if (!session?.accessToken) {
      return null;
    }

    try {
      const backendUrl = process.env.NEXT_PUBLIC_USER_API_URL || 'http://localhost:5200';
      const response = await fetch(`${backendUrl}/api/oauth2/token-status`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${session.accessToken}`,
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        return await response.json();
      }
      
      return null;
    } catch (error) {
      console.error('Error checking token status:', error);
      return null;
    }
  };

  const manualRefresh = async () => {
    if (!session?.accessToken) {
      console.warn('No access token available for manual refresh');
      return false;
    }

    // Check if the provider supports refresh tokens
    const tokenStatus = await checkTokenStatus();
    const provider = tokenStatus?.providers?.find((p: any) => p.hasRefreshToken && p.supportsRefresh);
    
    if (!provider) {
      console.warn('No refresh token available or provider does not support refresh');
      return false;
    }

    try {
      console.log('Triggering manual token refresh through re-authentication');
      // Force session update by signing in again (will use existing auth and trigger automatic refresh)
      await signIn();
      return true;
    } catch (error) {
      console.error('Error manually refreshing token:', error);
      return false;
    }
  };

  return {
    checkTokenStatus,
    manualRefresh,
  };
}