"use client"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Textarea } from "@/components/ui/textarea"
import { ComponentOrdering } from "@/components/ui/component-ordering"
import { ComponentConfig, TemplateConfig } from '@/lib/portfolio'
import { TemplateManager } from '@/lib/template-manager'
import { updatePortfolio } from '@/lib/portfolio/api'
import { usePortfolio } from '@/lib/contexts/portfolio-context'
import { Loading } from '@/components/loader'
import { getPortfolioTemplates } from '@/lib/templates'
import { templateRegistry } from '@/lib/template-registry'

interface PortfolioSettingsProps {
  portfolioId?: string;
  templates?: TemplateConfig[];
  templatesLoaded?: boolean;
  initialData?: {
    portfolio?: {
      components?: ComponentConfig[];
      templateId?: string;
    };
  };
  onSave?: (data: {
    templateName: string;
    components: string;
  }) => Promise<void>;
  readOnly?: boolean;
}

export function PortfolioSettings({ 
  portfolioId, 
  templates, 
  templatesLoaded, 
  initialData, 
  onSave, 
  readOnly = false 
}: PortfolioSettingsProps = {}) {
  const { getUserPortfolios } = usePortfolio();
  const userPortfolios = getUserPortfolios();
  const currentPortfolioId = portfolioId || userPortfolios[0]?.id;
  const currentPortfolio = userPortfolios.find(p => p.id === currentPortfolioId);
  

  
  // Initial values that will be updated when portfolio data changes
  const [initialTemplate, setInitialTemplate] = useState(initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu");
  const [initialComponents, setInitialComponents] = useState<ComponentConfig[]>(initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig());
  
  // Current form state
  const [template, setTemplate] = useState(initialTemplate);
  const [components, setComponents] = useState<ComponentConfig[]>(initialComponents);
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  

  
  // Update initial values when portfolio data changes
  useEffect(() => {
    const newTemplate = initialData?.portfolio?.templateId ?? currentPortfolio?.templateId ?? "gabriel-barzu";
    const newComponents = initialData?.portfolio?.components || TemplateManager.createDefaultComponentConfig();
    
    // Only update if the values have actually changed to prevent unnecessary re-renders
    if (newTemplate !== initialTemplate) {
      setInitialTemplate(newTemplate);
      setTemplate(newTemplate);
    }
    if (JSON.stringify(newComponents) !== JSON.stringify(initialComponents)) {
      setInitialComponents(newComponents);
      setComponents(newComponents);
    }
  }, [currentPortfolio, initialData, initialTemplate, initialComponents]);
  
  // Dynamic templates loaded from backend or passed as props
  const [availableTemplates, setAvailableTemplates] = useState([
    { id: 'gabriel-barzu', name: 'Gabriel Bârzu', description: 'Professional template with sidebar layout' },
    { id: 'modern', name: 'Modern', description: 'Clean and modern design' },
    { id: 'creative', name: 'Creative', description: 'Creative and colorful layout' },
    { id: 'professional', name: 'Professional', description: 'Classic professional design' }
  ]);

  // Load templates from backend or use passed templates
  useEffect(() => {
    if (templates && templatesLoaded) {
      // Use templates passed as props
      const templateOptions = templates.map(t => ({
        id: t.id,
        name: t.name,
        description: t.description
      }));
      setAvailableTemplates(templateOptions);
      console.log('📋 Using passed templates in PortfolioSettings:', templateOptions.map(t => `${t.name} (${t.id})`));
    } else {
      // Fallback to loading templates directly
      getPortfolioTemplates()
        .then(templates => {
          const templateOptions = templates.map(t => ({
            id: t.id,
            name: t.name,
            description: t.description
          }));
          setAvailableTemplates(templateOptions);
          console.log('📋 Loaded templates directly in PortfolioSettings:', templateOptions.map(t => `${t.name} (${t.id})`));
        })
        .catch(error => {
          console.error('Failed to load templates:', error);
          // Keep default fallback templates
        });
    }
  }, [templates, templatesLoaded]);

  // Function to check if there are any changes
  const hasChanges = () => {
    return (
      template !== initialTemplate ||
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

      // Ensure template registry is initialized
      await templateRegistry.initialize();
      
      // Get the actual template name from the backend
      const selectedTemplate = availableTemplates.find(t => t.id === template);
      const templateName = selectedTemplate?.name || template;

      // Save selected template to session storage for cross-component access
      try {
        sessionStorage.setItem('selectedTemplateName', templateName);
      } catch (error) {
        console.warn('Could not save to session storage:', error);
      }

      const dataToSave = {
        templateName: templateName,
        components: JSON.stringify(components)
      };

      if (onSave) {
        await onSave(dataToSave);
        
        // Update initial values after successful save to reflect current state
        setInitialTemplate(template);
        setInitialComponents([...components]);
      } else if (currentPortfolioId) {
        // Note: Template changes might need special handling or separate API call
        await updatePortfolio(currentPortfolioId, { 
          components: JSON.stringify(components),
          templateName: templateName // Also update template name if possible
        });
        
        // Update initial values after successful save
        setInitialTemplate(template);
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
    setTemplate(initialTemplate);
    setComponents(initialComponents);
  };

  if (loading && !initialData) {
    return (
      <div className="space-y-6">
        <h2 className="text-xl font-semibold mb-4">Portfolio Settings</h2>
        <div className="flex items-center justify-center py-8">
          <Loading className="scale-50" backgroundColor="white" />
          <span className="ml-4">Loading settings...</span>
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