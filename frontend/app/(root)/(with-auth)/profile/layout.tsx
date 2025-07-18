'use client';

import React, { useState, createContext, useContext } from 'react';
import ProfileSidebar from '@/components/profile-page/profile-sidebar';
import { User, Folder, Briefcase, Code, Layout, Settings } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

const ProfileContext = createContext<{
  activeTab: string;
  setActiveTab: (tab: string) => void;
}>({
  activeTab: 'basic-info',
  setActiveTab: () => {},
});

export const useProfile = () => useContext(ProfileContext);

export default function ProfileLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const [activeTab, setActiveTab] = useState('basic-info');

  const tabOptions = [
    { value: 'basic-info', label: 'Basic Info', icon: User },
    { value: 'projects', label: 'Projects', icon: Folder },
    { value: 'experience', label: 'Experience', icon: Briefcase },
    { value: 'skills', label: 'Skills', icon: Code },
    { value: 'template', label: 'Template', icon: Layout },
    { value: 'settings', label: 'Settings', icon: Settings },
  ];

  const getCurrentTabLabel = () => {
    const currentTab = tabOptions.find(tab => tab.value === activeTab);
    return currentTab ? currentTab.label : 'Select Tab';
  };

  return (
    <ProfileContext.Provider value={{ activeTab, setActiveTab }}>
      <div className="fixed top-16 left-0 right-0 bottom-0 flex lg:overflow-hidden">
        {/* Sidebar - Hidden on mobile, visible on desktop */}
        <div className="hidden lg:block w-72 h-full">
          <ProfileSidebar activeTab={activeTab} onTabChange={setActiveTab} />
        </div>
        
        {/* Main Content */}
        <div className="flex-1 bg-gray-50 overflow-y-auto">
          {/* Mobile Tab Selector */}
          <div className="lg:hidden bg-white border-b border-[#E2E8F0] p-4 sticky top-0 z-10">
            <Select value={activeTab} onValueChange={setActiveTab}>
              <SelectTrigger className="w-full bg-white border border-[#E2E8F0] rounded-lg h-10 px-3 text-sm text-[#020817] focus:ring-2 focus:ring-blue-500 focus:border-transparent">
                <SelectValue placeholder="Select a tab">
                  <div className="flex items-center gap-2">
                    {(() => {
                      const currentTab = tabOptions.find(tab => tab.value === activeTab);
                      if (currentTab) {
                        const Icon = currentTab.icon;
                        return (
                          <>
                            <Icon className="w-4 h-4 text-[#64748B]" />
                            <span>{currentTab.label}</span>
                          </>
                        );
                      }
                      return getCurrentTabLabel();
                    })()}
                  </div>
                </SelectValue>
              </SelectTrigger>
              <SelectContent className="bg-white border border-[#E2E8F0] rounded-lg shadow-lg z-50">
                {tabOptions.map((tab) => {
                  const Icon = tab.icon;
                  return (
                    <SelectItem
                      key={tab.value}
                      value={tab.value}
                      className="flex items-center gap-2 px-3 py-2 text-sm text-[#020817] hover:bg-[#F8FAFC] cursor-pointer"
                    >
                      <div className="flex items-center gap-2">
                        <Icon className="w-4 h-4 text-[#64748B]" />
                        <span>{tab.label}</span>
                      </div>
                    </SelectItem>
                  );
                })}
              </SelectContent>
            </Select>
          </div>
          {children}
        </div>
      </div>
    </ProfileContext.Provider>
  );
}
  