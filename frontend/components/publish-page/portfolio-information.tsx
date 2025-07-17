"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"

export function PortfolioInformation() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  
  // Form data state
  const [formData, setFormData] = useState({
    name: "",
    title: "",
    location: "",
    email: "",
    bio: "",
    github: "",
    linkedin: ""
  })

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (error) setError(null)
    if (success) setSuccess(false)
  }

  const handleSave = async () => {
    try {
      setLoading(true)
      setError(null)
      
      // TODO: Implement save functionality for basic info
      console.log('Saving basic info:', formData)
      
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
            Basic information about you and your professional background
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

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="space-y-4">
          <div>
            <Label htmlFor="name" className="text-sm font-medium text-slate-700">
              Full Name
            </Label>
            <Input 
              id="name" 
              placeholder="Enter your full name" 
              value={formData.name}
              onChange={(e) => handleInputChange("name", e.target.value)}
              className="mt-1"
            />
          </div>
          
          <div>
            <Label htmlFor="title" className="text-sm font-medium text-slate-700">
              Professional Title
            </Label>
            <Input 
              id="title" 
              placeholder="e.g., Software Developer" 
              value={formData.title}
              onChange={(e) => handleInputChange("title", e.target.value)}
              className="mt-1"
            />
          </div>
          
          <div>
            <Label htmlFor="location" className="text-sm font-medium text-slate-700">
              Location
            </Label>
            <Input 
              id="location" 
              placeholder="e.g., New York, NY" 
              value={formData.location}
              onChange={(e) => handleInputChange("location", e.target.value)}
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="email" className="text-sm font-medium text-slate-700">
              Email
            </Label>
            <Input 
              id="email" 
              type="email"
              placeholder="your.email@example.com" 
              value={formData.email}
              onChange={(e) => handleInputChange("email", e.target.value)}
              className="mt-1"
            />
          </div>
        </div>

        <div className="space-y-4">
          <div>
            <Label htmlFor="github" className="text-sm font-medium text-slate-700">
              GitHub Profile
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
              LinkedIn Profile
            </Label>
            <Input 
              id="linkedin" 
              placeholder="https://linkedin.com/in/username" 
              value={formData.linkedin}
              onChange={(e) => handleInputChange("linkedin", e.target.value)}
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="bio" className="text-sm font-medium text-slate-700">
              Professional Bio
            </Label>
            <Textarea 
              id="bio" 
              placeholder="Write a brief description about yourself and your professional background..." 
              value={formData.bio}
              onChange={(e) => handleInputChange("bio", e.target.value)}
              className="mt-1"
              rows={4}
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