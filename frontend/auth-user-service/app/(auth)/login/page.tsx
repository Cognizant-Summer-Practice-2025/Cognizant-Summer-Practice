'use client';

import React, { Suspense, useState } from 'react';
import Image from 'next/image';
import { useSession, signIn } from 'next-auth/react';
import { useRouter, useSearchParams } from 'next/navigation';
import RegistrationModal from '@/components/auth/registration-modal';
import { registerOAuthUser } from '@/lib/user';
import './login.css';

// OAuth Button Component
interface OAuthButtonProps {
  provider: string;
  callbackUrl?: string;
}

const OAuthButton: React.FC<OAuthButtonProps> = ({ provider, callbackUrl }) => {
  const providerConfig = {
    google: {
      name: 'Google',
      icon: '/icons/google-logo.svg'
    },
    github: {
      name: 'GitHub', 
      icon: '/icons/github-logo.svg'
    },
    facebook: {
      name: 'Facebook',
      icon: '/icons/facebook-logo.svg'
    },
    linkedin: {
      name: 'LinkedIn',
      icon: '/icons/linkedin-logo.svg'
    }
  };

  const config = providerConfig[provider as keyof typeof providerConfig];
  
  if (!config) return null;

  const handleSignIn = async () => {
    await signIn(provider, { callbackUrl });
  };

  return (
    <button onClick={handleSignIn} className="oauth-button">
      <Image
        src={config.icon}
        alt={config.name}
        width={24}
        height={24}
        className="oauth-icon"
      />
      <span>Continue with {config.name}</span>
    </button>
  );
};

const LoginContent = () => {
  const { data: session } = useSession();
  const router = useRouter();
  const searchParams = useSearchParams();
  
  // Registration modal state
  const [showRegistrationModal, setShowRegistrationModal] = useState(false);
  const [oauthData, setOauthData] = useState<{
    email: string;
    name: string;
    image: string;
    provider: string;
    providerId: string;
    accessToken: string;
    refreshToken: string;
    tokenExpiresAt: string;
  } | null>(null);
  const [isRegistering, setIsRegistering] = useState(false);

  const callbackUrl = searchParams.get('callbackUrl') || '/';

  // Parse URL parameters for OAuth registration
  React.useEffect(() => {
    const email = searchParams.get('email');
    const name = searchParams.get('name');
    const image = searchParams.get('image');
    const provider = searchParams.get('provider');
    const providerId = searchParams.get('providerId');
    const accessToken = searchParams.get('accessToken');
    const refreshToken = searchParams.get('refreshToken');
    const tokenExpiresAt = searchParams.get('tokenExpiresAt');
    const registerParam = searchParams.get('register');

    // Check for direct register parameters
    if (email && name && provider && providerId && accessToken) {
      setOauthData({
        email,
        name,
        image: image || '',
        provider,
        providerId,
        accessToken,
        refreshToken: refreshToken || '',
        tokenExpiresAt: tokenExpiresAt || ''
      });
      setShowRegistrationModal(true);
    } else if (searchParams.get('error')?.includes('/login?register=')) {
      // Handle error parameter that contains registration data
      try {
        const errorParam = searchParams.get('error');
        if (errorParam) {
          const registerMatch = errorParam.match(/register=(.+)/);
          if (registerMatch) {
            const registerString = decodeURIComponent(registerMatch[1]);
            const params = new URLSearchParams(registerString);

            const registerEmail = params.get('email');
            const registerName = params.get('name');
            const registerImage = params.get('image');
            const registerProvider = params.get('provider');
            const registerProviderId = params.get('providerId');
            const registerAccessToken = params.get('accessToken');
            const registerRefreshToken = params.get('refreshToken');
            const registerTokenExpiresAt = params.get('tokenExpiresAt');

            if (registerEmail && registerName && registerProvider && registerProviderId && registerAccessToken) {
              setOauthData({
                email: registerEmail,
                name: registerName,
                image: registerImage || '',
                provider: registerProvider,
                providerId: registerProviderId,
                accessToken: registerAccessToken,
                refreshToken: registerRefreshToken || '',
                tokenExpiresAt: registerTokenExpiresAt || ''
              });
              setShowRegistrationModal(true);

              // Clean up URL
              router.replace('/login', { scroll: false });
            }
          }
        }
      } catch {
        // Error parsing OAuth error parameters
      }
    } else if (registerParam && registerParam.includes('email=')) {
      // Handle register parameter with OAuth data
      try {
        // The register parameter might be just the start - check the full URL
        const fullUrl = window.location.href;
        const registerIndex = fullUrl.indexOf('register=');
        
        if (registerIndex !== -1) {
          const registerString = decodeURIComponent(fullUrl.substring(registerIndex + 9));
          const params = new URLSearchParams(registerString);

          const registerEmail = params.get('email');
          const registerName = params.get('name');
          const registerImage = params.get('image');
          const registerProvider = params.get('provider');
          const registerProviderId = params.get('providerId');
          const registerAccessToken = params.get('accessToken');
          const registerRefreshToken = params.get('refreshToken');
          const registerTokenExpiresAt = params.get('tokenExpiresAt');

          if (registerEmail && registerName && registerProvider && registerProviderId && registerAccessToken) {
            setOauthData({
              email: registerEmail,
              name: registerName,
              image: registerImage || '',
              provider: registerProvider,
              providerId: registerProviderId,
              accessToken: registerAccessToken,
              refreshToken: registerRefreshToken || '',
              tokenExpiresAt: registerTokenExpiresAt || ''
            });
            setShowRegistrationModal(true);
            
            // Clean up URL
            router.replace('/login', { scroll: false });
          }
        }
      } catch {
        // Error parsing OAuth register parameters
      }
    }
  }, [searchParams, router]);

  // Redirect if already authenticated
  React.useEffect(() => {
    if (session) {
      router.push(callbackUrl);
    }
  }, [session, router, callbackUrl]);

  const handleRegistrationSubmit = async (userData: {
    firstName: string;
    lastName: string;
    professionalTitle: string;
    bio: string;
    location: string;
  }) => {
    if (!oauthData) return;
    
    setIsRegistering(true);
    
    try {
      // Map provider string to enum number
      const providerMap: { [key: string]: number } = {
        'google': 0,
        'github': 1,
        'facebook': 2,
        'linkedin': 3
      };

      const providerNumber = providerMap[oauthData.provider.toLowerCase()] ?? 0;

      // Format token expiration
      let tokenExpiresAtFormatted = undefined;
      if (oauthData.tokenExpiresAt && oauthData.tokenExpiresAt !== '') {
        tokenExpiresAtFormatted = new Date(parseInt(oauthData.tokenExpiresAt) * 1000).toISOString();
      }

      const registrationData = {
        email: oauthData.email,
        firstName: userData.firstName,
        lastName: userData.lastName,
        professionalTitle: userData.professionalTitle,
        bio: userData.bio,
        location: userData.location,
        profileImage: oauthData.image,
        provider: providerNumber,
        providerId: oauthData.providerId,
        providerEmail: oauthData.email,
        accessToken: oauthData.accessToken,
        refreshToken: oauthData.refreshToken || undefined,
        tokenExpiresAt: tokenExpiresAtFormatted
      };

      await registerOAuthUser(registrationData);
      
      setShowRegistrationModal(false);
      setOauthData(null);
      
      // Sign in with the OAuth provider now that user is registered
      await signIn(oauthData.provider, { callbackUrl });
      
    } catch {
      // Registration error - could show an error toast here
    } finally {
      setIsRegistering(false);
    }
  };

  const handleRegistrationClose = () => {
    setShowRegistrationModal(false);
    setOauthData(null);
    // Clear URL parameters
    router.replace('/login');
  };

  return (
    <div className="login-container">
      {/* Hero Section (Desktop Only) */}
      <div className="hero-section desktop-only">
        <div className="hero-logo">
          <Image 
            src="/logo.svg" 
            alt="GoalKeeper Logo" 
            width={60} 
            height={60}
          />
        </div>
        <div className="hero-content">
          <h1 className="hero-title">GoalKeeper</h1>
          <p className="hero-subtitle">
            Showcase your work, connect with professionals, and build your career.
          </p>
          <div className="hero-features">
            <div className="hero-feature">
              <Image
                src="/icons/palette-colors.svg"
                alt="Customizable templates"
                width={24}
                height={24}
                className="hero-feature-icon"
              />
              <span className="hero-feature-text">Customizable portfolio templates</span>
            </div>
            <div className="hero-feature">
              <Image
                src="/icons/user-group.svg"
                alt="Connect with professionals"
                width={24}
                height={24}
                className="hero-feature-icon"
              />
              <span className="hero-feature-text">Connect with other professionals</span>
            </div>
            <div className="hero-feature">
              <Image
                src="/icons/chart-line-up.svg"
                alt="Track performance"
                width={24}
                height={24}
                className="hero-feature-icon"
              />
              <span className="hero-feature-text">Track portfolio performance</span>
            </div>
          </div>
        </div>
      </div>

      {/* Form Section */}
      <div className="form-section">
        <div className="form-container">
          {/* Mobile Header */}
          <div className="mobile-header mobile-only">
            <div className="mobile-logo">
              <Image 
                src="/logo.svg" 
                alt="GoalKeeper Logo" 
                width={64} 
                height={64}
              />
            </div>
            <h1 className="mobile-title">Welcome back</h1>
            <p className="mobile-subtitle">Sign in to your account</p>
          </div>

          {/* Desktop Header */}
          <div className="form-header desktop-only">
            <h1 className="form-title">Welcome back</h1>
            <p className="form-subtitle">Sign in to your account to continue</p>
          </div>

          {/* OAuth Buttons */}
          <div className="oauth-buttons">
            <OAuthButton provider="google" callbackUrl={callbackUrl} />
            <OAuthButton provider="github" callbackUrl={callbackUrl} />
            <OAuthButton provider="facebook" callbackUrl={callbackUrl} />
            <OAuthButton provider="linkedin" callbackUrl={callbackUrl} />
          </div>

          {/* Terms */}
          <div className="terms-section">
            <p className="terms-text">
              By continuing, you agree to our{' '}
              <a href="/terms" className="terms-link">Terms of Service</a> and{' '}
              <a href="/privacy" className="terms-link">Privacy Policy</a>
            </p>
          </div>
        </div>
      </div>

      {/* Registration Modal */}
      {oauthData && (
        <RegistrationModal
          isOpen={showRegistrationModal}
          onClose={handleRegistrationClose}
          onSubmit={handleRegistrationSubmit}
          userEmail={oauthData.email}
          userName={oauthData.name}
          userAvatar={oauthData.image}
          isLoading={isRegistering}
        />
      )}
    </div>
  );
};

const LoginPage = () => {
  return (
    <Suspense fallback={
      <div className="loading-container">
        <div className="loading-text">Loading...</div>
      </div>
    }>
      <LoginContent />
    </Suspense>
  );
};

export default LoginPage;