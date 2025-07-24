'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { signIn } from 'next-auth/react';
import RegistrationModal from '@/components/auth/registration-modal';
import { registerOAuthUser } from '@/lib/user';

const RegisterPage = () => {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isModalOpen, setIsModalOpen] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Get user info and OAuth data from URL params
  const userEmail = searchParams.get('email') || '';
  const userName = searchParams.get('name') || '';
  const userImage = searchParams.get('image') || '';
  const provider = searchParams.get('provider') || '';
  const providerId = searchParams.get('providerId') || '';
  const accessToken = searchParams.get('accessToken') || '';
  const refreshToken = searchParams.get('refreshToken') || '';
  const tokenExpiresAt = searchParams.get('tokenExpiresAt') || '';

  // Redirect if no email or OAuth data provided
  useEffect(() => {
    if (!userEmail || !provider || !providerId) {
      router.push('/login');
    }
  }, [userEmail, provider, providerId, router]);

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
      // Map provider name to match backend enum
      const providerMapping: { [key: string]: 'Google' | 'GitHub' | 'Facebook' | 'LinkedIn' } = {
        'google': 'Google',
        'github': 'GitHub', 
        'facebook': 'Facebook',
        'linkedin': 'LinkedIn'
      };
      
      const providerType = providerMapping[provider.toLowerCase()];
      if (!providerType) {
        throw new Error(`Unsupported OAuth provider: ${provider}`);
      }

      // Validate required OAuth fields
      if (!providerId) {
        throw new Error('Missing OAuth provider ID');
      }
      if (!accessToken) {
        throw new Error('Missing OAuth access token');
      }

      // Register the user with OAuth provider
      await registerOAuthUser({
        email: userEmail,
        firstName: userData.firstName,
        lastName: userData.lastName,
        professionalTitle: userData.professionalTitle,
        bio: userData.bio,
        location: userData.location,
        profileImage: userImage,
        provider: providerType,
        providerId: providerId,
        providerEmail: userEmail,
        accessToken: accessToken,
        refreshToken: refreshToken || undefined,
        tokenExpiresAt: tokenExpiresAt || undefined,
      });

      // Close modal
      setIsModalOpen(false);
      
      await signIn(provider, { 
        callbackUrl: '/',
        redirect: true 
      });

    } catch (error) {
      console.error('Registration error:', error);
      setError(error instanceof Error ? error.message : 'Registration failed. Please try again.');
      setIsLoading(false);
    }
  };

  if (!userEmail || !provider || !providerId) {
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