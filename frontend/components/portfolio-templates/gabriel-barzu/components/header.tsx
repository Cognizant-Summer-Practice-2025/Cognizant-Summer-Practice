import React from 'react';
import Image from 'next/image';
import { BasicInfo } from '@/lib/portfolio';
import { getSafeImageUrl } from '@/lib/image';

interface HeaderProps {
  basicInfo: BasicInfo;
}

export function Header({ basicInfo }: HeaderProps) {
  return (
    <header className="gb-header">
      <div className="header-content">
        <div className="profile-section">
          <div className="profile-image-container">
            <Image 
              src={getSafeImageUrl(basicInfo.profileImage)} 
              alt={basicInfo.name}
              className="profile-image"
              width={150}
              height={150}
            />
          </div>
          <div className="profile-info">
            <h1 className="profile-name">{basicInfo.name}</h1>
            <h2 className="profile-title">{basicInfo.title}</h2>
            {basicInfo.location && (
              <p className="profile-location">{basicInfo.location}</p>
            )}
          </div>
        </div>
        {basicInfo.bio && (
          <div className="bio-section">
            <p className="bio-text">{basicInfo.bio}</p>
          </div>
        )}
      </div>
    </header>
  );
} 