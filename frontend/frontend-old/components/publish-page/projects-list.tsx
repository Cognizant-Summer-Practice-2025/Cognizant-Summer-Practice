"use client"

import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useDraft } from "@/lib/contexts/draft-context"

export function ProjectsList() {
  const { getUserProjects } = usePortfolio()
  const { draftProjects, deleteDraftProject } = useDraft()
  const projects = getUserProjects()

  if (projects.length === 0 && draftProjects.length === 0) {
    return (
      <div className="border border-slate-200 rounded-lg p-6 bg-gray-50">
        <p className="text-sm text-gray-500 text-center">
          No projects added yet. Add your first project above.
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold text-slate-900">Your Projects</h3>
      
      {/* Existing Projects */}
      {projects.map((project) => (
        <div key={project.id} className="border border-slate-200 rounded-lg p-4 bg-white">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{project.title}</h4>
                {project.featured && (
                  <span className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded-full">
                    Featured
                  </span>
                )}
                <span className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                  Published
                </span>
              </div>
              {project.description && (
                <p className="text-sm text-gray-600 mb-3">{project.description}</p>
              )}
              <div className="flex flex-wrap gap-2 text-xs">
                {project.demoUrl && (
                  <a 
                    href={project.demoUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:underline"
                  >
                    Demo →
                  </a>
                )}
                {project.githubUrl && (
                  <a 
                    href={project.githubUrl} 
                    target="_blank" 
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:underline"
                  >
                    GitHub →
                  </a>
                )}
              </div>
              {project.technologies && project.technologies.length > 0 && (
                <div className="flex flex-wrap gap-1 mt-2">
                  {project.technologies.map((tech, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {tech}
                    </span>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      ))}

      {/* Draft Projects */}
      {draftProjects.map((project) => (
        <div key={project.id} className="border border-amber-200 rounded-lg p-4 bg-amber-50">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-2">
                <h4 className="font-medium text-slate-900">{project.title}</h4>
                {project.featured && (
                  <span className="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded-full">
                    Featured
                  </span>
                )}
                <span className="px-2 py-1 bg-amber-100 text-amber-800 text-xs rounded-full">
                  Draft
                </span>
              </div>
              {project.description && (
                <p className="text-sm text-gray-600 mb-3">{project.description}</p>
              )}
              <div className="flex flex-wrap gap-2 text-xs">
                {project.demoUrl && (
                  <span className="text-gray-500">Demo: {project.demoUrl}</span>
                )}
                {project.githubUrl && (
                  <span className="text-gray-500">GitHub: {project.githubUrl}</span>
                )}
              </div>
              {project.technologies && (
                <div className="flex flex-wrap gap-1 mt-2">
                  {project.technologies.split(',').map((tech, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-slate-100 text-slate-700 text-xs rounded"
                    >
                      {tech.trim()}
                    </span>
                  ))}
                </div>
              )}
            </div>
            <button
              onClick={() => deleteDraftProject(project.id)}
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