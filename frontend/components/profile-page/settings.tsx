"use client"

import { useState, useEffect } from 'react';
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ComponentOrdering } from "@/components/ui/component-ordering"
import { ComponentConfig } from '@/lib/portfolio';
import { TemplateManager } from '@/lib/template-manager';

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
  onSave?: (data: any) => void;
  readOnly?: boolean;
}

export default function Settings({ portfolioId, initialData, onSave, readOnly = false }: SettingsProps = {}) {
  const [visibility, setVisibility] = useState(initialData?.portfolio?.visibility ?? 0);
  const [allowMessages, setAllowMessages] = useState(initialData?.allowMessages ?? true);
  const [emailNotifications, setEmailNotifications] = useState(initialData?.emailNotifications ?? true);
  const [showVisibilityDropdown, setShowVisibilityDropdown] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Use provided component configuration or create default
  const [components, setComponents] = useState<ComponentConfig[]>(
    initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig()
  );

  const visibilityOptions = [
    { value: 0, label: "Public" },
    { value: 1, label: "Private" }, 
    { value: 2, label: "Unlisted" }
  ];

  const handleSave = async () => {
    if (!portfolioId && !onSave) return;

    try {
      setLoading(true);
      setError(null);

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
        // Implement API call to save portfolio settings
        const { updatePortfolio } = await import('@/lib/portfolio/api');
        await updatePortfolio(portfolioId, {
          visibility,
          components: JSON.stringify(components)
        });
        console.log('Settings saved successfully');
      }
    } catch (err) {
      console.error('Error saving settings:', err);
      setError('Failed to save settings');
    } finally {
      setLoading(false);
    }
  };



  if (loading && !initialData) {
  return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold mb-4">Settings</h2>
        <div className="flex items-center justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
          <span className="ml-2">Loading settings...</span>
        </div>
            </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold mb-4">Settings</h2>
      
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
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
        <div className="bg-white p-6 rounded-lg border">
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
        <div className="bg-white p-6 rounded-lg border">
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
      <div className="bg-white p-6 rounded-lg border">
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
              // Reset to initial values
              setVisibility(initialData?.portfolio?.visibility ?? 0);
              setAllowMessages(initialData?.allowMessages ?? true);
              setEmailNotifications(initialData?.emailNotifications ?? true);
              setComponents(initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig());
            }}
          >
            Reset
          </Button>
          <Button 
            onClick={handleSave}
            disabled={loading || !portfolioId}
          >
            {loading ? 'Saving...' : 'Save Settings'}
          </Button>
        </div>
      )}
    </div>
  );
} 