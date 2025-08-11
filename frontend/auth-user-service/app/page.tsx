'use client';

import { useEffect } from 'react';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { Loading } from '@/components/loader';

export default function HomePage() {
  const { status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (status === 'loading') return;
    
    if (status === 'unauthenticated') {
      router.push('/login');
      return;
    }
    
    if (status === 'authenticated') {
      // Redirect authenticated users to the home portfolio service
      const homeServiceUrl = process.env.NEXT_PUBLIC_HOME_PORTFOLIO_SERVICE || 'http://localhost:3001';
      window.location.href = homeServiceUrl;
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

  // Show redirecting message while redirecting to login
  if (status === 'unauthenticated') {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  // If authenticated, redirect to home service (this shouldn't normally be reached due to useEffect redirect)
  if (status === 'authenticated') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="bg-white rounded-lg shadow-sm p-8 w-full max-w-md">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-gray-900 mb-4">Redirecting...</h1>
            <p className="text-gray-600 mb-6">Taking you to the home page.</p>
          </div>
        </div>
      </div>
    );
  }

  return null;
}