'use client';

import React, { useState } from 'react';
import { Edit2, Trash2, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { ImageUpload } from '@/components/ui/image-upload';
import { getSafeImageUrl } from '@/lib/image';
import { Project } from '@/lib/portfolio';
import { createProject, updateProject, deleteProject } from '@/lib/portfolio/api';

interface ProjectsProps {
  projects?: Project[];
  portfolioId?: string;
  loading?: boolean;
  onProjectsUpdate?: (projects: Project[]) => void;
}

interface ProjectFormData {
  title: string;
  imageUrl: string;
  description: string;
  demoUrl: string;
  githubUrl: string;
  technologies: string;
  featured: boolean;
}





export default function Projects({ projects = [], portfolioId, loading = false, onProjectsUpdate }: ProjectsProps) {
  const [localProjects, setLocalProjects] = useState<Project[]>(projects);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editingProject, setEditingProject] = useState<Project | null>(null);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [projectForm, setProjectForm] = useState<ProjectFormData>({
    title: '',
    imageUrl: '',
    description: '',
    demoUrl: '',
    githubUrl: '',
    technologies: '',
    featured: false
  });
  const [selectedImageFile, setSelectedImageFile] = useState<File | null>(null);

  // Update local projects when props change
  React.useEffect(() => {
    setLocalProjects(projects);
  }, [projects]);

  const resetForm = React.useCallback(() => {
    setProjectForm({
      title: '',
      imageUrl: '',
      description: '',
      demoUrl: '',
      githubUrl: '',
      technologies: '',
      featured: false
    });
    setSelectedImageFile(null);
    setError(null);
  }, []);

  const handleFormChange = React.useCallback((field: string, value: string | boolean) => {
    setProjectForm(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  }, [error]);

  // Client-side URL validation helper
  const isValidUrl = (url: string): boolean => {
    if (!url.trim()) return true; // Empty is valid (optional field)
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  };

  const handleAddProject = async () => {
    if (!projectForm.title.trim()) {
      setError('Please enter a project title.');
      return;
    }

    // Validate URLs
    if (projectForm.demoUrl.trim() && !isValidUrl(projectForm.demoUrl.trim())) {
      setError('Demo URL must be a valid URL (e.g., https://example.com) or leave empty.');
      return;
    }
    
    if (projectForm.githubUrl.trim() && !isValidUrl(projectForm.githubUrl.trim())) {
      setError('GitHub URL must be a valid URL (e.g., https://github.com/username/repo) or leave empty.');
      return;
    }

    if (!portfolioId) {
      setError('No portfolio found. Please create a portfolio first by going to the Publish page.');
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      let finalImageUrl = projectForm.imageUrl;

      // Upload image if a file is selected
      if (selectedImageFile) {
        try {
          console.log('Profile: Uploading image for new project');
          const { uploadImage } = await import('@/lib/image');
          const response = await uploadImage(selectedImageFile, 'projects');
          finalImageUrl = response.imagePath;
          console.log('Profile: Image upload completed, URL:', finalImageUrl);
        } catch (uploadError) {
          console.error('Error uploading image:', uploadError);
          setError('Failed to upload image. Please try again.');
          return;
        }
      }

      // Clean and prepare data - only include URLs if they're valid
      const cleanDemoUrl = projectForm.demoUrl.trim();
      const cleanGithubUrl = projectForm.githubUrl.trim();

      const newProject = await createProject({
        portfolioId,
        title: projectForm.title.trim(),
        imageUrl: finalImageUrl.trim() || undefined,
        description: projectForm.description.trim() || undefined,
        demoUrl: cleanDemoUrl && isValidUrl(cleanDemoUrl) ? cleanDemoUrl : undefined,
        githubUrl: cleanGithubUrl && isValidUrl(cleanGithubUrl) ? cleanGithubUrl : undefined,
        technologies: projectForm.technologies.trim() ? projectForm.technologies.trim().split(',').map(t => t.trim()) : [],
        featured: projectForm.featured
      });

      const updatedProjects = [...localProjects, newProject];
      setLocalProjects(updatedProjects);
      onProjectsUpdate?.(updatedProjects);
      
      resetForm();
      setIsAddDialogOpen(false);
    } catch (err) {
      console.error('Error adding project:', err);
      setError('Failed to add project. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleEditProject = async () => {
    if (!editingProject || !projectForm.title.trim()) {
      setError('Please enter a project title.');
      return;
    }

    // Validate URLs
    if (projectForm.demoUrl.trim() && !isValidUrl(projectForm.demoUrl.trim())) {
      setError('Demo URL must be a valid URL (e.g., https://example.com) or leave empty.');
      return;
    }
    
    if (projectForm.githubUrl.trim() && !isValidUrl(projectForm.githubUrl.trim())) {
      setError('GitHub URL must be a valid URL (e.g., https://github.com/username/repo) or leave empty.');
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      let finalImageUrl = projectForm.imageUrl;

      // Upload image if a file is selected
      if (selectedImageFile) {
        try {
          console.log('Profile: Uploading image for project update');
          const { uploadImage } = await import('@/lib/image');
          const response = await uploadImage(selectedImageFile, 'projects');
          finalImageUrl = response.imagePath;
          console.log('Profile: Image upload completed, URL:', finalImageUrl);
        } catch (uploadError) {
          console.error('Error uploading image:', uploadError);
          setError('Failed to upload image. Please try again.');
          return;
        }
      }

      // Clean and prepare data - only include URLs if they're valid
      const cleanDemoUrl = projectForm.demoUrl.trim();
      const cleanGithubUrl = projectForm.githubUrl.trim();

      const updatedProject = await updateProject(editingProject.id, {
        title: projectForm.title.trim(),
        imageUrl: finalImageUrl.trim() || undefined,
        description: projectForm.description.trim() || undefined,
        demoUrl: cleanDemoUrl && isValidUrl(cleanDemoUrl) ? cleanDemoUrl : undefined,
        githubUrl: cleanGithubUrl && isValidUrl(cleanGithubUrl) ? cleanGithubUrl : undefined,
        technologies: projectForm.technologies.trim() ? projectForm.technologies.trim().split(',').map(t => t.trim()) : [],
        featured: projectForm.featured
      });

      const updatedProjects = localProjects.map(p => p.id === editingProject.id ? updatedProject : p);
      setLocalProjects(updatedProjects);
      onProjectsUpdate?.(updatedProjects);
      
      resetForm();
      setIsEditDialogOpen(false);
      setEditingProject(null);
    } catch (err) {
      console.error('Error updating project:', err);
      setError('Failed to update project. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDeleteProject = async (projectId: string) => {
    if (!confirm('Are you sure you want to delete this project?')) {
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      await deleteProject(projectId);
      const updatedProjects = localProjects.filter(p => p.id !== projectId);
      setLocalProjects(updatedProjects);
      onProjectsUpdate?.(updatedProjects);
    } catch (err) {
      console.error('Error deleting project:', err);
      setError('Failed to delete project. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const openEditDialog = (project: Project) => {
    setEditingProject(project);
    setProjectForm({
      title: project.title,
      imageUrl: project.imageUrl || '',
      description: project.description || '',
      demoUrl: project.demoUrl || '',
      githubUrl: project.githubUrl || '',
      technologies: project.technologies?.join(', ') || '',
      featured: project.featured || false
    });
    setIsEditDialogOpen(true);
  };

  const handleAddCancel = () => {
    setIsAddDialogOpen(false);
    resetForm();
  };

  const handleEditCancel = () => {
    setIsEditDialogOpen(false);
    setEditingProject(null);
    resetForm();
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-2 spinner-app-blue mx-auto mb-4"></div>
          <p className="text-gray-600">Loading projects...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Projects</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Manage your portfolio projects</p>
      
      <div className="space-y-4 sm:space-y-6">
        {/* Add Project Button */}
        <div>
          <Button
            onClick={() => setIsAddDialogOpen(true)}
            className="flex items-center gap-2"
          >
            <Plus className="w-4 h-4" />
            Add Project
          </Button>
        </div>

        {/* Add Project Dialog */}
        {isAddDialogOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
              <h2 className="text-xl font-semibold mb-4">Add New Project</h2>
              <p className="text-gray-600 mb-6">Add a new project to your portfolio</p>
              
              {error && (
                <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-600">{error}</p>
                </div>
              )}

                             <div className="space-y-4">
                 <div>
                   <Label htmlFor="title" className="text-sm font-medium text-slate-700">
                     Project Title *
                   </Label>
                   <Input
                     id="title"
                     value={projectForm.title}
                     onChange={(e) => handleFormChange("title", e.target.value)}
                     placeholder="Enter project title"
                     className="mt-1"
                   />
                 </div>

                 <div>
                   <ImageUpload
                     label="Project Image"
                     value={projectForm.imageUrl}
                     onFileSelect={setSelectedImageFile}
                     preview={true}
                   />
                 </div>

                <div>
                  <Label htmlFor="description" className="text-sm font-medium text-slate-700">
                    Description
                  </Label>
                  <Textarea
                    id="description"
                    value={projectForm.description}
                    onChange={(e) => handleFormChange("description", e.target.value)}
                    placeholder="Describe your project..."
                    className="mt-1"
                    rows={3}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="demoUrl" className="text-sm font-medium text-slate-700">
                      Demo URL
                    </Label>
                    <Input
                      id="demoUrl"
                      value={projectForm.demoUrl}
                      onChange={(e) => handleFormChange("demoUrl", e.target.value)}
                      placeholder="https://your-demo.com"
                      className="mt-1"
                    />
                  </div>

                  <div>
                    <Label htmlFor="githubUrl" className="text-sm font-medium text-slate-700">
                      GitHub URL
                    </Label>
                    <Input
                      id="githubUrl"
                      value={projectForm.githubUrl}
                      onChange={(e) => handleFormChange("githubUrl", e.target.value)}
                      placeholder="https://github.com/username/repo"
                      className="mt-1"
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="technologies" className="text-sm font-medium text-slate-700">
                    Technologies
                  </Label>
                  <Input
                    id="technologies"
                    value={projectForm.technologies}
                    onChange={(e) => handleFormChange("technologies", e.target.value)}
                    placeholder="React, Node.js, MongoDB (comma-separated)"
                    className="mt-1"
                  />
                </div>

                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="featured"
                    checked={projectForm.featured}
                    onCheckedChange={(checked) => handleFormChange("featured", checked === true)}
                  />
                  <Label htmlFor="featured" className="text-sm font-medium text-slate-700">
                    Featured Project
                  </Label>
                </div>
              </div>

              <div className="flex justify-end space-x-3 mt-6">
                <Button variant="outline" onClick={handleAddCancel}>
                  Cancel
                </Button>
                <Button onClick={handleAddProject} disabled={actionLoading}>
                  {actionLoading ? "Adding..." : "Add Project"}
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Edit Project Dialog */}
        {isEditDialogOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
              <h2 className="text-xl font-semibold mb-4">Edit Project</h2>
              <p className="text-gray-600 mb-6">Update your project details</p>
              
              {error && (
                <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
                  <p className="text-sm text-red-600">{error}</p>
                </div>
              )}

                             <div className="space-y-4">
                 <div>
                   <Label htmlFor="edit-title" className="text-sm font-medium text-slate-700">
                     Project Title *
                   </Label>
                   <Input
                     id="edit-title"
                     value={projectForm.title}
                     onChange={(e) => handleFormChange("title", e.target.value)}
                     placeholder="Enter project title"
                     className="mt-1"
                   />
                 </div>

                 <div>
                   <ImageUpload
                     label="Project Image"
                     value={projectForm.imageUrl}
                     onFileSelect={setSelectedImageFile}
                     preview={true}
                   />
                 </div>

                <div>
                  <Label htmlFor="edit-description" className="text-sm font-medium text-slate-700">
                    Description
                  </Label>
                  <Textarea
                    id="edit-description"
                    value={projectForm.description}
                    onChange={(e) => handleFormChange("description", e.target.value)}
                    placeholder="Describe your project..."
                    className="mt-1"
                    rows={3}
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="edit-demoUrl" className="text-sm font-medium text-slate-700">
                      Demo URL
                    </Label>
                    <Input
                      id="edit-demoUrl"
                      value={projectForm.demoUrl}
                      onChange={(e) => handleFormChange("demoUrl", e.target.value)}
                      placeholder="https://your-demo.com"
                      className="mt-1"
                    />
                  </div>

                  <div>
                    <Label htmlFor="edit-githubUrl" className="text-sm font-medium text-slate-700">
                      GitHub URL
                    </Label>
                    <Input
                      id="edit-githubUrl"
                      value={projectForm.githubUrl}
                      onChange={(e) => handleFormChange("githubUrl", e.target.value)}
                      placeholder="https://github.com/username/repo"
                      className="mt-1"
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="edit-technologies" className="text-sm font-medium text-slate-700">
                    Technologies
                  </Label>
                  <Input
                    id="edit-technologies"
                    value={projectForm.technologies}
                    onChange={(e) => handleFormChange("technologies", e.target.value)}
                    placeholder="React, Node.js, MongoDB (comma-separated)"
                    className="mt-1"
                  />
                </div>

                <div className="flex items-center space-x-2">
                  <Checkbox
                    id="edit-featured"
                    checked={projectForm.featured}
                    onCheckedChange={(checked) => handleFormChange("featured", checked === true)}
                  />
                  <Label htmlFor="edit-featured" className="text-sm font-medium text-slate-700">
                    Featured Project
                  </Label>
                </div>
              </div>

              <div className="flex justify-end space-x-3 mt-6">
                <Button variant="outline" onClick={handleEditCancel}>
                  Cancel
                </Button>
                <Button onClick={handleEditProject} disabled={actionLoading}>
                  {actionLoading ? "Updating..." : "Update Project"}
                </Button>
              </div>
            </div>
          </div>
        )}

        {/* Projects Grid */}
        {localProjects.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">No projects yet</h3>
            <p className="text-gray-600 mb-4">Start building your portfolio by adding your first project</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-2 gap-4 sm:gap-6">
            {localProjects.map((project) => (
              <div
                key={project.id}
                className="p-[1px] bg-white overflow-hidden rounded-lg border border-[#E2E8F0] flex flex-col justify-start items-start"
              >
                <img
                  className="w-full h-[150px] sm:h-[200px] object-cover"
                  src={getSafeImageUrl(project.imageUrl)}
                  alt={project.title}
                  onError={(e) => {
                    // Fallback to placeholder if image fails to load
                    e.currentTarget.src = getSafeImageUrl('');
                  }}
                />
                <div className="w-full pt-4 sm:pt-[23px] pb-4 sm:pb-6 px-4 sm:px-6 flex flex-col justify-start items-start gap-2">
                  <div className="w-full pb-[0.8px] flex flex-col justify-start items-start">
                    <h3 className="w-full text-[#020817] text-base sm:text-lg font-semibold font-['Inter'] leading-tight sm:leading-[28.8px]">
                      {project.title}
                    </h3>
                  </div>
                  <div className="w-full flex flex-col justify-start items-start">
                    <p className="w-full text-[#64748B] text-xs sm:text-sm font-normal font-['Inter'] leading-relaxed sm:leading-[19.6px]">
                      {project.description || 'No description available'}
                    </p>
                  </div>
                  {project.technologies && project.technologies.length > 0 && (
                    <div className="w-full pt-2 flex justify-start items-start gap-1 sm:gap-2 flex-wrap">
                      {project.technologies.map((tech, index) => (
                        <div
                          key={index}
                          className="px-2 py-1 bg-[#F1F5F9] rounded-md flex flex-col justify-start items-start"
                        >
                          <span className="text-[#020817] text-xs font-medium font-['Inter'] leading-[19.2px]">
                            {tech}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                  <div className="w-full pt-2 flex flex-col sm:flex-row justify-start items-start gap-2">
                    <Button
                      variant="outline"
                      className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal flex items-center gap-2"
                      onClick={() => openEditDialog(project)}
                      disabled={actionLoading}
                    >
                      <Edit2 className="w-4 h-4" />
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal flex items-center gap-2"
                      onClick={() => handleDeleteProject(project.id)}
                      disabled={actionLoading}
                    >
                      <Trash2 className="w-4 h-4" />
                      Delete
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}