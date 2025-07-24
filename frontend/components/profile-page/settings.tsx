"use client"

import { useState, useEffect } from 'react';
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ComponentOrdering } from "@/components/ui/component-ordering"
import { ComponentConfig } from '@/lib/portfolio';
import { TemplateManager } from '@/lib/template-manager';
import { usePortfolio } from '@/lib/contexts/portfolio-context';
import { Loading } from '@/components/loader';
import { updatePortfolio } from '@/lib/portfolio/api';

interface SettingsProps {
  portfolioId?: string;
  initialData?: {
    portfolio?: {
      visibility?: number;
      templateId?: string;
      components?: ComponentConfig[];
    };
    allowMessages?: boolean;
    emailNotifications?: boolean;
  };
  onSave?: (data: {
    portfolio: {
      visibility: number;
      components: string;
    };
    preferences: {
      allowMessages: boolean;
      emailNotifications: boolean;
    };
  }) => void;
  readOnly?: boolean;
}

export default function Settings({ portfolioId: propPortfolioId, initialData, onSave, readOnly = false }: SettingsProps = {}) {
  const { userPortfolioData, refreshUserPortfolios, loading: portfolioLoading } = usePortfolio();
  
  // Get the current portfolio from context or use provided portfolioId
  const currentPortfolio = userPortfolioData?.portfolios?.[0]; // Get first portfolio for now
  const portfolioId = propPortfolioId || currentPortfolio?.id;
  
  const [visibility, setVisibility] = useState(initialData?.portfolio?.visibility ?? currentPortfolio?.visibility ?? 0);
  const [allowMessages, setAllowMessages] = useState(initialData?.allowMessages ?? true);
  const [emailNotifications, setEmailNotifications] = useState(initialData?.emailNotifications ?? true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  
  // Use provided component configuration or from portfolio or create default
  const [components, setComponents] = useState<ComponentConfig[]>(() => {
    if (initialData?.portfolio?.components) {
      return initialData.portfolio.components;
    }
    if (currentPortfolio?.components) {
      try {
        return JSON.parse(currentPortfolio.components);
      } catch {
        return TemplateManager.createDefaultComponentConfig();
      }
    }
    return TemplateManager.createDefaultComponentConfig();
  });

  const visibilityOptions = [
    { value: 0, label: "Public" },
    { value: 1, label: "Private" }, 
    { value: 2, label: "Unlisted" }
  ];

  // Update state when portfolio data changes
  useEffect(() => {
    if (currentPortfolio && !initialData) {
      setVisibility(currentPortfolio.visibility);
      if (currentPortfolio.components) {
        try {
          const parsedComponents = JSON.parse(currentPortfolio.components);
          setComponents(parsedComponents);
        } catch {
          setComponents(TemplateManager.createDefaultComponentConfig());
        }
      }
    }
  }, [currentPortfolio, initialData]);

  const handleSave = async () => {
    if (!portfolioId && !onSave) {
      setError('No portfolio found to update');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(false);

      const dataToSave = {
        portfolio: {
          visibility,
          components: JSON.stringify(components)
        },
        preferences: {
          allowMessages,
          emailNotifications
        }
      };

      if (onSave) {
        await onSave(dataToSave);
      } else if (portfolioId) {
        await updatePortfolio(portfolioId, {
          visibility: visibility as 0 | 1 | 2,
          components: JSON.stringify(components)
        });
        
        // Refresh portfolio data to get updated values
        await refreshUserPortfolios();
        setSuccess(true);
        setTimeout(() => setSuccess(false), 3000);
      }
    } catch (err) {
      console.error('Error saving settings:', err);
      const errorMessage = err instanceof Error ? err.message : 'Failed to save settings';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };



  if ((portfolioLoading || loading) && !initialData && !currentPortfolio) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full h-full overflow-y-auto">
        <h2 className="text-2xl font-bold mb-4">Settings</h2>
        <div className="flex flex-col items-center justify-center py-8">
          <Loading className="scale-50" backgroundColor="white" />
          <span className="mt-4 text-gray-600">Loading settings...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm w-full">
      <div className="p-4 sm:p-6 lg:p-8">
        <div className="space-y-6">
          <h2 className="text-2xl font-bold mb-4">Settings</h2>
      
      {/* Success Message */}
      {success && (
        <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-sm text-green-600">Settings saved successfully!</p>
        </div>
      )}
      
      {/* Error Message */}
      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{error}</p>
          <button
            onClick={() => setError(null)}
            className="ml-2 text-red-500 hover:text-red-700"
          >
            ×
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Portfolio Settings */}
        <div className="p-6 rounded-lg border border-gray-200 bg-gray-50">
          <h3 className="text-lg font-medium mb-4">Portfolio Settings</h3>
          
          <div className="space-y-4">
            <div>
              <Label htmlFor="visibility">Portfolio Visibility</Label>
              <Select 
                value={visibility.toString()} 
                onValueChange={(value) => setVisibility(parseInt(value))}
                disabled={readOnly || loading}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {visibilityOptions.map(option => (
                    <SelectItem key={option.value} value={option.value.toString()}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <p className="text-sm text-gray-500 mt-1">
                {visibility === 0 && "Anyone can view your portfolio"}
                {visibility === 1 && "Only you can view your portfolio"}
                {visibility === 2 && "Only people with the link can view"}
              </p>
              </div>
            </div>
          </div>

        {/* Notification Settings */}
        <div className="p-6 rounded-lg border border-gray-200 bg-gray-50">
          <h3 className="text-lg font-medium mb-4">Notification Settings</h3>
            
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="allow-messages">Allow Messages</Label>
                <p className="text-sm text-gray-500">
                  Allow others to send you messages through your portfolio
                </p>
              </div>
              <input
                id="allow-messages"
                type="checkbox"
                checked={allowMessages}
                onChange={(e) => setAllowMessages(e.target.checked)}
                disabled={readOnly || loading}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>
            
            <div className="flex items-center justify-between">
              <div>
                <Label htmlFor="email-notifications">Email Notifications</Label>
                <p className="text-sm text-gray-500">
                  Receive email notifications for new messages and comments
                </p>
              </div>
              <input
                id="email-notifications"
                type="checkbox"
                checked={emailNotifications}
                onChange={(e) => setEmailNotifications(e.target.checked)}
                disabled={readOnly || loading}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
              />
            </div>
          </div>
        </div>
      </div>

      {/* Component Configuration */}
      <div className="p-6 rounded-lg border border-gray-200 bg-gray-50">
        <h3 className="text-lg font-medium mb-4">Portfolio Components</h3>
        <p className="text-sm text-gray-500 mb-4">
          Configure which sections appear in your portfolio and their order
        </p>
        
        <ComponentOrdering
          components={components}
          onComponentsChange={setComponents}
        />
      </div>

      {!readOnly && (
        <div className="flex justify-end space-x-3 pt-4">
          <Button 
            variant="outline" 
            disabled={loading}
            onClick={() => {
              // Reset to current portfolio values or initial values
              const resetVisibility = currentPortfolio?.visibility ?? initialData?.portfolio?.visibility ?? 0;
              const resetComponents = (() => {
                if (currentPortfolio?.components) {
                  try {
                    return JSON.parse(currentPortfolio.components);
                  } catch {
                    return TemplateManager.createDefaultComponentConfig();
                  }
                }
                return initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig();
              })();
              
              setVisibility(resetVisibility);
              setAllowMessages(initialData?.allowMessages ?? true);
              setEmailNotifications(initialData?.emailNotifications ?? true);
              setComponents(resetComponents);
              setError(null);
              setSuccess(false);
            }}
          >
            Reset
          </Button>
          <Button 
            onClick={handleSave}
            disabled={loading || !portfolioId}
            className="bg-app-blue hover:bg-app-blue-hover text-white"
          >
            {loading ? 'Saving...' : 'Save Settings'}
          </Button>
        </div>
      )}
        </div>
      </div>
    </div>
  );
} 