"use client"

interface Project {
  id: string
  title: string
  description: string
}

export function ProjectsList() {
  const projects: Project[] = [
    {
      id: "1",
      title: "E-commerce Platform",
      description: "A full-featured e-commerce platform built with React and Node.js"
    },
    {
      id: "2", 
      title: "Task Management App",
      description: "A collaborative task management application with real-time updates"
    }
  ]

  return (
    <div className="w-full px-4 py-4 md:px-6 md:py-8 bg-white rounded-lg border border-slate-200 flex flex-col gap-3 md:gap-4">
      <div className="pb-1 flex flex-col">
        <h3 className="text-slate-900 text-lg font-semibold">Your Projects</h3>
      </div>
      
      {projects.map((project) => (
        <div
          key={project.id}
          className="px-3 py-3 md:px-4 md:py-4 bg-white rounded-lg border border-slate-200 flex flex-col sm:flex-row sm:justify-between sm:items-center gap-3 sm:gap-0"
        >
          <div className="flex flex-col gap-1 min-w-0 flex-1">
            <div className="pb-1 flex flex-col">
              <h4 className="text-slate-900 text-base font-semibold truncate">{project.title}</h4>
            </div>
            <div className="flex flex-col">
              <p className="text-slate-500 text-sm leading-relaxed">{project.description}</p>
            </div>
          </div>
          
          <button className="p-2 rounded-lg flex flex-col justify-center items-center hover:bg-slate-50 transition-colors self-start sm:self-center">
            <span className="text-slate-500 text-sm">⋯</span>
          </button>
        </div>
      ))}
    </div>
  )
} 