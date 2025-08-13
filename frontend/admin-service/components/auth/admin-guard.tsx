'use client';

import React, { useEffect, useState } from 'react';
import { useUser } from '@/lib/contexts/user-context';
import { useAuth } from '@/lib/contexts/auth-context';
import { AdminAccessModal } from './admin-access-modal';
import { customSignOut } from '@/lib/auth/custom-signout';
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

        // Wait for authentication to complete
        if (authLoading || userLoading) {
          return;
        }

        // If not authenticated, redirect to auth service
        if (!isAuthenticated) {
          const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
          window.location.href = `${authServiceUrl}/login?callbackUrl=${encodeURIComponent(window.location.href)}`;
          return;
        }

        // If user data is loaded but user is not admin, show access modal
        if (user && !user.isAdmin) {
          setShowAccessModal(true);
          return;
        }

        // If no user data and authenticated, there might be an injection issue
        if (!user && isAuthenticated) {
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
        setShowAccessModal(true);
      } finally {
        setIsChecking(false);
      }
    };

    checkAdminAccess();
  }, [isAuthenticated, user, authLoading, userLoading]);

  const handleSignOut = async () => {
    try {
      await customSignOut();
    } catch (error) {
      Logger.error('Error during sign out', error);
      // Fallback redirect
      const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
      window.location.href = homeServiceUrl;
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
