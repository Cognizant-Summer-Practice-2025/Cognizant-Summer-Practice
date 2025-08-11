"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { useDraft } from "@/lib/contexts/draft-context"

export function AddExperience() {
  const { addDraftExperience } = useDraft()
  
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [experienceData, setExperienceData] = useState({
    jobTitle: "",
    companyName: "",
    startDate: "",
    endDate: "",
    isCurrent: false,
    description: "",
    skillsUsed: ""
  })

  const handleInputChange = (field: string, value: string | boolean) => {
    setExperienceData(prev => ({ ...prev, [field]: value }))
    if (error) setError(null)
    if (success) setSuccess(false)
  }

  const handleAddExperience = async () => {
    if (!experienceData.jobTitle.trim() || !experienceData.companyName.trim() || !experienceData.startDate) {
      setError("Please fill in all required fields (Job Title, Company, and Start Date).")
      return
    }

    try {
      setLoading(true)
      setError(null)

      // Save to draft context instead of API
      addDraftExperience({
        jobTitle: experienceData.jobTitle.trim(),
        companyName: experienceData.companyName.trim(),
        startDate: experienceData.startDate,
        endDate: experienceData.endDate,
        isCurrent: experienceData.isCurrent,
        description: experienceData.description.trim(),
        skillsUsed: experienceData.skillsUsed.trim()
      })

      // Reset form
      setExperienceData({
        jobTitle: "",
        companyName: "",
        startDate: "",
        endDate: "",
        isCurrent: false,
        description: "",
        skillsUsed: ""
      })

      setSuccess(true)
      
      // Hide success message after 3 seconds
      setTimeout(() => setSuccess(false), 3000)

    } catch (error) {
      console.error('Error adding experience draft:', error)
      setError('Failed to add experience. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="border border-slate-200 rounded-lg p-6 bg-white">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-xl font-semibold text-slate-900">Add New Experience</h3>
          <p className="text-sm text-slate-600 mt-1">
            Experience will be saved as draft and published when you hit publish
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
          <p className="text-sm text-green-600">✅ Experience added to drafts!</p>
        </div>
      )}

      <div className="grid grid-cols-1 gap-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label htmlFor="jobTitle" className="text-sm font-medium text-slate-700">
              Job Title *
            </Label>
            <Input
              id="jobTitle"
              value={experienceData.jobTitle}
              onChange={(e) => handleInputChange("jobTitle", e.target.value)}
              placeholder="Enter job title"
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="companyName" className="text-sm font-medium text-slate-700">
              Company Name *
            </Label>
            <Input
              id="companyName"
              value={experienceData.companyName}
              onChange={(e) => handleInputChange("companyName", e.target.value)}
              placeholder="Enter company name"
              className="mt-1"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label htmlFor="startDate" className="text-sm font-medium text-slate-700">
              Start Date *
            </Label>
            <Input
              id="startDate"
              type="date"
              value={experienceData.startDate}
              onChange={(e) => handleInputChange("startDate", e.target.value)}
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="endDate" className="text-sm font-medium text-slate-700">
              End Date
            </Label>
            <Input
              id="endDate"
              type="date"
              value={experienceData.endDate}
              onChange={(e) => handleInputChange("endDate", e.target.value)}
              disabled={experienceData.isCurrent}
              className="mt-1"
            />
          </div>
        </div>

        <div className="flex items-center space-x-2">
          <Checkbox
            id="isCurrent"
            checked={experienceData.isCurrent}
            onCheckedChange={(checked) => {
              handleInputChange("isCurrent", checked === true)
              if (checked) {
                handleInputChange("endDate", "")
              }
            }}
          />
          <Label htmlFor="isCurrent" className="text-sm font-medium text-slate-700">
            I currently work here
          </Label>
        </div>

        <div>
          <Label htmlFor="description" className="text-sm font-medium text-slate-700">
            Description
          </Label>
          <Textarea
            id="description"
            value={experienceData.description}
            onChange={(e) => handleInputChange("description", e.target.value)}
            placeholder="Describe your role and responsibilities..."
            className="mt-1"
            rows={4}
          />
        </div>

        <div>
          <Label htmlFor="skillsUsed" className="text-sm font-medium text-slate-700">
            Skills Used
          </Label>
          <Input
            id="skillsUsed"
            value={experienceData.skillsUsed}
            onChange={(e) => handleInputChange("skillsUsed", e.target.value)}
            placeholder="JavaScript, Python, Project Management (comma-separated)"
            className="mt-1"
          />
        </div>

        <div className="flex justify-end pt-4">
          <Button
            onClick={handleAddExperience}
            disabled={loading}
            className="px-6 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg"
          >
            {loading ? "Adding..." : "Add Experience"}
          </Button>
        </div>
      </div>
    </div>
  )
} 