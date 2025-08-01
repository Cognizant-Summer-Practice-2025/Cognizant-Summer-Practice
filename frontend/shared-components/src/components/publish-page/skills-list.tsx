"use client"

import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useDraft } from "@/lib/contexts/draft-context"

export function SkillsList() {
  const { getUserSkills } = usePortfolio()
  const { draftSkills, deleteDraftSkill } = useDraft()
  const skills = getUserSkills()

  if (skills.length === 0 && draftSkills.length === 0) {
    return (
      <div className="border border-slate-200 rounded-lg p-6 bg-gray-50">
        <p className="text-sm text-gray-500 text-center">
          No skills added yet. Add your first skill above.
        </p>
      </div>
    )
  }

  const getProficiencyColor = (level: number) => {
    if (level >= 80) return 'bg-green-500'
    if (level >= 60) return 'bg-blue-500'
    if (level >= 40) return 'bg-yellow-500'
    return 'bg-red-500'
  }

  const getProficiencyLabel = (level: number) => {
    if (level >= 80) return 'Expert'
    if (level >= 60) return 'Advanced'
    if (level >= 40) return 'Intermediate'
    return 'Beginner'
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-slate-900">Your Skills</h3>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {/* Existing Skills */}
        {skills.map((skill) => (
          <div key={skill.id} className="border border-slate-200 rounded-lg p-4 bg-white hover:shadow-md transition-shadow">
            <div className="flex justify-between items-start mb-2">
              <h4 className="font-medium text-slate-900">{skill.name}</h4>
              <span className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                Published
              </span>
            </div>
            <p className="text-sm text-gray-600 mb-3">{skill.category}</p>
            <div className="space-y-1">
              <div className="flex justify-between text-sm">
                <span>{getProficiencyLabel(skill.proficiencyLevel || 0)}</span>
                <span>{skill.proficiencyLevel || 0}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full ${getProficiencyColor(skill.proficiencyLevel || 0)}`}
                  style={{ width: `${skill.proficiencyLevel || 0}%` }}
                ></div>
              </div>
            </div>
          </div>
        ))}

        {/* Draft Skills */}
        {draftSkills.map((skill) => (
          <div key={skill.id} className="border border-amber-200 rounded-lg p-4 bg-amber-50 hover:shadow-md transition-shadow">
            <div className="flex justify-between items-start mb-2">
              <h4 className="font-medium text-slate-900">{skill.name}</h4>
              <div className="flex items-center gap-2">
                <span className="px-2 py-1 bg-amber-100 text-amber-800 text-xs rounded-full">
                  Draft
                </span>
                <button
                  onClick={() => deleteDraftSkill(skill.id)}
                  className="text-red-600 hover:text-red-800 text-xs"
                >
                  Remove
                </button>
              </div>
            </div>
            <p className="text-sm text-gray-600 mb-3">{skill.category}</p>
            <div className="space-y-1">
              <div className="flex justify-between text-sm">
                <span>{getProficiencyLabel(skill.proficiencyLevel)}</span>
                <span>{skill.proficiencyLevel}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className={`h-2 rounded-full ${getProficiencyColor(skill.proficiencyLevel)}`}
                  style={{ width: `${skill.proficiencyLevel}%` }}
                ></div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
} 