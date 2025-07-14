"use client"

import { useState } from "react"
import { PublishHeader } from "@/components/publish-page/publish-header"
import { PublishTabs } from "@/components/publish-page/publish-tabs"
import { PortfolioInformation } from "@/components/publish-page/portfolio-information"
import { AddProject } from "@/components/publish-page/add-project"
import { ProjectsList } from "@/components/publish-page/projects-list"
import { AddBlogPost } from "@/components/publish-page/add-blog-post"
import { BlogPostsList } from "@/components/publish-page/blog-posts-list"
import { PortfolioSettings } from "@/components/publish-page/portfolio-settings"
import { PublishSidebar } from "@/components/publish-page/publish-sidebar"

export default function Publish() {
  const [activeTab, setActiveTab] = useState("basic-info")

  const renderTabContent = () => {
    switch (activeTab) {
      case "basic-info":
        return <PortfolioInformation />
      case "projects":
        return (
          <div className="flex flex-col gap-4 md:gap-6">
            <AddProject />
            <ProjectsList />
          </div>
        )
      case "blog-posts":
        return (
          <div className="flex flex-col gap-4 md:gap-6">
            <AddBlogPost />
            <BlogPostsList />
          </div>
        )
      case "settings":
        return <PortfolioSettings />
      default:
        return <PortfolioInformation />
    }
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <PublishHeader />
      
      <div className="px-4 py-4 md:px-6 md:py-6 lg:px-8 lg:py-8">
        <div className="max-w-7xl mx-auto">
            {/* Mobile and Tablet Layout */}
           <div className="block xl:hidden">
             <PublishTabs activeTab={activeTab} onTabChange={setActiveTab} />
             <div className="mt-12">
               {renderTabContent()}
             </div>
           </div>
           
           {/* Desktop Layout */}
           <div className="hidden xl:flex xl:gap-4">
             {/* Main Content */}
             <div className="flex-1">
               <PublishTabs activeTab={activeTab} onTabChange={setActiveTab} />
               <div className="mt-16">
                 {renderTabContent()}
               </div>
             </div>
            
            {/* Sidebar */}
            <div className="w-80 flex-shrink-0">
              <PublishSidebar />
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}    