"use client";

import { useState } from "react";

interface Template {
  id: string;
  name: string;
  description: string;
  image: string;
}

const templates: Template[] = [
  {
    id: "modern",
    name: "Modern",
    description: "Clean and minimal design",
    image: "/placeholder-modern.jpg"
  },
  {
    id: "creative", 
    name: "Creative",
    description: "Bold and artistic layout",
    image: "/placeholder-creative.jpg"
  },
  {
    id: "professional",
    name: "Professional", 
    description: "Corporate and structured",
    image: "/placeholder-professional.jpg"
  }
];

export default function Template() {
  const [selectedTemplate, setSelectedTemplate] = useState("modern");

  return (
    <div className="bg-white rounded-lg shadow-sm p-8 w-full min-h-[600px] overflow-hidden">
      <div className="flex flex-col gap-6">

        <div className="flex flex-col">
          <h1 className="text-2xl font-semibold text-[#020817] leading-[38.4px] mb-1">
            Portfolio Template
          </h1>
          <p className="text-sm text-[#64748B] leading-[22.4px]">
            Choose a template for your portfolio
          </p>
        </div>

        <div className="flex gap-6 w-full">
          {templates.map((template) => (
            <div
              key={template.id}
              onClick={() => setSelectedTemplate(template.id)}
              className={`flex-1 relative cursor-pointer rounded-lg p-[1px] transition-all ${
                selectedTemplate === template.id
                  ? "outline outline-1 outline-[#2563EB] outline-offset-[-1px] shadow-[0px_0px_0px_2px_rgba(37,99,235,0.20)]"
                  : "outline outline-1 outline-[#E2E8F0] outline-offset-[-1px] hover:outline-[#2563EB] hover:shadow-[0px_0px_0px_2px_rgba(37,99,235,0.10)]"
              }`}
            >
              <div className="bg-white rounded-lg overflow-hidden flex flex-col h-full">
                {/* Template Image */}
                <div className="relative h-[150px] bg-gray-100 flex items-center justify-center">
                  <div className="w-full h-full bg-gradient-to-br from-gray-200 to-gray-300 flex items-center justify-center">
                    <span className="text-gray-500 text-sm">Template Preview</span>
                  </div>
                </div>
                
                {/* Template Info */}
                <div className="flex flex-col gap-[3px] p-4 pt-[15px] pb-4">
                  <div className="pb-[0.59px]">
                    <h3 className="text-base font-semibold text-[#020817] leading-[25.6px]">
                      {template.name}
                    </h3>
                  </div>
                  <div>
                    <p className="text-sm text-[#64748B] leading-[22.4px]">
                      {template.description}
                    </p>
                  </div>
                </div>
              </div>

              {/* Current Badge */}
              {selectedTemplate === template.id && (
                <div className="absolute top-2 right-2 bg-[#2563EB] rounded-md px-2 py-[3.5px] z-10">
                  <span className="text-xs font-medium text-[#F8FAFC] leading-[19.2px]">
                    Current
                  </span>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
} 