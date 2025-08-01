'use client';

import React from 'react';
import { ProfileProvider } from '@cognizant-summer-practice/shared-components';
import ProfileLayoutContent from './profile-layout-content';

export default function ProfileLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <ProfileProvider>
      <ProfileLayoutContent>
        {children}
      </ProfileLayoutContent>
    </ProfileProvider>
  );
}
  