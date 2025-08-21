'use client';

import React, { useEffect, useState } from 'react';
import { useUser } from '@/lib/contexts/user-context';
import { useAuth } from '@/lib/hooks/use-auth';
import { AdminAccessModal } from './admin-access-modal';
import { LoadingOverlay } from '@/components/loader/loading-overlay';
import { Logger } from '@/lib/logger';

interface AdminGuardProps {
  children: React.ReactNode;
}

export function AdminGuard({ children }: AdminGuardProps) {
  const { user, loading: userLoading } = useUser();
  const { isAuthenticated, loading: authLoading } = useAuth();
  const [showAccessModal, setShowAccessModal] = useState(false);
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    const checkAdminAccess = async () => {
      try {
        setIsChecking(true);
        console.log('[admin][guard] check: start', { authLoading, userLoading, isAuthenticated, hasUser: !!user });

        // Wait for authentication to complete
        if (authLoading || userLoading) {
          console.log('[admin][guard] waiting auth/user');
          return;
        }

        // If not authenticated, redirect to auth service
        if (!isAuthenticated) {
          console.log('[admin][guard] not authenticated, redirect to auth');
          const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE;
          window.location.href = `${authServiceUrl}/api/sso/callback?callbackUrl=${encodeURIComponent(window.location.href)}`;
          return;
        }

        // If user data is loaded but user is not admin, show access modal
        if (user && !user.isAdmin) {
          console.log('[admin][guard] user is not admin');
          setShowAccessModal(true);
          return;
        }

        // If no user data and authenticated, there might be an injection issue
        if (!user && isAuthenticated) {
          console.log('[admin][guard] authenticated but no user, fallback modal in 2s');
          Logger.warn('User is authenticated but no user data found in admin service');
          // Try to refresh user data
          setTimeout(() => {
            if (!user) {
              setShowAccessModal(true);
            }
          }, 2000); // Give 2 seconds for data to load
          return;
        }

      } catch (error) {
        Logger.error('Error checking admin access', error);
        console.log('[admin][guard] error', error);
        setShowAccessModal(true);
      } finally {
        setIsChecking(false);
        console.log('[admin][guard] check: done');
      }
    };

    checkAdminAccess();
  }, [isAuthenticated, user, authLoading, userLoading]);

  const { logout } = useAuth();
  const handleSignOut = async () => {
    try {
      await logout();
    } catch (error) {
      Logger.error('Error during sign out', error);
    }
  };

  // Show loading while checking authentication and admin rights
  if (authLoading || userLoading || isChecking) {
    return <LoadingOverlay isOpen={true} />;
  }

  // Show access denied modal
  if (showAccessModal) {
    return <AdminAccessModal isOpen={true} onSignOut={handleSignOut} />;
  }

  // Show admin content only if user is authenticated and is admin
  if (isAuthenticated && user && user.isAdmin) {
    return <>{children}</>;
  }

  // Default loading state
  return <LoadingOverlay isOpen={true} />;
}
