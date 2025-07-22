import React from 'react';
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
            <img 
              src={getSafeImageUrl(basicInfo.profileImage)} 
              alt={basicInfo.name}
              className="profile-image"
              onError={(e) => {
                // Fallback to placeholder if image fails to load
                e.currentTarget.src = getSafeImageUrl('');
              }}
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