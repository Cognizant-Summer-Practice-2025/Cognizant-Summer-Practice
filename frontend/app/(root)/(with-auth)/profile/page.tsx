
import React from 'react';
import BasicInfo from '@/components/basic-info';

const ProfilePage = () => {
  return (
    <div className="h-full p-8 overflow-hidden">
      <div className="max-w-4xl mx-auto h-full flex items-start">
        <BasicInfo />
      </div>
    </div>
  );
};

export default ProfilePage;