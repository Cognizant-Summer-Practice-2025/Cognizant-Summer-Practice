'use client';

import React from 'react';
import { ProfileProvider } from '@/lib/contexts/profile-context';
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
  