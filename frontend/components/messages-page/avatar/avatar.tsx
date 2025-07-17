import React from 'react';
import { Avatar as AntdAvatar } from 'antd';

interface AvatarProps {
  src: string;
  alt: string;
}

const Avatar: React.FC<AvatarProps> = ({ src, alt }) => {
  return <AntdAvatar src={src} alt={alt} />;
};

export default Avatar;