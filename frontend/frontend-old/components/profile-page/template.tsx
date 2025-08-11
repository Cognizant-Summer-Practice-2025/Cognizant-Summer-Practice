"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { getPortfolioTemplates, getDefaultTemplate } from "@/lib/templates";
import { templateRegistry } from "@/lib/template-registry";
import { TemplateManager } from "@/lib/template-manager";
import { TemplateConfig } from "@/lib/portfolio";
import { usePortfolio } from "@/lib/contexts/portfolio-context";
import { updatePortfolio } from "@/lib/portfolio/api";
import { Loading } from "@/components/loader";

export default function Template() {
  const { userPortfolioData, refreshUserPortfolios } = usePortfolio();
  const [templates, setTemplates] = useState<TemplateConfig[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState("gabriel-barzu");
  const [loading, setLoading] = useState(false);
  const [templatesLoading, setTemplatesLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Get current portfolio
  const currentPortfolio = userPortfolioData?.portfolios?.find(p => p.isPublished) || userPortfolioData?.portfolios?.[0];

  // Load templates from template registry
  useEffect(() => {
    const loadTemplates = async () => {
      try {
        setTemplatesLoading(true);
        
        // Initialize template registry and get dynamic templates
        await templateRegistry.initialize();
        const portfolioTemplates = await getPortfolioTemplates();
        
        setTemplates(portfolioTemplates);
        console.log('📋 Loaded templates in profile page:', portfolioTemplates.map(t => `${t.name} (${t.id})`));
        
      } catch (error) {
        console.error('❌ Failed to load templates:', error);
        // Fallback to default template
        const defaultTemplate = getDefaultTemplate();
        setTemplates([defaultTemplate]);
      } finally {
        setTemplatesLoading(false);
      }
    };

    loadTemplates();
  }, []);

  // Initialize selected template from current portfolio
  useEffect(() => {
    if (templates.length === 0) return;

    if (currentPortfolio?.template) {
      // Find template by name first
      const templateByName = templates.find(t => t.name === currentPortfolio.template!.name);
      if (templateByName) {
        setSelectedTemplate(templateByName.id);
        return;
      }
      
      // Use template ID directly if available
      if (currentPortfolio.template!.id && templates.find(t => t.id === currentPortfolio.template!.id)) {
        setSelectedTemplate(currentPortfolio.template!.id);
          return;
      }
    }
    
    if (currentPortfolio?.templateId) {
      // Try to use templateId directly or convert from UUID
      let templateId = currentPortfolio.templateId;
      
      // If it looks like a UUID, try to convert it
      if (templateId.length > 20) {
        templateId = templateRegistry.convertUuidToId(templateId) || templateId;
      }
      
      if (templates.find(t => t.id === templateId)) {
        setSelectedTemplate(templateId);
        return;
      }
    }
    
    // Default to first available template
    if (templates.length > 0) {
      setSelectedTemplate(templates[0].id);
    }
  }, [currentPortfolio, templates]);

  // Helper function to get template name by ID
  const getTemplateNameById = (templateId: string) => {
    const template = templates.find(t => t.id === templateId);
    return template?.name || templateId;
  };

  // Handle template save
  const handleSaveTemplate = async () => {
    if (!currentPortfolio) {
      setError("No portfolio found. Please create a portfolio first.");
      return;
    }

    if (!selectedTemplate) {
      setError("Please select a template.");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(false);

      const templateName = getTemplateNameById(selectedTemplate);

      // Update portfolio with new template and reset components to default
      const updateData = {
        templateName: templateName,
        // Reset components to default configuration for the new template
        components: JSON.stringify(TemplateManager.createDefaultComponentConfig())
      };

      await updatePortfolio(currentPortfolio.id, updateData);

      // Save selected template to session storage for cross-component access
      try {
        sessionStorage.setItem('selectedTemplateName', templateName);
        sessionStorage.setItem('selectedTemplateId', selectedTemplate);
      } catch (sessionError) {
        console.warn('Could not save to session storage:', sessionError);
      }

      // Refresh portfolio data to reflect changes
      await refreshUserPortfolios();

      setSuccess(true);
      setTimeout(() => setSuccess(false), 3000);

      console.log('✅ Template updated successfully:', templateName);

    } catch (err) {
      console.error('❌ Error updating template:', err);
      setError(err instanceof Error ? err.message : 'Failed to update template. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Check if template has changed from current portfolio
  const hasChanges = () => {
    if (!currentPortfolio || templates.length === 0) return selectedTemplate !== "gabriel-barzu";
    
    // Check by template first
    if (currentPortfolio.template) {
      const currentTemplateId = currentPortfolio.template.id;
      return selectedTemplate !== currentTemplateId;
    }
    
    // Fallback to templateId comparison
    if (currentPortfolio.templateId) {
      let currentTemplateId = currentPortfolio.templateId;
      
      // Convert UUID to frontend ID if needed
      if (currentTemplateId.length > 20) {
        currentTemplateId = templateRegistry.convertUuidToId(currentTemplateId) || currentTemplateId;
      }
      
      return selectedTemplate !== currentTemplateId;
    }
    
    return true;
  };

  if (templatesLoading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] overflow-hidden">
        <div className="flex items-center justify-center h-32">
          <Loading className="w-8 h-8" backgroundColor="transparent" />
          <span className="ml-2 text-gray-600">Loading templates...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] overflow-hidden">
      <div className="flex flex-col gap-4 sm:gap-6">

        <div className="flex flex-col">
          <h1 className="text-xl sm:text-2xl font-semibold text-[#020817] leading-tight sm:leading-[38.4px] mb-1">
            Portfolio Template
          </h1>
          <p className="text-sm text-[#64748B] leading-[22.4px]">
            Choose a template for your portfolio
          </p>
        </div>

        {/* Status Messages */}
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
            ✅ Template updated successfully!
            <button 
              onClick={() => setSuccess(false)}
              className="ml-2 text-green-500 hover:text-green-700"
            >
              ×
            </button>
          </div>
        )}

        {currentPortfolio && (
          <div className="bg-blue-50 border border-blue-200 text-blue-700 px-4 py-3 rounded">
            <span className="text-sm">
              Current template: <strong>{currentPortfolio.template?.name || 'Unknown'}</strong>
            </span>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6 w-full">
          {templates.map((template) => (
            <div
              key={template.id}
              onClick={() => setSelectedTemplate(template.id)}
              className={`relative cursor-pointer rounded-lg p-[1px] transition-all border-2 ${
                selectedTemplate === template.id
                  ? "border-[#2563EB] shadow-[0px_0px_0px_2px_rgba(37,99,235,0.20)]"
                  : "border-[#E2E8F0] hover:border-[#2563EB] hover:shadow-[0px_0px_0px_2px_rgba(37,99,235,0.10)]"
              }`}
            >
              <div className="bg-white rounded-lg overflow-hidden flex flex-col h-full">
                {/* Template Image */}
                <div className="relative h-[120px] sm:h-[150px] bg-gray-100 flex items-center justify-center overflow-hidden rounded-t-lg">
                  <Image
                    src={template.previewImage}
                    alt={template.name}
                    fill
                    className="object-cover"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.style.display = 'none';
                      const fallback = document.createElement('div');
                      fallback.className = 'absolute inset-0 bg-gradient-to-br from-gray-200 to-gray-300 flex items-center justify-center';
                      fallback.innerHTML = `<span class="text-gray-500 text-xs sm:text-sm">${template.name} Preview</span>`;
                      target.parentElement!.appendChild(fallback);
                    }}
                  />
                </div>
                
                {/* Template Info */}
                <div className="flex flex-col gap-[3px] p-3 sm:p-4 pt-3 sm:pt-[15px] pb-3 sm:pb-4">
                  <div className="pb-[0.59px]">
                    <h3 className="text-sm sm:text-base font-semibold text-[#020817] leading-tight sm:leading-[25.6px]">
                      {template.name}
                    </h3>
                  </div>
                  <div>
                    <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
                      {template.description}
                    </p>
                  </div>
                </div>
              </div>

              {/* Selected Badge */}
              {selectedTemplate === template.id && (
                <div className="absolute top-2 right-2 bg-[#2563EB] rounded-md px-2 py-[3.5px] z-10">
                  <span className="text-xs font-medium text-[#F8FAFC] leading-[19.2px]">
                    Selected
                  </span>
                </div>
              )}

              {/* Current Portfolio Badge */}
              {currentPortfolio && currentPortfolio.template?.id === template.id && selectedTemplate !== template.id && (
                <div className="absolute top-2 right-2 bg-gray-500 rounded-md px-2 py-[3.5px] z-10">
                  <span className="text-xs font-medium text-white leading-[19.2px]">
                    Current
                  </span>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Save Button */}
        <div className="flex justify-end gap-3 pt-4 border-t">
          <Button
            onClick={handleSaveTemplate}
            disabled={loading || !hasChanges()}
            className="px-6 py-2"
          >
            {loading ? (
              <>
                <Loading className="w-4 h-4 mr-2" backgroundColor="transparent" />
                Saving...
              </>
            ) : (
              'Save Template'
            )}
          </Button>
        </div>
      </div>
    </div>
  );
} 