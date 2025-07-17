import React from 'react';
import { Avatar } from '@/components/ui/avatar/avatar';
import { Button } from '@/components/ui/button/button';
import styles from './messages-header.module.css';

interface MessagesHeaderProps {
  contactName: string;
  avatarSrc: string;
  onProfileClick: () => void;
}

const MessagesHeader: React.FC<MessagesHeaderProps> = ({ contactName, avatarSrc, onProfileClick }) => {
  return (
    <div className={styles.header}>
      <Avatar src={avatarSrc} alt={`${contactName}'s avatar`} />
      <div className={styles.contactInfo}>
        <h2 className={styles.contactName}>{contactName}</h2>
      </div>
      <Button text="Profile" onClick={onProfileClick} className={styles.profileButton} />
    </div>
  );
};

export default MessagesHeader;