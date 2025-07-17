"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { ComponentOrdering } from "@/components/ui/component-ordering"
import { ComponentConfig } from '@/lib/portfolio'
import { TemplateManager } from '@/lib/template-manager'
import { updatePortfolio } from '@/lib/portfolio/api'
import { usePortfolio } from '@/lib/contexts/portfolio-context'

interface PortfolioSettingsProps {
  portfolioId?: string;
  initialData?: {
    portfolio?: {
      visibility?: number;
      templateId?: string;
      components?: ComponentConfig[];
      title?: string;
      bio?: string;
    };
  };
  onSave?: (data: any) => void;
  readOnly?: boolean;
}

export function PortfolioSettings({ portfolioId, initialData, onSave, readOnly = false }: PortfolioSettingsProps = {}) {
  const { getUserPortfolios } = usePortfolio();
  const userPortfolios = getUserPortfolios();
  const currentPortfolioId = portfolioId || userPortfolios[0]?.id;
  const currentPortfolio = userPortfolios.find(p => p.id === currentPortfolioId);
  
  const [visibility, setVisibility] = useState(initialData?.portfolio?.visibility ?? currentPortfolio?.visibility ?? 0);
  const [template, setTemplate] = useState(initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu");
  const [title, setTitle] = useState(initialData?.portfolio?.title ?? currentPortfolio?.title ?? "");
  const [bio, setBio] = useState(initialData?.portfolio?.bio ?? currentPortfolio?.bio ?? "");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Available templates from the portfolio-templates folder
  const availableTemplates = [
    { id: 'gabriel-barzu', name: 'Gabriel Barzu', componentName: 'gabriel-barzu', description: 'Professional template with sidebar layout' },
    { id: 'modern', name: 'Modern', componentName: 'modern', description: 'Clean and modern design' },
    { id: 'creative', name: 'Creative', componentName: 'creative', description: 'Creative and colorful layout' },
    { id: 'professional', name: 'Professional', componentName: 'professional', description: 'Classic professional design' }
  ];
  
  // Use provided component configuration or create default
  const [components, setComponents] = useState<ComponentConfig[]>(
    initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig()
  );

  const handleSave = async () => {
    if (!currentPortfolioId && !onSave) {
      setError("No portfolio found to save settings.");
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const dataToSave = {
        templateId: template,
        title: title.trim(),
        bio: bio.trim(),
        visibility
      };

      if (onSave) {
        await onSave(dataToSave);
      } else if (currentPortfolioId) {
        await updatePortfolio(currentPortfolioId, dataToSave);
      }
    } catch (err) {
      console.error('Error saving portfolio settings:', err);
      setError('Failed to save portfolio settings');
    } finally {
      setLoading(false);
    }
  };

  const updateComponentVisibility = (componentId: string, isVisible: boolean) => {
    setComponents(components.map(comp => 
      comp.id === componentId 
        ? { ...comp, isVisible }
        : comp
    ));
  };

  const reorderComponents = (startIndex: number, endIndex: number) => {
    const result = Array.from(components);
    const [removed] = result.splice(startIndex, 1);
    result.splice(endIndex, 0, removed);

    // Update order values
    const reorderedComponents = result.map((comp, index) => ({
      ...comp,
      order: index + 1
    }));

    setComponents(reorderedComponents);
  };

  const visibilityOptions = [
    { value: 0, label: "Public - Anyone can view" },
    { value: 1, label: "Private - Only you can view" },
    { value: 2, label: "Unlisted - Only with link" }
  ];

  if (loading && !initialData) {
    return (
      <div className="space-y-6">
        <h2 className="text-xl font-semibold mb-4">Portfolio Settings</h2>
        <div className="flex items-center justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
          <span className="ml-2">Loading settings...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-semibold mb-4">Portfolio Settings</h2>
      
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
        {/* Basic Settings */}
        <div className="bg-white p-6 rounded-lg border space-y-4">
          <h3 className="text-lg font-medium mb-4">Basic Information</h3>
          
          <div>
            <Label htmlFor="title">Portfolio Title</Label>
            <Input
              id="title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="My Portfolio"
              disabled={readOnly || loading}
            />
          </div>
          
          <div>
            <Label htmlFor="bio">Portfolio Description</Label>
            <Textarea
              id="bio"
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              placeholder="Tell visitors about your portfolio..."
              className="min-h-[100px]"
              disabled={readOnly || loading}
            />
          </div>
          
          <div>
            <Label htmlFor="visibility">Visibility</Label>
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
          </div>
        </div>

        {/* Template Settings */}
        <div className="bg-white p-6 rounded-lg border space-y-4">
          <h3 className="text-lg font-medium mb-4">Template Selection</h3>
          
          <div>
            <Label htmlFor="template">Choose Template</Label>
            <Select 
              value={template} 
              onValueChange={setTemplate}
              disabled={readOnly || loading}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {availableTemplates.map(tmpl => (
                  <SelectItem key={tmpl.id} value={tmpl.componentName}>
                    <div className="flex flex-col">
                      <span className="font-medium">{tmpl.name}</span>
                      <span className="text-xs text-gray-500">{tmpl.description}</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <p className="text-xs text-gray-500 mt-1">
              Select from our collection of professionally designed templates
            </p>
          </div>
          
          {/* Custom CSS temporarily disabled */}
          <div className="opacity-50 pointer-events-none">
            <Label htmlFor="custom-css">Custom CSS (Coming Soon)</Label>
            <Textarea
              id="custom-css"
              value=""
              placeholder="/* Custom CSS will be available in a future update */"
              className="min-h-[100px] font-mono text-sm"
              disabled={true}
            />
            <p className="text-xs text-gray-400 mt-1">
              Custom CSS functionality is temporarily disabled and will be available in a future update
            </p>
          </div>
        </div>
      </div>

      {/* Component Configuration */}
      <div className="bg-white p-6 rounded-lg border">
        <h3 className="text-lg font-medium mb-4">Portfolio Sections</h3>
        <p className="text-sm text-gray-500 mb-4">
          Configure which sections appear in your portfolio and their display order
        </p>
        
        <ComponentOrdering
          components={components}
          onReorder={reorderComponents}
          onToggleVisibility={updateComponentVisibility}
          disabled={readOnly || loading}
        />
      </div>

      {!readOnly && (
        <div className="flex justify-end space-x-3 pt-4">
          <Button 
            variant="outline" 
            disabled={loading}
            onClick={() => {
              // Reset to initial values
              setVisibility(initialData?.portfolio?.visibility ?? currentPortfolio?.visibility ?? 0);
              setTemplate(initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu");
              setTitle(initialData?.portfolio?.title ?? currentPortfolio?.title ?? "");
              setBio(initialData?.portfolio?.bio ?? currentPortfolio?.bio ?? "");
              setComponents(initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig());
            }}
          >
            Reset
          </Button>
          <Button 
            onClick={handleSave}
            disabled={loading || (!currentPortfolioId && !onSave)}
          >
            {loading ? 'Saving...' : 'Save Changes'}
          </Button>
        </div>
      )}
    </div>
  )
} 