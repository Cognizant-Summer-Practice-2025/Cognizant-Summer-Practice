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
  onSave?: (data: {
    templateName: string;
    title: string;
    bio: string;
    visibility: 0 | 1 | 2;
    components: string;
  }) => Promise<void>;
  readOnly?: boolean;
}

export function PortfolioSettings({ portfolioId, initialData, onSave, readOnly = false }: PortfolioSettingsProps = {}) {
  const { getUserPortfolios } = usePortfolio();
  const userPortfolios = getUserPortfolios();
  const currentPortfolioId = portfolioId || userPortfolios[0]?.id;
  const currentPortfolio = userPortfolios.find(p => p.id === currentPortfolioId);
  

  
  // Initial values that will be updated when portfolio data changes
  const [initialVisibility, setInitialVisibility] = useState(initialData?.portfolio?.visibility ?? currentPortfolio?.visibility ?? 0);
  const [initialTemplate, setInitialTemplate] = useState(initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu");
  const [initialTitle, setInitialTitle] = useState(initialData?.portfolio?.title ?? currentPortfolio?.title ?? "");
  const [initialBio, setInitialBio] = useState(initialData?.portfolio?.bio ?? currentPortfolio?.bio ?? "");
  const [initialComponents, setInitialComponents] = useState<ComponentConfig[]>(initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig());
  
  // Current form state
  const [visibility, setVisibility] = useState(initialVisibility);
  const [template, setTemplate] = useState(initialTemplate);
  const [title, setTitle] = useState(initialTitle);
  const [bio, setBio] = useState(initialBio);
  const [components, setComponents] = useState<ComponentConfig[]>(initialComponents);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  

  
  // Update initial values when portfolio data changes
  useEffect(() => {
    const newVisibility = initialData?.portfolio?.visibility ?? currentPortfolio?.visibility ?? 0;
    const newTemplate = initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu";
    const newTitle = initialData?.portfolio?.title ?? currentPortfolio?.title ?? "";
    const newBio = initialData?.portfolio?.bio ?? currentPortfolio?.bio ?? "";
    const newComponents = initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig();
    
    // Only update if the values have actually changed to prevent unnecessary re-renders
    if (newVisibility !== initialVisibility) {
      setInitialVisibility(newVisibility);
      setVisibility(newVisibility);
    }
    if (newTemplate !== initialTemplate) {
      setInitialTemplate(newTemplate);
      setTemplate(newTemplate);
    }
    if (newTitle !== initialTitle) {
      setInitialTitle(newTitle);
      setTitle(newTitle);
    }
    if (newBio !== initialBio) {
      setInitialBio(newBio);
      setBio(newBio);
    }
    if (JSON.stringify(newComponents) !== JSON.stringify(initialComponents)) {
      setInitialComponents(newComponents);
      setComponents(newComponents);
    }
  }, [currentPortfolio, initialData, initialVisibility, initialTemplate, initialTitle, initialBio, initialComponents]);
  
  // Available templates from the portfolio-templates folder
  const availableTemplates = [
    { id: 'gabriel-barzu', name: 'Gabriel Bârzu', description: 'Professional template with sidebar layout' },
    { id: 'modern', name: 'Modern', description: 'Clean and modern design' },
    { id: 'creative', name: 'Creative', description: 'Creative and colorful layout' },
    { id: 'professional', name: 'Professional', description: 'Classic professional design' }
  ];

  // Function to check if there are any changes
  const hasChanges = () => {
    return (
      visibility !== initialVisibility ||
      template !== initialTemplate ||
      title.trim() !== initialTitle ||
      bio.trim() !== initialBio ||
      JSON.stringify(components) !== JSON.stringify(initialComponents)
    );
  };

  const handleSave = async () => {
    if (!currentPortfolioId && !onSave) {
      setError("No portfolio found to save settings.");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(false);

      const dataToSave = {
        templateName: availableTemplates.find(t => t.id === template)?.name || template,
        title: title.trim(),
        bio: bio.trim(),
        visibility: visibility as 0 | 1 | 2,
        components: JSON.stringify(components)
      };

      if (onSave) {
        await onSave(dataToSave);
        
        // Update initial values after successful save to reflect current state
        setInitialVisibility(visibility);
        setInitialTemplate(template);
        setInitialTitle(title.trim());
        setInitialBio(bio.trim());
        setInitialComponents([...components]);
      } else if (currentPortfolioId) {
        await updatePortfolio(currentPortfolioId, dataToSave);
        
        // Update initial values after successful save
        setInitialVisibility(visibility);
        setInitialTemplate(template);
        setInitialTitle(title.trim());
        setInitialBio(bio.trim());
        setInitialComponents([...components]);
      }
      
      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000); // Hide success message after 3 seconds
    } catch (err) {
      console.error('Error saving portfolio settings:', err);
      const errorMessage = err instanceof Error ? err.message : 'Failed to save portfolio settings';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setVisibility(initialVisibility);
    setTemplate(initialTemplate);
    setTitle(initialTitle);
    setBio(initialBio);
    setComponents(initialComponents);
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

      {success && (
        <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded">
          ✅ Portfolio settings saved successfully!
          <button 
            onClick={() => setSuccess(false)}
            className="ml-2 text-green-500 hover:text-green-700"
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
                  <SelectItem key={tmpl.id} value={tmpl.id}>
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
          onComponentsChange={setComponents}
        />
      </div>

      {!readOnly && (
        <div className="flex justify-end space-x-3 pt-4">
          <Button 
            variant="outline" 
            disabled={loading || !hasChanges()}
            onClick={handleReset}
          >
            Reset
          </Button>
          <Button 
            onClick={handleSave}
            disabled={loading || !hasChanges() || (!currentPortfolioId && !onSave)}
          >
            {loading ? 'Saving...' : 'Save Changes'}
          </Button>
        </div>
      )}
    </div>
  )
} 