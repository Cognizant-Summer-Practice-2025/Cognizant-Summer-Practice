import React from 'react';
import { Avatar as ShadcnAvatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';

interface AvatarProps {
  src: string;
  alt: string;
  fallback?: string;
  className?: string;
}

const Avatar: React.FC<AvatarProps> = ({ src, alt, fallback, className }) => {
  return (
    <ShadcnAvatar className={className}>
      <AvatarImage src={src} alt={alt} />
      <AvatarFallback>{fallback || alt.charAt(0).toUpperCase()}</AvatarFallback>
    </ShadcnAvatar>
  );
};

export default Avatar;