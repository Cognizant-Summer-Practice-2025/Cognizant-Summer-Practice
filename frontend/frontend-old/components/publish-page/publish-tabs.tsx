"use client"

interface PublishTabsProps {
  activeTab: string;
  onTabChange: (tab: string) => void;
}

export function PublishTabs({ activeTab, onTabChange }: PublishTabsProps) {
  const tabs = [
    { id: "basic-info", label: "Basic Info", icon: "👤" },
    { id: "projects", label: "Projects", icon: "💼" },
    { id: "experience", label: "Experience", icon: "🏢" },
    { id: "blog-posts", label: "Blog Posts", icon: "📝" },
    { id: "settings", label: "Settings", icon: "⚙️" },
  ];

  return (
    <div className="flex flex-col gap-3 md:gap-4">
      <div className="flex flex-wrap gap-2 md:gap-3">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => onTabChange(tab.id)}
            className={`px-4 py-2 md:px-6 md:py-3 rounded-lg border transition-colors ${
              activeTab === tab.id
                ? "bg-slate-900 text-white border-slate-900"
                : "bg-white text-slate-600 border-slate-200 hover:bg-slate-50"
            }`}
          >
            <span className="flex items-center gap-2 text-sm md:text-base">
              <span>{tab.icon}</span>
              {tab.label}
            </span>
          </button>
        ))}
      </div>
    </div>
  );
} 