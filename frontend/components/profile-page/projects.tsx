'use client';

import React from 'react';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';

export default function Projects() {
  const projects = [
    {
      id: 1,
      title: "E-Commerce Platform",
      description: "Full-stack e-commerce solution built with React and Node.js",
      image: "https://placehold.co/754x200",
      technologies: ["React", "Node.js", "MongoDB"]
    },
    {
      id: 2,
      title: "Task Management App",
      description: "Collaborative task management tool with real-time updates",
      image: "https://placehold.co/754x200",
      technologies: ["Vue.js", "Socket.io", "PostgreSQL"]
    }
  ];

  return (
    <div className="bg-white rounded-lg shadow-sm p-8 w-full min-h-[600px]">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Projects</h1>
      <p className="text-gray-600 mb-8">Manage your portfolio projects</p>
      
      <div className="space-y-6">
        {/* Add Project Button */}
        <div>
          <Button className="px-4 py-2 bg-[#2563EB] hover:bg-[#1d4ed8] text-[#F8FAFC] text-sm font-normal rounded-lg flex justify-center items-center gap-2">
            <Plus className="w-[14px] h-[14px]" />
            Add Project
          </Button>
        </div>

        {/* Projects Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {projects.map((project) => (
            <div
              key={project.id}
              className="p-[1px] bg-white overflow-hidden rounded-lg border border-[#E2E8F0] flex flex-col justify-start items-start"
            >
              <img
                className="w-full h-[200px] object-cover"
                src={project.image}
                alt={project.title}
              />
              <div className="w-full pt-[23px] pb-6 px-6 flex flex-col justify-start items-start gap-2">
                <div className="w-full pb-[0.8px] flex flex-col justify-start items-start">
                  <h3 className="w-full text-[#020817] text-lg font-semibold font-['Inter'] leading-[28.8px]">
                    {project.title}
                  </h3>
                </div>
                <div className="w-full flex flex-col justify-start items-start">
                  <p className="w-full text-[#64748B] text-sm font-normal font-['Inter'] leading-[19.6px]">
                    {project.description}
                  </p>
                </div>
                <div className="w-full pt-2 flex justify-start items-start gap-2 flex-wrap">
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
                <div className="w-full pt-2 flex justify-start items-start gap-2">
                  <Button
                    variant="outline"
                    className="px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal"
                  >
                    Edit
                  </Button>
                  <Button
                    variant="outline"
                    className="px-[17px] py-[9px] rounded-lg border border-[#E2E8F0] text-[#020817] text-sm font-normal"
                  >
                    Delete
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
} 