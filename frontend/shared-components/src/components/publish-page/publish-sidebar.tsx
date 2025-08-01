"use client"

import { usePortfolio } from "@/lib/contexts/portfolio-context"

interface PublishSidebarProps {
  totalProjects: number
  totalExperience: number
  totalSkills: number
  totalBlogPosts: number
  hasDrafts: boolean
}

export function PublishSidebar({ totalProjects, totalExperience, totalSkills, totalBlogPosts, hasDrafts }: PublishSidebarProps) {
  const { getUserPortfolios } = usePortfolio()
  
  const portfolios = getUserPortfolios()
  const currentPortfolio = portfolios[0]
  
  const isPublished = currentPortfolio?.isPublished || false

  return (
    <div className="bg-white rounded-lg shadow-sm border p-6 space-y-6">
      {/* Publication Status */}
      <div>
        <h3 className="text-lg font-semibold mb-3">Publication Status</h3>
        <div className="flex items-center gap-2 mb-4">
          <div className={`w-3 h-3 rounded-full ${isPublished ? 'bg-green-500' : 'bg-gray-400'}`}></div>
          <span className="text-sm font-medium">
            {isPublished ? '🌐 Published' : '📝 Draft'}
          </span>
        </div>
        
        {hasDrafts && (
          <div className="mb-4 p-3 bg-amber-50 border border-amber-200 rounded-lg">
            <p className="text-sm text-amber-700">
              📝 You have unsaved changes that will be saved when you publish.
            </p>
          </div>
        )}
      </div>

      {/* Content Overview */}
      <div>
        <h3 className="text-lg font-semibold mb-3">Content Overview</h3>
        <div className="space-y-3">
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-600">💼 Projects</span>
            <span className="text-sm font-medium">{totalProjects}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-600">🏢 Experience</span>
            <span className="text-sm font-medium">{totalExperience}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-600">🔧 Skills</span>
            <span className="text-sm font-medium">{totalSkills}</span>
          </div>
          <div className="flex justify-between items-center">
            <span className="text-sm text-gray-600">📝 Blog Posts (Optional)</span>
            <span className="text-sm font-medium">{totalBlogPosts}</span>
          </div>
        </div>
      </div>

      {/* Portfolio Completion */}
      <div>
        <h3 className="text-lg font-semibold mb-3">Portfolio Completion</h3>
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <span className={`text-sm ${totalProjects > 0 ? 'text-green-600' : 'text-gray-400'}`}>
              {totalProjects > 0 ? '✅' : '⬜'} Projects Added
            </span>
          </div>
          <div className="flex items-center gap-2">
            <span className={`text-sm ${totalExperience > 0 ? 'text-green-600' : 'text-gray-400'}`}>
              {totalExperience > 0 ? '✅' : '⬜'} Experience Added
            </span>
          </div>
          <div className="flex items-center gap-2">
            <span className={`text-sm ${totalSkills > 0 ? 'text-green-600' : 'text-gray-400'}`}>
              {totalSkills > 0 ? '✅' : '⬜'} Skills Added
            </span>
          </div>
          <div className="flex items-center gap-2">
            <span className={`text-sm ${totalBlogPosts > 0 ? 'text-green-600' : 'text-gray-400'}`}>
              {totalBlogPosts > 0 ? '✅' : '⬜'} Blog Posts Added
            </span>
          </div>
        </div>
      </div>

      {/* Performance (only show if published) */}
      {isPublished && currentPortfolio && (
        <div>
          <h3 className="text-lg font-semibold mb-3">Performance</h3>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">👀 Views</span>
              <span className="text-sm font-medium">{currentPortfolio.viewCount || 0}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">👍 Likes</span>
              <span className="text-sm font-medium">{currentPortfolio.likeCount || 0}</span>
            </div>
          </div>
        </div>
      )}
    </div>
  )
} 