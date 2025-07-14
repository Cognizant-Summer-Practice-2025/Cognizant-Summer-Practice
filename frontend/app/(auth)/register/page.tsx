'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { signIn } from 'next-auth/react';
import RegistrationModal from '@/components/auth/registration-modal';
import { registerUser } from '@/lib/api';

const RegisterPage = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isModalOpen, setIsModalOpen] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Get user info from URL params
  const userEmail = searchParams.get('email') || '';
  const userName = searchParams.get('name') || '';
  const userImage = searchParams.get('image') || '';

  // Redirect if no email provided
  useEffect(() => {
    if (!userEmail) {
      router.push('/login');
    }
  }, [userEmail, router]);

  const handleModalClose = () => {
    setIsModalOpen(false);
    router.push('/login');
  };

  const handleRegistrationSubmit = async (userData: {
    firstName: string;
    lastName: string;
    professionalTitle: string;
    bio: string;
    location: string;
  }) => {
    setIsLoading(true);
    setError(null);

    try {
      // Register the user
      await registerUser({
        email: userEmail,
        firstName: userData.firstName,
        lastName: userData.lastName,
        professionalTitle: userData.professionalTitle,
        bio: userData.bio,
        location: userData.location,
        avatarUrl: userImage,
      });

      // Close modal
      setIsModalOpen(false);
      
      // Now sign in the user (this should work since user is created)
      await signIn('google', { 
        callbackUrl: '/profile',
        redirect: true 
      });

    } catch (error) {
      console.error('Registration error:', error);
      setError(error instanceof Error ? error.message : 'Registration failed. Please try again.');
      setIsLoading(false);
    }
  };

  if (!userEmail) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-600">Redirecting to login...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full space-y-8 p-8">
        <div className="text-center">
          <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
            Almost there!
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            We need a few more details to complete your account setup.
          </p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-md p-4">
            <div className="text-sm text-red-600">{error}</div>
          </div>
        )}
      </div>

      <RegistrationModal
        isOpen={isModalOpen}
        onClose={handleModalClose}
        onSubmit={handleRegistrationSubmit}
        userEmail={userEmail}
        userName={userName}
        userAvatar={userImage}
        isLoading={isLoading}
      />
    </div>
  );
};

export default RegisterPage; 