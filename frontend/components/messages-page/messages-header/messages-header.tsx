import React from 'react';
import { Avatar } from 'antd';
import { Button } from '@/components/ui/button';
import './style.css';

interface MessagesHeaderProps {
  contactName: string;
  avatarSrc: string;
  onProfileClick: () => void;
}

const MessagesHeader: React.FC<MessagesHeaderProps> = ({ contactName, avatarSrc, onProfileClick }) => {
  return (
    <div className="messages-header">
      <Avatar src={avatarSrc} size={40} className="contact-avatar">
        {contactName.charAt(0).toUpperCase()}
      </Avatar>
      <div className="contact-info">
        <h2 className="contact-name">{contactName}</h2>
      </div>
      <Button onClick={onProfileClick} className="profile-button">
        Profile
      </Button>
    </div>
  );
};

export default MessagesHeader;