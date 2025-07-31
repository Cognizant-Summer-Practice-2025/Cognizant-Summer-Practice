"use client"

import React, { createContext, useContext, useState, ReactNode } from 'react'

interface DraftProject {
  id: string
  title: string
  imageUrl: string
  description: string
  demoUrl: string
  githubUrl: string
  technologies: string
  featured: boolean
  selectedImageFile?: File | null 
}

interface DraftExperience {
  id: string
  jobTitle: string
  companyName: string
  startDate: string
  endDate: string
  isCurrent: boolean
  description: string
  skillsUsed: string
}

interface DraftBlogPost {
  id: string
  title: string
  content: string
  featuredImageUrl: string
  tags: string
  publishImmediately: boolean
  selectedImageFile?: File | null // Store the selected file for later upload
}

interface DraftSkill {
  id: string
  name: string
  categoryType: string // 'hard_skills' or 'soft_skills'
  subcategory: string // 'frontend', 'backend', 'communication', etc.
  category: string // Full category path for display
  proficiencyLevel: number
}

interface DraftContextType {
  // Draft data
  draftProjects: DraftProject[]
  draftExperience: DraftExperience[]
  draftBlogPosts: DraftBlogPost[]
  draftSkills: DraftSkill[]
  
  // Project methods
  addDraftProject: (project: Omit<DraftProject, 'id'>) => void
  updateDraftProject: (id: string, project: Partial<DraftProject>) => void
  deleteDraftProject: (id: string) => void
  
  // Experience methods
  addDraftExperience: (experience: Omit<DraftExperience, 'id'>) => void
  updateDraftExperience: (id: string, experience: Partial<DraftExperience>) => void
  deleteDraftExperience: (id: string) => void
  
  // Blog post methods
  addDraftBlogPost: (blogPost: Omit<DraftBlogPost, 'id'>) => void
  updateDraftBlogPost: (id: string, blogPost: Partial<DraftBlogPost>) => void
  deleteDraftBlogPost: (id: string) => void
  
  // Skill methods
  addDraftSkill: (skill: Omit<DraftSkill, 'id'>) => void
  updateDraftSkill: (id: string, skill: Partial<DraftSkill>) => void
  deleteDraftSkill: (id: string) => void
  
  // Utility methods
  clearAllDrafts: () => void
  hasDraftData: () => boolean
}

const DraftContext = createContext<DraftContextType | undefined>(undefined)

export function DraftProvider({ children }: { children: ReactNode }) {
  const [draftProjects, setDraftProjects] = useState<DraftProject[]>([])
  const [draftExperience, setDraftExperience] = useState<DraftExperience[]>([])
  const [draftBlogPosts, setDraftBlogPosts] = useState<DraftBlogPost[]>([])
  const [draftSkills, setDraftSkills] = useState<DraftSkill[]>([])

  // Project methods
  const addDraftProject = (project: Omit<DraftProject, 'id'>) => {
    const newProject: DraftProject = {
      ...project,
      id: crypto.randomUUID()
    }
    setDraftProjects(prev => [...prev, newProject])
  }

  const updateDraftProject = (id: string, project: Partial<DraftProject>) => {
    setDraftProjects(prev =>
      prev.map(p => p.id === id ? { ...p, ...project } : p)
    )
  }

  const deleteDraftProject = (id: string) => {
    setDraftProjects(prev => prev.filter(p => p.id !== id))
  }

  // Experience methods
  const addDraftExperience = (experience: Omit<DraftExperience, 'id'>) => {
    const newExperience: DraftExperience = {
      ...experience,
      id: crypto.randomUUID()
    }
    setDraftExperience(prev => [...prev, newExperience])
  }

  const updateDraftExperience = (id: string, experience: Partial<DraftExperience>) => {
    setDraftExperience(prev =>
      prev.map(e => e.id === id ? { ...e, ...experience } : e)
    )
  }

  const deleteDraftExperience = (id: string) => {
    setDraftExperience(prev => prev.filter(e => e.id !== id))
  }

  // Blog post methods
  const addDraftBlogPost = (blogPost: Omit<DraftBlogPost, 'id'>) => {
    const newBlogPost: DraftBlogPost = {
      ...blogPost,
      id: crypto.randomUUID()
    }
    setDraftBlogPosts(prev => [...prev, newBlogPost])
  }

  const updateDraftBlogPost = (id: string, blogPost: Partial<DraftBlogPost>) => {
    setDraftBlogPosts(prev =>
      prev.map(b => b.id === id ? { ...b, ...blogPost } : b)
    )
  }

  const deleteDraftBlogPost = (id: string) => {
    setDraftBlogPosts(prev => prev.filter(b => b.id !== id))
  }

  // Skill methods
  const addDraftSkill = (skill: Omit<DraftSkill, 'id'>) => {
    const newSkill: DraftSkill = {
      ...skill,
      id: crypto.randomUUID()
    }
    setDraftSkills(prev => [...prev, newSkill])
  }

  const updateDraftSkill = (id: string, skill: Partial<DraftSkill>) => {
    setDraftSkills(prev =>
      prev.map(s => s.id === id ? { ...s, ...skill } : s)
    )
  }

  const deleteDraftSkill = (id: string) => {
    setDraftSkills(prev => prev.filter(s => s.id !== id))
  }

  // Utility methods
  const clearAllDrafts = () => {
    setDraftProjects([])
    setDraftExperience([])
    setDraftBlogPosts([])
    setDraftSkills([])
  }

  const hasDraftData = () => {
    return draftProjects.length > 0 || draftExperience.length > 0 || draftBlogPosts.length > 0 || draftSkills.length > 0
  }

  const value: DraftContextType = {
    draftProjects,
    draftExperience,
    draftBlogPosts,
    draftSkills,
    addDraftProject,
    updateDraftProject,
    deleteDraftProject,
    addDraftExperience,
    updateDraftExperience,
    deleteDraftExperience,
    addDraftBlogPost,
    updateDraftBlogPost,
    deleteDraftBlogPost,
    addDraftSkill,
    updateDraftSkill,
    deleteDraftSkill,
    clearAllDrafts,
    hasDraftData
  }

  return (
    <DraftContext.Provider value={value}>
      {children}
    </DraftContext.Provider>
  )
}

export function useDraft() {
  const context = useContext(DraftContext)
  if (context === undefined) {
    throw new Error('useDraft must be used within a DraftProvider')
  }
  return context
} 