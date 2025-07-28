"use client"

import { useState, useEffect } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useUser } from "@/lib/contexts/user-context"
import { updatePortfolio, createPortfolioAndGetId } from "@/lib/portfolio/api"
import { TemplateManager } from "@/lib/template-manager"

export function PortfolioInformation() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  
  const { getUserPortfolios, refreshUserPortfolios } = usePortfolio()
  const { user } = useUser()
  const userPortfolios = getUserPortfolios()
  const currentPortfolio = userPortfolios[0] // Get first portfolio
  
  // Form data state - only portfolio-specific fields
  const [formData, setFormData] = useState({
    title: "",
    description: "",
    visibility: 0,
    github: "",
    linkedin: ""
  })

  // Helper function to get selected template name
  const getSelectedTemplateName = () => {
    // Priority order:
    // 1. Current portfolio's template name (if portfolio exists)
    // 2. Check for any recently selected template in session storage
    // 3. Default to Gabriel Bârzu
    
    if (currentPortfolio?.templateName) {
      return currentPortfolio.templateName;
    }
    
    // Check session storage for recently selected template
    try {
      const savedTemplate = sessionStorage.getItem('selectedTemplateName');
      if (savedTemplate && savedTemplate !== 'undefined') {
        return savedTemplate;
      }
    } catch (error) {
      console.warn('Could not read from session storage:', error);
    }
    
    // Default fallback
    return 'Gabriel Bârzu';
  };

  // Load current portfolio data
  useEffect(() => {
    if (currentPortfolio) {
      setFormData({
        title: currentPortfolio.title || "",
        description: currentPortfolio.bio || "",
        visibility: currentPortfolio.visibility || 0,
        github: "", // These would need to be added to portfolio model if needed
        linkedin: ""
      })
    }
  }, [currentPortfolio])

  const handleInputChange = (field: string, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (error) setError(null)
    if (success) setSuccess(false)
  }

  const handleSave = async () => {
    try {
      setLoading(true)
      setError(null)
      
      const dataToSave = {
        title: formData.title.trim(),
        bio: formData.description.trim(),
        visibility: formData.visibility as 0 | 1 | 2
      }
      
      if (currentPortfolio) {
        // Update existing portfolio
        await updatePortfolio(currentPortfolio.id, dataToSave)
      } else if (user?.id) {
        // Create new portfolio with basic information
        const portfolioData = {
          userId: user.id,
          templateName: getSelectedTemplateName(),
          title: dataToSave.title || 'My Portfolio',
          bio: dataToSave.bio || 'Welcome to my portfolio',
          visibility: dataToSave.visibility,
          isPublished: false,
          components: JSON.stringify(TemplateManager.createDefaultComponentConfig())
        }
        
        await createPortfolioAndGetId(portfolioData)
      }
      
      // Refresh portfolio data
      await refreshUserPortfolios()
      
      setSuccess(true)
      setTimeout(() => setSuccess(false), 3000)
      
    } catch (error) {
      console.error('Error saving portfolio information:', error)
      setError('Failed to save portfolio information. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="border border-slate-200 rounded-lg p-6 bg-white">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-xl font-semibold text-slate-900">Portfolio Information</h3>
          <p className="text-sm text-slate-600 mt-1">
            Basic information about your portfolio and how it should be displayed
          </p>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{error}</p>
        </div>
      )}

      {success && (
        <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-sm text-green-600">✅ Portfolio information saved!</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-4">
          <div>
            <Label htmlFor="title" className="text-sm font-medium text-slate-700">
              Portfolio Title
            </Label>
            <Input 
              id="title" 
              placeholder="My Portfolio" 
              value={formData.title}
              onChange={(e) => handleInputChange("title", e.target.value)}
              className="mt-1"
            />
          </div>
          
          <div>
            <Label htmlFor="description" className="text-sm font-medium text-slate-700">
              Portfolio Description
            </Label>
            <Textarea 
              id="description" 
              placeholder="Tell visitors about your portfolio and what they can expect to find..." 
              value={formData.description}
              onChange={(e) => handleInputChange("description", e.target.value)}
              className="mt-1"
              rows={4}
            />
          </div>
        </div>

        <div className="space-y-4">
          <div>
            <Label htmlFor="visibility" className="text-sm font-medium text-slate-700">
              Portfolio Visibility
            </Label>
            <Select 
              value={formData.visibility.toString()} 
              onValueChange={(value) => handleInputChange("visibility", parseInt(value))}
            >
              <SelectTrigger className="mt-1">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">Public - Anyone can view</SelectItem>
                <SelectItem value="1">Private - Only you can view</SelectItem>
                <SelectItem value="2">Unlisted - Only with link</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label htmlFor="github" className="text-sm font-medium text-slate-700">
              GitHub Profile (Optional)
            </Label>
            <Input 
              id="github" 
              placeholder="https://github.com/username" 
              value={formData.github}
              onChange={(e) => handleInputChange("github", e.target.value)}
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="linkedin" className="text-sm font-medium text-slate-700">
              LinkedIn Profile (Optional)
            </Label>
            <Input 
              id="linkedin" 
              placeholder="https://linkedin.com/in/username" 
              value={formData.linkedin}
              onChange={(e) => handleInputChange("linkedin", e.target.value)}
              className="mt-1"
            />
          </div>
        </div>
      </div>

      <div className="flex justify-end pt-6">
        <Button
          onClick={handleSave}
          disabled={loading}
          className="px-6 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg"
        >
          {loading ? "Saving..." : "Save Information"}
        </Button>
      </div>
    </div>
  )
} 