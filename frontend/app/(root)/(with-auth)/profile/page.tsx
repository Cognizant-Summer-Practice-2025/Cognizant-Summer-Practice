'use client';

import React from 'react';
import BasicInfo from '@/components/profile-page/basic-info';
import Projects from '@/components/profile-page/projects';
import Experience from '@/components/profile-page/experience';
import Skills from '@/components/profile-page/skills';
import Template from '@/components/profile-page/template';
import Settings from '@/components/profile-page/settings';
import { useProfile } from './layout';

const ProfilePage = () => {
  const { activeTab } = useProfile();

  console.log('Current activeTab:', activeTab);

  const renderContent = () => {
    switch (activeTab) {
      case 'basic-info':
        return <BasicInfo />;
      case 'projects':
        return <Projects />;
      case 'experience':
        return <Experience />;
      case 'skills':
        return <Skills />;
      case 'template':
        return <Template />;
      case 'settings':
        return <Settings />;
      default:
        return <BasicInfo />;
    }
  };

  return (
    <div className="h-full p-8 overflow-hidden">
      <div className="max-w-4xl mx-auto h-full flex items-start">
        {renderContent()}
      </div>
    </div>
  );
};

export default ProfilePage;