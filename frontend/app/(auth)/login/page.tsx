'use client';

import React from 'react';
import Image from 'next/image';
import { Button } from '@/components/ui/button';
import { signIn, useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';

const LoginPage = () => {
  const { data: session } = useSession();
  const router = useRouter();

  // Redirect if already authenticated
  React.useEffect(() => {
    if (session) {
      router.push('/profile');
    }
  }, [session, router]);

  const handleGitHubSignIn = async () => {
    await signIn('github', { callbackUrl: '/profile' });
  };

  const handleGoogleSignIn = async () => {
    await signIn('google', { callbackUrl: '/profile' });
  };

  return (
    <div className="w-[400px] max-w-[400px] flex flex-col gap-8">
      {/* Header Section */}
      <div className="flex flex-col gap-2">
        <div className="flex flex-col items-center">
          <h1 className="text-center text-slate-900 text-3xl font-semibold">
            Welcome back
          </h1>
        </div>
        <div className="flex flex-col items-center pb-1">
          <p className="text-center text-slate-600 text-base">
            Sign in with your preferred account to continue
          </p>
        </div>
      </div>

      {/* Social Login Buttons */}
      <div className="flex flex-col gap-4">
        <Button
          onClick={handleGoogleSignIn}
          variant="outline"
          className="w-full py-4 bg-white border border-slate-200 rounded-lg flex items-center justify-center gap-4 hover:bg-gray-50"
        >
          <Image
            src="/icons/google-logo.svg"
            alt="Google"
            width={20}
            height={20}
            className="w-5 h-5"
          />
          <span className="text-slate-900 text-sm">Continue with Google</span>
        </Button>

        <Button
          onClick={handleGitHubSignIn}
          variant="outline"
          className="w-full py-4 bg-white border border-slate-200 rounded-lg flex items-center justify-center gap-4 hover:bg-gray-50"
        >
          <Image
            src="/icons/github-logo.svg"
            alt="GitHub"
            width={25}
            height={25}
            className="w-6 h-6"
          />
          <span className="text-slate-900 text-sm">Continue with GitHub</span>
        </Button>

        <Button
          variant="outline"
          className="w-full py-4 bg-white border border-slate-200 rounded-lg flex items-center justify-center gap-4 hover:bg-gray-50"
        >
          <Image
            src="/icons/linkedin-logo.svg"
            alt="LinkedIn"
            width={20}
            height={20}
            className="w-5 h-5"
          />
          <span className="text-slate-900 text-sm">Continue with LinkedIn</span>
        </Button>

        <Button
          variant="outline"
          className="w-full py-4 bg-white border border-slate-200 rounded-lg flex items-center justify-center gap-4 hover:bg-gray-50"
        >
          <Image
            src="/icons/facebook-logo.svg"
            alt="Facebook"
            width={20}
            height={20}
            className="w-5 h-5"
          />
          <span className="text-slate-900 text-sm">Continue with Facebook</span>
        </Button>
      </div>

      {/* Terms and Privacy */}
      <div className="text-center text-sm text-slate-600">
        <p>
          By continuing, you agree to our{' '}
          <button className="text-blue-600 hover:underline">
            Terms of Service
          </button>
          {' '}and{' '}
          <button className="text-blue-600 hover:underline">
            Privacy Policy
          </button>
        </p>
      </div>
    </div>
  );
};

export default LoginPage;