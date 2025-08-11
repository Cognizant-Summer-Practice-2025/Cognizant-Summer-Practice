import React from 'react';
import Image from 'next/image';
import { UserProfile } from '@/lib/portfolio';
import { Badge } from '@/components/ui/badge';
import { MapPin, Mail } from 'lucide-react';
import { getSafeImageUrl } from '@/lib/image';

interface HeaderProps {
  basicInfo: UserProfile;
}

export function Header({ basicInfo }: HeaderProps) {
  return (
    <div className="modern-header">
      {basicInfo.profileImage && (
        <Image 
          src={getSafeImageUrl(basicInfo.profileImage)} 
          alt={basicInfo.name}
          className="modern-profile-image"
          width={120}
          height={120}
        />
      )}
      
      <h1 className="modern-profile-name">{basicInfo.name}</h1>
      
      {basicInfo.title && (
        <p className="modern-profile-title">{basicInfo.title}</p>
      )}
      
      <div className="flex items-center justify-center gap-4 mb-4">
        {basicInfo.location && (
          <Badge variant="secondary" className="flex items-center gap-1">
            <MapPin size={12} />
            {basicInfo.location}
          </Badge>
        )}
        
        {basicInfo.email && (
          <Badge variant="secondary" className="flex items-center gap-1">
            <Mail size={12} />
            {basicInfo.email}
          </Badge>
        )}
      </div>
      
      {basicInfo.bio && (
        <p className="modern-profile-bio">{basicInfo.bio}</p>
      )}
    </div>
  );
} 