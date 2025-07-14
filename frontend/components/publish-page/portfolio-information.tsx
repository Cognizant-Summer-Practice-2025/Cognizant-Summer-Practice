"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { SkillTag } from "./skill-tag"

export function PortfolioInformation() {
  const [skills, setSkills] = useState(["React", "Node.js", "TypeScript", "GraphQL"])
  const [newSkill, setNewSkill] = useState("")

  const addSkill = () => {
    if (newSkill.trim() && !skills.includes(newSkill.trim())) {
      setSkills([...skills, newSkill.trim()])
      setNewSkill("")
    }
  }

  const removeSkill = (skillToRemove: string) => {
    setSkills(skills.filter(skill => skill !== skillToRemove))
  }

  return (
    <div className="w-full pb-6 md:pb-8 flex flex-col gap-4 md:gap-6">
      <div className="flex flex-col">
        <h2 className="text-slate-900 text-xl md:text-2xl font-semibold">Portfolio Information</h2>
      </div>
      
      <div className="px-4 py-4 md:px-6 md:py-6 bg-white rounded-lg border border-slate-200 flex flex-col gap-4 md:gap-6">
        <div className="flex flex-col gap-2">
          <Label htmlFor="title" className="text-slate-900 text-sm font-medium">
            Portfolio Title
          </Label>
          <Input
            id="title"
            defaultValue="Alex Johnson - Full Stack Developer"
            className="p-3 rounded-lg border border-slate-200"
          />
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="description" className="text-slate-900 text-sm font-medium">
            Description
          </Label>
          <Textarea
            id="description"
            defaultValue="Crafting digital experiences with modern web technologies"
            className="p-3 rounded-lg border border-slate-200 min-h-[60px] md:min-h-[80px]"
          />
        </div>

        <div className="flex flex-col gap-2">
          <Label className="text-slate-900 text-sm font-medium">
            Skills & Technologies
          </Label>
          
          <div className="flex flex-col sm:flex-row gap-2">
            <Input
              value={newSkill}
              onChange={(e) => setNewSkill(e.target.value)}
              placeholder="Add a skill..."
              className="flex-1 p-3 rounded-lg border border-slate-200"
              onKeyPress={(e) => e.key === 'Enter' && addSkill()}
            />
            <Button
              onClick={addSkill}
              className="px-3 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg w-full sm:w-auto"
            >
              <span className="text-white text-sm">➕</span>
            </Button>
          </div>

          <div className="flex flex-wrap gap-2 pt-2">
            {skills.map((skill) => (
              <SkillTag key={skill} skill={skill} onRemove={removeSkill} />
            ))}
          </div>
        </div>
      </div>
    </div>
  )
} 