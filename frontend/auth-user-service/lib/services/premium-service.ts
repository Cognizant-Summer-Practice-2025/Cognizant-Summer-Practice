import { loadStripe } from '@stripe/stripe-js';
import { Stripe } from '@stripe/stripe-js';

const STRIPE_PUBLISHABLE_KEY = process.env.NEXT_PUBLIC_STRIPE_PUBLISHABLE_KEY || 'pk_test_your_stripe_publishable_key_here';

export interface PremiumStatus {
  isPremium: boolean;
}

export interface CheckoutSessionResponse {
  checkoutUrl: string;
}

export class PremiumService {
  private static instance: PremiumService;
  private stripe: Stripe | null = null;

  private constructor() {
    this.initializeStripe();
  }

  public static getInstance(): PremiumService {
    if (!PremiumService.instance) {
      PremiumService.instance = new PremiumService();
    }
    return PremiumService.instance;
  }

  private async initializeStripe() {
    try {
      this.stripe = await loadStripe(STRIPE_PUBLISHABLE_KEY);
    } catch (error) {
      console.error('Failed to initialize Stripe:', error);
    }
  }

  async checkPremiumStatus(): Promise<PremiumStatus> {
    try {
      const response = await fetch('/api/premium/status', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      if (!response.ok) {
        throw new Error('Failed to check premium status');
      }

      return await response.json();
    } catch (error) {
      console.error('Error checking premium status:', error);
      return { isPremium: false };
    }
  }

  async createCheckoutSession(successUrl: string, cancelUrl: string): Promise<string> {
    console.log('🔍 [DEBUG] PremiumService.createCheckoutSession called with:', { successUrl, cancelUrl });
    
    try {
      const response = await fetch('/api/premium/create-checkout-session', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          successUrl,
          cancelUrl,
        }),
      });

      console.log('🔍 [DEBUG] Frontend API response:', {
        status: response.status,
        statusText: response.statusText,
        ok: response.ok
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('❌ [DEBUG] Frontend API error response:', errorText);
        throw new Error(`Failed to create checkout session: ${response.status} ${response.statusText}`);
      }

      const data: { checkoutUrl?: string } = await response.json();
      console.log('✅ [DEBUG] Frontend API success response:', data);
      
      if (!data.checkoutUrl) {
        throw new Error('No checkout URL received from API');
      }

      return data.checkoutUrl;
    } catch (error) {
      console.error('❌ [DEBUG] Error in PremiumService.createCheckoutSession:', error);
      throw error;
    }
  }

  async cancelSubscription(): Promise<boolean> {
    try {
      const response = await fetch('/api/premium/cancel', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      return response.ok;
    } catch (error) {
      console.error('Error cancelling subscription:', error);
      return false;
    }
  }

  redirectToStripe(checkoutUrl: string): void {
    if (this.stripe) {
      window.location.href = checkoutUrl;
    } else {
      console.error('Stripe not initialized');
    }
  }
}

export const premiumService = PremiumService.getInstance();
