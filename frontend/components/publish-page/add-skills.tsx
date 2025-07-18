"use client"

import { useState } from "react"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { SkillDropdown } from "@/components/ui/skill-dropdown"
import { useDraft } from "@/lib/contexts/draft-context"
import { usePortfolio } from "@/lib/contexts/portfolio-context"

export function AddSkills() {
  const { addDraftSkill, draftSkills } = useDraft()
  const { getUserSkills } = usePortfolio()
  const existingSkills = getUserSkills()
  
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [newProficiency, setNewProficiency] = useState(50)
  const [selectedSkillData, setSelectedSkillData] = useState<{
    categoryType: string
    subcategory: string
    skillName: string
    fullCategoryPath: string
  } | null>(null)

  const handleSkillSelect = (skillData: {
    categoryType: string
    subcategory: string
    skillName: string
    fullCategoryPath: string
  }) => {
    setSelectedSkillData(skillData)
    setError(null) // Clear any previous errors when a skill is selected
  }

  const handleAddSkill = async () => {
    if (!selectedSkillData) {
      setError("Please select a skill.")
      return
    }

    // Check if skill already exists in existing skills or drafts
    const allSkills = [
      ...existingSkills.map(s => s.name.toLowerCase()),
      ...draftSkills.map(s => s.name.toLowerCase())
    ]
    
    if (allSkills.includes(selectedSkillData.skillName.toLowerCase())) {
      setError("Skill already exists.")
      return
    }

    try {
      setLoading(true)
      setError(null)

      // Save to draft context instead of API
      const draftSkillData = {
        name: selectedSkillData.skillName,
        categoryType: selectedSkillData.categoryType,
        subcategory: selectedSkillData.subcategory,
        category: selectedSkillData.fullCategoryPath,
        proficiencyLevel: newProficiency
      };
      
      addDraftSkill(draftSkillData);

      // Reset form
      setSelectedSkillData(null)
      setNewProficiency(50)

      setSuccess(true)
      
      // Hide success message after 3 seconds
      setTimeout(() => setSuccess(false), 3000)

    } catch (error) {
      console.error('Error adding skill draft:', error)
      setError('Failed to add skill. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  const getProficiencyLabel = (level: number) => {
    if (level >= 80) return 'Expert'
    if (level >= 60) return 'Advanced'
    if (level >= 40) return 'Intermediate'
    return 'Beginner'
  }

  return (
    <div className="border border-slate-200 rounded-lg p-6 bg-white">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-xl font-semibold text-slate-900">Add New Skill</h3>
          <p className="text-sm text-slate-600 mt-1">
            Skills will be saved as draft and published when you hit publish
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
          <p className="text-sm text-green-600">✅ Skill added to drafts!</p>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Skill Selection */}
        <div>
          <SkillDropdown
            onSkillSelect={handleSkillSelect}
            disabled={loading}
            className="w-full"
          />
        </div>
        
        {/* Proficiency Selection */}
        <div className="space-y-4">
          <div>
            <Label htmlFor="skill-proficiency" className="text-sm font-medium text-slate-700">
              Proficiency ({newProficiency}% - {getProficiencyLabel(newProficiency)})
            </Label>
            <input
              id="skill-proficiency"
              type="range"
              min="1"
              max="100"
              value={newProficiency}
              onChange={(e) => setNewProficiency(parseInt(e.target.value))}
              className="mt-1 w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
              disabled={loading}
            />
          </div>
          
          <div className="flex items-end">
            <Button
              onClick={handleAddSkill}
              disabled={!selectedSkillData || loading}
              className="w-full px-6 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg"
            >
              {loading ? "Adding..." : "Add Skill"}
            </Button>
          </div>
        </div>
      </div>

      {/* Selected Skill Preview */}
      {selectedSkillData && (
        <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
          <p className="text-sm text-blue-800">
            <strong>Selected:</strong> {selectedSkillData.skillName} 
            <span className="text-blue-600"> ({selectedSkillData.fullCategoryPath})</span>
          </p>
        </div>
      )}
    </div>
  )
} 