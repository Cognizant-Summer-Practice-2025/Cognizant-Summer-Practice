"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"

export function AddProject() {
  const [projectData, setProjectData] = useState({
    title: "",
    imageUrl: "",
    description: "",
    demoUrl: "",
    githubUrl: ""
  })

  const handleInputChange = (field: string, value: string) => {
    setProjectData(prev => ({ ...prev, [field]: value }))
  }

  const handleAddProject = () => {
    // Handle project submission
    console.log("Adding project:", projectData)
  }

  return (
    <div className="w-full flex flex-col gap-4 md:gap-6">
      <div className="flex flex-col">
        <h2 className="text-slate-900 text-xl md:text-2xl font-semibold">Add New Project</h2>
      </div>
      
      <div className="px-4 py-4 md:px-6 md:py-6 bg-white rounded-lg border border-slate-200 flex flex-col gap-4 md:gap-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 flex flex-col gap-2">
            <Label htmlFor="project-title" className="text-slate-900 text-sm font-medium">
              Project Title
            </Label>
            <Input
              id="project-title"
              placeholder="My Awesome Project"
              value={projectData.title}
              onChange={(e) => handleInputChange("title", e.target.value)}
              className="p-3 rounded-lg border border-slate-200"
            />
          </div>
          
          <div className="flex-1 flex flex-col gap-2">
            <Label htmlFor="image-url" className="text-slate-900 text-sm font-medium">
              Image URL
            </Label>
            <Input
              id="image-url"
              placeholder="https://example.com/image.jpg"
              value={projectData.imageUrl}
              onChange={(e) => handleInputChange("imageUrl", e.target.value)}
              className="p-3 rounded-lg border border-slate-200"
            />
          </div>
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="project-description" className="text-slate-900 text-sm font-medium">
            Description
          </Label>
          <Textarea
            id="project-description"
            placeholder="Describe your project..."
            value={projectData.description}
            onChange={(e) => handleInputChange("description", e.target.value)}
            className="p-3 rounded-lg border border-slate-200 min-h-[80px] md:min-h-[100px]"
          />
        </div>

        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 flex flex-col gap-2">
            <Label htmlFor="demo-url" className="text-slate-900 text-sm font-medium">
              Demo URL
            </Label>
            <Input
              id="demo-url"
              placeholder="https://myproject.com"
              value={projectData.demoUrl}
              onChange={(e) => handleInputChange("demoUrl", e.target.value)}
              className="p-3 rounded-lg border border-slate-200"
            />
          </div>
          
          <div className="flex-1 flex flex-col gap-2">
            <Label htmlFor="github-url" className="text-slate-900 text-sm font-medium">
              GitHub URL
            </Label>
            <Input
              id="github-url"
              placeholder="https://github.com/username/repo"
              value={projectData.githubUrl}
              onChange={(e) => handleInputChange("githubUrl", e.target.value)}
              className="p-3 rounded-lg border border-slate-200"
            />
          </div>
        </div>

        <Button
          onClick={handleAddProject}
          className="w-full px-3 py-3 bg-slate-900 hover:bg-slate-800 rounded-lg"
        >
          <span className="text-white text-sm">➕ Add Project</span>
        </Button>
      </div>
    </div>
  )
} 