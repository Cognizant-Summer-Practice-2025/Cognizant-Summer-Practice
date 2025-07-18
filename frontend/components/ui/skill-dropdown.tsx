"use client"

import { useState, useEffect } from "react"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Label } from "@/components/ui/label"
import skillsConfig from "@/lib/skills-config.json"

interface SkillDropdownProps {
  onSkillSelect: (skill: {
    categoryType: string
    subcategory: string
    skillName: string
    fullCategoryPath: string
  }) => void
  disabled?: boolean
  value?: {
    categoryType?: string
    subcategory?: string
    skillName?: string
  }
  className?: string
}

interface CategoryData {
  label: string
  subcategories: Record<string, SubcategoryData>
}

interface SubcategoryData {
  label: string
  skills: string[]
}

export function SkillDropdown({ onSkillSelect, disabled = false, value, className }: SkillDropdownProps) {
  const [selectedCategory, setSelectedCategory] = useState<string>(value?.categoryType || "")
  const [selectedSubcategory, setSelectedSubcategory] = useState<string>(value?.subcategory || "")
  const [selectedSkill, setSelectedSkill] = useState<string>(value?.skillName || "")

  // Helper function to safely get category data
  const getCategoryData = (categoryKey: string): CategoryData | null => {
    return (skillsConfig.skillCategories as Record<string, CategoryData>)[categoryKey] || null
  }

  // Helper function to safely get subcategory data
  const getSubcategoryData = (categoryKey: string, subcategoryKey: string): SubcategoryData | null => {
    const category = getCategoryData(categoryKey)
    return category?.subcategories?.[subcategoryKey] || null
  }

  // Helper function to safely get skills array
  const getSkillsArray = (categoryKey: string, subcategoryKey: string): string[] => {
    const subcategory = getSubcategoryData(categoryKey, subcategoryKey)
    return subcategory?.skills || []
  }

  // Helper function to notify parent
  const notifyParent = (category: string, subcategory: string, skill: string) => {
    const categoryData = getCategoryData(category)
    const subcategoryData = getSubcategoryData(category, subcategory)
    
    if (categoryData && subcategoryData) {
      const skillData = {
        categoryType: category, // This is the technical key like 'hard_skills'
        subcategory: subcategory, // This is the technical key like 'frontend'
        skillName: skill,
        fullCategoryPath: `${categoryData.label} > ${subcategoryData.label}`
      };
      
      onSkillSelect(skillData);
    }
  }

  // Reset subcategory and skill when category changes
  useEffect(() => {
    if (selectedCategory && selectedCategory !== value?.categoryType) {
      setSelectedSubcategory("")
      setSelectedSkill("")
    }
  }, [selectedCategory, value?.categoryType])

  // Reset skill when subcategory changes
  useEffect(() => {
    if (selectedSubcategory && selectedSubcategory !== value?.subcategory) {
      setSelectedSkill("")
    }
  }, [selectedSubcategory, value?.subcategory])

  // Handle category change
  const handleCategoryChange = (categoryKey: string) => {
    setSelectedCategory(categoryKey)
    setSelectedSubcategory("")
    setSelectedSkill("")
  }

  // Handle subcategory change
  const handleSubcategoryChange = (subcategoryKey: string) => {
    setSelectedSubcategory(subcategoryKey)
    setSelectedSkill("")
  }

  // Handle skill change - notify parent immediately
  const handleSkillChange = (skillName: string) => {
    setSelectedSkill(skillName)
    // Use current values, not state values which may be stale
    if (selectedCategory && selectedSubcategory) {
      notifyParent(selectedCategory, selectedSubcategory, skillName)
    }
  }

  const categories = Object.entries(skillsConfig.skillCategories as Record<string, CategoryData>)
  const subcategories = selectedCategory 
    ? Object.entries(getCategoryData(selectedCategory)?.subcategories || {})
    : []
  const skills = selectedCategory && selectedSubcategory
    ? getSkillsArray(selectedCategory, selectedSubcategory)
    : []

  return (
    <div className={`grid grid-cols-1 md:grid-cols-3 gap-4 ${className}`}>
      {/* Category Selection */}
      <div>
        <Label htmlFor="skill-category" className="text-sm font-medium text-slate-700">
          Skill Category
        </Label>
        <Select 
          value={selectedCategory} 
          onValueChange={handleCategoryChange} 
          disabled={disabled}
        >
          <SelectTrigger className="mt-1">
            <SelectValue placeholder="Select category..." />
          </SelectTrigger>
          <SelectContent>
            {categories.map(([key, category]) => (
              <SelectItem key={key} value={key}>
                {category.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Subcategory Selection */}
      <div>
        <Label htmlFor="skill-subcategory" className="text-sm font-medium text-slate-700">
          Subcategory
        </Label>
        <Select 
          value={selectedSubcategory} 
          onValueChange={handleSubcategoryChange} 
          disabled={disabled || !selectedCategory}
        >
          <SelectTrigger className="mt-1">
            <SelectValue placeholder="Select subcategory..." />
          </SelectTrigger>
          <SelectContent>
            {subcategories.map(([key, subcategory]) => (
              <SelectItem key={key} value={key}>
                {(subcategory as SubcategoryData).label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Skill Selection */}
      <div>
        <Label htmlFor="skill-name" className="text-sm font-medium text-slate-700">
          Specific Skill
        </Label>
        <Select 
          value={selectedSkill} 
          onValueChange={handleSkillChange} 
          disabled={disabled || !selectedSubcategory}
        >
          <SelectTrigger className="mt-1">
            <SelectValue placeholder="Select skill..." />
          </SelectTrigger>
          <SelectContent>
            {skills.map((skill: string) => (
              <SelectItem key={skill} value={skill}>
                {skill}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  )
} 