'use client';

import React, { useState } from 'react';
import { Edit2, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { FormDialog, EditFormDialog, FormField } from '@/components/ui/form-dialog';
import { Experience as ExperienceType } from '@/lib/portfolio';
import { createExperience, updateExperience, deleteExperience } from '@/lib/portfolio/api';
import { Loading } from '@/components/loader';

interface ExperienceProps {
  experiences?: ExperienceType[];
  portfolioId?: string;
  loading?: boolean;
  onExperiencesUpdate?: (experiences: ExperienceType[]) => void;
}

interface ExperienceFormData {
  jobTitle: string;
  companyName: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  description: string;
  skillsUsed: string;
}

// Define form fields for experience
const experienceFormFields: FormField[] = [
  {
    name: 'jobTitle',
    label: 'Job Title',
    type: 'text',
    placeholder: 'Enter job title',
    required: true
  },
  {
    name: 'companyName',
    label: 'Company Name',
    type: 'text',
    placeholder: 'Enter company name',
    required: true
  },
  {
    name: 'startDate',
    label: 'Start Date',
    type: 'date',
    required: true
  },
  {
    name: 'isCurrent',
    label: 'I currently work here',
    type: 'checkbox'
  },
  {
    name: 'endDate',
    label: 'End Date',
    type: 'date',
    dependsOn: 'isCurrent',
    dependsOnValue: false
  },
  {
    name: 'description',
    label: 'Description',
    type: 'textarea',
    placeholder: 'Describe your responsibilities and achievements...',
    rows: 3
  },
  {
    name: 'skillsUsed',
    label: 'Skills Used',
    type: 'text',
    placeholder: 'React, Node.js, MongoDB (comma-separated)'
  }
];

export default function Experience({ experiences = [], portfolioId, loading = false, onExperiencesUpdate }: ExperienceProps) {
  const [localExperiences, setLocalExperiences] = useState<ExperienceType[]>(experiences);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editingExperience, setEditingExperience] = useState<ExperienceType | null>(null);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [experienceForm, setExperienceForm] = useState<ExperienceFormData>({
    jobTitle: '',
    companyName: '',
    startDate: '',
    endDate: '',
    isCurrent: false,
    description: '',
    skillsUsed: ''
  });

  // Update local experiences when props change
  React.useEffect(() => {
    setLocalExperiences(experiences);
  }, [experiences]);

  const formatDatePeriod = (startDate: string, endDate?: string, isCurrent?: boolean) => {
    const start = new Date(startDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
    if (isCurrent) {
      return `${start} - Present`;
    }
    if (endDate) {
      const end = new Date(endDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
      return `${start} - ${end}`;
    }
    return start;
  };

  const resetForm = React.useCallback(() => {
    setExperienceForm({
      jobTitle: '',
      companyName: '',
      startDate: '',
      endDate: '',
      isCurrent: false,
      description: '',
      skillsUsed: ''
    });
    setError(null);
  }, []);

  const handleFormChange = React.useCallback((field: string, value: string | boolean) => {
    setExperienceForm(prev => ({ ...prev, [field]: value }));
    if (error) setError(null);
  }, [error]);

  const handleAddExperience = async () => {
    if (!experienceForm.jobTitle.trim() || !experienceForm.companyName.trim() || !experienceForm.startDate) {
      setError('Please fill in all required fields (Job Title, Company, and Start Date).');
      return;
    }

    if (!portfolioId) {
      setError('No portfolio found. Please create a portfolio first by going to the Publish page.');
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      const newExperience = await createExperience({
        portfolioId,
        jobTitle: experienceForm.jobTitle.trim(),
        companyName: experienceForm.companyName.trim(),
        startDate: experienceForm.startDate,
        endDate: experienceForm.isCurrent ? undefined : experienceForm.endDate,
        isCurrent: experienceForm.isCurrent,
        description: experienceForm.description.trim() || undefined,
        skillsUsed: experienceForm.skillsUsed.trim() ? experienceForm.skillsUsed.trim().split(',').map(s => s.trim()) : []
      });

      const updatedExperiences = [...localExperiences, newExperience];
      setLocalExperiences(updatedExperiences);
      onExperiencesUpdate?.(updatedExperiences);
      
      resetForm();
      setIsAddDialogOpen(false);
    } catch (err) {
      console.error('Error adding experience:', err);
      setError('Failed to add experience. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleEditExperience = async () => {
    if (!editingExperience || !experienceForm.jobTitle.trim() || !experienceForm.companyName.trim() || !experienceForm.startDate) {
      setError('Please fill in all required fields (Job Title, Company, and Start Date).');
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      const updatedExperience = await updateExperience(editingExperience.id, {
        jobTitle: experienceForm.jobTitle.trim(),
        companyName: experienceForm.companyName.trim(),
        startDate: experienceForm.startDate,
        endDate: experienceForm.isCurrent ? undefined : experienceForm.endDate,
        isCurrent: experienceForm.isCurrent,
        description: experienceForm.description.trim() || undefined,
        skillsUsed: experienceForm.skillsUsed.trim() ? experienceForm.skillsUsed.trim().split(',').map(s => s.trim()) : []
      });

      const updatedExperiences = localExperiences.map(e => e.id === editingExperience.id ? updatedExperience : e);
      setLocalExperiences(updatedExperiences);
      onExperiencesUpdate?.(updatedExperiences);
      
      resetForm();
      setIsEditDialogOpen(false);
      setEditingExperience(null);
    } catch (err) {
      console.error('Error updating experience:', err);
      setError('Failed to update experience. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDeleteExperience = async (experienceId: string) => {
    if (!confirm('Are you sure you want to delete this experience?')) {
      return;
    }

    try {
      setActionLoading(true);
      setError(null);

      await deleteExperience(experienceId);
      const updatedExperiences = localExperiences.filter(e => e.id !== experienceId);
      setLocalExperiences(updatedExperiences);
      onExperiencesUpdate?.(updatedExperiences);
    } catch (err) {
      console.error('Error deleting experience:', err);
      setError('Failed to delete experience. Please try again.');
    } finally {
      setActionLoading(false);
    }
  };

  const openEditDialog = (experience: ExperienceType) => {
    setEditingExperience(experience);
    setExperienceForm({
      jobTitle: experience.jobTitle,
      companyName: experience.companyName,
      startDate: experience.startDate,
      endDate: experience.endDate || '',
      isCurrent: experience.isCurrent || false,
      description: experience.description || '',
      skillsUsed: experience.skillsUsed?.join(', ') || ''
    });
    setIsEditDialogOpen(true);
  };

  const handleAddCancel = () => {
    setIsAddDialogOpen(false);
    resetForm();
  };

  const handleEditCancel = () => {
    setIsEditDialogOpen(false);
    setEditingExperience(null);
    resetForm();
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <Loading className="scale-50" backgroundColor="white" />
          <p className="text-gray-600 mt-4">Loading experience...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Experience</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Add your work experience and achievements</p>
    
      <div className="space-y-4 sm:space-y-6">
        {/* Add Experience Dialog */}
        <div>
          <FormDialog
            title="Add New Experience"
            description="Add a new work experience to your portfolio"
            triggerLabel="Add Experience"
            isOpen={isAddDialogOpen}
            onOpenChange={setIsAddDialogOpen}
            fields={experienceFormFields}
            formData={experienceForm}
            onFormChange={handleFormChange}
            onSubmit={handleAddExperience}
            onCancel={handleAddCancel}
            isEdit={false}
            loading={actionLoading}
            error={error}
            submitLabel="Add Experience"
          />
        </div>

        {/* Edit Experience Dialog */}
        <EditFormDialog
          title="Edit Experience"
          description="Update your work experience details"
          isOpen={isEditDialogOpen}
          onOpenChange={setIsEditDialogOpen}
          fields={experienceFormFields}
          formData={experienceForm}
          onFormChange={handleFormChange}
          onSubmit={handleEditExperience}
          onCancel={handleEditCancel}
          loading={actionLoading}
          error={error}
          submitLabel="Update Experience"
        />

        {/* Experience Cards */}
        {localExperiences.length === 0 ? (
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2-2v2m8 0V6a2 2 0 012 2v6a2 2 0 01-2 2H6a2 2 0 01-2-2V8a2 2 0 012-2V6" />
              </svg>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">No experience yet</h3>
            <p className="text-gray-600 mb-4">Add your work experience to showcase your professional journey</p>
          </div>
        ) : (
          <div className="space-y-4 sm:space-y-6 lg:space-y-8">
            {localExperiences.map((experience) => (
              <div
                key={experience.id}
                className="w-full p-4 sm:p-5 bg-white rounded-lg border border-[#E2E8F0] flex flex-col justify-start items-start gap-3 sm:gap-4"
              >
                <div className="w-full flex flex-col sm:flex-row sm:justify-between sm:items-start gap-3 sm:gap-4">
                  <div className="w-full flex flex-col justify-start items-start gap-1 sm:gap-2">
                    <div className="w-full flex flex-col justify-start items-start">
                      <h3 className="w-full text-[#020817] text-base sm:text-lg font-bold font-['Inter'] leading-tight sm:leading-[29.95px]">
                        {experience.jobTitle}
                      </h3>
                    </div>
                    <div className="w-full flex flex-col justify-start items-start">
                      <p className="w-full text-[#020817] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                        {experience.companyName}
                      </p>
                    </div>
                    <div className="w-full flex flex-col justify-start items-start">
                      <p className="w-full text-[#64748B] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                        {formatDatePeriod(experience.startDate, experience.endDate, experience.isCurrent)}
                      </p>
                    </div>
                  </div>
                  <div className="w-full sm:w-auto flex flex-col sm:flex-row justify-start items-start gap-2 sm:gap-[4.5px]">
                    <Button
                      variant="outline"
                      className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal flex items-center gap-2"
                      onClick={() => openEditDialog(experience)}
                      disabled={actionLoading}
                    >
                      <Edit2 className="w-4 h-4" />
                      Edit
                    </Button>
                    <Button
                      variant="outline"
                      className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal flex items-center gap-2"
                      onClick={() => handleDeleteExperience(experience.id)}
                      disabled={actionLoading}
                    >
                      <Trash2 className="w-4 h-4" />
                      Delete
                    </Button>
                  </div>
                </div>
                {experience.description && (
                  <div className="w-full flex flex-col justify-start items-start">
                    <p className="w-full text-[#020817] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                      {experience.description}
                    </p>
                  </div>
                )}
                {experience.skillsUsed && experience.skillsUsed.length > 0 && (
                  <div className="w-full flex justify-start items-start gap-2 sm:gap-[4.5px] flex-wrap">
                    {experience.skillsUsed.map((skill, index) => (
                      <div
                        key={index}
                        className="px-2 py-[1.5px] bg-[#F1F5F9] rounded-md flex justify-start items-start"
                      >
                        <span className="text-[#020817] text-xs font-medium font-['Inter'] leading-[19.2px]">
                          {skill}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
