"use client"

import { useState } from "react"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"

export function PortfolioSettings() {
  const [isPublic, setIsPublic] = useState(true)
  const [template, setTemplate] = useState("modern")
  const [customCSS, setCustomCSS] = useState("")

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
            onClick={() => setIsPublic(!isPublic)}
            className={`w-11 h-6 rounded-full relative transition-all flex-shrink-0 ${
              isPublic ? "bg-blue-600" : "bg-slate-300"
            }`}
          >
            <div
              className={`w-4.5 h-4.5 bg-white rounded-full absolute top-0.75 transition-all ${
                isPublic ? "left-5.75" : "left-0.75"
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
              <SelectItem value="modern">Modern</SelectItem>
              <SelectItem value="classic">Classic</SelectItem>
              <SelectItem value="minimal">Minimal</SelectItem>
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
    </div>
  )
} 