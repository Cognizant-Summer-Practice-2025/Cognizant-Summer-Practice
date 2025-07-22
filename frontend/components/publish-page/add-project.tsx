"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { ImageUpload } from "@/components/ui/image-upload"
import { useDraft } from "@/lib/contexts/draft-context"

export function AddProject() {
  const { addDraftProject } = useDraft()
  
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [projectData, setProjectData] = useState({
    title: "",
    imageUrl: "",
    description: "",
    demoUrl: "",
    githubUrl: "",
    technologies: "",
    featured: false
  })
  const [selectedImageFile, setSelectedImageFile] = useState<File | null>(null)

  const handleInputChange = (field: string, value: string | boolean) => {
    setProjectData(prev => ({ ...prev, [field]: value }))
    if (error) setError(null)
    if (success) setSuccess(false)
  }

  const handleAddProject = async () => {
    if (!projectData.title.trim()) {
      setError("Please enter a project title.")
      return
    }

    try {
      setLoading(true)
      setError(null)

      addDraftProject({
        title: projectData.title.trim(),
        imageUrl: projectData.imageUrl.trim(), 
        description: projectData.description.trim(),
        demoUrl: projectData.demoUrl.trim(),
        githubUrl: projectData.githubUrl.trim(),
        technologies: projectData.technologies.trim(),
        featured: projectData.featured,
        selectedImageFile: selectedImageFile 
      })

      // Reset form
      setProjectData({
        title: "",
        imageUrl: "",
        description: "",
        demoUrl: "",
        githubUrl: "",
        technologies: "",
        featured: false
      })
      setSelectedImageFile(null)

      setSuccess(true)
      
      // Hide success message after 3 seconds
      setTimeout(() => setSuccess(false), 3000)

    } catch (error) {
      console.error('Error adding project draft:', error)
      setError('Failed to add project. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="border border-slate-200 rounded-lg p-6 bg-white">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-xl font-semibold text-slate-900">Add New Project</h3>
          <p className="text-sm text-slate-600 mt-1">
            Project will be saved as draft and published when you hit publish
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
          <p className="text-sm text-green-600">✅ Project added to drafts!</p>
        </div>
      )}

      <div className="grid grid-cols-1 gap-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label htmlFor="title" className="text-sm font-medium text-slate-700">
              Project Title *
            </Label>
            <Input
              id="title"
              value={projectData.title}
              onChange={(e) => handleInputChange("title", e.target.value)}
              placeholder="Enter project title"
              className="mt-1"
            />
          </div>

          <div>
            <ImageUpload
              label="Project Image"
              value={projectData.imageUrl}
              onFileSelect={setSelectedImageFile}
              preview={true}
            />
          </div>
        </div>

        <div>
          <Label htmlFor="description" className="text-sm font-medium text-slate-700">
            Description
          </Label>
          <Textarea
            id="description"
            value={projectData.description}
            onChange={(e) => handleInputChange("description", e.target.value)}
            placeholder="Describe your project..."
            className="mt-1"
            rows={3}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label htmlFor="demoUrl" className="text-sm font-medium text-slate-700">
              Demo URL
            </Label>
            <Input
              id="demoUrl"
              value={projectData.demoUrl}
              onChange={(e) => handleInputChange("demoUrl", e.target.value)}
              placeholder="https://your-demo.com"
              className="mt-1"
            />
          </div>

          <div>
            <Label htmlFor="githubUrl" className="text-sm font-medium text-slate-700">
              GitHub URL
            </Label>
            <Input
              id="githubUrl"
              value={projectData.githubUrl}
              onChange={(e) => handleInputChange("githubUrl", e.target.value)}
              placeholder="https://github.com/username/repo"
              className="mt-1"
            />
          </div>
        </div>

        <div>
          <Label htmlFor="technologies" className="text-sm font-medium text-slate-700">
            Technologies
          </Label>
          <Input
            id="technologies"
            value={projectData.technologies}
            onChange={(e) => handleInputChange("technologies", e.target.value)}
            placeholder="React, TypeScript, Node.js (comma-separated)"
            className="mt-1"
          />
        </div>

        <div className="flex items-center space-x-2">
          <Checkbox
            id="featured"
            checked={projectData.featured}
            onCheckedChange={(checked) => handleInputChange("featured", checked === true)}
          />
          <Label htmlFor="featured" className="text-sm font-medium text-slate-700">
            Featured Project
          </Label>
        </div>

        <div className="flex justify-end pt-4">
          <Button
            onClick={handleAddProject}
            disabled={loading}
            className="px-6 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg"
          >
            {loading ? "Adding..." : "Add Project"}
          </Button>
        </div>
      </div>
    </div>
  )
} 