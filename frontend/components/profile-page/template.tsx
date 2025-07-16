"use client";

import { useState } from "react";
import { PORTFOLIO_TEMPLATES } from "@/lib/templates";
import { TemplateConfig } from "@/lib/interfaces";

const templates: TemplateConfig[] = PORTFOLIO_TEMPLATES;

export default function Template() {
  const [selectedTemplate, setSelectedTemplate] = useState("gabriel-barzu");

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] overflow-hidden">
      <div className="flex flex-col gap-4 sm:gap-6">

        <div className="flex flex-col">
          <h1 className="text-xl sm:text-2xl font-semibold text-[#020817] leading-tight sm:leading-[38.4px] mb-1">
            Portfolio Template
          </h1>
          <p className="text-sm text-[#64748B] leading-[22.4px]">
            Choose a template for your portfolio
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6 w-full">
          {templates.map((template) => (
            <div
              key={template.id}
              onClick={() => setSelectedTemplate(template.id)}
              className={`relative cursor-pointer rounded-lg p-[1px] transition-all ${
                selectedTemplate === template.id
                  ? "outline outline-1 outline-[#2563EB] outline-offset-[-1px] shadow-[0px_0px_0px_2px_rgba(37,99,235,0.20)]"
                  : "outline outline-1 outline-[#E2E8F0] outline-offset-[-1px] hover:outline-[#2563EB] hover:shadow-[0px_0px_0px_2px_rgba(37,99,235,0.10)]"
              }`}
            >
              <div className="bg-white rounded-lg overflow-hidden flex flex-col h-full">
                {/* Template Image */}
                <div className="relative h-[120px] sm:h-[150px] bg-gray-100 flex items-center justify-center">
                  <img 
                    src={template.previewImage} 
                    alt={template.name}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.style.display = 'none';
                      const fallback = document.createElement('div');
                      fallback.className = 'w-full h-full bg-gradient-to-br from-gray-200 to-gray-300 flex items-center justify-center';
                      fallback.innerHTML = `<span class="text-gray-500 text-xs sm:text-sm">${template.name} Preview</span>`;
                      target.parentElement!.appendChild(fallback);
                    }}
                  />
                </div>
                
                {/* Template Info */}
                <div className="flex flex-col gap-[3px] p-3 sm:p-4 pt-3 sm:pt-[15px] pb-3 sm:pb-4">
                  <div className="pb-[0.59px]">
                    <h3 className="text-sm sm:text-base font-semibold text-[#020817] leading-tight sm:leading-[25.6px]">
                      {template.name}
                    </h3>
                  </div>
                  <div>
                    <p className="text-xs sm:text-sm text-[#64748B] leading-[22.4px]">
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