'use client';

import { useAuth } from '@/lib/contexts/auth-context';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { Loading } from '@/components/loader';

export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const { isAuthenticated, loading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (loading) {
      return;
    }

    if (!isAuthenticated) {
      const authServiceUrl = process.env.NEXT_PUBLIC_AUTH_USER_SERVICE || 'http://localhost:3000';
      const currentUrl = window.location.href;
      window.location.href = `${authServiceUrl}/api/sso/callback?callbackUrl=${encodeURIComponent(currentUrl)}`;
      return;
    }
  }, [isAuthenticated, loading]);

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full max-w-md min-h-[200px]">
          <div className="flex flex-col items-center justify-center py-8">
            <Loading className="scale-50" backgroundColor="white" />
            <span className="mt-4 text-gray-600">Loading...</span>
          </div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  return (
    <>
      {children}
    </>
  );
}
