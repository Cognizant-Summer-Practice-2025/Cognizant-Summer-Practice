"use client"

import { useState } from "react"
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs"
import { PublishHeader } from "@/components/publish-page/publish-header"
import { PortfolioInformation } from "@/components/publish-page/portfolio-information"
import { AddProject } from "@/components/publish-page/add-project"
import { ProjectsList } from "@/components/publish-page/projects-list"
import { AddExperience } from "@/components/publish-page/add-experience"
import { ExperienceList } from "@/components/publish-page/experience-list"
import { AddSkills } from "@/components/publish-page/add-skills"
import { SkillsList } from "@/components/publish-page/skills-list"
import { AddBlogPost } from "@/components/publish-page/add-blog-post"
import { BlogPostsList } from "@/components/publish-page/blog-posts-list"
import { PortfolioSettings } from "@/components/publish-page/portfolio-settings"
import { PublishSidebar } from "@/components/publish-page/publish-sidebar"
import { LoadingModal } from "@/components/ui/loading-modal"
import { usePortfolio } from "@/lib/contexts/portfolio-context"
import { useDraft } from "@/lib/contexts/draft-context"
import { useUser } from "@/lib/contexts/user-context"
import { TemplateManager } from "@/lib/template-manager"
import { updatePortfolio, savePortfolioContent, createPortfolioAndGetId } from "@/lib/portfolio/api"

export default function Publish() {
  const [publishing, setPublishing] = useState(false)
  const [publishError, setPublishError] = useState<string | null>(null)
  const [showLoadingModal, setShowLoadingModal] = useState(false)
  
  const { user } = useUser()
  
  const { 
    getUserProjects, 
    getUserExperience, 
    getUserSkills, 
    getUserBlogPosts,
    getUserPortfolios,
    refreshUserPortfolios
  } = usePortfolio()

  const {
    draftProjects,
    draftExperience, 
    draftBlogPosts,
    draftSkills,
    clearAllDrafts,
    hasDraftData
  } = useDraft()

  const projects = getUserProjects()
  const experience = getUserExperience()
  const skills = getUserSkills()
  const blogPosts = getUserBlogPosts()
  const portfolios = getUserPortfolios()
  
  const currentPortfolio = portfolios[0]

  // Combined data (existing + draft)
  const totalProjects = projects.length + draftProjects.length
  const totalExperience = experience.length + draftExperience.length
  const totalSkills = skills.length + draftSkills.length
  const totalBlogPosts = blogPosts.length + draftBlogPosts.length

  const validatePortfolioData = () => {
    const errors = []
    
    // Check if at least one item exists in each category (including drafts)
    if (totalProjects === 0) {
      errors.push("Add at least one project")
    }
    if (totalExperience === 0) {
      errors.push("Add at least one experience")
    }
    if (totalSkills === 0) {
      errors.push("Add at least one skill")
    }
    if (totalBlogPosts === 0) {
      errors.push("Add at least one blog post")
    }
    
    return errors
  }

  const handlePublish = async () => {
    console.log('🚀 Starting publish process...')
    console.log('Current portfolio:', currentPortfolio)
    
    const validationErrors = validatePortfolioData()
    if (validationErrors.length > 0) {
      console.log('❌ Validation failed:', validationErrors)
      setPublishError(`Please complete your portfolio:\n• ${validationErrors.join('\n• ')}`)
      return
    }

    console.log('✅ Validation passed, starting publish...')
    setPublishing(true)
    setShowLoadingModal(true)
    setPublishError(null)

    try {
      // Check if we have draft data to save
      if (hasDraftData()) {
        console.log('Publishing draft data using split approach...')
        
        let portfolioId: string;
        
        // If no current portfolio exists, create a new one first
        if (!currentPortfolio) {
          console.log('Creating new portfolio...')
          // Create basic portfolio data
          const portfolioData = {
            userId: user?.id || 'default-user-id',
            templateName: 'Gabriel Bârzu', // Default template name
            title: 'My Portfolio',
            bio: 'Welcome to my portfolio',
            visibility: 0 as 0 | 1 | 2, // Public
            isPublished: false, // Will be set to true after content is saved
            components: JSON.stringify(TemplateManager.createDefaultComponentConfig())
          }
          
          portfolioId = await createPortfolioAndGetId(portfolioData);
          console.log('Created new portfolio with ID:', portfolioId);
        } else {
          portfolioId = currentPortfolio.id;
          console.log('Using existing portfolio ID:', portfolioId);
        }
        
        // Prepare projects data
        const projects = draftProjects.map(project => ({
          portfolioId: portfolioId,
          title: project.title,
          imageUrl: project.imageUrl || '',
          description: project.description || '',
          demoUrl: project.demoUrl || '',
          githubUrl: project.githubUrl || '',
          technologies: project.technologies ? project.technologies.split(',').map(t => t.trim()).filter(t => t.length > 0) : [],
          featured: project.featured
        }))

        // Prepare experience data
        const experience = draftExperience.map(exp => ({
          portfolioId: portfolioId,
          jobTitle: exp.jobTitle,
          companyName: exp.companyName,
          startDate: exp.startDate,
          endDate: exp.isCurrent ? undefined : exp.endDate,
          isCurrent: exp.isCurrent,
          description: exp.description || '',
          skillsUsed: exp.skillsUsed ? exp.skillsUsed.split(',').map(s => s.trim()).filter(s => s.length > 0) : []
        }))

        // Prepare skills data
        const skillsData = draftSkills.map((skill, index) => ({
          portfolioId: portfolioId,
          name: skill.name,
          categoryType: skill.categoryType,
          subcategory: skill.subcategory,
          category: skill.category || '',
          proficiencyLevel: skill.proficiencyLevel,
          displayOrder: skills.length + index + 1
        }))

        // Prepare blog posts data
        const blogPosts = draftBlogPosts.map(blog => ({
          portfolioId: portfolioId,
          title: blog.title,
          content: blog.content || '',
          featuredImageUrl: blog.featuredImageUrl || '',
          tags: blog.tags ? blog.tags.split(',').map(t => t.trim()).filter(t => t.length > 0) : [],
          isPublished: blog.publishImmediately
        }))

        // Use the new split approach to save content
        const contentData = {
          projects: projects.length > 0 ? projects : undefined,
          experience: experience.length > 0 ? experience : undefined,
          skills: skillsData.length > 0 ? skillsData : undefined,
          blogPosts: blogPosts.length > 0 ? blogPosts : undefined,
          publishPortfolio: true
        }

        console.log('Saving portfolio content using split approach:', contentData)
        const result = await savePortfolioContent(portfolioId, contentData)
        console.log('Save content result:', result)

        // Clear drafts after successful save
        clearAllDrafts()
      } else {
        // Handle case when no draft data exists
        if (!currentPortfolio) {
          console.log('No portfolio exists and no draft data. Creating basic portfolio...')
          const portfolioData = {
            userId: user?.id || 'default-user-id',
            templateName: 'Gabriel Bârzu', // Default template name
            title: 'My Portfolio',
            bio: 'Welcome to my portfolio',
            visibility: 0 as 0 | 1 | 2, // Public
            isPublished: true, // Publish immediately since no content to add
            components: JSON.stringify(TemplateManager.createDefaultComponentConfig())
          }
          
          await createPortfolioAndGetId(portfolioData);
        } else {
          // Just publish the existing portfolio if no drafts
          console.log('Publishing existing portfolio...')
          const updatedPortfolio = {
            isPublished: true
          }
          
          await updatePortfolio(currentPortfolio.id, updatedPortfolio)
        }
      }

      await refreshUserPortfolios()
      console.log('✅ Portfolio published successfully!')
      console.log('🎉 All processes completed!')
      
      // Show success message
      setPublishError(null)
      
    } catch (error) {
      console.error('Error publishing portfolio:', error)
      let errorMessage = 'Failed to publish portfolio. Please try again.'
      
      // Try to extract more specific error information
      if (error instanceof Error) {
        errorMessage = error.message
      } else if (typeof error === 'object' && error !== null && 'response' in error) {
        try {
          const response = (error as { response: Response }).response
          const errorData = await response.json()
          errorMessage = errorData.message || errorData.title || errorMessage
        } catch {
          const response = (error as { response: Response }).response
          errorMessage = `Server error: ${response.status} ${response.statusText}`
        }
      }
      
      setPublishError(errorMessage)
    } finally {
      console.log('📝 Cleaning up publish process...')
      setPublishing(false)
      setShowLoadingModal(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Fixed header positioned under app header */}
      <div className="fixed top-16 left-0 right-0 z-40">
        <PublishHeader onPublish={handlePublish} publishing={publishing} />
      </div>
      
      {/* Main content with proper top margin */}
      <div className="pt-32 pb-6">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
            {/* Main Content */}
            <div className="lg:col-span-3">
              <div className="bg-white rounded-lg shadow-sm border p-6">
                {publishError && (
                  <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
                    <p className="text-sm text-red-600 whitespace-pre-line">{publishError}</p>
                  </div>
                )}
                
                <Tabs defaultValue="basic-info" className="w-full">
                  <TabsList className="grid w-full grid-cols-6 mb-6">
                    <TabsTrigger value="basic-info">Basic Info</TabsTrigger>
                    <TabsTrigger value="projects">Projects</TabsTrigger>
                    <TabsTrigger value="experience">Experience</TabsTrigger>
                    <TabsTrigger value="skills">Skills</TabsTrigger>
                    <TabsTrigger value="blog-posts">Blog Posts</TabsTrigger>
                    <TabsTrigger value="settings">Settings</TabsTrigger>
                  </TabsList>

                  <TabsContent value="basic-info" className="space-y-6">
                    <PortfolioInformation />
                  </TabsContent>

                  <TabsContent value="projects" className="space-y-6">
                    <div className="space-y-6">
                      <AddProject />
                      <ProjectsList />
                    </div>
                  </TabsContent>

                  <TabsContent value="experience" className="space-y-6">
                    <div className="space-y-6">
                      <AddExperience />
                      <ExperienceList />
                    </div>
                  </TabsContent>

                  <TabsContent value="skills" className="space-y-6">
                    <div className="space-y-6">
                      <AddSkills />
                      <SkillsList />
                    </div>
                  </TabsContent>

                  <TabsContent value="blog-posts" className="space-y-6">
                    <div className="space-y-6">
                      <AddBlogPost />
                      <BlogPostsList />
                    </div>
                  </TabsContent>

                  <TabsContent value="settings" className="space-y-6">
                    <PortfolioSettings 
                      portfolioId={currentPortfolio?.id}
                      onSave={async (settingsData) => {
                        // If no portfolio exists, we'll need to create one or update draft settings
                        if (!currentPortfolio) {
                          // TODO: Store settings in draft/temp state for when portfolio is created
                          return;
                        }
                        
                        // Update existing portfolio
                        await updatePortfolio(currentPortfolio.id, settingsData);
                        await refreshUserPortfolios();
                      }}
                    />
                  </TabsContent>
                </Tabs>
              </div>
            </div>

            {/* Sidebar */}
            <div className="lg:col-span-1">
              <div className="sticky top-32 space-y-6">
                <PublishSidebar 
                  totalProjects={totalProjects}
                  totalExperience={totalExperience}
                  totalSkills={totalSkills}
                  totalBlogPosts={totalBlogPosts}
                  hasDrafts={hasDraftData()}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Loading Modal */}
      <LoadingModal 
        isOpen={showLoadingModal}
        title="Publishing Portfolio..."
        message="Please wait while we publish your portfolio and save all your content. This may take a few moments."
      />
    </div>
  )
}    