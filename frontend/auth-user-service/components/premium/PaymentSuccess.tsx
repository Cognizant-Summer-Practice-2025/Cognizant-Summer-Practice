'use client';

import React, { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { CheckCircle, Crown, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { premiumService } from '@/lib/services/premium-service';

export default function PaymentSuccess() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isPremium, setIsPremium] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const upgraded = searchParams.get('upgraded');
    
    if (upgraded === 'true') {
      // Wait a moment for Stripe webhook to process, then check status
      setTimeout(() => {
        checkPremiumStatus();
      }, 2000);
    } else if (upgraded === 'false') {
      // User cancelled payment
      router.push('/ai');
    }
  }, [searchParams, router]);

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

  const handleContinueToAI = () => {
    router.push('/ai');
  };

  const handleGoHome = () => {
    router.push('/');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <Crown className="w-8 h-8 text-blue-600 animate-pulse" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-4">
            Processing Your Upgrade...
          </h2>
          <p className="text-gray-600">
            Please wait while we activate your premium features.
          </p>
        </div>
      </div>
    );
  }

  if (isPremium) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="max-w-md mx-auto text-center p-8">
          <div className="bg-white rounded-lg shadow-lg p-8">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <CheckCircle className="w-8 h-8 text-green-600" />
            </div>
            
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
              Welcome to Premium! 🎉
            </h2>
            
            <p className="text-gray-600 mb-6">
              Your account has been successfully upgraded. You now have access to all premium features including AI-powered portfolio analysis and tech news summaries.
            </p>
            
            <div className="space-y-3">
              <Button
                onClick={handleContinueToAI}
                className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold py-3 px-6 rounded-lg shadow-lg hover:shadow-xl transition-all duration-200"
                size="lg"
              >
                <ArrowRight className="w-5 h-5 mr-2" />
                Start Using AI Features
              </Button>
              
              <Button
                onClick={handleGoHome}
                variant="outline"
                className="w-full"
                size="lg"
              >
                Back to Home
              </Button>
            </div>
            
            <div className="mt-6 p-4 bg-green-50 rounded-lg">
              <h3 className="font-semibold text-green-900 mb-2">Your Premium Benefits:</h3>
              <ul className="text-sm text-green-800 space-y-1">
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

  // If payment was successful but premium status is not active yet
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="max-w-md mx-auto text-center p-8">
        <div className="bg-white rounded-lg shadow-lg p-8">
          <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-6">
            <Crown className="w-8 h-8 text-yellow-600" />
          </div>
          
          <h2 className="text-2xl font-bold text-gray-900 mb-4">
            Payment Received!
          </h2>
          
          <p className="text-gray-600 mb-6">
            Thank you for your payment! Your premium features are being activated. This usually takes just a few moments.
          </p>
          
          <div className="space-y-3">
            <Button
              onClick={checkPremiumStatus}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 px-6 rounded-lg"
              size="lg"
            >
              Check Status Again
            </Button>
            
            <Button
              onClick={handleGoHome}
              variant="outline"
              className="w-full"
              size="lg"
            >
              Go to Home
            </Button>
          </div>
          
          <p className="text-sm text-gray-500 mt-4">
            If you continue to experience issues, please contact support.
          </p>
        </div>
      </div>
    </div>
  );
}
