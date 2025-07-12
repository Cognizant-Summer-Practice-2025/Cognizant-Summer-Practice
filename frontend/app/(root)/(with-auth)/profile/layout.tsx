'use client';

import React, { useState, createContext, useContext } from 'react';
import ProfileSidebar from '@/components/profile-page/profile-sidebar';

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

  return (
    <ProfileContext.Provider value={{ activeTab, setActiveTab }}>
      <div className="fixed top-16 left-0 right-0 bottom-0 flex overflow-hidden">
        {/* Sidebar */}
        <div className="w-72 h-full">
          <ProfileSidebar activeTab={activeTab} onTabChange={setActiveTab} />
        </div>
        
        {/* Main Content */}
        <div className="flex-1 bg-gray-50 overflow-hidden">
          {children}
        </div>
      </div>
    </ProfileContext.Provider>
  );
}
  