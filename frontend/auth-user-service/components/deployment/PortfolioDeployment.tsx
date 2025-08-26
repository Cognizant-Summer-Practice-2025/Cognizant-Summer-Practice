'use client';

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Loader2, Rocket, AlertCircle } from 'lucide-react';
import { useSession } from 'next-auth/react';
import { deployPortfolio, DeploymentRequest } from '@/lib/deployment/api';

interface DeploymentState {
  loading: boolean;
  error: string | null;
  deploymentUrl: string | null;
  deploymentId: string | null;
}



interface PortfolioDeploymentProps {
  templateName?: string;
  portfolioId?: string;
  projectName?: string;
  customDomain?: string;
  environment?: 'Development' | 'Staging' | 'Production';
  onDeploymentComplete?: (deploymentUrl: string) => void;
  onError?: (error: string) => void;
}

export default function PortfolioDeployment({
  templateName = 'creative',
  portfolioId,
  projectName,
  customDomain,
  environment = 'Production',
  onDeploymentComplete,
  onError
}: PortfolioDeploymentProps) {
  const { data: session } = useSession();
  const [state, setState] = useState<DeploymentState>({
    loading: false,
    error: null,
    deploymentUrl: null,
    deploymentId: null
  });

  const getUserId = (): string => {
    // Try to get user ID from session - check different possible properties
    if (session?.user) {
      // Try common user ID properties
      const user = session.user as Record<string, unknown>;
      if (typeof user.id === 'string') return user.id;
      if (typeof user.sub === 'string') return user.sub;
      if (typeof user.userId === 'string') return user.userId;
    }
    
    // Fallback for testing - this should be removed in production
    console.warn('No user ID found in session, using fallback');
    return '00000000-0000-0000-0000-000000000001';
  };

  const handleDeployPortfolio = async () => {
    if (!session) {
      setState(prev => ({
        ...prev,
        error: 'Please sign in to deploy your portfolio'
      }));
      return;
    }

    setState(prev => ({
      ...prev,
      loading: true,
      error: null,
      deploymentUrl: null,
      deploymentId: null
    }));

    try {
      const deploymentRequest: DeploymentRequest = {
        userId: getUserId(),
        portfolioId,
        templateName,
        includeAllComponents: true,
        projectName,
        customDomain,
        environment
      };

      console.log('🚀 Starting portfolio deployment:', deploymentRequest);

      // Call the deployment API using the API module
      const response = await deployPortfolio(deploymentRequest);

      console.log('✅ Deployment response:', response);

      if (response.status === 'Failed') {
        throw new Error(response.errorMessage || 'Deployment failed');
      }

      setState(prev => ({
        ...prev,
        loading: false,
        deploymentUrl: response.deploymentUrl,
        deploymentId: response.deploymentId
      }));

      // Call success callback
      if (onDeploymentComplete && response.deploymentUrl) {
        onDeploymentComplete(response.deploymentUrl);
      }

    } catch (error) {
      console.error('❌ Deployment error:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to deploy portfolio. Please try again.';
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage
      }));

      // Call error callback
      if (onError) {
        onError(errorMessage);
      }
    }
  };

  const handleVisitPortfolio = () => {
    if (state.deploymentUrl) {
      window.open(state.deploymentUrl, '_blank', 'noopener,noreferrer');
    }
  };

  const handleCopyUrl = async () => {
    if (state.deploymentUrl) {
      try {
        await navigator.clipboard.writeText(state.deploymentUrl);
        console.log('📋 URL copied to clipboard');
      } catch (error) {
        console.error('Failed to copy URL:', error);
      }
    }
  };

  if (state.loading) {
    return (
      <div className="py-12 text-center">
        <div className="flex items-center gap-3 text-gray-600 justify-center">
          <Loader2 className="w-6 h-6 animate-spin" />
          <span className="text-lg">Deploying Your Portfolio to Vercel...</span>
        </div>
        <div className="mt-4 text-sm text-gray-500">
          Creating your portfolio site and deploying it to the web
        </div>
      </div>
    );
  }

  if (state.error) {
    return (
      <div className="py-6">
        <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
          <div className="flex items-center gap-2 text-red-800">
            <AlertCircle className="w-5 h-5" />
            <span className="font-medium">Deployment Failed</span>
          </div>
          <p className="text-red-700 mt-1">{state.error}</p>
          <Button
            onClick={handleDeployPortfolio}
            className="mt-4 bg-red-600 hover:bg-red-700 text-white"
            size="sm"
          >
            Try Again
          </Button>
        </div>
      </div>
    );
  }

  if (state.deploymentUrl) {
    return (
      <div className="bg-white rounded-lg shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
            <Rocket className="w-5 h-5 text-green-600" />
            Portfolio Successfully Deployed!
          </h2>
          <p className="text-gray-600 mt-1">
            Your portfolio is now live on the web
          </p>
        </div>
        
        <div className="p-6">
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-2">
              <Rocket className="w-5 h-5 text-green-600" />
              <span className="font-medium text-green-900">Deployment URL</span>
            </div>
            <a 
              href={state.deploymentUrl} 
              target="_blank" 
              rel="noopener noreferrer"
              className="text-blue-600 hover:text-blue-800 underline break-all"
            >
              {state.deploymentUrl}
            </a>
            <div className="mt-4 flex gap-2">
              <Button
                onClick={handleVisitPortfolio}
                className="bg-green-600 hover:bg-green-700 text-white"
              >
                Visit Portfolio
              </Button>
              <Button
                onClick={handleCopyUrl}
                variant="outline"
              >
                Copy URL
              </Button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Default state - deploy button
  return (
    <div className="py-6">
      <Button
        onClick={handleDeployPortfolio}
        disabled={state.loading || !session}
        className="px-8 py-4 bg-purple-600 hover:bg-purple-700 text-white text-lg font-medium rounded-lg flex items-center gap-3 shadow-md hover:shadow-lg transition-all duration-200"
        size="lg"
      >
        {state.loading ? (
          <>
            <Loader2 className="w-6 h-6 animate-spin" />
            Deploying Portfolio...
          </>
        ) : (
          <>
            <Rocket className="w-6 h-6" />
            Deploy Portfolio
          </>
        )}
      </Button>
      
      {!session && (
        <p className="text-sm text-gray-500 mt-2">
          Please sign in to deploy your portfolio
        </p>
      )}
      
      <div className="mt-4 text-sm text-gray-600">
        <p><strong>Template:</strong> {templateName}</p>
        <p><strong>Environment:</strong> {environment}</p>
        {customDomain && <p><strong>Custom Domain:</strong> {customDomain}</p>}
      </div>
    </div>
  );
}
