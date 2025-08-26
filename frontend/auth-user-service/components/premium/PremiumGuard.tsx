'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useSession } from 'next-auth/react';
import { premiumService } from '@/lib/services/premium-service';
import { Loader2, Crown, Lock } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface PremiumGuardProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export default function PremiumGuard({ children, fallback }: PremiumGuardProps) {
  const { data: session, status } = useSession();
  const router = useRouter();
  const [isPremium, setIsPremium] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (status === 'loading') return;

    if (status === 'unauthenticated') {
      router.push('/login');
      return;
    }

    checkPremiumStatus();
  }, [status, router]);

  const checkPremiumStatus = async () => {
    try {
      setLoading(true);
      const status = await premiumService.checkPremiumStatus();
      setIsPremium(status.isPremium);
    } catch (error) {
      console.error('Error checking premium status:', error);
      setIsPremium(false);
    } finally {
      setLoading(false);
    }
  };

  const handleUpgradeToPremium = async () => {
    try {
      const successUrl = `${window.location.origin}/ai?upgraded=true`;
      const cancelUrl = `${window.location.origin}/ai?upgraded=false`;
      
      const checkoutUrl = await premiumService.createCheckoutSession(successUrl, cancelUrl);
      premiumService.redirectToStripe(checkoutUrl);
    } catch (error) {
      console.error('Error creating checkout session:', error);
    }
  };

  if (status === 'loading' || loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-8 h-8 animate-spin mx-auto mb-4 text-blue-600" />
          <p className="text-gray-600">Checking premium status...</p>
        </div>
      </div>
    );
  }

  if (!session) {
    return null;
  }

  if (isPremium === false) {
    if (fallback) {
      return <>{fallback}</>;
    }

    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md mx-auto text-center p-8">
          <div className="bg-white rounded-lg shadow-lg p-8">
            <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <Lock className="w-8 h-8 text-yellow-600" />
            </div>
            
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
              Premium Access Required
            </h2>
            
            <p className="text-gray-600 mb-6">
              This feature is only available for premium users. Upgrade your account to unlock AI-powered portfolio analysis and tech news summaries.
            </p>
            
            <div className="space-y-3">
              <Button
                onClick={handleUpgradeToPremium}
                className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold py-3 px-6 rounded-lg shadow-lg hover:shadow-xl transition-all duration-200"
                size="lg"
              >
                <Crown className="w-5 h-5 mr-2" />
                Upgrade to Premium
              </Button>
              
              <Button
                onClick={() => router.push('/')}
                variant="outline"
                className="w-full"
                size="lg"
              >
                Back to Home
              </Button>
            </div>
            
            <div className="mt-6 p-4 bg-blue-50 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">Premium Benefits:</h3>
              <ul className="text-sm text-blue-800 space-y-1">
                <li>• AI-powered portfolio recommendations</li>
                <li>• Tech news summaries</li>
                <li>• Advanced analytics</li>
                <li>• Priority support</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}
