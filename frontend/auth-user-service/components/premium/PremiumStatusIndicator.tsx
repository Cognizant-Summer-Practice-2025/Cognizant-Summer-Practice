'use client';

import React, { useEffect, useState } from 'react';
import { Crown, X } from 'lucide-react';
import { premiumService } from '@/lib/services/premium-service';

interface PremiumStatusIndicatorProps {
  className?: string;
  showText?: boolean;
}

export default function PremiumStatusIndicator({ 
  className = '', 
  showText = false 
}: PremiumStatusIndicatorProps) {
  const [isPremium, setIsPremium] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    checkPremiumStatus();
  }, []);

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

  if (loading) {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        <div className="w-4 h-4 bg-gray-300 rounded-full animate-pulse"></div>
        {showText && <span className="text-sm text-gray-500">Loading...</span>}
      </div>
    );
  }

  if (isPremium) {
    return (
      <div className={`flex items-center gap-2 ${className}`}>
        <Crown className="w-4 h-4 text-yellow-500 fill-current" />
        {showText && <span className="text-sm text-yellow-600 font-medium">Premium</span>}
      </div>
    );
  }

  return (
    <div className={`flex items-center gap-2 ${className}`}>
      <X className="w-4 h-4 text-gray-400" />
      {showText && <span className="text-sm text-gray-500">Free</span>}
    </div>
  );
}
