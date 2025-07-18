'use client';

import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Experience as ExperienceType } from '@/lib/portfolio';

interface ExperienceProps {
  experiences?: ExperienceType[];
  portfolioId?: string;
  loading?: boolean;
}

export default function Experience({ experiences = [], portfolioId, loading = false }: ExperienceProps) {
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

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px] flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading experience...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-sm p-4 sm:p-6 lg:p-8 w-full min-h-[600px]">
      <h1 className="text-xl sm:text-2xl font-semibold text-gray-900 mb-4 sm:mb-6">Experience</h1>
      <p className="text-sm sm:text-base text-gray-600 mb-6 sm:mb-8">Add your work experience and achievements</p>
      
      <div className="space-y-4 sm:space-y-6">
        {/* Add Experience Button */}
        <div>
          <Button className="w-full sm:w-auto px-4 py-2 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg flex justify-center items-center gap-2">
            <Plus className="w-[14px] h-[14px]" />
            Add Experience
          </Button>
        </div>

        {/* Experience Cards */}
        {experiences.length === 0 ? (
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
            {experiences.map((experience) => (
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