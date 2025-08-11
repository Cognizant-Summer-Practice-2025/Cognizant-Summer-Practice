"use client"

import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useDraft } from "@/lib/contexts/draft-context"

export function ExperienceList() {
  const { getUserExperience } = usePortfolio()
  const { draftExperience, deleteDraftExperience } = useDraft()
  const experience = getUserExperience()

  if (experience.length === 0 && draftExperience.length === 0) {
    return (
      <div className="border border-slate-200 rounded-lg p-6 bg-gray-50">
        <p className="text-sm text-gray-500 text-center">
          No experience added yet. Add your first experience above.
        </p>
      </div>
    )
  }

  const formatDate = (dateString: string) => {
    if (!dateString) return ''
    return new Date(dateString).toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short' 
    })
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-slate-900">Your Experience</h3>
      
      {/* Existing Experience */}
      {experience.map((exp) => (
        <div key={exp.id} className="border border-slate-200 rounded-lg p-4 bg-white">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{exp.jobTitle}</h4>
                <span className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                  Published
                </span>
              </div>
              <p className="text-sm text-blue-600 mb-2">{exp.companyName}</p>
              <p className="text-xs text-gray-500 mb-3">
                {formatDate(exp.startDate)} - {exp.isCurrent ? 'Present' : formatDate(exp.endDate || '')}
              </p>
              {exp.description && (
                <p className="text-sm text-gray-600 mb-3">{exp.description}</p>
              )}
              {exp.skillsUsed && exp.skillsUsed.length > 0 && (
                <div className="flex flex-wrap gap-1">
                  {exp.skillsUsed.map((skill, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      ))}

      {/* Draft Experience */}
      {draftExperience.map((exp) => (
        <div key={exp.id} className="border border-amber-200 rounded-lg p-4 bg-amber-50">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{exp.jobTitle}</h4>
                <span className="px-2 py-1 bg-amber-100 text-amber-800 text-xs rounded-full">
                  Draft
                </span>
              </div>
              <p className="text-sm text-blue-600 mb-2">{exp.companyName}</p>
              <p className="text-xs text-gray-500 mb-3">
                {formatDate(exp.startDate)} - {exp.isCurrent ? 'Present' : formatDate(exp.endDate)}
              </p>
              {exp.description && (
                <p className="text-sm text-gray-600 mb-3">{exp.description}</p>
              )}
              {exp.skillsUsed && (
                <div className="flex flex-wrap gap-1">
                  {exp.skillsUsed.split(',').map((skill, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {skill.trim()}
                    </span>
                  ))}
                </div>
              )}
            </div>
            <button
              onClick={() => deleteDraftExperience(exp.id)}
              className="ml-4 text-red-600 hover:text-red-800 text-sm"
            >
              Remove
            </button>
          </div>
        </div>
      ))}
    </div>
  )
} 