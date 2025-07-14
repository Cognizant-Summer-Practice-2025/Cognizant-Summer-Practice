"use client"

interface PublishTabsProps {
  activeTab: string
  onTabChange: (tab: string) => void
}

export function PublishTabs({ activeTab, onTabChange }: PublishTabsProps) {
  const tabs = [
    { id: "basic-info", label: "Basic Info" },
    { id: "projects", label: "Projects" },
    { id: "blog-posts", label: "Blog Posts" },
    { id: "settings", label: "Settings" }
  ]

  return (
    <div className="w-full bg-white border border-slate-200 rounded-2xl p-1.5 flex items-center shadow-sm">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          onClick={() => onTabChange(tab.id)}
          className={`
            relative flex-1 px-6 py-3 rounded-xl text-sm font-medium transition-all duration-200 ease-in-out
            ${activeTab === tab.id
              ? "bg-slate-900 text-white shadow-lg shadow-slate-900/25 transform scale-[1.02]"
              : "text-slate-600 hover:text-slate-900 hover:bg-slate-50"
            }
          `}
        >
          <span className="relative z-10">{tab.label}</span>
          {activeTab === tab.id && (
            <div className="absolute inset-0 bg-gradient-to-r from-slate-900 to-slate-800 rounded-xl" />
          )}
        </button>
      ))}
    </div>
  )
} 