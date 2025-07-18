'use client';

import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Project } from '@/lib/portfolio';

interface ProjectsProps {
  projects?: Project[];
  portfolioId?: string;
  loading?: boolean;
}

export default function Projects({ projects = [], portfolioId, loading = false }: ProjectsProps) {

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading projects...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px]">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Projects</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Manage your portfolio projects</p>
      
      <div className="space-y-4 sm:space-y-6">
        {/* Add Project Button */}
        <div>
          <Button className="w-full sm:w-auto px-4 py-2 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg flex justify-center items-center gap-2">
            <Plus className="w-[14px] h-[14px]" />
            Add Project
          </Button>
        </div>

        {/* Projects Grid */}
        {projects.length === 0 ? (
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
            {projects.map((project) => (
              <div
                key={project.id}
                className="p-[1px] bg-white overflow-hidden rounded-lg border border-[#E2E8F0] flex flex-col justify-start items-start"
              >
                <img
                  className="w-full h-[150px] sm:h-[200px] object-cover"
                  src={project.imageUrl || "https://placehold.co/754x200"}
                  alt={project.title}
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
                    className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal"
                  >
                    Edit
                  </Button>
                  <Button
                    variant="outline"
                    className="w-full sm:w-auto px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal"
                  >
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