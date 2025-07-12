'use client';

import React from 'react';
import { User, Folder, Briefcase, Code, Layout, Settings } from 'lucide-react';

interface ProfileSidebarProps {
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}

export default function ProfileSidebar({ activeTab = 'projects', onTabChange }: ProfileSidebarProps) {
  const menuItems = [
    { id: 'basic-info', label: 'Basic Info', icon: User },
    { id: 'projects', label: 'Projects', icon: Folder },
    { id: 'experience', label: 'Experience', icon: Briefcase },
    { id: 'skills', label: 'Skills', icon: Code },
    { id: 'template', label: 'Template', icon: Layout },
    { id: 'settings', label: 'Settings', icon: Settings },
  ];

  return (
    <div className="w-full h-full p-6 bg-white border-r border-[#E2E8F0] flex flex-col justify-start items-start gap-6">
      {/* Profile Card */}
      <div className="self-stretch h-[296px] relative rounded-lg border border-[#E2E8F0]">
        {/* Profile Picture */}
        <img 
          className="w-20 h-20 absolute left-[95.5px] top-[25px] rounded-full" 
          src="https://placehold.co/80x80" 
          alt="Profile"
        />
        
        {/* Name */}
        <div className="w-[221px] absolute left-[25px] top-[121px] flex flex-col justify-start items-center">
          <div className="text-center text-[#020817] text-[40px] font-bold font-['Inter'] leading-[48px]">
            John Doe
          </div>
        </div>
        
        {/* Title */}
        <div className="w-[221px] absolute left-[25px] top-[177px] flex flex-col justify-start items-center">
          <div className="text-center text-[#64748B] text-xl font-medium font-['Inter'] leading-8">
            Full Stack Developer
          </div>
        </div>
        
        {/* Stats */}
        <div className="w-[221px] absolute left-[25px] top-[225px] flex justify-center items-start gap-8">
          <div className="flex flex-col justify-start items-center">
            <div className="text-center text-[#020817] text-2xl font-semibold font-['Inter'] leading-6">
              1,234
            </div>
            <div className="text-center text-[#64748B] text-sm font-medium font-['Inter'] leading-[22.4px]">
              Views
            </div>
          </div>
          <div className="flex flex-col justify-start items-center">
            <div className="text-center text-[#020817] text-2xl font-semibold font-['Inter'] leading-6">
              49
            </div>
            <div className="text-center text-[#64748B] text-sm font-medium font-['Inter'] leading-[22.4px]">
              Likes
            </div>
          </div>
        </div>
      </div>
      
      {/* Navigation Menu */}
      <div className="self-stretch flex flex-col justify-start items-start gap-1">
        {menuItems.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.id;
          
          return (
            <button
              key={item.id}
              onClick={() => onTabChange?.(item.id)}
              className={`self-stretch px-4 py-3 rounded-lg flex justify-start items-center gap-3 transition-colors ${
                isActive 
                  ? 'bg-[#2563EB] text-[#F8FAFC]' 
                  : 'hover:bg-gray-50 text-[#64748B]'
              }`}
            >
              <div className="w-4 flex flex-col justify-start items-center">
                <Icon className="w-[14px] h-[14px]" />
              </div>
              <div className="text-sm font-normal font-['Arial']">
                {item.label}
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
} 