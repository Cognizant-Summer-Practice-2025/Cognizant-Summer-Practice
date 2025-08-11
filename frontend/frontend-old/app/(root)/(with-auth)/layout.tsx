'use client';

import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { Loading } from '@/components/loader';

export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const { status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === 'loading') {
      // Still loading, don't redirect yet
      return;
    }

    if (status === 'unauthenticated') {
      // User is not authenticated, redirect to login
      router.push('/login');
      return;
    }
  }, [status, router]);

  // Show loading while checking authentication
  if (status === 'loading') {
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

  // Show nothing while redirecting
  if (status === 'unauthenticated') {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  // User is authenticated, show the protected content
  return (
    <>
      {children}
    </>
  );
}
