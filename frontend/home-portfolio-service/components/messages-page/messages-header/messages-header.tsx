import React from 'react';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
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
      <Avatar className="contact-avatar w-10 h-10">
        <AvatarImage src={avatarSrc} alt={contactName} />
        <AvatarFallback>{contactName.charAt(0).toUpperCase()}</AvatarFallback>
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