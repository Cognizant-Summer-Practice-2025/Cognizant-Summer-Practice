import React, { useState } from 'react';
import UserSearchModal from '../user-search-modal/user-search-modal';
import { SearchUser } from '@/lib/user';
import './style.css';

interface Contact{
    id: string;
    name: string;
    avatar: string;
    lastMessage: string;
    timestamp: string;
    isActive?: boolean;
    isOnline?: boolean;
    unreadCount?: number;
    userId?: string;
    professionalTitle?: string;
}

interface MessagesSidebarProps {
    contacts: Contact[];
    selectedContact: Contact | null;
    onSelectContact: (contact: Contact) => void;
    onNewConversation?: (user: SearchUser) => void;
}

const ContactItem: React.FC<{
    contact: Contact;
    isSelected: boolean;
    onClick: () => void;
}> = ({contact, isSelected, onClick}) => {
    return (
        <div 
      className={`contact-item ${isSelected ? 'contact-item-active' : ''}`}
      onClick={onClick}
    >
      <div className="contact-avatar-container">
        <img 
          className="contact-avatar" 
          src={contact.avatar} 
          alt={contact.name}
        />
      </div>
      <div className="contact-details">
        <div className="contact-header">
          <div className="contact-name">{contact.name}</div>
          <div className="contact-timestamp">{contact.timestamp}</div>
        </div>
        <div className="contact-message">
          {contact.lastMessage && contact.lastMessage.length > 100 
            ? `${contact.lastMessage.substring(0, 100)}...` 
            : contact.lastMessage}
        </div>
        <div className="contact-status">
          {(contact.unreadCount || 0) > 0 && (
            <div className="unread-badge">
              {contact.unreadCount}
           </div>
          )}
        </div>
      </div>
    </div>
    );
};

const MessagesSidebar: React.FC<MessagesSidebarProps> = ({
  contacts,
  selectedContact,
  onSelectContact,
  onNewConversation
}) => {
  const [isSearchModalVisible, setIsSearchModalVisible] = useState(false);

  const handleNewButtonClick = () => {
    setIsSearchModalVisible(true);
  };

  const handleUserSelect = (user: SearchUser) => {
    if (onNewConversation) {
      onNewConversation(user);
    }
    setIsSearchModalVisible(false);
  };

  const handleSearchModalClose = () => {
    setIsSearchModalVisible(false);
  };

  return (
    <div className="messages-sidebar">
      <div className="sidebar-header">
        <div className="sidebar-title">Messages</div>
        <div className="new-message-button" onClick={handleNewButtonClick}>
          <div className="new-message-icon">+</div>
          <div className="new-message-text">New</div>
        </div>
      </div>
      
      <div className="contacts-list">
        {contacts.map((contact) => (
          <ContactItem
            key={contact.id}
            contact={contact}
            isSelected={selectedContact?.id === contact.id}
            onClick={() => onSelectContact(contact)}
          />
        ))}
      </div>

      <UserSearchModal
        visible={isSearchModalVisible}
        onClose={handleSearchModalClose}
        onUserSelect={handleUserSelect}
      />
    </div>
  );
};

export default MessagesSidebar;