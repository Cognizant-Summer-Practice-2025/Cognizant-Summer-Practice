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
    // Blog posts are optional, so we don't validate them
    if (totalProjects === 0) {
      errors.push("Add at least one project")
    }
    if (totalExperience === 0) {
      errors.push("Add at least one experience")
    }
    if (totalSkills === 0) {
      errors.push("Add at least one skill")
    }
    // Blog posts are optional - no validation required
    
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
        
        // Verify we have a valid portfolio ID
        if (!portfolioId) {
          throw new Error('Failed to obtain portfolio ID for saving content');
        }
        
        // Upload images for projects and prepare projects data
        const projects = await Promise.all(draftProjects.map(async (project) => {
          let finalImageUrl = project.imageUrl || '';
          
          // Upload image if a file was selected
          if (project.selectedImageFile) {
            try {
              console.log('📷 Uploading image for project:', project.title);
              const { uploadImage } = await import('@/lib/image');
              const response = await uploadImage(project.selectedImageFile, 'projects');
              finalImageUrl = response.imagePath;
              console.log('✅ Project image uploaded:', finalImageUrl);
            } catch (uploadError) {
              console.error('❌ Failed to upload image for project:', project.title, uploadError);
              throw new Error(`Failed to upload image for project "${project.title}": ${uploadError instanceof Error ? uploadError.message : 'Unknown error'}`);
            }
          }
          
          return {
            portfolioId: portfolioId,
            title: project.title,
            imageUrl: finalImageUrl,
            description: project.description || '',
            demoUrl: project.demoUrl || '',
            githubUrl: project.githubUrl || '',
            technologies: project.technologies ? project.technologies.split(',').map(t => t.trim()).filter(t => t.length > 0) : [],
            featured: project.featured
          };
        }))

        // Prepare experience data
        const experience = draftExperience.map(exp => ({
          portfolioId: portfolioId,
          jobTitle: exp.jobTitle,
          companyName: exp.companyName,
          startDate: exp.startDate, // Keep as string in YYYY-MM-DD format
          endDate: exp.isCurrent ? undefined : exp.endDate || undefined, // Use undefined for optional dates
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

        // Upload images for blog posts and prepare blog posts data
        const blogPosts = await Promise.all(draftBlogPosts.map(async (blog) => {
          let finalImageUrl = blog.featuredImageUrl || '';
          
          // Upload image if a file was selected
          if (blog.selectedImageFile) {
            try {
              console.log('📷 Uploading image for blog post:', blog.title);
              const { uploadImage } = await import('@/lib/image');
              const response = await uploadImage(blog.selectedImageFile, 'blog_posts');
              finalImageUrl = response.imagePath;
              console.log('✅ Blog post image uploaded:', finalImageUrl);
            } catch (uploadError) {
              console.error('❌ Failed to upload image for blog post:', blog.title, uploadError);
              throw new Error(`Failed to upload image for blog post "${blog.title}": ${uploadError instanceof Error ? uploadError.message : 'Unknown error'}`);
            }
          }
          
          return {
            portfolioId: portfolioId,
            title: blog.title,
            content: blog.content || '',
            featuredImageUrl: finalImageUrl,
            tags: blog.tags ? blog.tags.split(',').map(t => t.trim()).filter(t => t.length > 0) : [],
            isPublished: blog.publishImmediately
          };
        }))

        // Use the new split approach to save content
        const contentData = {
          projects: projects.length > 0 ? projects : undefined,
          experience: experience.length > 0 ? experience : undefined,
          skills: skillsData.length > 0 ? skillsData : undefined,
          blogPosts: blogPosts.length > 0 ? blogPosts : undefined,
          publishPortfolio: true
        }

        await savePortfolioContent(portfolioId, contentData)

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
      <div className="fixed top-16 left-0 right-0 z-40">
        <PublishHeader onPublish={handlePublish} publishing={publishing} />
      </div>
      
      <div className="pt-40 pb-6">
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
                  <TabsList className="flex flex-wrap w-full gap-1 mb-6 h-auto p-1 bg-slate-100 rounded-lg">
                    <TabsTrigger 
                      value="basic-info" 
                      className="flex-1 min-w-[120px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Basic Info
                    </TabsTrigger>
                    <TabsTrigger 
                      value="projects" 
                      className="flex-1 min-w-[100px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Projects
                    </TabsTrigger>
                    <TabsTrigger 
                      value="experience" 
                      className="flex-1 min-w-[110px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Experience
                    </TabsTrigger>
                    <TabsTrigger 
                      value="skills" 
                      className="flex-1 min-w-[80px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Skills
                    </TabsTrigger>
                    <TabsTrigger 
                      value="blog-posts" 
                      className="flex-1 min-w-[100px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Blog Posts
                    </TabsTrigger>
                    <TabsTrigger 
                      value="settings" 
                      className="flex-1 min-w-[90px] text-xs sm:text-sm whitespace-nowrap px-2 py-2 sm:px-3 sm:py-2.5"
                    >
                      Settings
                    </TabsTrigger>
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
                        try {
                          if (!currentPortfolio) {
                            // Create a basic portfolio first with the settings data
                            const portfolioData = {
                              userId: user?.id || 'default-user-id',
                              templateName: settingsData.templateName || 'Gabriel Bârzu',
                              title: 'My Portfolio', // Default title, will be set in basic info
                              bio: 'Welcome to my portfolio', // Default bio, will be set in basic info
                              visibility: 0 as 0 | 1 | 2, // Default visibility, will be set in basic info
                              isPublished: false, // Keep as draft until publish is clicked
                              components: settingsData.components || JSON.stringify(TemplateManager.createDefaultComponentConfig())
                            };
                            
                            await createPortfolioAndGetId(portfolioData);
                          } else {
                            // Update existing portfolio with template and components only
                            const updateData = {
                              components: settingsData.components
                            };
                            
                            // Note: Template changes might need special handling
                            await updatePortfolio(currentPortfolio.id, updateData);
                          }
                          
                          // Refresh portfolio data to reflect changes
                          await refreshUserPortfolios();
                        } catch (error) {
                          console.error('Error saving portfolio settings:', error);
                          throw error; // Let the component handle the error display
                        }
                      }}
                    />
                  </TabsContent>
                </Tabs>
              </div>
            </div>

            {/* Sidebar */}
            <div className="lg:col-span-1">
              <div className="sticky top-40 space-y-6">
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