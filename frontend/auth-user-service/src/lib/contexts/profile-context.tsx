'use client';

import React, { useState, createContext, useContext, ReactNode } from 'react';

interface ProfileContextType {
  activeTab: string;
  setActiveTab: (tab: string) => void;
}

const ProfileContext = createContext<ProfileContextType>({
  activeTab: 'basic-info',
  setActiveTab: () => {},
});

export const useProfile = () => useContext(ProfileContext);

interface ProfileProviderProps {
  children: ReactNode;
}

export function ProfileProvider({ children }: ProfileProviderProps) {
  const [activeTab, setActiveTab] = useState('basic-info');

  return (
    <ProfileContext.Provider value={{ activeTab, setActiveTab }}>
      {children}
    </ProfileContext.Provider>
  );
}
