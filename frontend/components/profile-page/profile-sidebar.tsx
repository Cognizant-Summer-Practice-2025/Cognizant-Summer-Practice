'use client';

import React from 'react';
import { User, Folder, Briefcase, Code, Layout, Settings, FileText } from 'lucide-react';
import { usePortfolio } from '@/lib/contexts/portfolio-context';

interface ProfileSidebarProps {
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}

export default function ProfileSidebar({ activeTab = 'basic-info', onTabChange }: ProfileSidebarProps) {
  const { hasPublishedPortfolio } = usePortfolio();

  const basicMenuItems = [
    { id: 'basic-info', label: 'Basic Info', icon: User },
  ];

  const portfolioMenuItems = [
    { id: 'projects', label: 'Projects', icon: Folder },
    { id: 'experience', label: 'Experience', icon: Briefcase },
    { id: 'skills', label: 'Skills', icon: Code },
    { id: 'template', label: 'Template', icon: Layout },
    { id: 'settings', label: 'Settings', icon: Settings },
  ];

  const renderMenuItem = (item: typeof basicMenuItems[0]) => {
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
  };

  return (
    <div className="w-full h-full p-6 bg-white border-r border-[#E2E8F0] flex flex-col justify-start items-start gap-6">
      {/* Profile Card */}
      <div className="self-stretch h-[296px] lg:h-[296px] md:h-[250px] sm:h-[220px] rounded-lg border border-[#E2E8F0] flex flex-col justify-between items-center p-4 sm:p-6">
        <div className="flex flex-col items-center">
          {/* Profile Picture */}
          <img 
            className="w-16 h-16 sm:w-20 sm:h-20 rounded-full mb-3 sm:mb-4" 
            src="https://placehold.co/80x80" 
            alt="Profile"
          />
          
          {/* Name */}
          <div className="text-center text-[#020817] text-2xl sm:text-3xl lg:text-[40px] font-bold font-['Inter'] leading-tight mb-1 sm:mb-2">
            John Doe
          </div>
          
          {/* Title */}
          <div className="text-center text-[#64748B] text-sm sm:text-lg lg:text-xl font-medium font-['Inter'] leading-tight">
            Full Stack Developer
          </div>
        </div>
        
        {/* Stats */}
        <div className="flex justify-center items-start gap-4 sm:gap-6 lg:gap-8 mb-2">
          <div className="flex flex-col justify-start items-center">
            <div className="text-center text-[#020817] text-lg sm:text-xl lg:text-2xl font-semibold font-['Inter'] leading-6">
              1,234
            </div>
            <div className="text-center text-[#64748B] text-xs sm:text-sm font-medium font-['Inter'] leading-[22.4px]">
              Views
            </div>
          </div>
          <div className="flex flex-col justify-start items-center">
            <div className="text-center text-[#020817] text-lg sm:text-xl lg:text-2xl font-semibold font-['Inter'] leading-6">
              49
            </div>
            <div className="text-center text-[#64748B] text-xs sm:text-sm font-medium font-['Inter'] leading-[22.4px]">
              Likes
            </div>
          </div>
        </div>
      </div>
      
      {/* Menu */}
      <div className="self-stretch flex flex-col justify-start items-start gap-4">
        {/* Basic Section */}
        <div className="self-stretch flex flex-col justify-start items-start gap-1">
          {basicMenuItems.map(renderMenuItem)}
        </div>

        {/* Portfolio Section - Only show if user has a portfolio */}
        {hasPublishedPortfolio && (
          <div className="self-stretch flex flex-col justify-start items-start gap-1">
            {/* Portfolio Section Header */}
            <div className="self-stretch px-4 py-2 flex justify-start items-center gap-2">
              <FileText className="w-4 h-4 text-[#64748B]" />
              <div className="text-xs font-semibold text-[#64748B] uppercase tracking-wider">
                Portfolio
              </div>
            </div>
            
            {/* Portfolio Menu Items */}
            {portfolioMenuItems.map(renderMenuItem)}
          </div>
        )}

        {/* Show create portfolio message if no portfolio */}
        {!hasPublishedPortfolio && (
          <div className="self-stretch px-4 py-3 bg-[#F1F5F9] rounded-lg border border-[#E2E8F0]">
            <div className="text-center text-[#64748B] text-xs font-medium mb-2">
              Create a portfolio to access additional features
            </div>
            <button
              onClick={() => window.location.href = '/publish'}
              className="w-full px-3 py-2 bg-[#2563EB] text-white text-xs font-medium rounded-md hover:bg-[#1D4ED8] transition-colors"
            >
              Create Portfolio
            </button>
          </div>
        )}
      </div>
    </div>
  );
} 