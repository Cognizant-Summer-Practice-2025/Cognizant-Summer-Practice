'use client';

import React, { createContext, useContext, useState, useCallback, useEffect, ReactNode } from 'react';
import { SERVICES } from '@/lib/config/services';

interface JWTUser {
  id: string;
  email: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  isAdmin?: boolean;
}

interface JWTAuthContextType {
  isAuthenticated: boolean;
  user: JWTUser | null;
  loading: boolean;
  login: (token: string) => Promise<boolean>;
  logout: () => void;
  refreshAuth: () => Promise<void>;
}

const JWTAuthContext = createContext<JWTAuthContextType | undefined>(undefined);

export function JWTAuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<JWTUser | null>(null);
  const [loading, setLoading] = useState(true);

  const getStoredToken = useCallback((): string | null => {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('jwt_auth_token') || sessionStorage.getItem('jwt_auth_token');
  }, []);

  const storeToken = useCallback((token: string, persistent = true) => {
    if (typeof window === 'undefined') return;
    if (persistent) localStorage.setItem('jwt_auth_token', token);
    else sessionStorage.setItem('jwt_auth_token', token);
  }, []);

  const clearStoredToken = useCallback(() => {
    if (typeof window === 'undefined') return;
    localStorage.removeItem('jwt_auth_token');
    sessionStorage.removeItem('jwt_auth_token');
  }, []);

  const verifyToken = useCallback(async (token: string): Promise<JWTUser | null> => {
    try {
      const authServiceUrl = SERVICES.AUTH_USER_SERVICE;
      console.log('[admin][auth] verifyToken: calling verify-jwt');
      const resp = await fetch(`${authServiceUrl}/api/auth/verify-jwt`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token })
      });
      if (!resp.ok) {
        if (resp.status === 401) { clearStoredToken(); return null; }
        console.log('[admin][auth] verifyToken: non-401 error', resp.status);
        return null;
      }
      const data = await resp.json();
      console.log('[admin][auth] verifyToken: success', !!data?.user);
      return data?.success && data?.user ? data.user : null;
    } catch {
      console.log('[admin][auth] verifyToken: exception');
      return null;
    }
  }, [clearStoredToken]);

  const attemptAutoLogin = useCallback(async (): Promise<boolean> => {
    try {
      const authServiceUrl = SERVICES.AUTH_USER_SERVICE;
      console.log('[admin][auth] attemptAutoLogin: calling get-jwt');
      const resp = await fetch(`${authServiceUrl}/api/auth/get-jwt`, {
        method: 'GET',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
      });
      if (!resp.ok) { console.log('[admin][auth] attemptAutoLogin: no active session'); return false; }
      const data = await resp.json();
      if (data?.success && data?.token) {
        const userData = await verifyToken(data.token);
        if (userData) {
          storeToken(data.token, true);
          setUser(userData);
          setIsAuthenticated(true);
          console.log('[admin][auth] attemptAutoLogin: success');
          return true;
        }
      }
      return false;
    } catch {
      console.log('[admin][auth] attemptAutoLogin: exception');
      return false;
    }
  }, [storeToken, verifyToken]);

  const login = useCallback(async (token: string): Promise<boolean> => {
    try {
      setLoading(true);
      console.log('[admin][auth] login: start');
      const userData = await verifyToken(token);
      if (userData) {
        setUser(userData);
        setIsAuthenticated(true);
        storeToken(token, true);
        console.log('[admin][auth] login: success', userData.email);
        return true;
      }
      return false;
    } finally { setLoading(false); }
  }, [verifyToken, storeToken]);

  const logout = useCallback(async () => {
    setUser(null);
    setIsAuthenticated(false);
    clearStoredToken();
    console.log('[admin][auth] logout: initiated');
    try {
      if (typeof window !== 'undefined') {
        const authServiceUrl = SERVICES.AUTH_USER_SERVICE;
        const callbackUrl = SERVICES.ADMIN_SERVICE;
        const services = [
          SERVICES.HOME_PORTFOLIO_SERVICE,
          SERVICES.MESSAGES_SERVICE,
          SERVICES.ADMIN_SERVICE,
        ].filter(Boolean).join(',');
        await fetch(`${authServiceUrl}/api/auth/auto-signout?callbackUrl=${encodeURIComponent(callbackUrl)}`, {
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
        });
        const cascadeUrl = `${authServiceUrl}/api/auth/cascade-signout?services=${encodeURIComponent(services)}&return=${encodeURIComponent(callbackUrl)}`;
        console.log('[admin][auth] logout: redirecting to cascade', cascadeUrl);
        window.location.href = cascadeUrl;
      }
    } catch {}
  }, [clearStoredToken]);

  const refreshAuth = useCallback(async () => {
    try {
      setLoading(true);
      const token = getStoredToken();
      if (!token) {
        console.log('[admin][auth] refreshAuth: no token, trying auto-login');
        const ok = await attemptAutoLogin();
        if (!ok) { setIsAuthenticated(false); setUser(null); }
        return;
      }
      console.log('[admin][auth] refreshAuth: verifying stored token');
      const userData = await verifyToken(token);
      if (userData) { setUser(userData); setIsAuthenticated(true); }
      else { setIsAuthenticated(false); setUser(null); clearStoredToken(); }
    } finally { setLoading(false); }
  }, [getStoredToken, verifyToken, clearStoredToken, attemptAutoLogin]);

  useEffect(() => {
    if (typeof window === 'undefined') return;
    const run = async () => {
      console.log('[admin][auth] init effect: starting');
      const params = new URLSearchParams(window.location.search);
      const ssoToken = params.get('ssoToken');
      if (ssoToken) {
        const ok = await login(ssoToken);
        if (ok) {
          const url = new URL(window.location.href);
          url.searchParams.delete('ssoToken');
          window.history.replaceState({}, '', url.toString());
          console.log('[admin][auth] init effect: ssoToken consumed');
          return;
        }
      }
      const auto = await attemptAutoLogin();
      console.log('[admin][auth] init effect: auto-login result', auto);
      if (!auto) await refreshAuth();
    };
    run().finally(() => {
      console.log('[admin][auth] init effect: setLoading false');
      setLoading(false);
    });
  }, [login, attemptAutoLogin, refreshAuth]);

  const value: JWTAuthContextType = { isAuthenticated, user, loading, login, logout, refreshAuth };

  return (
    <JWTAuthContext.Provider value={value}>{children}</JWTAuthContext.Provider>
  );
}

export function useJWTAuth(): JWTAuthContextType {
  const ctx = useContext(JWTAuthContext);
  if (!ctx) throw new Error('useJWTAuth must be used within JWTAuthProvider');
  return ctx;
}


