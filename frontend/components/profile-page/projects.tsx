'use client';

import React, { useState } from 'react';
import Image from 'next/image';
import { Edit2, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { FormDialog, EditFormDialog, FormField } from '@/components/ui/form-dialog';
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

// Helper function to validate URL
const isValidUrl = (string: string): boolean => {
  try {
    new URL(string);
    return true;
  } catch {
    return false;
  }
};

// Helper function to get a safe image URL
const getSafeImageUrl = (imageUrl: string | undefined): string => {
  if (!imageUrl || imageUrl.trim() === '') {
    return "https://placehold.co/754x200";
  }
  
  const trimmedUrl = imageUrl.trim();
  if (isValidUrl(trimmedUrl)) {
    return trimmedUrl;
  }
  
  return "https://placehold.co/754x200";
};

// Define form fields for projects
const projectFormFields: FormField[] = [
  {
    name: 'title',
    label: 'Project Title',
    type: 'text',
    placeholder: 'Enter project title',
    required: true
  },
  {
    name: 'imageUrl',
    label: 'Image URL',
    type: 'url',
    placeholder: 'https://example.com/image.jpg'
  },
  {
    name: 'description',
    label: 'Description',
    type: 'textarea',
    placeholder: 'Describe your project...',
    rows: 3
  },
  {
    name: 'demoUrl',
    label: 'Demo URL',
    type: 'url',
    placeholder: 'https://your-demo.com'
  },
  {
    name: 'githubUrl',
    label: 'GitHub URL',
    type: 'url',
    placeholder: 'https://github.com/username/repo'
  },
  {
    name: 'technologies',
    label: 'Technologies',
    type: 'text',
    placeholder: 'React, Node.js, MongoDB (comma-separated)'
  },
  {
    name: 'featured',
    label: 'Featured Project',
    type: 'checkbox'
  }
];

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
    setError(null);
  }, []);

  const handleFormChange = React.useCallback((field: string, value: string | boolean) => {
    setProjectForm(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  }, [error]);

  const handleAddProject = async () => {
    if (!projectForm.title.trim()) {
      setError('Please enter a project title.');
      return;
    }

    if (!portfolioId) {
      setError('No portfolio found. Please create a portfolio first by going to the Publish page.');
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      const newProject = await createProject({
        portfolioId,
        title: projectForm.title.trim(),
        imageUrl: projectForm.imageUrl.trim() || undefined,
        description: projectForm.description.trim() || undefined,
        demoUrl: projectForm.demoUrl.trim() || undefined,
        githubUrl: projectForm.githubUrl.trim() || undefined,
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

    try {
      setActionLoading(true);
      setError(null);

      const updatedProject = await updateProject(editingProject.id, {
        title: projectForm.title.trim(),
        imageUrl: projectForm.imageUrl.trim() || undefined,
        description: projectForm.description.trim() || undefined,
        demoUrl: projectForm.demoUrl.trim() || undefined,
        githubUrl: projectForm.githubUrl.trim() || undefined,
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
        {/* Add Project Dialog */}
        <div>
          <FormDialog
            title="Add New Project"
            description="Add a new project to your portfolio"
            triggerLabel="Add Project"
            isOpen={isAddDialogOpen}
            onOpenChange={setIsAddDialogOpen}
            fields={projectFormFields}
            formData={projectForm}
            onFormChange={handleFormChange}
            onSubmit={handleAddProject}
            onCancel={handleAddCancel}
            isEdit={false}
            loading={actionLoading}
            error={error}
            submitLabel="Add Project"
          />
        </div>

        {/* Edit Project Dialog */}
        <EditFormDialog
          title="Edit Project"
          description="Update your project details"
          isOpen={isEditDialogOpen}
          onOpenChange={setIsEditDialogOpen}
          fields={projectFormFields}
          formData={projectForm}
          onFormChange={handleFormChange}
          onSubmit={handleEditProject}
          onCancel={handleEditCancel}
          loading={actionLoading}
          error={error}
          submitLabel="Update Project"
        />

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
                <Image
                  className="w-full h-[150px] sm:h-[200px] object-cover"
                  src={getSafeImageUrl(project.imageUrl)}
                  alt={project.title}
                  width={754}
                  height={200}
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