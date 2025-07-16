"use client"

import { useState } from "react"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { PORTFOLIO_TEMPLATES } from "@/lib/templates"
import { ComponentOrdering } from "@/components/ui/component-ordering"
import { ComponentConfig } from "@/lib/interfaces"

export function PortfolioSettings() {
  const [visibility, setVisibility] = useState(0) // 0=Public, 1=Private, 2=Unlisted
  const [template, setTemplate] = useState("gabriel-barzu")
  const [customCSS, setCustomCSS] = useState("")
  
  // Mock component configuration - in real app, this would come from API
  const [components, setComponents] = useState<ComponentConfig[]>([
    { id: '1', type: 'about', order: 1, isVisible: true },
    { id: '2', type: 'experience', order: 2, isVisible: true },
    { id: '3', type: 'projects', order: 3, isVisible: true },
    { id: '4', type: 'skills', order: 4, isVisible: true },
    { id: '5', type: 'blog_posts', order: 5, isVisible: false },
    { id: '6', type: 'contact', order: 6, isVisible: true }
  ])

  return (
    <div className="w-full pb-6 md:pb-8 flex flex-col gap-4 md:gap-6">
      <div className="flex flex-col">
        <h2 className="text-slate-900 text-xl md:text-2xl font-semibold">Portfolio Settings</h2>
      </div>
      
      <div className="px-4 py-4 md:px-6 md:py-6 bg-white rounded-lg border border-slate-200 flex flex-col gap-4 md:gap-6">
        <div className="py-3 md:py-4 border-b border-slate-200 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0">
          <div className="flex flex-col gap-1">
            <div className="pb-1 flex flex-col">
              <h4 className="text-slate-900 text-base font-medium">Make Portfolio Public</h4>
            </div>
            <div className="flex flex-col">
              <p className="text-slate-500 text-sm">Allow others to discover and view your portfolio</p>
            </div>
          </div>
          
          <button
            onClick={() => setVisibility(visibility === 0 ? 1 : 0)}
            className={`w-11 h-6 rounded-full relative transition-all flex-shrink-0 ${
              visibility === 0 ? "bg-blue-600" : "bg-slate-300"
            }`}
          >
            <div
              className={`w-4.5 h-4.5 bg-white rounded-full absolute top-0.75 transition-all ${
                visibility === 0 ? "left-5.75" : "left-0.75"
              }`}
            />
          </button>
        </div>

        <div className="flex flex-col gap-2">
          <Label className="text-slate-900 text-sm font-medium">
            Template
          </Label>
          <Select value={template} onValueChange={setTemplate}>
            <SelectTrigger className="p-3 rounded-lg border border-slate-200">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PORTFOLIO_TEMPLATES.map((templateOption) => (
                <SelectItem key={templateOption.id} value={templateOption.id}>
                  {templateOption.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="pt-4 md:pt-6 flex flex-col gap-2">
          <Label htmlFor="custom-css" className="text-slate-900 text-sm font-medium">
            Custom CSS (Advanced)
          </Label>
          <Textarea
            id="custom-css"
            placeholder="/* Add your custom styles here */"
            value={customCSS}
            onChange={(e) => setCustomCSS(e.target.value)}
            className="p-3 rounded-lg border border-slate-200 min-h-[120px] md:min-h-[160px] font-mono text-sm"
          />
        </div>
      </div>
      
      {/* Portfolio Sections Ordering */}
      <ComponentOrdering 
        components={components}
        onComponentsChange={setComponents}
      />
    </div>
  )
} 