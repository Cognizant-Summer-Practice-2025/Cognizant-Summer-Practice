'use client';

import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';

export default function Experience() {
  const experiences = [
    {
      id: 1,
      title: "Senior Frontend Developer",
      company: "Tech Solutions Inc.",
      period: "Jan 2022 - Present",
      description: "Led development of modern web applications using React and TypeScript. Collaborated with cross-functional teams to deliver high-quality user experiences.",
      skills: ["React", "TypeScript", "Team Leadership"]
    },
    {
      id: 2,
      title: "Full Stack Developer",
      company: "StartupXYZ",
      period: "Jun 2020 - Dec 2021",
      description: "Developed and maintained full-stack applications using Node.js and React. Implemented RESTful APIs and managed database operations.",
      skills: ["Node.js", "MongoDB", "API Development"]
    }
  ];

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
                      {experience.title}
                    </h3>
                  </div>
                  <div className="w-full flex flex-col justify-start items-start">
                    <p className="w-full text-[#020817] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                      {experience.company}
                    </p>
                  </div>
                  <div className="w-full flex flex-col justify-start items-start">
                    <p className="w-full text-[#64748B] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                      {experience.period}
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
              <div className="w-full flex flex-col justify-start items-start">
                <p className="w-full text-[#020817] text-sm sm:text-base font-normal font-['Inter'] leading-relaxed sm:leading-[25.6px]">
                  {experience.description}
                </p>
              </div>
              <div className="w-full flex justify-start items-start gap-2 sm:gap-[4.5px] flex-wrap">
                {experience.skills.map((skill, index) => (
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
            </div>
          ))}
        </div>
      </div>
    </div>
  );
} 